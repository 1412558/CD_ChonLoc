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
            int k = 10;
            int type = 2;
            List<string> listTest = new List<string>();
            List<int> correct_result = new List<int>();

            program.GetDictionary("dir_pre\\dictionary_dataCSV.csv");
            StreamReader streamReader = new StreamReader("config.txt");
            string line = streamReader.ReadLine();
            streamReader.Close();
            if(Int32.Parse(line.Split(' ')[0]) == 1)
            {
                type = 1;
                k = Int32.Parse(line.Split(' ')[1]);
            }

            // classifier được khởi tạo với tham số là mảng string (tên lớp) => key_class
            string[] arrClass = new string[program._predictionDictionary.Count];
            foreach(var key in program._predictionDictionary.Keys)
            {
                arrClass[key] = key.ToString();
            }
            var classifier = new BayesClassifier(arrClass);


            if(type ==1) // cross-validation
            {
                var inputRows = DataAccess.DataTable.New.ReadCsv("dir_pre\\dataCSV.csv").Rows.ToList();
                int size = inputRows.Count;
                int n = size / k;
                Random ran = new Random();
                // ram dom n phần tử (mỗi pt chỉ vị trí thứ i trong row để test, phần còn lại để trainning)
                List<Row> test = new List<Row>();
                for(int i = 0; i<n; i++)
                {
                    int j = ran.Next(0, inputRows.Count);
                    // add row được random vào test, remove row đó khỏi trainning
                    test.Add(inputRows.ElementAt(j));
                    inputRows.RemoveAt(j);
                }

                // trainning
                foreach (var item in inputRows)
                {
                    classifier.Train(item["class"], item["text"]);
                }

                // load Test-data
                listTest = test.Select(row => row["text"]).ToList();
                // load Test-result
                correct_result = test.Select(row => Int32.Parse(row["class"])).ToList();
            }
            else if( type == 2)
            {
                // trainning data
                var dataTable = DataAccess.DataTable.New.ReadCsv("dir_pre\\trainning_dataCSV.csv");
                foreach (var item in dataTable.Rows)
                {
                    classifier.Train(item["class"], item["text"]);
                }
                dataTable = DataAccess.DataTable.New.ReadCsv("dir_pre\\test_dataCSV.csv");
                // load Test-data
                listTest = dataTable.Rows.Select(row => row["text"]).ToList();
                // load Test-result
                correct_result = dataTable.Rows.Select(row => Int32.Parse(row["class"])).ToList();
            }
            
            List<int> result = new List<int>();
            foreach (string text in listTest)
            {
                string rs = classifier.Classify(text);
                result.Add(Int32.Parse(rs));
                streamWriter.WriteLine( rs + "," + text);
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
        public void GetDictionary(string path_dictionary_CSV)
        {
            var dataTable = DataAccess.DataTable.New.ReadCsv(path_dictionary_CSV);
            foreach (var item in dataTable.Rows)
            {
                this._predictionDictionary.Add(Int32.Parse(item["key_class"]), item["name_class"]);
            }
        }
    }
}
