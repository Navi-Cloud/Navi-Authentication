#!/bin/bash

# NEEDS-G++ supports more C++17 standard or more[likely GCC/G++ 9 wil work.]
# For macOS, Clang 12 will work fine.
# In case of DYLIB Linking error, enable verbose stage so we can quickly investigate issue.
g++ -v -std=c++17 ./LocalTestRunner/generate_test_script.cpp -o generateScript

# Generate Script
./generateScript

# Change Permission
chmod a+x run_test.sh

# Run Test
./run_test.sh

# Post - Process
rm ./generateScript ./run_test.sh
