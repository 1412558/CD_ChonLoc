using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using libsvm;
using System.Data;
using DataAccess;

namespace SVM
{
    class Program
    {
        List<string> vocabulary { get; set; }
        C_SVC model { get; set; }
        Dictionary<int, string> _predictionDictionary { get; set; } = new Dictionary<int, string>();

        static void Main(string[] args)
        {
            Program program = new Program();

            Console.WriteLine("Processing GetDictionary...");
            program.GetDictionary("dir_pre\\dictionary_dataCSV.csv");

            Console.WriteLine("Processing Create_Train_SVMmodel...");
            program.Create_Train_SVMmodel("dir_pre\\trainning_dataCSV.csv", 1);

            Console.WriteLine("Processing Test");
            program.Test("dir_pre\\test_dataCSV.csv", "dir_result\\result.txt");
        }

        public void Test(string path_dataCSV_test, string pathResult)
        {    
            FileStream fs = new FileStream(pathResult, FileMode.Create);
            StreamWriter streamWriter = new StreamWriter(fs);
            var dataTable = DataAccess.DataTable.New.ReadCsv(path_dataCSV_test);
            List<string> listText = dataTable.Rows.Select(row => row["text"]).ToList();
            List<int> correct_result = dataTable.Rows.Select(row => Int32.Parse(row["class"])).ToList();
            List<int> result = new List<int>();
            foreach (string text in listText)
            {
                int rs = this.Predict(text);
                result.Add(rs);
                streamWriter.WriteLine( rs + "," + text);
            }


            int n_class = _predictionDictionary.Count;
            int total_count_correct = 0; // tổng số vb phân đúng
            double[] PiArr = new double[n_class];
            double[] RiArr = new double[n_class];

            for (int i = 0; i <n_class; i++)
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
                PiArr[i] = 1.0*count_correct / count_machine;
                RiArr[i] = 1.0*count_correct / count_doc;
            }
            double Pmacro = PiArr.Average();
            double Rmacro = RiArr.Average();
            double Fmacro = (2 * Pmacro * Rmacro) / (Pmacro + Rmacro);
            double Fmicro = 1.0*total_count_correct / correct_result.Count;

            for (int i = 0; i < n_class; i++)
            {
                streamWriter.WriteLine("P" + i + "=" + "P" + this._predictionDictionary[i] + "=" + PiArr[i]);
                streamWriter.WriteLine("R" + i + "=" + "R" + this._predictionDictionary[i] + "=" + RiArr[i]);
            }
            streamWriter.WriteLine("Fmacro=" + Fmacro);
            streamWriter.WriteLine("Fmicro=" + Fmicro);

            streamWriter.Close();
            fs.Close();
        }

        public void GetDictionary(string path_dictionary_CSV)
        {
            var dataTable = DataAccess.DataTable.New.ReadCsv(path_dictionary_CSV);
            foreach(var item in dataTable.Rows)
            {
                this._predictionDictionary.Add(Int32.Parse(item["key_class"]), item["name_class"]);
            }
        }
        public void Create_Train_SVMmodel(string path_dataCSV_trainning, double C)
        {
            var dataTable = DataAccess.DataTable.New.ReadCsv(path_dataCSV_trainning);
            List<string> x = dataTable.Rows.Select(row => row["text"]).ToList();
            double[] y = dataTable.Rows.Select(row => double.Parse(row["class"])).ToArray();
            
            vocabulary = x.SelectMany(GetWords).Distinct().OrderBy(word => word).ToList();
            var problemBuilder = new TextClassificationProblemBuilder();
            var problem = problemBuilder.CreateProblem(x, y.ToArray(), vocabulary.ToList());
            model = new C_SVC(problem, KernelHelper.LinearKernel(),C);
        }
        public int Predict(string input)
        {
            var newX = TextClassificationProblemBuilder.CreateNode(input, vocabulary);
            var predictedY = model.Predict(newX);
            return (int)predictedY;
        }

        private static IEnumerable<string> GetWords(string x)
        {
            return x.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}
