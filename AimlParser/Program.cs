using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace AimlParser
{
    class Program
    {
        static string inputDirectory = @"C:\Users\peter\Documents\Projects\WheelChairRobot\ChatBotConsoleTest\ChatBotConsoleTest\aiml";
        static HashSet<string> wordsFound = new HashSet<string>();

        static void Main(string[] args)
        {
            foreach (string file in Directory.EnumerateFiles(inputDirectory, "*.aiml")) {
                ProcessFile(file);
            }

            TextWriter output = new StreamWriter("output.txt");
            List<string> sorted = new List<string>(wordsFound);
            sorted.Sort();
            foreach (string s in sorted) {
                output.WriteLine(s);
            }
            output.Close();
        }

        private static void ProcessFile(string file)
        {
            XmlReader reader = XmlReader.Create(new StreamReader(file));

            while (reader.Read()) {
                if (reader.NodeType == XmlNodeType.Text) {
                    string contents = reader.ReadContentAsString();
                    ProcessContents(contents);
                }
            }

            reader.Close();
        }

        private static void ProcessContents(string contents)
        {
            string[] words = contents.Split(new char[] { ' ', '.', ',', ';', '(', ')', '?', '!' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string word in words) {
                string filtered = "";
                foreach (char c in word) {
                    if (char.IsLetter(c))
                        filtered += char.ToLower(c);
                }
                if (filtered.Length > 1) {
                    wordsFound.Add(filtered);
                }
            }
        }
    }
}
