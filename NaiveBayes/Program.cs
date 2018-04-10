using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccess;
using System.IO;

namespace NaiveBayes
{
    class Program
    {
        Dictionary<int, string> _predictionDictionary { get; set; } = new Dictionary<int, string>();
        static void Main(string[] args)
        {
            Program program = new Program();
            FileStream fs = new FileStream("dir_result\\result.txt", FileMode.Create);
            StreamWriter streamWriter = new StreamWriter(fs);
            var list_file_raw = Directory.GetFiles("dir_trainning\\", "*.*", SearchOption.AllDirectories).ToList();

            Console.WriteLine("Processing GetDictionary...");
            program.GetDictionary(list_file_raw);

            Console.WriteLine("Processing Pretreatment...");
            //program.Pretreatment(list_file_raw, "dir_pre\\input_dataCSV.csv");

            var dataTable = DataAccess.DataTable.New.ReadCsv("dir_pre\\input_trainning_dataCSV.csv");
            List<string> x = dataTable.Rows.Select(row => row["text"]).ToList();

            List<string> key_0= new List<string>();
            List<string> key_1 = new List<string>();
            foreach(var item in dataTable.Rows)
            {
                int key = Int32.Parse(item["class"]);
                if (key == 0)
                    key_0.Add(item["text"]);
                if (key == 1)
                    key_1.Add(item["text"]);
            }


            var classifier = new BayesClassifier("0", "1");

            // Train the key_0 quotes.
            foreach (var quote in key_0)
                classifier.Train("0",quote);

            // Train the key_1 quotes.
            foreach (var quote in key_1)
                classifier.Train("1", quote);

            dataTable = DataAccess.DataTable.New.ReadCsv("dir_pre\\input_test_dataCSV.csv");
            List<string> listText = dataTable.Rows.Select(row => row["text"]).ToList();
            List<int> correct_result = dataTable.Rows.Select(row => Int32.Parse(row["class"])).ToList();

            List<int> result = new List<int>();
            foreach (string text in listText)
            {
                string rs = classifier.Classify(text);
                result.Add(Int32.Parse(rs));
                streamWriter.WriteLine( rs + ", " + text);
            }

            int n_class = program._predictionDictionary.Count;
            int total_count_correct = 0; // tổng số vb phân đúng
            double[] PiArr = new double[n_class];
            double[] RiArr = new double[n_class];

            for (int i = 0; i < n_class; i++)
            {
                int count_correct = 0; // slvb được phân đúng vào ci
                for (int j = 0; j < correct_result.Count; j++)
                {
                    if (correct_result.ElementAt(j) == i && result.ElementAt(j) == i)
                        count_correct++;
                }
                total_count_correct += count_correct;
                int count_machine = 0; // slvb được hệ thống phân vào lớp ci
                int count_doc = 0; // slvb thuộc lớp ci ban đầu
                count_machine = result.Count(item => item == i); // đếm sl pt = ci
                count_doc = correct_result.Count(item => item == i);// đếm sl pt = ci
                PiArr[i] = 1.0 * count_correct / count_machine;
                RiArr[i] = 1.0 * count_correct / count_doc;
            }
            double Pmacro = PiArr.Average();
            double Rmacro = RiArr.Average();
            double Fmacro = (2 * Pmacro * Rmacro) / (Pmacro + Rmacro);
            double Fmicro = 1.0 * total_count_correct / correct_result.Count;

            for (int i = 0; i < n_class; i++)
            {
                streamWriter.WriteLine("P" + i + "=" + "P" + program._predictionDictionary[i] + "=" + PiArr[i]);
                streamWriter.WriteLine("R" + i + "=" + "R" + program._predictionDictionary[i] + "=" + RiArr[i]);
            }
            streamWriter.WriteLine("Fmacro=" + Fmacro);
            streamWriter.WriteLine("Fmicro=" + Fmicro);
            //streamWriter.WriteLine(Pmacro + ", " + Rmacro + ", " + Fmacro + ", " + Fmicro);
            streamWriter.Close();
            fs.Close();
        }

        public void GetDictionary(List<string> list_file_raw)
        {
            for (int i = 0; i < list_file_raw.Count; i++)
            {
                string fileName = list_file_raw.ElementAt(i);
                FileInfo fi = new FileInfo(fileName);

                // i <=> ClassName
                this._predictionDictionary.Add(i, fi.Name.Split('.')[0]);
            }
        }
        public void Pretreatment(List<string> list_file_raw, string out_path_dataCSV) // process stopword, stem
        {
            MyLibraries.Process_Stem_StopWord process_stem_stopWord = new MyLibraries.Process_Stem_StopWord("stopwords.txt");
            FileStream fs = new FileStream(out_path_dataCSV, FileMode.Create);
            StreamWriter streamWriter = new StreamWriter(fs);
            streamWriter.WriteLine("class,text");
            for (int i = 0; i < list_file_raw.Count; i++)
            {
                string fileName = list_file_raw.ElementAt(i);
                StreamReader streamReader = new StreamReader(fileName);
                string line = "";
                while ((line = streamReader.ReadLine()) != null)
                {
                    string rs = process_stem_stopWord.process_stopword_stem(line);
                    streamWriter.WriteLine(i + "," + rs);
                }
                streamReader.Close();
            }
            streamWriter.Close();
            fs.Close();
        }
    }
}
