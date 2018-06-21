using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RobotWebServerTest
{
    class Program
    {
        static void Main(string[] args)
        {
            new Single().init();
        }
    }

    //Single instance non static
    class Single
    {
        SerialPort serialPortSonar = new SerialPort();
        SerialPort serialPortRobot = new SerialPort();
        Timer timer;
        string dataReceived = "";

        const int STOP_DISTANCE = 150;  // distance in cm to stop.

        public void init()
        {
            try {


                if (SerialPort.GetPortNames().Length <= 1) {
                    Console.WriteLine("{0} Only {1} Serial ports detected!", getTimestamp(), SerialPort.GetPortNames().Length);
                }
                else {
                    serialPortSonar.PortName = SerialPort.GetPortNames()[0];
                    serialPortSonar.BaudRate = 57600;
                    serialPortSonar.DataReceived += new SerialDataReceivedEventHandler(port_DataReceived);
                    serialPortSonar.Open();

                    serialPortRobot.PortName = SerialPort.GetPortNames()[1];
                    serialPortRobot.BaudRate = 57600;
                    serialPortRobot.DataReceived += new SerialDataReceivedEventHandler(robot_DataReceived);
                    serialPortRobot.Open();

                    timer = new Timer(new TimerCallback(timer_Tick), null, 400, 400);
                }
            }
            catch (Exception ex) {
                Console.WriteLine(ex.ToString());
            }

            while (true) {
                string s = Console.ReadLine();
                if (s.StartsWith("q")) {
                    return;
                }
                else {
                    StartMoving();
                }
            }

        }

        private void timer_Tick(object state)
        {
            lock (serialPortRobot) {
                serialPortRobot.WriteLine("k");
            }
        }

        private void robot_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort sp = (SerialPort)sender;
            Console.WriteLine(sp.ReadExisting());
        }

        private void port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort sp = (SerialPort)sender;
            dataReceived += sp.ReadExisting();
            ProcessData();
        }

        private void ProcessData()
        {
            while (dataReceived.Contains("\r\n")) {
                string line = dataReceived.Substring(0, dataReceived.IndexOf("\r\n"));
                dataReceived = dataReceived.Substring(line.Length + 2);
                ProcessLine(line);
            }
        }

        private void ProcessLine(string line)
        {
            string[] valueText = line.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            int[] values = new int[valueText.Length];
            for (int i = 0; i < valueText.Length; ++i) {
                int v = 0;
                if (int.TryParse(valueText[i], out v)) {
                    values[i] = v;
                }
                else {
                    values[i] = 0;
                }
            }

            int total = 0;
            int count = 0;
            for (int i = 0; i < values.Length; ++i) {
                if (values[i] != 0) {
                    total += values[i];
                    count += 1;
                }
            }
            int average = 0;
            if (count > 0)
                average = total / count;

            ProcessDistance(average);
        }

        private void ProcessDistance(int average)
        {
            //Console.WriteLine("{0} Distance: {1} cm", getTimestamp(), average);

            if (average > 0 && average < STOP_DISTANCE) {
                StopMoving();
            }
        }

        private void StopMoving()
        {
            //Console.WriteLine("{0} Stopping.", getTimestamp());
            lock (serialPortRobot) {
                serialPortRobot.WriteLine("l100");
                serialPortRobot.WriteLine("f100");
            }
        }

        private void StartMoving()
        {
            Console.WriteLine("{0} Starting.", getTimestamp());
            lock (serialPortRobot) {
                serialPortRobot.WriteLine("l90");
                serialPortRobot.WriteLine("f75");
            }
        }

        private String getTimestamp()
        {
            return "[" + DateTime.Now.ToString("h:mm:ss.FFF") + "] ";
        }
    }
}
