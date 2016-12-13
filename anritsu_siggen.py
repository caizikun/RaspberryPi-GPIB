#anritsu_siggen.py
import sys
import simplessh
from time import sleep

class anritsu:
    def __init__(self, addr = 11, comport = '/dev/ttyUSB0'):
            self.con = simplessh.Connection( host = '192.168.1.66',username = 'pi',
                                        private_key = None,
                                        password = 'raspberry',
                                        port = 22 )
            self.comport = comport
            self.addr = addr
            self.con.execute(r'echo ++mode 1 >%s; ' % self.comport)
            self.con.execute(r'echo ++ifc >%s; '% self.comport)
            self.con.execute(r'echo ++read_tmo_ms 200 >%s; '% self.comport)
            self.con.execute(r'echo ++auto 1 >%s; ' % self.comport)
            self.out_file = 'output.txt'
            self.con.execute('stty raw -echo < /dev/ttyUSB0; cat -vte </dev/ttyUSB0 > %s' % self.out_file)

    def send_cmd(self, cmd):
        self.con.execute(r'echo ++addr %s >%s; '% (self.addr, self.comport))
        self.con.execute(r'echo '+ cmd + ' >%s; ' % self.comport)

    def RF_ON(self):
        self.con.execute(r'echo ++addr %s >%s; '% (self.addr, self.comport))
        self.con.execute(r'echo RF1 >%s; ' % self.comport)

    def RF_OFF(self):
        self.con.execute(r'echo ++addr %s >%s; '% (self.addr, self.comport))
        self.con.execute(r'echo RF0 >%s; ' % self.comport)

    def set_freq(self, freq):
        self.con.execute(r'echo ++addr %s >%s; '% (self.addr, self.comport))
        self.con.execute(r'echo CF1 %s GH CF1 >%s; ' % (str(freq), self.comport))

    def get_freq(self):
        self.con.execute(r'echo ++addr %s >%s; '% (self.addr, self.comport))
        self.con.execute(r'truncate -s 0 %s; stty raw -echo < %s; tail <%s > %s | echo OF1 >%s; '
                         %(self.out_file, self.comport, self.comport,self.out_file, self.comport))

        sleep(1)
        self.con.get(self.out_file)
        f = open(self.out_file)
        output = f.readline()
        f.close()
        return output.strip('\x00').strip('\n').strip(' ').strip('^M')

    def set_power(self, power):
        self.con.execute(r'echo ++addr %s >%s; '% (self.addr, self.comport))
        self.con.execute(r'echo L0 %s DM L0 >%s; ' % (str(power), self.comport))

    def get_power(self):
        self.con.execute(r'echo ++addr %s >%s; '% (self.addr, self.comport))
        self.con.execute(r'echo ++read >%s; ' % self.comport)
        self.con.execute(r'truncate -s 0 %s; stty raw -echo < %s; tail  <%s > %s | echo OL0 >%s; '
                         %(self.out_file, self.comport, self.comport,self.out_file, self.comport))

        sleep(1)
        self.con.get(self.out_file)
        f = open(self.out_file)
        output = f.readline()
        f.close()
        return output.strip('\x00').strip('\n').strip(' ').strip('^M')

def anritsu_test():
    # test of the class
    from random import randint

    freq = randint(1,67)
    power = randint(-20,10)
    print(freq)
    print(power)
    an = anritsu()
    an.set_freq(freq)
    x = an.get_freq()
    print(x)
    an.set_power(power)
    x = an.get_power()
    print(x)

    # def set_step_sweep(self, start, stop, pts, dwell):
    #     pass
