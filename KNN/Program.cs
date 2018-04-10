using Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MyLibraries;
using DataAccess;

namespace KNN
{
    class Program
    {
        int num_decimal { get; set; } = 10;
        List<Feature_Idf> featureList { get; set; } = new List<Feature_Idf>();
        Dictionary<int, string> _predictionDictionary { get; set; } = new Dictionary<int, string>();
        List<Class_KNN> list_KNN { get; set; } = new List<Class_KNN>();
        static void Main(string[] args)
        {
            Program program = new Program();
            StreamReader streamReader = new StreamReader("config.txt");
            string line = streamReader.ReadLine();
            int k = 5; // mặc định lấy k=5
            int Euclid_OR_Cosine = 1; // mặc định tính theo cosine
            if(line != null)
            {
                k = Int32.Parse(line.Split(',')[0]);
                Euclid_OR_Cosine = Int32.Parse(line.Split(',')[1]);
            }
            var list_file_raw = Directory.GetFiles("dir_trainning\\", "*.*", SearchOption.AllDirectories).ToList();
          
            program.GetDictionary(list_file_raw);
            //program.Pretreatment(list_file_raw, "dir_pre\\input_dataCSV.csv");

            Console.WriteLine("Processing getFeature...");
            program.getFeature("dir_pre\\input_trainning_dataCSV.csv");

            Console.WriteLine("Processing BOW_Trainning...");
            program.BOW_Trainning("dir_pre\\input_trainning_dataCSV.csv");

            Console.WriteLine("Processing predict...");
            program.KNN("dir_pre\\input_test_dataCSV.csv", "dir_result\\result.txt", k, Euclid_OR_Cosine); // k=5, 1= tính theo cosine 2= tính theo Euclid

        }
        public void KNN(string path_dataCSV_test, string output, int num_k, int cosine_OR_euclid)
        {
            var dataTable = DataAccess.DataTable.New.ReadCsv(path_dataCSV_test);
            List<string> list_inputTest = dataTable.Rows.Select(row => row["text"]).ToList();
            List<int> correct_result = dataTable.Rows.Select(row => Int32.Parse(row["class"])).ToList();
            List<int> result = new List<int>();

            MyLibraries.Euclid_Similar cal_similar_euclid = new MyLibraries.Euclid_Similar();
            MyLibraries.Cosine_Similar cal_similar_cosine = new MyLibraries.Cosine_Similar();
            MyLibraries.Process_Stem_StopWord process_stopword_stem = new MyLibraries.Process_Stem_StopWord("stopwords.txt");
            FileStream fs = new FileStream(output, FileMode.Create);
            StreamWriter streamWriter = new StreamWriter(fs);

            foreach (string text in list_inputTest)
            {
                List<Class_KNN> list_class_KNN = new List<Class_KNN>();
                // xử lý stem, remove stopwords
                var a = process_stopword_stem.process_stopword_stem(text);
                // vector hóa dựa vào feature đã được trainning
                var b = this.Calculate_BOW(a.Split(' ').ToList());

                // tính similar giữa vb b với các vb được trainning
                if (cosine_OR_euclid == 1) // cosine_OR_euclid=1 là tính theo cosine
                {
                    foreach (var item in this.list_KNN)
                    {
                        item.value_similar = cal_similar_cosine.Calculate_Similar(item.bow, b);
                        //item.bow = null;
                        list_class_KNN.Add(item);
                    }

                    // sắp xếp độ tương tự tăng
                    list_class_KNN.Sort(
                        delegate (Class_KNN d1, Class_KNN d2)
                        {
                            if (d1.value_similar > d2.value_similar)
                                return -1;
                            return 1;
                        });
                }
                else // cosine_OR_euclid=2 là tính theo Euclid
                {
                    foreach (var item in this.list_KNN)
                    {
                        item.value_similar = cal_similar_euclid.Calculate_Similar(item.bow, b);
                        //item.bow = null;
                        list_class_KNN.Add(item);
                    }

                    // sắp xếp độ tương tự giảm dần
                    list_class_KNN.Sort(
                        delegate (Class_KNN d1, Class_KNN d2)
                        {
                            if (d1.value_similar < d2.value_similar)
                                return -1;
                            return 1;
                        });
                }

                // lấy ra k phần tử đầu (key của class)
                List<int> temp = new List<int>();
                for (int k = 0; k < num_k; k++)
                {
                    temp.Add(list_class_KNN.ElementAt(k).key_class);
                    // in ra k phần tử đầu
                    //Console.WriteLine(list_class_KNN.ElementAt(k).value_similar + " <=> class: " + list_class_KNN.ElementAt(k).key_class);
                }
                var rs_key_class = temp.GroupBy(x => x).OrderByDescending(x => x.Count()).First().Key;
                result.Add(rs_key_class);
                //streamWriter.WriteLine(rs_key_class + ", " + text);
            }

            int n_class = _predictionDictionary.Count;
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
                streamWriter.WriteLine("P" + i + "=" + "P" + this._predictionDictionary[i] + "=" + PiArr[i]);
                streamWriter.WriteLine("R" + i + "=" + "R" + this._predictionDictionary[i] + "=" + RiArr[i]);
            }
            streamWriter.WriteLine("Fmacro=" + Fmacro);
            streamWriter.WriteLine("Fmicro=" + Fmicro);

            //streamWriter.WriteLine(Pmacro + ", " + Rmacro + ", " + Fmacro + ", " + Fmicro);

            streamWriter.Close();
            fs.Close();
        }
        public double[] Calculate_BOW(List<string> dj)
        {
            // input phải được stem, remove stopword trước rồi
            // split d into dj
            //List<string> dj = input.Split(' ').ToList();
            // count max appearance of a word in dj
            int max_appearance_word = dj.GroupBy(x => x).Max(x => x.Count());
            int size_featureList = this.featureList.Count;
            double[] result = new double[size_featureList];

            for (int i = 0; i < size_featureList; i++)
            {
                int num_Wi_In_Dj = 0;
                // count Wi in Dj
                num_Wi_In_Dj = dj.Count(x => x == this.featureList.ElementAt(i).feature);

                double tf = (1.0 * num_Wi_In_Dj / max_appearance_word) * this.featureList.ElementAt(i).idf;
                result[i] = Math.Round(tf, num_decimal);
                //tf_Idf.Add(Math.Round(tf, num_decimal).ToString());
            }
            return result;
        }
        public void BOW_Trainning(string path_dataCSV_trainning)
        {
            var dataTable = DataAccess.DataTable.New.ReadCsv(path_dataCSV_trainning);
            List<string> list_inputText = dataTable.Rows.Select(row => row["text"]).ToList();
            List<int> list_inputClass = dataTable.Rows.Select(row => Int32.Parse(row["class"])).ToList();
            // duyệt từng văn bảng tính td_idf
            for(int i = 0; i< list_inputText.Count; i++)
            {
                // split d into dj
                List<string> dj = list_inputText.ElementAt(i).Split(' ').ToList();

                // count max appearance of a word in dj
                int max_appearance_word = dj.GroupBy(x => x).Max(x => x.Count());

                int size_featureList = this.featureList.Count;
                double[] tf_Idf = new double[size_featureList];
                for (int j = 0; j < size_featureList; j++)
                {
                    int num_Wi_In_Dj = 0;
                    // count Wi in Dj
                    num_Wi_In_Dj = dj.Count(x => x == this.featureList.ElementAt(j).feature);

                    double tf = (1.0 * num_Wi_In_Dj / max_appearance_word) * this.featureList.ElementAt(j).idf;
                    tf_Idf[j] = Math.Round(tf, num_decimal);
                }
                Class_KNN knn = new Class_KNN();
                knn.bow = tf_Idf;
                knn.key_class = list_inputClass.ElementAt(i);
                list_KNN.Add(knn);
            }
        }

        public void getFeature(string path_dataCSV_trainning)
        {
            var dataTable = DataAccess.DataTable.New.ReadCsv(path_dataCSV_trainning);
            List<string> list_inputText = dataTable.Rows.Select(row => row["text"]).ToList();

            List<string> list_feature = list_inputText.SelectMany(GetWords).Distinct().OrderBy(word => word).ToList();

            // calculate Idf
            int num_document = list_inputText.Count();
            foreach (var Wi in list_feature)
            {
                int num_Document_Contain_Wi = 0;
                // count document contains Wi
                foreach (var i in list_inputText)
                {
                    string[] j = i.Split(' ').ToArray();

                    foreach (string k in j)
                    {
                        if (String.Compare(Wi, k, true) == 0)
                        {
                            num_Document_Contain_Wi++;
                            break;
                        }
                    }
                }

                double idf = Math.Log10(1.0 * num_document / num_Document_Contain_Wi);
                Feature_Idf fea_idf = new Feature_Idf
                {
                    feature = Wi,
                    idf = Math.Round(idf, num_decimal)
                };
                this.featureList.Add(fea_idf);
            }
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

        private static IEnumerable<string> GetWords(string x)
        {
            return x.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}
