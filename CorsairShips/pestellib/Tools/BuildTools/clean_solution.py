import glob
import shutil
import os
import argparse

parser = argparse.ArgumentParser(description='Cleans bin and obj dirs for all projects')
parser.add_argument('--path', type=str, required=True)
args = parser.parse_args()

bin_patt = os.path.join(args.path, '*', 'bin')
obj_patt = os.path.join(args.path, '*', 'obj')
list = glob.glob(bin_patt)
list.extend(glob.glob(obj_patt))
cache_dir = os.path.join(args.path, '.vs')
if os.path.isdir(cache_dir):
    list.append(cache_dir)

for l in list:
    print "remove '%s'" % l
    shutil.rmtree(l)

