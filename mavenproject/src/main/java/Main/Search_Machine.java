/*
 * To change this license header, choose License Headers in Project Properties.
 * To change this template file, choose Tools | Templates
 * and open the template in the editor.
 */
package Main;

import Model.DocumentVector;
import Model.Feature_Idf;
import java.io.BufferedReader;
import java.io.BufferedWriter;
import java.io.FileNotFoundException;
import java.io.FileReader;
import java.io.FileWriter;
import java.io.IOException;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.Comparator;
import java.util.List;
import java.util.Vector;
import org.tartarus.martin.Stemmer;

/**
 *
 * @author Kuga
 */
public class Search_Machine {

    String path_stopword = "src\\main\\java\\File\\stopwords.txt";
    String path_resultBOW = "src\\main\\java\\File\\resultBOW.txt";
    String path_feature = "src\\main\\java\\File\\feature.txt";
    String path_raw_text = "src\\main\\java\\File\\raw_text.txt";
    List<String> stopwords = new ArrayList<>();

    public static void main(String[] args) throws IOException {
        Search_Machine sm = new Search_Machine();
        sm.Search("src\\main\\java\\File\\keyword_search.txt","src\\main\\java\\File\\output_search.txt");
    }

    public void Search(String input, String output) throws FileNotFoundException, IOException {
        StreamReader streamReader;
        String keyword;
        String line;
        int num_result = 0;
        // read keyword // read keyword
        streamReader = new StreamReader(input);
        keyword = streamReader.readLine();
        if (keyword == null) {
            return;
        }
        num_result = Integer.valueOf(streamReader.readLine());
        streamReader.close();

        // read stopword insert to stopword LIST
        streamReader = new StreamReader(path_stopword);
        while ((line = streamReader.readLine()) != null) {
            stopwords.add(line);
        }
        streamReader.close();

        // get [] word from keyword
        // strArr includes total word of keyword
        String[] strArr = keyword.split("\\W+");

        // remove stopword
        List<String> listStr_after_stopword = MyLibraries.removeStopwords(stopwords, strArr);

        // porter stemming
        List<String> listKeyword_after_stem = new ArrayList<>();
        Stemmer porterStemmer = new Stemmer();
        for (String str : listStr_after_stopword) {
            porterStemmer.add(str.toCharArray(), str.length());
            porterStemmer.stem();
            listKeyword_after_stem.add(porterStemmer.toString());
        }

        List<Feature_Idf> listfeature_Idf = new ArrayList<>();
        streamReader = new StreamReader(path_feature);
        while ((line = streamReader.readLine()) != null) {
            String[] str = line.split(" ");
            Feature_Idf temp = new Feature_Idf(str[0], Double.valueOf(str[1]));
            listfeature_Idf.add(temp);
        }

        // calculate TF_IDF of key word
        int max_appearance_word = MyLibraries.getMaxFrequency(listKeyword_after_stem);
        List<Double> listTf_idf_keyword = new ArrayList<>();
        for (int i = 0; i < listfeature_Idf.size(); i++) {
            int count = 0;
            for (String j : listKeyword_after_stem) {
                if (listfeature_Idf.get(i).feature.compareToIgnoreCase(j) == 0) {
                    count++;
                }
            }
            listTf_idf_keyword.add((1.0 * count / max_appearance_word) * listfeature_Idf.get(i).idf);
        }

        streamReader = new StreamReader(path_resultBOW);
        List<DocumentVector> Doc_Vector = new ArrayList<>();
        int index = 0;
        while ((line = streamReader.readLine()) != null) {
            String[] str_tf_idf = line.split(" ");
            List<Double> double_tf_idf_list = new ArrayList<>();
            for (String i : str_tf_idf) {
                double_tf_idf_list.add(Double.valueOf(i));
            }
            DocumentVector doc_vec = new DocumentVector();
            doc_vec.setIndex_document(index);
            doc_vec.setValue_Euclid(MyLibraries.Euclideanorm(double_tf_idf_list, listTf_idf_keyword));
            index++;
            Doc_Vector.add(doc_vec);
        }

        Doc_Vector.sort(Comparator.comparingDouble(DocumentVector::getValue_Euclid));
//        for (DocumentVector a : Doc_Vector) {
//            System.out.println(a.getValue_Euclid() + "-" +a.getIndex_document());
//        }
//        for(Double d:listTf_idf_keyword){
//            System.out.print(d+" ");
//        }
        
        streamReader = new StreamReader(path_raw_text);
        List<String> listRaw_text = new ArrayList<>();
        while ((line = streamReader.readLine()) != null) {
            listRaw_text.add(line);
        }

        if (num_result > Doc_Vector.size()) {
            num_result = Doc_Vector.size();
        }
        
        StreamWriter sw = new StreamWriter(output,false);
        for (int i = 0; i < num_result; i++) {
            sw.writeLine(Doc_Vector.get(i).getValue_Euclid() + "-" + listRaw_text.get(Doc_Vector.get(i).getIndex_document()));
            //System.out.println(listRaw_text.get(Doc_Vector.get(i).getIndex_document()));
        }
        sw.flush();
        sw.close();

    }
}
