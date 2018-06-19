using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
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
        private const String PORT = "8080";
        string localIP;
        SerialPort serialPort = new SerialPort();
        string webPage;

        public void init()
        {
            try {
                using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0)) {
                    socket.Connect("8.8.8.8", 65530);
                    IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
                    localIP = endPoint.Address.ToString();
                }

                Console.WriteLine("http://" + localIP + ":" + PORT + "/");

                LoadWebPage();

                WebServer ws = new WebServer(SendResponse, "http://" + localIP + ":" + PORT + "/");
                ws.Run();

                if (SerialPort.GetPortNames().Length == 0) {
                    Console.WriteLine(getTimestamp() + "No Serial ports detected!");
                }
                else {
                    serialPort.PortName = SerialPort.GetPortNames()[0];
                    serialPort.BaudRate = 57600;
                    serialPort.DataReceived += new SerialDataReceivedEventHandler(port_DataReceived);
                    serialPort.Open();
                }

               
            }
            catch (Exception ex) {
                Console.WriteLine(ex.ToString());
            }

            Console.ReadLine();

        }

        private void port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort sp = (SerialPort)sender;
            Console.Write(sp.ReadExisting());
        }

        void LoadWebPage()
        {
            webPage = File.ReadAllText("index.html");
            webPage = webPage.Replace("%IP%", localIP);
            webPage = webPage.Replace("%PORT%", PORT);
        }

        private string SendResponse(HttpListenerRequest request)
        {
            // Only handle one request at a time.
            lock (this) {
                try {
                    String url = request.RawUrl;

                    if (url.Contains("/xy/")) {
                        String xy = url.Substring(url.IndexOf("/xy/") + "/xy/".Length);
                        String[] values = xy.Split('/');
                        Console.WriteLine(getTimestamp() + "Received request to move to {0},{1}", values[0], values[1]);
                        serialPort.WriteLine("l" + values[0]);
                        serialPort.WriteLine("f" + values[1]);
                    }
                    else if (url.Contains("/k")) {
                        serialPort.WriteLine("k"); //Forward the keep alive message
                    }
                    else {
                        Console.WriteLine(getTimestamp() + "Unknown request: " + url);
                    }
                }
                catch (Exception e) {
                    Console.WriteLine(getTimestamp() + e.ToString());
                }
                return webPage;
            }
        }

        private String getTimestamp()
        {
            return "[" + DateTime.Now.ToString() + "] ";
        }
    }
}
