using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FaceExpressionServerWinForms
{
    public partial class Form1 : Form
    {
        const int PORT_SEND = 11000;
        const int PORT_RECIEVE = 11001;
        const string SERVER_IP = "127.0.0.1";

        public Form1()
        {
            InitializeComponent();
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            send("expression", "happy%100");
        }

        private void button2_Click(object sender, EventArgs e)
        {

        }

        //Why c# doesnt have this method is unknown
        public static long GetCurrentMilli()
        {
            DateTime Jan1970 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            TimeSpan javaSpan = DateTime.UtcNow - Jan1970;
            return (long)javaSpan.TotalMilliseconds;

        }

        private void send(String param, String value)
        {
            String message = "t:" + GetCurrentMilli() + ";";
            message += "s:127.0.0.1;";
            message += "p:" + PORT_RECIEVE + ";";
            message += "d:" + param + "=" + value;

            Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            IPAddress serverAddr = IPAddress.Parse(SERVER_IP);
            IPEndPoint endPoint = new IPEndPoint(serverAddr, PORT_SEND);
            byte[] send_buffer = Encoding.ASCII.GetBytes(message);
            sock.SendTo(send_buffer, endPoint);
        }
    }
}
