import re
import tempfile
import regex
import argparse
import pymongo
import bson
import csv
import os
import decimal
import zipfile
import urllib2
from datetime import datetime
import email.utils
import shutil

GEOIP_URL = "http://geolite.maxmind.com/download/geoip/database/GeoLite2-Country-CSV.zip"
INSERT_BATCH_SZ = 1000
TEMP = tempfile.gettempdir()
TEMP_ZIP = os.path.join(TEMP, os.path.basename(GEOIP_URL))
TEMP_UZIP = os.path.join(TEMP, os.path.basename(GEOIP_URL)[:-4])

parser = argparse.ArgumentParser(description="Deploys GeoLite2 Country DB to MongoDB")
parser.add_argument("--mongo", type=str, required=True)
parser.add_argument("--db-name", type=str, default="geo")
parser.add_argument("--drop", action='store_true')
args = parser.parse_args()

c = pymongo.MongoClient(args.mongo)
version = c[args.db_name]["version"]
country = c[args.db_name]["country"]
country_v6 = c[args.db_name]["country_v6"]

country_sz = country.count({})
country_v6_sz = country_v6.count({})
if country_sz > 0:
    if args.drop:
        print("Collection 'country' contains %i documents which will be removed." % country_sz)
        country.drop()
    else:
        print("ERROR: Collection 'country' contains %i documents. Drop collection to continue or use --drop flag." % country_sz)
        exit(2)

if country_v6_sz > 0:
    if args.drop:
        print("Collection 'country_v6' contains %i documents which will be removed." % country_v6_sz)
        country_v6.drop()
    else:
        print("ERROR: Collection 'country_v6' contains %i documents. Drop collection to continue or use --drop flag." % country_v6_sz)
        exit(2)

print("Downloading %s." % GEOIP_URL)

response = urllib2.urlopen(GEOIP_URL)
zip = response.read()
zip_sz = len(zip)
d = email.utils.parsedate(response.headers['last-modified'])[:6]
db_time = datetime(d[0], d[1], d[2], d[3], d[4], d[5])
with open(TEMP_ZIP, "wb") as f:
    f.write(zip)
print("Download complete. File size %i." % zip_sz)

print("Unzipping file to %s." % TEMP_UZIP)
zip_ref = zipfile.ZipFile(TEMP_ZIP, 'r')
zip_ref.extractall(TEMP_UZIP)
zip_ref.close()

print("Unzip completed.")

os.unlink(TEMP_ZIP)

IPV4_CSV_PATH = None
IPV6_CSV_PATH = None
L10N_CSV_PATH = None

print("Searching CSV files.")
for d in os.walk(TEMP_UZIP):
    for f in d[2]:
        if 'ipv4.csv' in f.lower():
            IPV4_CSV_PATH = os.path.join(d[0], f)
        elif 'ipv6.csv' in f.lower():
            IPV6_CSV_PATH = os.path.join(d[0], f)
        elif 'locations-en.csv' in f.lower():
            L10N_CSV_PATH = os.path.join(d[0], f)

print("IPV4_CSV_PATH=%s" % IPV4_CSV_PATH)
print("IPV6_CSV_PATH=%s" % IPV6_CSV_PATH)
print("L10N_CSV_PATH=%s" % L10N_CSV_PATH)
print("Search completed.")

if L10N_CSV_PATH is None or (IPV4_CSV_PATH is None and IPV6_CSV_PATH is None):
    print("ERROR: Required files not found")
    exit(1)

def cidr_to_range_ipv4(cidr):
    m = re.match("(\\d{1,3})\\.(\\d{1,3})\\.(\\d{1,3})\\.(\\d{1,3})/(\\d+)", cidr)
    if m is None:
        return None
    bytes = []
    for i in range(1, 5, 1):
        bytes.append(int(m.group(i)))
    mask = int(m.group(5))
    start = (bytes[0] << 24) | (bytes[1] << 16) | (bytes[2] << 8) | bytes[3]
    end = 1
    mask = 32 - mask - 1
    while mask > 0:
        end <<= 1
        end |= 1
        mask -= 1
    end |= start
    return {"start": start, "end": end}

def cidr_to_range_ipv6(cidr):
    cidr = cidr.lower()
    m = regex.match("(?:([0-9a-f]{1,4})::?)*([0-9a-f]{1,4})?/(\\d+)", cidr)
    if m is None:
        return None
    sep_idx = cidr.find("::")
    sep_at = 0
    if sep_idx > -1:
        sep_at = cidr[:sep_idx].count(":") + 1
    words = []
    for c in m.captures(1):
        words.append(int("0x" + c, base=16))
    if len(m.captures(2)) > 0:
        words.append(int("0x" + m.captures(2)[0], base=16))
    if sep_at > 0:
        sep_ins_amount = 8 - len(words)
        while sep_ins_amount > 0:
            words.insert(sep_at, 0)
            sep_ins_amount -= 1

    mask = int(m.captures(3)[0])
    start = (words[0] << 112) | (words[1] << 96) | (words[2] << 80) | (words[3] << 64) | (words[4] << 48) | (words[5] << 32) | (words[6] << 16) | words[7]
    end = 1
    mask = 128 - mask - 1
    while mask > 0:
        end <<= 1
        end |= 1
        mask -= 1
    end |= start
    return {"start": start, "end": end}

def cidr_to_range(cidr):
    ret = cidr_to_range_ipv4(cidr)
    if ret is not None:
        return ret
    return cidr_to_range_ipv6(cidr)

def ipv4_int_to_str(ip):
    bytes = []
    bytes.append(ip >> 24)
    bytes.append((ip >> 16) & 0xFF)
    bytes.append((ip >> 8) & 0xFF)
    bytes.append(ip & 0xFF)
    return "%i.%i.%i.%i" % (bytes[0], bytes[1], bytes[2], bytes[3])

def ipv6_int_to_str(ip):
    bytes = []
    bytes.append(ip >> 112)
    bytes.append((ip >> 96) & 0xFFFF)
    bytes.append((ip >> 80) & 0xFFFF)
    bytes.append((ip >> 64) & 0xFFFF)
    bytes.append((ip >> 48) & 0xFFFF)
    bytes.append((ip >> 32) & 0xFFFF)
    bytes.append((ip >> 16) & 0xFFFF)
    bytes.append(ip & 0xFFFF)
    return "%x.%x.%x.%x.%x.%x.%x.%x" % (bytes[0], bytes[1], bytes[2], bytes[3], bytes[4], bytes[5], bytes[6], bytes[7])

country.create_index("start")
country.create_index("end")
country_v6.create_index("start")
country_v6.create_index("end")

print("Loading localization.")
geo_code_to_iso = {}
with open(L10N_CSV_PATH) as f:
    reader = csv.reader(f)
    line = 1
    header = next(reader)
    for r in reader:
        geo_code_to_iso[int(r[0])] = r[4]
print("Loading completed. dict size %i." % len(geo_code_to_iso))

insert_docs = []

print("Importing ipv4 data.")
line = 1
with open(IPV4_CSV_PATH) as f:
    reader = csv.reader(f)
    header = next(reader)
    for r in reader:
        line += 1
        ip_range = cidr_to_range_ipv4(r[0])
        if ip_range is None:
            print(r[0] + " parse error. line:%i" % line)
            continue
        d_start = ip_range["start"]
        d_end = ip_range["end"]
        geo_code = 0
        if len(r[1])> 0:
            geo_code = int(r[1])
        elif len(r[2]) > 0:
            geo_code = int(r[2])
        else:
            print("Unknown country. line:%i" % line)
            continue
        insert_docs.append({"network": r[0], "start":  d_start, "end": d_end, "country_iso": geo_code_to_iso[geo_code]})
        if len(insert_docs) >= INSERT_BATCH_SZ:
            country.insert_many(insert_docs)
            insert_docs = []

if len(insert_docs) > 0:
    country.insert_many(insert_docs)
    insert_docs = []
    
print("Import ipv4 data completed. Imported %i documents." % (line - 1))

print("Importing ipv6 data.")
with open(IPV6_CSV_PATH) as f:
    reader = csv.reader(f)
    line = 1
    header = next(reader)
    for r in reader:
        line += 1
        ip_range = cidr_to_range_ipv6(r[0])
        if ip_range is None:
            print(r[0] + " parse error. line:%i" % line)
            continue
        d_start = str(ip_range["start"])
        d_end = str(ip_range["end"])
        geo_code = 0
        if len(r[1])> 0:
            geo_code = int(r[1])
        elif len(r[2]) > 0:
            geo_code = int(r[2])
        else:
            print("Unknown country. line:%i" % line)
            continue
        insert_docs.append({"network": r[0], "start":  d_start, "end": d_end, "country_iso": geo_code_to_iso[geo_code]})
        if len(insert_docs) >= INSERT_BATCH_SZ:
            country_v6.insert_many(insert_docs)
            insert_docs = []

if len(insert_docs) > 0:
    country_v6.insert_many(insert_docs)
    insert_docs = []

print("Import ipv6 data completed. Imported %i documents." % (line - 1))

version.insert_one({"db_time": db_time, "import_time": datetime.utcnow()})
shutil.rmtree(TEMP_UZIP)


