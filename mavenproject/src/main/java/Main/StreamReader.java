/*
 * To change this license header, choose License Headers in Project Properties.
 * To change this template file, choose Tools | Templates
 * and open the template in the editor.
 */
package Main;

import java.io.BufferedReader;
import java.io.FileNotFoundException;
import java.io.FileReader;
import java.io.IOException;

/**
 *
 * @author Kuga
 */
public class StreamReader {

    private BufferedReader bf;
    private FileReader fr;
    
    public StreamReader(String fileName) throws FileNotFoundException
    {
        this.fr = new FileReader(fileName);
        this.bf = new BufferedReader(this.fr);
    }
    public void close() throws IOException{
        this.bf.close();
        this.fr.close();
    }
    
    public String readLine() throws IOException
    {
        return this.bf.readLine();
    }
}
