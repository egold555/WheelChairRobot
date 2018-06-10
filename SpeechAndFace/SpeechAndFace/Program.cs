using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Speech.Synthesis;
using System.Net.Sockets;
using System.Net;
using System.IO;
using AIMLbot;
using CsvHelper;

namespace SpeechAndFace
{
    class Program
    {
        static SpeechSynthesizer synth = new SpeechSynthesizer();

        const int PORT_SEND = 11000;
        const int PORT_RECIEVE = 11001;
        const string SERVER_IP = "127.0.0.1";

        private static Bot AimlBot;
        private static User myUser;

        static Dictionary<string, WordRecord> emotionalWords = new Dictionary<string, WordRecord>();

        static void Main(string[] args)
        {
            Console.WriteLine("Loading...");
            //Set up speech synth
            //http://www.kobaspeech.com/en/download-voices
            try
            {
                synth.SelectVoice("Vocalizer Karen - English (Australia) For KobaSpeech 3");
            }
            catch (Exception e)
            {
                //Incase they dont have the voice installed, we will just use the default microsoft david voice witch every computer should have
                synth.SelectVoice("Microsoft David Desktop");
            }

            synth.SetOutputToDefaultAudioDevice();

            synth.SpeakProgress += new EventHandler<SpeakProgressEventArgs>(synth_SpeakProgress);
            synth.StateChanged += new EventHandler<StateChangedEventArgs>(synth_StateChanged);

            AimlBot = new Bot();
            myUser = new User("user", AimlBot);

            AimlBot.loadSettings();
            AimlBot.isAcceptingUserInput = false;

            //https://docs.google.com/document/d/1wNT25hJRyupcG51aO89UcQEiG-HkXRXusukADpFnDs4/pub
            AimlBot.loadAIMLFromFiles();
            AimlBot.isAcceptingUserInput = true;

            //Read CSV file of words->emotions
            CsvReader reader = new CsvReader(new StreamReader("words.csv"));
            emotionalWords = reader.GetRecords<WordRecord>().ToDictionary(wordRecord => wordRecord.Word);

            //speak("Hello World this is a test of using text to speech sync'd to face movements, Currently I express no emotion");
            //Console.ReadKey();
            Console.WriteLine("Ready");
            while (true) {
                var input = Console.ReadLine();
                if (input.Equals("exit", StringComparison.OrdinalIgnoreCase)) {
                    break;
                }
                string output = getBotResponce(input).Trim();
                Console.WriteLine("Bot: " + output);
                speak(output);
            }

            disposeEverything();
        }

        private static void disposeEverything()
        {
            synth.Dispose();
        }

        //Handle pausing the mouth when we reach a pause in the sentence
        private static bool progressStartSpeaking = false;
        private static void synth_SpeakProgress(object sender, SpeakProgressEventArgs e)
        {
            string word = e.Text;
            Console.WriteLine("Speaking: " + word);

            string wordToFind = word.ToLower().Trim();
            wordToFind = wordToFind.Replace(".", "");
            wordToFind = wordToFind.Replace(",", "");
            wordToFind = wordToFind.Replace("?", "");
            wordToFind = wordToFind.Replace("!", "");
            wordToFind = wordToFind.Replace(":", "");
            wordToFind = wordToFind.Replace(";", "");
            wordToFind = wordToFind.Replace("'", "");

            if (emotionalWords.ContainsKey(wordToFind)) {
                WordRecord record = emotionalWords[wordToFind];
                Console.WriteLine("Found record: " + record.Word + "-" + record.Response);
                send("expression", record.Response);
            }

            if (progressStartSpeaking) {
                progressStartSpeaking = false;
                send("talking", "true");
            }

            if (word.EndsWith(".") || word.EndsWith(",") || word.EndsWith("?") || word.EndsWith("!")) {
                progressStartSpeaking = true;
                send("talking", "false");
            }
        }

        private static void synth_StateChanged(object sender, StateChangedEventArgs e)
        {
            Console.WriteLine("\nSynthesizer State: {0}    Previous State: {1}\n", e.State, e.PreviousState);
            if(e.State == SynthesizerState.Speaking) {
                send("talking", "true");
            }
            else {
                send("talking", "false");
            }
        }

        public static void speak(string text)
        {
            synth.Speak(text);
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

        private static String getBotResponce(String input)
        {
            Request r = new Request(input, myUser, AimlBot);
            Result res = AimlBot.Chat(r);
            return (res.Output);
        }
    }

    class WordRecord
    {
        public string Word { get; set; }
        public string Tag { get; set; }
        public string Response { get; set; }
    }
}
