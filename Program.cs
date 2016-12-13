using System;
using System.Threading;
using System.IO;
using System.Diagnostics;
using Renci.SshNet;

namespace InstrumentControl
{
    public class Connection
    {
        public bool isConnected = false;

        // SSH connection properties
        public static string host = "192.168.1.66";
        public static string user = "pi";
        public static string pass = "raspberry";

        // GPIB properties
        public string out_file = "output.txt";
        public string comport = "/dev/ttyUSB0";
        public SshClient client = new SshClient(host, user, pass);
        public SftpClient sftp = new SftpClient(host, user, pass);

        public void InitializeSSH()
        {
            // Make SSH/SFTP Connection

            client.Connect();
            sftp.Connect();

            // Initializing GPIB Sequence
            SshCommand cmd1 = client.CreateCommand(string.Format("echo ++mode 1 >{0}", comport)); cmd1.Execute();
            SshCommand cmd2 = client.CreateCommand(string.Format("echo ++ver >{0}", comport)); cmd2.Execute();
            SshCommand cmd3 = client.CreateCommand(string.Format("echo ++read_tmo_ms 200 >{0}", comport)); cmd3.Execute();
            SshCommand cmd4 = client.CreateCommand(string.Format("echo ++auto 1 >{0}", comport)); cmd4.Execute();

            // Initialize Recording
            SshCommand cmd5 = client.CreateCommand(string.Format("truncate -s 0 >{0}", out_file)); cmd5.Execute();

            Process p = new Process();
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardInput = true;
            p.StartInfo.RedirectStandardOutput = false;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.FileName = @"C:\Program Files (x86)\PuTTY\plink.exe";
            p.StartInfo.Arguments = String.Format(" -ssh {0}@{1} -pw {2}", user, host, pass);
            p.Start();

            Thread.Sleep(5000);
            p.StandardInput.WriteLine("./record.sh");
            Thread.Sleep(5000);

            // Verify Connection
            var output = client.RunCommand("echo Initialized");
            Console.WriteLine(output.Result);
            isConnected = true;
        }

        public void Close()
        {
            client.Disconnect();
            sftp.Disconnect();
        }

        public void SendCMD(string cmd)
        {
            if (!isConnected)
            {//Check Connection
                Console.WriteLine("Initialization needed");
                InitializeSSH();
            }

            // Send Command
            SshCommand cmd1 = client.CreateCommand(cmd); cmd1.Execute();
            var output = client.RunCommand("echo cmdSent");
            Console.WriteLine(output.Result);
        }

        public string Read()
        {
            try { File.Delete(out_file); }
            catch { }

            using (Stream file1 = File.OpenWrite(out_file))
            {
                sftp.DownloadFile(out_file, file1);
            }

            try
            {   // Open the text file using a stream reader.
                using (StreamReader sr = new StreamReader(out_file))
                {
                    // Read the stream to a string, and write the string to the console.
                    String line = sr.ReadToEnd();
                    Console.WriteLine(line.Replace(@"^M", "").Replace("\0", string.Empty).Trim());
                    return line.Replace(@"^M", "").Replace("\0", string.Empty).Trim();   
                }
                
            }
            catch (Exception e)
            {
                Console.WriteLine("The file could not be read:");
                Console.WriteLine(e.Message);
                return "";
            }

            

        }

    }

    public class Anritsu
    {
        public Anritsu(Connection con, int addr)
        {
            _connection = con;
            _addr = addr;
        }

        private Connection _connection;
        private int _addr;

        //private int addr = 11; // GPIB Address

        public void RFOn()
        {
            if (!_connection.isConnected)
            {//Check Connection
                Console.WriteLine("Initialization needed");
                _connection.InitializeSSH();
            }

            // Turn RF On
            SshCommand cmd1 = _connection.client.CreateCommand(string.Format("echo ++addr {0} >{1};echo RF1 >{1};", _addr, _connection.comport)); cmd1.Execute();
            var output = _connection.client.RunCommand("echo RFON");
            Console.WriteLine(output.Result);

        }

        public void RFOff()
        {
            if (!_connection.isConnected)
            {//Check Connection
                Console.WriteLine("Initialization needed");
                _connection.InitializeSSH();
            }

            // Turn RF Off
            SshCommand cmd1 = _connection.client.CreateCommand(string.Format("echo ++addr {0} >{1};echo RF0 >{1};", _addr, _connection.comport)); cmd1.Execute();
            var output = _connection.client.RunCommand("echo RFON");
            Console.WriteLine(output.Result);

        }

        public void set_freq(string f)
        // f is a string in GHz
        {
            if (!_connection.isConnected)
            {//Check Connection
                Console.WriteLine("Initialization needed");
                _connection.InitializeSSH();
            }

            // Set Freq
            SshCommand cmd1 = _connection.client.CreateCommand(string.Format("echo ++addr {0} >{1};echo CF1 {2} GH CF1 >{1};", _addr, _connection.comport, f)); cmd1.Execute();
            var output = _connection.client.RunCommand("echo FreqSet");
            Console.WriteLine(output.Result);
        }

        public void set_power(string p)
        // p is a string in dBm
        {
            if (!_connection.isConnected)
            {//Check Connection
                Console.WriteLine("Initialization needed");
                _connection.InitializeSSH();
            }
            // Set Power
            SshCommand cmd1 = _connection.client.CreateCommand(string.Format("echo ++addr {0} >{1};echo L0 {2} DM L0 >{1};", _addr, _connection.comport, p)); cmd1.Execute();
            var output = _connection.client.RunCommand("echo PowSet");
            Console.WriteLine(output.Result);
        }

        public string get_freq()
        // returns freq in MHz
        {
            if (!_connection.isConnected)
            {//Check Connection
                Console.WriteLine("Initialization needed");
                _connection.InitializeSSH();
            }

            // Get Freq
            SshCommand cmd1 = _connection.client.CreateCommand(string.Format("truncate -s 0 >  {0}; echo ++addr {1} >{2}; echo OF1 >{2};", _connection.out_file, _addr, _connection.comport)); cmd1.Execute();
            var output = _connection.client.RunCommand("echo FreqAcq");
            Console.WriteLine(output.Result);

            // Gather data, write to output file, and read.
            string frequency = _connection.Read();
            return frequency;
        }
        public string get_power()
        // returns power in dBm
        {
            if (!_connection.isConnected)
            {//Check Connection
                Console.WriteLine("Initialization needed");
                _connection.InitializeSSH();
            }

            // Get Power
            SshCommand cmd1 = _connection.client.CreateCommand(string.Format("truncate -s 0 > {0};echo ++addr {1} >{2}; echo OL0 >{2}; ", _connection.out_file, _addr, _connection.comport)); cmd1.Execute();
            var output = _connection.client.RunCommand("echo PowAcq");
            Console.WriteLine(output.Result);

            // Gather data, write to output file, and read.
            string power  = _connection.Read();
            return power;
        }
    }

    public class Keysight
    {
        public Keysight(Connection con, int addr)
        {
            _connection = con;
            _addr = addr;
        }
        private Connection _connection;
        private int _addr;
        // GPIB properties
        //private int addr = 28;

        public void SetAtten(string attenValue)
        {
            if (!_connection.isConnected)
            {//Check Connection
                Console.WriteLine("Initialization needed");
                _connection.InitializeSSH();
            }

            int atten = Int32.Parse(attenValue);
            int Y = atten / 10 * 10;
            int X = atten % 10;
            string setY = Y.ToString();
            string setX = X.ToString();
            // Set Atten
            SshCommand cmd1 = _connection.client.CreateCommand(string.Format("echo ++addr {0} >{2};echo ATT:X {1} >{2};", _addr, setX, _connection.comport)); cmd1.Execute();
            SshCommand cmd2 = _connection.client.CreateCommand(string.Format("echo ++addr {0} >{2};echo ATT:Y {1} >{2};", _addr, setY, _connection.comport)); cmd2.Execute();
            var output = _connection.client.RunCommand("echo AttenSet");
            Console.WriteLine(output.Result);
        }

        public string get_attenX()
        // returns power in dBm
        {
            if (!_connection.isConnected)
            {//Check Connection
                Console.WriteLine("Initialization needed");
                _connection.InitializeSSH();
            }

            // Get Atten
            SshCommand cmd1 = _connection.client.CreateCommand(string.Format("truncate -s 0 > {0};echo ++addr {1} >{2}; echo ATT:X? >{2}; ", _connection.out_file, _addr, _connection.comport)); cmd1.Execute();
            var output = _connection.client.RunCommand("echo PowAcq");
            Console.WriteLine(output.Result);

            // Gather data, write to output file, and read.
            string attenuator = _connection.Read();
            return attenuator;
        }

        public string get_attenY()
        // returns power in dBm
        {
            if (!_connection.isConnected)
            {//Check Connection
                Console.WriteLine("Initialization needed");
                _connection.InitializeSSH();
            }

            // Get Atten
            SshCommand cmd1 = _connection.client.CreateCommand(string.Format("truncate -s 0 > {0};echo ++addr {1} >{2}; echo ATT:Y? >{2}; ", _connection.out_file, _addr, _connection.comport)); cmd1.Execute();
            var output = _connection.client.RunCommand("echo PowAcq");
            Console.WriteLine(output.Result);

            // Gather data, write to output file, and read.
            string attenuator = _connection.Read();
            return attenuator;
        }
    }

    public class Test
    {

        public static void Main(string[] args)
        {
            Random rand = new Random();
            int freq = rand.Next(1, 68);
            int pow = rand.Next(-20, 1);
            Console.WriteLine(freq.ToString());
            Console.WriteLine(pow.ToString());

            Connection con = new Connection();
            con.InitializeSSH();

            Anritsu an = new Anritsu(con, 11);
            an.RFOn();
            an.RFOff();
            an.set_freq(freq.ToString());
            an.set_power(pow.ToString());
            an.get_freq();
            an.get_power();

            //Keysight key = new Keysight(con, 28);
            //key.SetAtten("22");
            //key.get_attenX();
            //key.get_attenY();

            Console.ReadKey();
        }
    }
}
