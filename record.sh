#! /bin/bash
touch output.txt
truncate -s 0 output.txt
stty raw -echo < /dev/ttyUSB0
cat -vte </dev/ttyUSB0 > output.txt
