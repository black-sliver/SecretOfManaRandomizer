#!/bin/sh

# This is just a test script

cc test.c -L ../../bin/Release/net10.0/native -l:SoMRandomizer.api.so -o test
