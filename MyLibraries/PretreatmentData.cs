using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyLibraries
{
    public class PretreatmentData
    {
        public static void Pretreatment(List<string> list_file_raw, string out_path_dataCSV, string out_path_dictionary_CSV) // process stopword, stem
        {
            MyLibraries.Process_Stem_StopWord process_stem_stopWord = new MyLibraries.Process_Stem_StopWord("stopwords.txt");
            FileStream fs1 = new FileStream(out_path_dataCSV, FileMode.Create);
            StreamWriter streamWriter1 = new StreamWriter(fs1);

            FileStream fs2 = new FileStream(out_path_dictionary_CSV, FileMode.Create);
            StreamWriter streamWriter2 = new StreamWriter(fs2);

            streamWriter1.WriteLine("class,text");
            streamWriter2.WriteLine("key_class,name_class");
            for (int i = 0; i < list_file_raw.Count; i++)
            {
                string fileName = list_file_raw.ElementAt(i);
                FileInfo fi = new FileInfo(fileName);
                StreamReader streamReader = new StreamReader(fileName);
                string line = "";
                while ((line = streamReader.ReadLine()) != null)
                {
                    string rs = process_stem_stopWord.process_stopword_stem(line);
                    streamWriter1.WriteLine(i + "," + rs);
                }
                streamReader.Close();

                streamWriter2.WriteLine(i + "," + fi.Name.Split('.')[0]);

            }
            streamWriter1.Close();
            fs1.Close();
            streamWriter2.Close();
            fs2.Close();
        }
    }
}
