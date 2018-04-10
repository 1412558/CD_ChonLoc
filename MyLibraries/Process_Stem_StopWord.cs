using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MyLibraries
{
    public class Process_Stem_StopWord
    {
        string path_inputStopWord = "stopwords.txt";
        List<string> stopwords { get; set; } = new List<string>();
        public Process_Stem_StopWord(string path_inputStopWord)
        {
            this.path_inputStopWord = path_inputStopWord;
            this.getStopwords();
        }
        static void Main(string[] args)
        {
            //FileStream fs = new FileStream("result.txt", FileMode.Create);
            //StreamWriter streamWriter = new StreamWriter(fs);
        }
        public void getStopwords()
        {
            StreamReader streamReader;
            string line;
            // read stopword insert to stopword LIST
            streamReader = new StreamReader(this.path_inputStopWord);
            while ((line = streamReader.ReadLine()) != null)
            {
                this.stopwords.Add(line.ToLower());
            }
            streamReader.Close();
        }
        public string process_stopword_stem(string raw_text) // return string after stem and remove stopword
        {
            Iveonik.Stemmers.EnglishStemmer stemer = new Iveonik.Stemmers.EnglishStemmer();
            var strArr = Regex.Split(raw_text, @"\W+");
            int size_strArr = strArr.Length;
            List<string> result = new List<string>();
            for (int i = 0; i < size_strArr; i++)
            {
                bool flag = false; //<=> strArr[i] not in stopwords
                // remove stopword
                foreach (var sword in this.stopwords)
                {
                    if (String.Compare(strArr[i], sword, true) == 0)
                    {
                        flag = true;
                        break;
                    }
                }

                // if flag = false, stem(strArr[i]) and then push List result
                if (flag == false)
                {
                    string word = stemer.Stem(strArr[i]);
                    result.Add(word);
                }
            }
            return String.Join(" ", result).Trim().ToLower();
        }
    }
}
