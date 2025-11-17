#!/bin/bash

# Kill all mono-sgen processes
pkill -9 -f mono-sgen

# Remove compiled sources from bin/ob
find . -type d -name "bin" -exec rm  -rf {} 2> /dev/null \;
find . -type d -name "obj" -exec rm  -rf {} 2> /dev/null \;

# Remove Xcode build cache
rm -rf ~/Library/Developer/Xcode/DerivedData

# Remove Rider cache
rm -rf .idea
