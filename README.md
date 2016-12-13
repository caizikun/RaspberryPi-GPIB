# RaspberryPi-GPIB
Using a network Raspberry Pi to Control GPIB Instruments using a Prologix GPIB Adapter

# Python
The python implementation uses the simplessh.py file to facilitate the conneciton. The anritsu_siggen.py and keysigt_atten.py are examples of how to make the connection and send commands usint the Prologix GPIB-USB adapter

# C#
The c# version is nearly fully contained in the program.cs file. On the raspberry pi, the file record.sh needs to be loaded into the root directory so the c# program can initiate it after a connection. This is only necessary if you want to be able to "Read" from the device. If you only want to "Write", it is unecessary. 


