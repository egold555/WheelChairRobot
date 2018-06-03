using System;
using System.Speech.Recognition;
using System.Windows.Forms;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;

namespace VoiceSynthRecTestAutomation
{
    class Program
    {

        static SpeechRecognitionEngine recEngine = new SpeechRecognitionEngine();

        static void Main(string[] args)
        {
            Console.WriteLine("Recognizing. \nPress ENTER to stop");

            Choices cmds = new Choices();
            cmds.Add(new String[] { "say hello", "print my name" });
            GrammarBuilder gBuilder = new GrammarBuilder();
            gBuilder.Append(cmds);
            Grammar grammar = new Grammar(gBuilder);

            recEngine.LoadGrammarAsync(grammar);
            recEngine.SetInputToDefaultAudioDevice();

            recEngine.RecognizeAsync(RecognizeMode.Multiple);

            //recognizer.Enabled = true;
            recEngine.SpeechRecognized += new EventHandler<SpeechRecognizedEventArgs>(recognizer_SpeechRecognized);

            //String name = "piper";//Riley

            Console.ReadLine();
            Console.WriteLine("Stopping recognizer ...");
            recEngine.Dispose();

        }

        private static void recognizer_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            Console.WriteLine(e.Result.Text + " (" + e.Result.Confidence + ")");
        }

    }
}
