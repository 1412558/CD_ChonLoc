/*
 * To change this license header, choose License Headers in Project Properties.
 * To change this template file, choose Tools | Templates
 * and open the template in the editor.
 */
package Main;

import java.util.ArrayList;
import java.util.HashMap;
import java.util.Iterator;
import java.util.List;
import java.util.Map;
import java.util.Set;
import static javafx.scene.input.KeyCode.T;

/**
 *
 * @author Kuga
 */
public class MyLibraries {

    public static int getMaxFrequency(List<String> l) {
        Map<String, Integer> map = new HashMap<String, Integer>();
        int lenght = l.size();
        int maxCount = 0;
        if (lenght > 0) {
            maxCount = 1;
        }
        for (int i = 0; i < lenght; i++) {
            String key = l.get(i);
            if (map.containsKey(key)) {
                int value = map.get(key);
                map.put(key, value + 1);

                if (maxCount < value + 1) {
                    maxCount = value + 1;
                }

            } else {
                map.put(key, 1);
            }
        }
        return maxCount;
    }

    public static int getStringFrequency(List<String> l, String str) {
        int maxCount = 0;
        for (String item : l) {
            if (item.compareToIgnoreCase(str) == 0) {
                maxCount++;
            }
        }
        return maxCount;
    }

    public static List<String> removeStopwords(List<String> stopwords, String[] strArr) {
        List<String> listStr_after_stopword = new ArrayList<>();
        for (String str : strArr) {
            boolean flag = true;
            for (String word : stopwords) {
                if (str.compareToIgnoreCase(word) == 0) {
                    flag = false;
                    break;
                }
            }
            if (flag) {
                listStr_after_stopword.add(str.toLowerCase());
            }
        }
        return listStr_after_stopword;
    }
    
    public static double calculateVectorDistance(double[] array1, double[] array2)
    {
        double Sum = 0.0;
        for(int i=0;i<array1.length;i++) {
           Sum = Sum + Math.pow((array1[i]-array2[i]),2.0);
        }
        return Math.sqrt(Sum);
    }
    
    public static double Euclideanorm(List<Double> l1, List<Double> l2) {
        double Sum = 0.0;
        for (int i = 0; i < l1.size(); i++) {
            Sum = Sum + Math.pow((l1.get(i) - l2.get(i)), 2.0);
        }
        return Math.sqrt(Sum);
    }
}
