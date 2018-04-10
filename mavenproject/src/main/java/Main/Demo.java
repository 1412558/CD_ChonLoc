/*
 * To change this license header, choose License Headers in Project Properties.
 * To change this template file, choose Tools | Templates
 * and open the template in the editor.
 */
package Main;

import java.io.BufferedReader;
import java.io.BufferedWriter;
import java.io.File;
import java.io.FileInputStream;
import java.io.FileNotFoundException;
import java.io.FileReader;
import java.io.FileWriter;
import java.io.IOException;
import java.io.InputStreamReader;
import java.nio.file.Path;
import java.nio.file.Paths;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.List;
import java.util.stream.Collectors;
import java.util.stream.Stream;
import org.apache.commons.math3.util.Precision;
import org.tartarus.martin.Stemmer;

/**
 *
 * @author Kuga
 */
// https://www.foreach.be/blog/java-and-net-comparing-streams-linq
//https://stackoverflow.com/questions/31381992/java-equivalent-of-where-clause-in-c-sharp-linq
public class Demo {

    public String path = System.getProperty("user.dir") + "src\\main\\java\\File\\";

    public static void main(String[] args) throws FileNotFoundException, IOException {

        Demo d = new Demo();
        String s = "loneli";
        System.out.println(s.equalsIgnoreCase("Loneli"));
        //System.out.println(Precision.round(5.425, 2));
    }

    public Demo() {
    }

    public String getPath() {
        return path;
    }

    public void setPath(String path) {
        this.path = path;
    }

}
