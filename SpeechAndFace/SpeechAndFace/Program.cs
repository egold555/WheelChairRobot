using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Speech.Synthesis;
using System.Net.Sockets;
using System.Net;
using System.IO;

namespace SpeechAndFace
{
    class Program
    {
        static SpeechSynthesizer synth = new SpeechSynthesizer();

        const int PORT_SEND = 11000;
        const int PORT_RECIEVE = 11001;
        const string SERVER_IP = "127.0.0.1";

        static Dictionary<string, string> emotionalWords = new Dictionary<string, string>();

        static void Main(string[] args)
        {
            //Set up speech synth
            //synth.SelectVoice("Vocalizer Karen - English (Australia) For KobaSpeech 3");
            //synth.SpeakProgress += new EventHandler<SpeakProgressEventArgs>(synth_SpeakProgress);

            //Read CSV file of words->emotions
            readWordFile();

            foreach(String word in emotionalWords.Keys)
            {
                Console.WriteLine(word + " = " + emotionalWords[word]);
            }

            //speak("Hello World");
            Console.ReadLine();
        }

        private static void readWordFile()
        {
            using (StreamReader sr = new StreamReader("words.csv"))
            {
                String line;

                while ((line = sr.ReadLine()) != null)
                {
                    string[] parts = line.Split(',');
                    string word = parts[0].ToLower();
                    string emotion = parts[1].ToLower();

                    if (!word.Contains("#"))
                    {
                        if (!string.IsNullOrEmpty(word))
                        {
                            if (String.IsNullOrEmpty(emotion))
                            {
                                emotion = "null";
                            }
                            try
                            {
                                emotionalWords.Add(word, emotion);
                            }
                            catch (System.ArgumentException e)
                            {
                                Console.WriteLine("ERROR: " + e.Message + "(" + word + ")");
                            }
                        }
                    }
                }
            }
        }

        private static void synth_SpeakProgress(object sender, SpeakProgressEventArgs e)
        {
            
        }

        public static void speak(string text)
        {
            send("talking", "true");
            synth.Speak(text);
            send("talking", "false");
        }


        private static long GetCurrentMilli()
        {
            DateTime Jan1970 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            TimeSpan javaSpan = DateTime.UtcNow - Jan1970;
            return (long)javaSpan.TotalMilliseconds;
        }

        private static void send(String param, String value)
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
