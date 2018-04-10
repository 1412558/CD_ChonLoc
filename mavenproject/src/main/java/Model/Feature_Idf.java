/*
 * To change this license header, choose License Headers in Project Properties.
 * To change this template file, choose Tools | Templates
 * and open the template in the editor.
 */
package Model;

/**
 *
 * @author Kuga
 */
public class Feature_Idf {
    public String feature;
    public double idf ;

    public Feature_Idf() {
    }

    public Feature_Idf(String feature, double idf) {
        this.feature = feature;
        this.idf = idf;
    }

    public String getFeature() {
        return feature;
    }

    public void setFeature(String feature) {
        this.feature = feature;
    }

    public double getIdf() {
        return idf;
    }

    public void setIdf(double idf) {
        this.idf = idf;
    }
    
}
