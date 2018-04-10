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
        TextClassificationProblemBuilder problemBuilder { get; set; }
        svm_problem problem { get; set; }
        Dictionary<int, string> _predictionDictionary { get; set; } = new Dictionary<int, string>();

        static void Main(string[] args)
        {
            Program program = new Program();
            var list_file_raw = Directory.GetFiles("dir_trainning\\", "*.*", SearchOption.AllDirectories).ToList();

            Console.WriteLine("Processing GetDictionary...");
            program.GetDictionary(list_file_raw);

            Console.WriteLine("Processing Pretreatment...");
            program.Pretreatment(list_file_raw, "dir_pre\\input_dataCSV.csv");


            Console.WriteLine("Processing Create_Train_SVMmodel...");
            program.Create_Train_SVMmodel("dir_pre\\input_trainning_dataCSV.csv", 1);

            Console.WriteLine("Processing Test");
            program.Test("dir_pre\\input_test_dataCSV.csv", "dir_result\\result.txt",1);

            //Console.WriteLine("Process completed");
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
        public void Test(string path_dataCSV_test, string pathResult, double C)
        {    
            FileStream fs = new FileStream(pathResult, FileMode.Create);
            StreamWriter streamWriter = new StreamWriter(fs);
            var dataTable = DataAccess.DataTable.New.ReadCsv(path_dataCSV_test);
            List<string> listText = dataTable.Rows.Select(row => row["text"]).ToList();
            List<int> correct_result = dataTable.Rows.Select(row => Int32.Parse(row["class"])).ToList();
            List<int> result = new List<int>();
            foreach (string text in listText)
            {
                int rs = this.Predict_ReturnInt(text);
                result.Add(rs);
                //streamWriter.WriteLine( rs + ", " + text);
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

            //for (int i = 0; i < n_class; i++)
            //{
            //    streamWriter.WriteLine("P" + i + "=" + "P" + this._predictionDictionary[i] + "=" + PiArr[i]);
            //    streamWriter.WriteLine("R" + i + "=" + "R" + this._predictionDictionary[i] + "=" + RiArr[i]);
            //}
            //streamWriter.WriteLine("Fmacro=" + Fmacro);
            //streamWriter.WriteLine("Fmicro=" + Fmicro);

            streamWriter.WriteLine(Pmacro +", " + Rmacro + ", " + Fmacro +", " + Fmicro);

            streamWriter.Close();
            fs.Close();
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
                    streamWriter.WriteLine(i + ","+ rs);
                }
                streamReader.Close();
            }
            streamWriter.Close();
            fs.Close();
        }
        public void Create_Train_SVMmodel(string path_dataCSV_trainning, double C)
        {
            var dataTable = DataAccess.DataTable.New.ReadCsv(path_dataCSV_trainning);
            List<string> x = dataTable.Rows.Select(row => row["text"]).ToList();
            double[] y = dataTable.Rows.Select(row => double.Parse(row["class"])).ToArray();
            
            vocabulary = x.SelectMany(GetWords).Distinct().OrderBy(word => word).ToList();
            problemBuilder = new TextClassificationProblemBuilder();
            problem = problemBuilder.CreateProblem(x, y.ToArray(), vocabulary.ToList());
            model = new C_SVC(problem, KernelHelper.LinearKernel(),C);
        }
        public int Predict_ReturnInt(string input)
        {
            var newX = TextClassificationProblemBuilder.CreateNode(input, vocabulary);
            var predictedY = model.Predict(newX);
            return (int)predictedY;
        }
        public string Predict_ReturnString(string input)
        {
            var newX = TextClassificationProblemBuilder.CreateNode(input, vocabulary);
            var predictedY = model.Predict(newX);
            return this._predictionDictionary[(int)predictedY];
        }
        private static IEnumerable<string> GetWords(string x)
        {
            return x.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}
