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

/**
 *
 * @author Kuga
 */
public class StreamWriter {
    private BufferedWriter bw;
    private FileWriter fw;
    
    public StreamWriter(String fileName, boolean type) throws FileNotFoundException, IOException
    {
        this.fw = new FileWriter(fileName, type);
        this.bw = new BufferedWriter(this.fw);
    }
    public void close() throws IOException{
        this.bw.close();
        this.fw.close();
    }
    
    public void writeLine(String str) throws IOException
    {
        this.bw.write(str+"\n");
    }
    public void write(String str) throws IOException
    {
        this.bw.write(str);
    }
    public void flush() throws IOException
    {
        this.bw.flush();
        this.fw.flush();
    }
}
