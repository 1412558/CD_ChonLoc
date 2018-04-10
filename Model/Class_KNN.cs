using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public class Class_KNN
    {
        public int key_class { get; set; }
        public double[] bow { get; set; }
       // public double value_Euclid { get; set; }
       // public double value_cosine { get; set; }
        public double value_similar { get; set; }
    }
}
