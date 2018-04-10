/*
 * To change this license header, choose License Headers in Project Properties.
 * To change this template file, choose Tools | Templates
 * and open the template in the editor.
 */
package Model;

import java.util.List;

/**
 *
 * @author Kuga
 */
public class DocumentVector {

    public double value_Euclid;
    public int index_document;
    public DocumentVector() {
    }
    public DocumentVector(double value_Euclid, int index_document) {
        this.value_Euclid = value_Euclid;
        this.index_document = index_document;
    }

    public double getValue_Euclid() {
        return value_Euclid;
    }

    public void setValue_Euclid(double value_Euclid) {
        this.value_Euclid = value_Euclid;
    }

    public int getIndex_document() {
        return index_document;
    }

    public void setIndex_document(int index_document) {
        this.index_document = index_document;
    }
    
}
