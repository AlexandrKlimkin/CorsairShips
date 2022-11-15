import os
import sys
import json
import csv

def logError(msg):
    print "!!!!!!!!!!!!!!!!!!!!!!!!!!!!"
    print "Error: " + msg
    print "!!!!!!!!!!!!!!!!!!!!!!!!!!!!"

def terminate(message):
    logError(message)
    sys.exit(1)

def formatNum(num):
    return "{:,}".format(num)

class ProductPriceData:
    def __init__(self, rawData):
        tempArray = rawData.split(";")

        if len(tempArray) < 4:
            terminate("It should be at least one non basic price in Price tab")

        self.baseCountry = tempArray[0].strip()
        self.basePrice = long(tempArray[1])
        self.countryPrices = {}
        self.keysOrder = []
        for i in xrange(2, len(tempArray), 2):
            # print str(i) + ": " + tempArray[i] + " <> " + str(i + 1) + ": " + tempArray[i + 1]
            key = tempArray[i].strip()
            value = long(tempArray[i + 1])

            self.keysOrder.append(key)
            self.countryPrices[key] = value

            i += 1

        print str(self.basePrice )

        # for key in self.keysOrder:
        #   print key + ">>" + str(self.countryPrices[key])

    def processPrice(self, countryId, exchangeRate, minimalPrice, priceMultiplier, roundMultiplier):
        #todo: check key exists

        if (not countryId in self.countryPrices):
            print "Skip " + countryId + " because there is no such country in source price"
            return

        print
        print countryId + ": ----------------------------------------------"
        multiplied = long(self.basePrice * exchangeRate * priceMultiplier)
        print "Base: " + formatNum(self.basePrice) + " exchangeRate: " + str(exchangeRate) + " priceMultiplier: " + str(priceMultiplier) + " roundMultiplier: "+ str(roundMultiplier)
        print "Multiplied: " + formatNum(multiplied)

        floorValue = long(multiplied / 1000000) * 1000000
        fractionalValue = long(multiplied - floorValue)

        print "Floor: " + formatNum(floorValue) + ". Decimal: " + formatNum(fractionalValue)

        if (fractionalValue > 750000):
            fractionalValue = 990000
        elif (fractionalValue > 250000):
            fractionalValue = 490000
        else:
            floorValue -= 1000000
            fractionalValue = 990000

        multiplied = floorValue + fractionalValue
        print "Eldorado price: " + formatNum(multiplied)

        #remove unused decimal part
        tempMultiplier = 1000000 / roundMultiplier
        rounded = long(multiplied / tempMultiplier) * tempMultiplier

        print "Rounded: " + formatNum(rounded)

        minPrice = long(minimalPrice * 1000000)
        if rounded < minPrice:
            self.countryPrices[countryId] = minPrice
            print "Replaced by minimal price: " + formatNum(minPrice)
        else:
            self.countryPrices[countryId] = rounded

    def generateOutput(self):
        output = self.baseCountry + "; " + str(self.basePrice) + "; "
        countriesLen = len(self.keysOrder)
        for i in range(0, countriesLen):
            countryId = self.keysOrder[i]
            output += countryId + "; " + str(self.countryPrices[countryId])
            if (i < countriesLen - 1):
                output += "; "

        return output
            


def processRecord(rawRecord):
    productId = rawRecord["Product ID"]
    if len(rawRecord["Pricing Template ID"]) > 0:
        print productId + " has a pricing template and will be skipped"
        return 

    priceData = ProductPriceData(rawRecord["Price"])

    for countryId in countriesData:
        exchangeRate = countriesData[countryId]["ExchangeRate"]
        roundMultiplier = countriesData[countryId]["RoundMultiplier"]
        minimalPrice    = countriesData[countryId]["MinimalPrice"]
        priceMultiplier = countriesData[countryId]["PriceMultiplier"]

        priceData.processPrice(countryId, exchangeRate, minimalPrice, priceMultiplier, roundMultiplier)

    rawRecord["Price"] = priceData.generateOutput()
    return rawRecord

current_dir = os.path.dirname(os.path.abspath(__file__))

argsCount = len(sys.argv)
if argsCount < 2:
    terminate("Give me a path to products CSV file")

pathToProducts = sys.argv[argsCount - 1]
if (not os.path.isfile(pathToProducts) or pathToProducts[-4:].lower() != ".csv"):
    logError(pathToProducts[-4:])
    terminate(pathToProducts + " is not a valid csv file. Check path or extension")

countriesData = {}

countriesFilepath = os.path.join(current_dir, "CountryPrices.json")
if (not os.path.isfile(countriesFilepath)):
    terminate("There is no json file with prices multipliers CountryPrices.json")

with open(countriesFilepath) as jsonPrices:
    try:
        countriesData = json.loads(jsonPrices.read().decode("utf-8", "replace"))
    except ValueError as e:
        terminate(countriesFilepath + " JSON is corrupted: " + str(e))

if len(countriesData) == 0:
    terminate(countriesFilepath + " is empty")

processedData = []
dataKeys = []

with open (pathToProducts, "rb") as csvfile:
    try:
        reader = csv.DictReader(csvfile, delimiter=',')
        dataKeys = reader.fieldnames

        for record in reader:
            processedData.append(processRecord(record))
    except ValueError as e:
        terminate(pathToProducts + " Failed to parse CSV file: " + str(e))

pathToExportFile = os.path.join(os.path.dirname(pathToProducts), "ExportPrices.csv")
with open (pathToExportFile, "wb") as csvfile:
    writer = csv.DictWriter(csvfile, fieldnames=dataKeys, delimiter=',', lineterminator='\n')
    writer.writeheader()
    for data in processedData:
        if data is None:
            logError("One of the processed records is null")
            continue
        writer.writerow(data)