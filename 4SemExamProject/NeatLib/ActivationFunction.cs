using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeatLib
{
    public static class ActivationFunction
    {
        public delegate double ActivationMethod(double inputSum);

        public static ActivationMethod[] activationFunctions = new ActivationMethod[]
        {
            Identity,
            Sigmoid
        };

        public static double Identity(double inputSum)
        {
            return inputSum;
        }

        public static double Sigmoid(double inputSum)
        {
            return 1 / (1 + Math.Exp(-inputSum));
        }
    }
}