#!/usr/bin/python

import sys
import os

def main():

    dataFile = open("systemTestsList.txt")
    

    for line in dataFile.readlines():
        testFileName, expectedResults = line.split(',')
        expectedResults = expectedResults.strip()
	if testFileName[0] == '#':
		continue;

        # need to exec bugreport.exe testFileName and check stdout vs expectedResults
        cmd = "./bin/Debug/bugreport.exe" + "  tests/simple/heap/" + testFileName
        print "running : " + testFileName
        results = os.popen(cmd).readlines()
        if len(results) == 1:
            if expectedResults == "":
                print "OK."
                continue
            else:
                print "ERROR: " + testFileName + " expected == " + expectedResults + " got nothing."
        else:
            if expectedResults == "" or expectedResults != results[1][:len(expectedResults)] or len(results) > 2:
                print "ERROR: expected \"%s\" got " % expectedResults
                for line in results[1:]:
                    print "\t%s" % line.strip()
            else:  
                print "OK."
        

if __name__ == "__main__":
    main()
    
    
    
