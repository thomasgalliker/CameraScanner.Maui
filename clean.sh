#!/bin/bash
#
find . -type d -name "bin" -exec rm  -rf {} 2> /dev/null \;
find . -type d -name "obj" -exec rm  -rf {} 2> /dev/null \;
