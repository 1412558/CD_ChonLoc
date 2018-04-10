/*
 * To change this license header, choose License Headers in Project Properties.
 * To change this template file, choose Tools | Templates
 * and open the template in the editor.
 */
package Main;

import java.io.BufferedReader;
import java.io.BufferedWriter;
import java.io.FileNotFoundException;
import java.io.FileReader;
import java.io.FileWriter;
import java.io.IOException;
import java.lang.reflect.Array;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.HashSet;
import java.util.List;
import java.util.stream.Collectors;
import org.apache.commons.math3.util.Precision;
import org.tartarus.martin.Stemmer;

/**
 *
 * @author Kuga
 */
public class BagOfWord {

    List<String> inputBOW = new ArrayList<>();
    List<Double> listIdf_Feature = new ArrayList<>();

    public static void main(String[] args) throws IOException {
        BagOfWord bow = new BagOfWord();
        String path = "src\\main\\java\\File\\";
        bow.TienXuLy(path + "raw_text.txt", path + "tienxuly.txt", path + "stopwords.txt");
        bow.BOW_TfIdf(path + "tienxuly.txt", path + "resultBOW.txt", path + "feature.txt", path + "round.txt");
    }

    public void TienXuLy(String input, String output, String inputStopWord) throws FileNotFoundException, IOException {
        String line;
        List<String> inputText = new ArrayList<>();
        List<String> result = new ArrayList<>();
        List<String> stopwords = new ArrayList<>();

        FileReader FRstream;
        BufferedReader bufferReader;
        FileWriter FWstream;
        BufferedWriter bufferWriter;

        // read document insert to inputText LIST
        FRstream = new FileReader(input);
        bufferReader = new BufferedReader(FRstream);
        while ((line = bufferReader.readLine()) != null) {
            inputText.add(line);
        }
        bufferReader.close();
        FRstream.close();

        // read stopword insert to stopword LIST
        FRstream = new FileReader(inputStopWord);
        bufferReader = new BufferedReader(new FileReader(inputStopWord));
        while ((line = bufferReader.readLine()) != null) {
            stopwords.add(line);
        }
        bufferReader.close();
        FRstream.close();

        for (String rawText : inputText) {
            // get [] word from inputText
            // 1 item <=> 1 document
            // strArr includes total words of item
            List<String> listStr_after_stopword = new ArrayList<>();
            List<String> listStr_after_stem = new ArrayList<>();
            String[] strArr = rawText.split("\\W+");

            // remove stopword
            for (String str : strArr) {
                boolean flag = true;
                for (String word : stopwords) {
                    if (str.compareToIgnoreCase(word) == 0) {
                        flag = false;
                        break;
                    }
//                    listStr_after_stopword = Arrays.stream(strArr)
//                            .filter(x -> x.compareToIgnoreCase(sword) != 0)
//                            .collect(Collectors.toList());
                }
                if (flag) {
                    listStr_after_stopword.add(str.toLowerCase());
                }
            }

            // porter stemming
            Stemmer porterStemmer = new Stemmer();
            for (String str : listStr_after_stopword) {
                porterStemmer.add(str.toCharArray(), str.length());
                porterStemmer.stem();
                listStr_after_stem.add(porterStemmer.toString());
            }

            // merge words 
            String outstr = String.join(" ", listStr_after_stem);
            result.add(outstr);
        }

        // write texts file
        FWstream = new FileWriter(output, false);
        bufferWriter = new BufferedWriter(FWstream);
        for (String out : result) {
            bufferWriter.write(out + "\n");
        }
        bufferWriter.flush();
        FWstream.flush();
        bufferWriter.close();
        FWstream.close();
    }

    public void BOW_TfIdf(String input, String output, String featurelist, String round) throws FileNotFoundException, IOException {
        String line;
        int num_decimal = 0;
        List<String> listresult = new ArrayList<>();
        List<String> listFeature = new ArrayList<>();
        List<String> result = new ArrayList<>();

        FileReader fr;
        BufferedReader bufferReader;
        FileWriter fw;
        BufferedWriter bufferWriter;

        fr= new FileReader(round);
        bufferReader = new BufferedReader(fr);
        num_decimal = Integer.valueOf(bufferReader.readLine());
        bufferReader.close();
        fr.close();

        fr = new FileReader(input);
        bufferReader = new BufferedReader(fr);
        while ((line = bufferReader.readLine()) != null) {
            inputBOW.add(line);
            listFeature.addAll(Arrays.asList(line.split(" ")));
        }
        bufferReader.close();
        fr.close();
        // get featurelist
        List<String> temp = listFeature.stream().distinct().collect(Collectors.toList());
        listFeature = temp;
        // calculate Idf
        int num_document = inputBOW.size();
        for (String Wi : listFeature) {
            int num_Document_Contain_Wi = 0;
            // count document contains Wi

            for (String text : inputBOW) {
                String[] strArr = text.split(" ");
                for (String t: strArr)
                {
                    if (t.equalsIgnoreCase(Wi)) {
                    num_Document_Contain_Wi++;
                    break;
                }
                }

            }
            double idf;
            if (num_document == 0 || num_Document_Contain_Wi == 0) {
                idf = 0;
            } else {
                idf = Math.log10(1.0 * num_document / num_Document_Contain_Wi);
            }
            listIdf_Feature.add(Precision.round(idf, num_decimal));
        }

        for (String d : inputBOW) {
            // split d into dj
            List<String> dj = Arrays.asList(d.split(" "));
            List<String> listTf_Idf = new ArrayList<>();

            // count max appearance of a word in dj
            int max_appearance_word = MyLibraries.getMaxFrequency(dj);
            for (int i = 0; i < listFeature.size(); i++) {
                // count Wi in Dj
                int num_Wi_In_Dj = MyLibraries.getStringFrequency(dj, listFeature.get(i));
                double tf_idf = (1.0 * num_Wi_In_Dj / max_appearance_word) * listIdf_Feature.get(i);
                listTf_Idf.add(String.valueOf(Precision.round(tf_idf, num_decimal)));
                System.out.println(tf_idf);
            }
            result.add(String.join(" ", listTf_Idf));
        }

        // write feature, idf_feature to file
        fw = new FileWriter(featurelist, false);
        bufferWriter = new BufferedWriter(fw);
        for (int i = 0; i < listFeature.size(); i++) {
            bufferWriter.write(listFeature.get(i) + " " + listIdf_Feature.get(i) + "\n");
        }
        bufferWriter.flush();
        fw.flush();
        bufferWriter.close();
        fw.close();

        // tf_idf to file
        fw = new FileWriter(output, false);
        bufferWriter = new BufferedWriter(fw);
        for (String str : result) {
            bufferWriter.write(str + "\n");
        }
        bufferWriter.flush();
        fw.flush();
        bufferWriter.close();
        fw.close();
    }
}
