using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exercise2
{
    public static class InputData
    {
        public static List<double> Production = new List<double>();//{ 0, 3061, 0, 3885, 0, 5886, 0, 7416, 7063, 0};

        public static List<double> Delta_t = new List<double>();// { 48, 12, 15, 12, 17, 12, 18, 12, 72, 100 };

        public static List<double> Pressure = new List<double>();// {134.6, 121.4, 134.6, 116.8, 134.8, 104.1, 134.6, 91, 79.4, 134.6};

        public static List<double> W;//= new List<double>();

        public static List<double> X; // = new List<double>();
        public static List<double> Y; // = new List<double>();

        public static void Compute_W()
        {
            W = new List<double>();
            X = new List<double>();
            Y = new List<double>();
            //if(Production.Count != Delta_t.Count || Production.Count 
            //    != Pressure.Count || Delta_t.Count != Pressure.Count)
            //{
            //    throw new Exception("There are mismatch between input data");
            //}

            for (int i = 0; i < Pressure.Count; i++)
            {
                if(Production[i] == 0)
                {
                    W.Add(0);                    
                    continue;
                }

                double result = ((Math.Pow(Pressure[0] * 0.1, 2)) - (Math.Pow(Pressure[i] * 0.1, 2))) / Production[i];
                X.Add(Production[i]);
                W.Add(result);
                Y.Add(result);
            }
        }
    }
}
 