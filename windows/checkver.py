import sys
import os
import re
import datetime

# make sure version number embedded in the code is the same as version
# number given as argument. to be used as a verification check in build
# script

APP_VER_RE = 'APP_VER = "([^"]+)"'

ver_file_and_regex = [("Form1.cs", APP_VER_RE)]

def get_regex_in_file(path, regexp):
    filedata = file(path).read()
    match = re.search(regexp, filedata)
    if match:
        return match.group(1)

def usage():
    print("Usage: checkver.py version")

def main():
    if len(sys.argv) != 2:
        usage()
        return 1
    version = sys.argv[1]
    for (f, regex) in ver_file_and_regex:
        ver = get_regex_in_file(f, regex)
        if ver != version:
          print("%s cannot be found in file %s. Probably didn't update version number." % (version, f))
          return 1
    return 0

if __name__ == "__main__":
    exitcode = main()
    sys.exit(exitcode)
