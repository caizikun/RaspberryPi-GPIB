#keysight_atten.py
import sys
import simplessh
from time import sleep

class keysight:
    def __init__(self, addr = 28, comport = '/dev/ttyUSB0'):
            self.con = simplessh.Connection( host = '192.168.1.66',username = 'pi',
                                        private_key = None,
                                        password = 'raspberry',
                                        port = 22 )
            self.comport = comport
            self.addr = addr

            self.con.execute(r'echo ++mode 1 >%s; ' % self.comport)
            self.con.execute(r'echo ++ifc >%s; '% self.comport)
            self.con.execute(r'echo ++read_tmo_ms 100 >%s; '% self.comport)
            self.con.execute(r'echo ++auto 1 >%s; ' % self.comport)

            # This can only be set once! All devices will have to output to that file.
            self.out_file = 'output.txt'
            self.con.execute('stty raw -echo < /dev/ttyUSB0; cat -vte </dev/ttyUSB0 > %s' % self.out_file)

            # attenuator parameters
            self.attenx = 1
            self.atteny = 10


    def send_cmd(self, cmd):
        self.con.execute(r'echo ++addr %s >%s; '% (self.addr, self.comport))
        self.con.execute(r'echo '+ cmd + ' >%s; ' % self.comport)



    def set_atten(self, atten):

        y = atten // self.atteny
        x = atten % self.atteny
        setx = x * self.attenx
        sety = y * self.atteny
        self.con.execute(r'echo ++addr %s >%s; '% (self.addr, self.comport))
        self.con.execute(r'echo ATT:X '+ str(setx) + ' >%s; ' % self.comport)
        self.con.execute(r'echo ATT:Y '+ str(sety) + ' >%s; ' % self.comport)

    def get_attenx(self):
        self.con.execute(r'echo ++addr %s >%s; '% (self.addr, self.comport))
        self.con.execute(r'echo ++read >%s; ' % self.comport)
        self.con.execute(r'truncate -s 0 %s; stty raw -echo < %s; tail  <%s > %s | echo ATT:X? >%s; '
                         %(self.out_file, self.comport, self.comport,self.out_file, self.comport))

        sleep(.3)
        self.con.get(self.out_file)
        f = open(self.out_file)
        output = f.readline()
        f.close()
        if output == '':
            return 0
        else:
            return eval(output.strip('\x00').strip('\n').strip(' ').strip('$'))

    def get_atteny(self):
        self.con.execute(r'echo ++addr %s >%s; '% (self.addr, self.comport))
        self.con.execute(r'echo ++read >%s; ' % self.comport)
        self.con.execute(r'truncate -s 0 %s; stty raw -echo < %s; tail  <%s > %s | echo ATT:Y? >%s; '
                         %(self.out_file, self.comport, self.comport,self.out_file, self.comport))

        sleep(.3)
        self.con.get(self.out_file)
        f = open(self.out_file)
        output = f.readline()
        f.close()
        if output == '':
            return 0
        else:
            return eval(output.strip('\x00').strip('\n').strip(' ').strip('$'))

    def get_atten(self):
        xx = self.get_attenx()
        sleep(.3)
        yy = self.get_atteny()
        return xx + yy

def keysight_test():
    # test of the class
    from random import randint

    atten = randint(0,60)
    print(atten)
    ke = keysight()
    ke.set_atten(atten)
    sleep(.5)
    ke.get_atten()
