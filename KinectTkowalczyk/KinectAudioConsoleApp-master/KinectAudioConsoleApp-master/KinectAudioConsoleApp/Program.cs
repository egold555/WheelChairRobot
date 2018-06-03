using Microsoft.Kinect;
using Microsoft.Speech.AudioFormat;
using Microsoft.Speech.Recognition;
using System;
using System.IO;
using System.Linq;

namespace KinectAudioConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            KinectSensor _sensor;
            SpeechRecognitionEngine _sre;

            _sensor = (from sensorToCheck in KinectSensor.KinectSensors
                       where sensorToCheck.Status == KinectStatus.Connected
                       select sensorToCheck).FirstOrDefault();

            _sensor.Start();

            var audioSource = _sensor.AudioSource;

            using (var source = audioSource.Start())
            {
                audioSource.EchoCancellationMode = EchoCancellationMode.None;
                audioSource.AutomaticGainControlEnabled = false;

                _sre = CreateSpeechRecognizer();

                using (Stream s = source)
                {
                    _sre.SetInputToAudioStream(s,
                      new SpeechAudioFormatInfo(EncodingFormat.Pcm, 16000, 16, 1, 32000, 2, null)
                                              );

                    Console.WriteLine("Recognizing. \nPress ENTER to stop");
                    _sre.RecognizeAsync(RecognizeMode.Multiple);
                    Console.ReadLine();
                    Console.WriteLine("Stopping recognizer ...");
                    _sre.RecognizeAsyncStop();
                }
            }
        }

        private static SpeechRecognitionEngine CreateSpeechRecognizer()
        {
            RecognizerInfo ri = GetKinectRecognizer();
            if (ri == null)
            {
                Console.WriteLine("There was a problem initializing Speech Recognition." +
                                    "Ensure you have the Microsoft Speech SDK installed." +
                                    "Failed to load Speech SDK");
                return null;
            }

            SpeechRecognitionEngine sre;
            try
            {
                sre = new SpeechRecognitionEngine(ri.Id);
            }
            catch
            {
                Console.WriteLine("There was a problem initializing Speech Recognition." +
                                    "Ensure you have the Microsoft Speech SDK installed and configured." +
                                     "Failed to load Speech SDK");
                return null;
            }

            var grammar = new Choices();
            //grammar.Add("red");
            //grammar.Add("green");
            //grammar.Add("blue");
            //grammar.Add("eric");
            //grammar.Add("stop");
            //grammar.Add("hello world");
            grammar.Add("The");
            grammar.Add("quick");
            grammar.Add("brown");
            grammar.Add("fox");
            grammar.Add("jumps");
            grammar.Add("over");
            grammar.Add("the");
            grammar.Add("lazy");
            grammar.Add("dog");
            grammar.Add("super");
            

                        var gb = new GrammarBuilder { Culture = ri.Culture };
            gb.Append(grammar);

            var g = new Grammar(gb);

            sre.LoadGrammar(g);
            sre.SpeechRecognized += new EventHandler<SpeechRecognizedEventArgs>(sre_SpeechRecognized);
            sre.SpeechHypothesized += new EventHandler<SpeechHypothesizedEventArgs>(sre_SpeechHypothesized);
            sre.SpeechRecognitionRejected += new EventHandler<SpeechRecognitionRejectedEventArgs>(sre_SpeechRecognitionRejected);

            return sre;
        }

        static void sre_SpeechRecognitionRejected(object sender, SpeechRecognitionRejectedEventArgs e)
        {
            Console.WriteLine("\nSpeech Rejected");
        }

        static void sre_SpeechHypothesized(object sender, SpeechHypothesizedEventArgs e)
        {
            Console.Write("\rSpeech Hypothesized: \t{0}\tConf:\t{1}", e.Result.Text, e.Result.Confidence);
        }

        static void sre_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            if (e.Result.Confidence > 0.5)
            {
                Console.WriteLine("\nSpeech Recognized: \t{0}", e.Result.Text);
            }
        }

        private static RecognizerInfo GetKinectRecognizer()
        {
            Func<RecognizerInfo, bool> matchingFunc = r =>
            {
                string value;
                r.AdditionalInfo.TryGetValue("Kinect", out value);
                return "True".Equals(value, StringComparison.InvariantCultureIgnoreCase) && "en-US".Equals(r.Culture.Name, StringComparison.InvariantCultureIgnoreCase);
            };
            return SpeechRecognitionEngine.InstalledRecognizers().Where(matchingFunc).FirstOrDefault();
        }
    }
}