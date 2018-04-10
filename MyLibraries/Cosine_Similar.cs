using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyLibraries
{
    public class Cosine_Similar
    {
        // return = 1- cosine => giá trị càng nhỏ thì càng tương tự
        public double Calculate_Similar(double[] v1, double[] v2)
        {
            double sum = 0;
            double sum_v1 = 0;
            double sum_v2 = 0;
            for (int i = 0; i < v1.Length; i++)
            {
                // tính tích vô hướng
                sum += v1[i] * v2[i];

                // tính chiều dài vector v1, v2
                sum_v1 += v1[i] * v1[i];
                sum_v2 += v2[i] * v2[i];
            }
            return sum / (Math.Sqrt(sum_v1) * Math.Sqrt(sum_v2));
        }
    }
}
