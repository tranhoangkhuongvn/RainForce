﻿using System;
using RainForce.Models;

namespace RainForce.Utilities
{
    public class Graph
    {
        public bool NeedsBackPropagation { get; }


        public Graph(bool backProp)
        {
            NeedsBackPropagation = backProp;
        }

        public void Backward(Matrix m,Matrix outt, int d,int ix)
        {
            for (int i = 0, n = d; i < n; i++)
            {
                m.BackPropWeights[d * ix + i] += outt.BackPropWeights[i];
            }
        }
        public void Backward(Matrix m, Matrix outt,int n)
        {
            for (var i = 0; i < n; i++)
            {
                // grad for z = tanh(x) is (1 - z^2)
                var mwi = outt.Weights[i];
                m.BackPropWeights[i] += (1.0 - mwi * mwi)* outt.BackPropWeights[i];
            }
        }

        public void Backward(Matrix m1, Matrix m2,Matrix outt, int d)
        {
            for (var i = 0; i < m1.NumberOfRows; i++)
            { // loop over rows of m1
                for (var j = 0; j < m2.NumberOfColumns; j++)
                { // loop over cols of m2
                    for (var k = 0; k < m1.NumberOfColumns; k++)
                    { // dot product loop
                        var b = outt.BackPropWeights[d * i + j];
                        m1.BackPropWeights[m1.NumberOfColumns * i + k] += m2.Weights[m2.NumberOfColumns * k + j] * b;
                        m2.BackPropWeights[m2.NumberOfColumns * k + j] += m1.Weights[m1.NumberOfColumns * i + k] * b;
                    }
                }
            }
        }

        //public Matrix RowPluck(Matrix m, int ix)
        //{
        //    Util.Assert(ix >= 0 && ix < m.NumberOfRows);
        //    var d = m.NumberOfColumns;
        //    var outt = new Matrix(d, 1);
        //    for (int i = 0, n = d; i < n; i++)
        //    {
        //        outt.Weights[i] = m.Weights[d * ix + i];
        //    }

        //    if (NeedsBackPropagation)
        //    {
        //        Backward(m,outt,d,ix);
        //    }
        //    return outt;
        //}

        public Matrix GetMatrixFromTangent(Matrix m)
        {
            // tanh nonlinearity
            var outt = new Matrix(m.NumberOfRows, m.NumberOfColumns);
            var n = m.Weights.Length;
            for (var i = 0; i < n; i++)
            {
                outt.Weights[i] = Math.Tanh(m.Weights[i]);
            }
            if (NeedsBackPropagation)
            {
                Backward(m, outt, n);
            }
            return outt;
        }

        //public Matrix GetMatrixFromSigmoid(Matrix m)
        //{
        //    // sigmoid nonlinearity
        //    var outt = new Matrix(m.NumberOfRows, m.NumberOfColumns);
        //    var n = m.Weights.Length;
        //    for (var i = 0; i < n; i++)
        //    {
        //        outt.Weights[i] = Util.Sig(m.Weights[i]);
        //    }

        //    if (NeedsBackPropagation)
        //    {
        //        Backward(m, outt, n);
        //    }
        //    return outt;
        //}

        //public Matrix Relu(Matrix m)
        //{
        //    var outt = new Matrix(m.NumberOfRows, m.NumberOfColumns);
        //    var n = m.Weights.Length;
        //    for (var i = 0; i < n; i++)
        //    {
        //        outt.Weights[i] = Math.Max(0, m.Weights[i]);
        //    }
        //    if (NeedsBackPropagation)
        //    {
        //        Backward(m, outt, n);
        //    }
        //    return outt;
        //}

        public Matrix MultiplyMatrix(Matrix m1, Matrix m2)
        {
            // multiply matrices m1 * m2
            Util.Assert(m1.NumberOfColumns == m2.NumberOfRows, "matrix multiply dimensions misaligned");

            var n = m1.NumberOfRows;
            var d = m2.NumberOfColumns;
            var outt = new Matrix(n, d);
            for (var i = 0; i < m1.NumberOfRows; i++)
            { // loop over rows of m1
                for (var j = 0; j < m2.NumberOfColumns; j++)
                { // loop over cols of m2
                    var dot = 0.0;
                    for (var k = 0; k < m1.NumberOfColumns; k++)
                    { // dot product loop
                        dot += m1.Weights[m1.NumberOfColumns* i + k] * m2.Weights[m2.NumberOfColumns * k + j];
                    }
                    outt.Weights[d * i + j] = dot;
                }
            }
            if (NeedsBackPropagation)
            {
                Backward(m1,m2, outt, d);
            }
            return outt;
        }

        public Matrix AddMatrix(Matrix m1, Matrix m2)
        {
            Util.Assert(m1.Weights.Length == m2.Weights.Length);
            var outt = new Matrix(m1.NumberOfRows, m1.NumberOfColumns);
            for (int i = 0, n = m1.Weights.Length; i < n; i++)
            {
                outt.Weights[i] = m1.Weights[i] + m2.Weights[i];
            }
            if (NeedsBackPropagation)
            {
                for (int i = 0, n = m1.Weights.Length; i < n; i++)
                {
                    m1.BackPropWeights[i] += outt.BackPropWeights[i];
                    m2.BackPropWeights[i] += outt.BackPropWeights[i];
                }
            }
            return outt;
        }
        public Matrix Dot(Matrix m1, Matrix m2)
        {
            Util.Assert(m1.Weights.Length == m2.Weights.Length);
            var outt = new Matrix(1, 1);
            var dot = 0.0;
            for (int i = 0, n = m1.Weights.Length; i < n; i++)
            {
                dot += m1.Weights[i] * m2.Weights[i];
            }
            outt.Weights[0] = dot;
            if (NeedsBackPropagation)
            {
                    for (int i = 0, n = m1.Weights.Length; i < n; i++)
                    {
                        m1.BackPropWeights[i] += m2.Weights[i] * outt.BackPropWeights[0];
                        m2.BackPropWeights[i] += m1.Weights[i] * outt.BackPropWeights[0];
                    }
            }
            return outt;
        }

        public Matrix Eltmul(Matrix m1, Matrix m2)
        {
            Util.Assert(m1.Weights.Length == m2.Weights.Length);
            var outt = new Matrix(m1.NumberOfRows,m1.NumberOfColumns);
            for (int i = 0, n = m1.Weights.Length; i < n; i++)
            {
                outt.Weights[i] = m1.Weights[i] * m2.Weights[i];
            }
            if (NeedsBackPropagation)
            {
                    for (int i = 0, n = m1.Weights.Length; i < n; i++)
                    {
                        m1.BackPropWeights[i] += m2.Weights[i] * outt.BackPropWeights[i];
                        m2.BackPropWeights[i] += m1.Weights[i] * outt.BackPropWeights[i];
                    }
            }
            return outt;
        }
    }
}
