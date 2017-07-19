# test_over_ssh.py
import time
import sys
import simplessh
start = time.time()
con = simplessh.Connection()

# Test python execution
# con.execute('python test_keithley.py')


# truncate -s 0 output_prologix.txt;
# Set up Prologix Commands
con.execute(r'echo ++mode 1 >/dev/ttyUSB0; ')
con.execute(r'echo ++ifc >/dev/ttyUSB0; ')
con.execute(r'echo ++read_tmo_ms 200 >/dev/ttyUSB0; ')

# con.execute(r'echo ++addr 1o >/dev/ttyUSB0; ')

# con.execute(r'\x03')
time.sleep(1)
con.execute(r'echo ++addr 10 >/dev/ttyUSB0; ')
# con.execute(r'echo *CLS; >/dev/ttyUSB0; ')
# con.execute(r'echo INIT:CONT OFF; >/dev/ttyUSB0; ')

con.execute(r'truncate -s 0 output_prologix.txt;stty raw -echo < /dev/ttyUSB0; tail </dev/ttyUSB0 > output_prologix.txt | echo *IDN? >/dev/ttyUSB0; ')

time.sleep(1)
con.get('output_prologix.txt')
x = open('output_prologix.txt')
print(1)
[print(line) for line in x]
x.close()
