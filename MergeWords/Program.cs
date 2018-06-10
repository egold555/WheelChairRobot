using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;

namespace MergeWords
{
    class Program
    {
        static string inputFileCsv = @"C:\Users\peter\Documents\Projects\WheelChairRobot\SpeechAndFace\SpeechAndFace\words\combined.csv";
        static string filterFile = @"C:\Users\peter\Documents\Projects\WheelChairRobot\AimlParser\bin\Debug\output.txt";
        static string outputFileCsv = @"C:\Users\peter\Documents\Projects\WheelChairRobot\SpeechAndFace\SpeechAndFace\words\filtered.csv";

        static void Main(string[] args)
        {
            CsvReader reader = new CsvReader(new StreamReader(inputFileCsv));
            IEnumerable<WordRecord> allwords = reader.GetRecords<WordRecord>().ToList();
            reader.Dispose();

            string[] filters = File.ReadAllLines(filterFile);
            HashSet<string> filterHash = new HashSet<string>(from s in filters
                                                             let lower = s.Trim().ToLower()
                                                             where IsSingleWord(lower)
                                                             select lower);
            List<WordRecord> filteredWords = (from w in allwords
                                       where w != null
                                       let lower = w.Word.Trim().ToLower()
                                       where filterHash.Contains(lower)
                                       select new WordRecord() { Word = lower, Tag = w.Tag, Response = w.Response }).ToList();
            Dictionary<string, WordRecord> filterDict = new Dictionary<string, WordRecord>();
            foreach (WordRecord w in filteredWords) {
                if (!filterDict.ContainsKey(w.Word))
                    filterDict[w.Word] = w;
            }

            CsvWriter writer = new CsvWriter(new StreamWriter(outputFileCsv));
            writer.WriteRecords(from w in filterDict.Values orderby w.Tag, w.Word select w);
            writer.Dispose();
        }

        static bool IsSingleWord(string s)
        {
            return !s.Contains(" ");
        }
    }


    class WordRecord
    {
        public string Word { get; set; }
        public string Tag { get; set; }
        public string Response { get; set; }
    }
}
