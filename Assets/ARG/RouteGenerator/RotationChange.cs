using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RotationChange : MonoBehaviour 
{
    /// <summary>
    /// 将G^2连续的n次广义有理bezier样条中所有离散点都储存在矩阵C中,行向量是点的坐标    all the discrete points of a G^2 generalized rational B´ezier curve of degree n are stored in matrix C
    /// </summary>
    /// <param name="rotInterData">rot_inter_data储存的是需要插值的N个四元素（表示旋转）的N*4矩阵，行向量是插值四元素坐标 A N*4 matrix of which the row vectors represent the orientational interpolation quaternions</param>
    /// <param name="vrotInterData">vrot_inter_data储存的是N个插值四元素处的单位切向量（表示速度）的N*4矩阵，行向量是单位切向量坐标 A N*4 matrix of which the row vectors represent the unit tangent quaternions at the orientational interpolation quaternions</param>
    /// <param name="crotInterData">crot_inter_data储存的是N个插值四元素处的曲率向量（表示加速度）的N*4矩阵，行向量是曲率向量坐标  A N*4 matrix of which the row vectors represent the curvature quaternions at the orientational interpolation quaternions</param>
    /// <param name="W">W储存的是广义有理n次Bezier曲线的初始权因子的(N-1)*(n+1)矩阵，行向量是每段广义有理n次Bezier曲线的n+1个（初始）权因子（w_0,w_1,...,w_n） </param>
    /// <param name="beta2">beta2储存的是G^1Hermite插值的一阶及二阶几何连续形状参数的(N-1)*4矩阵，行向量是每段广义有理n次Bezier曲线在t=0和t=1处的一阶及t=0和t=1处的二阶几何连续的形状参数 A (N-1)*4 matrix of which the k-th row vector stores the four shape parameters of the k-th Bezier curve segment of G^2 Hermite interpolation spline at the endpoints</param>
    /// <param name="tao">tao表示离散的步长  step length</param>
    /// <returns>坐标点数组  all the discrete points of a G^2 generalized rational B´ezier curve of degree n </returns>
    static float[,] G2SrbSpline(float[,] rotInterData, float[,] vrotInterData, float[,] crotInterData, float[,] W, float[,] beta2, float tao)
    {
        int N = rotInterData.GetLength(0); //N是插值点个数   the number of the positional interpolation vectors is N
        int M = rotInterData.GetLength(1); //维数  dimension
        int n = W.GetLength(1) - 1; //广义有理Bezier曲线的次数是n=5(G^2Hermite插值)  the degree of the generalized rational Bezier spline for G^2 Hermite interpolation is n=5
        float[,] X = rotAllControl2(rotInterData, vrotInterData, crotInterData, W, beta2); //将组成样条的N-1段Bezier曲线的所有控制顶点储存在X中   a (n+1)(N-1)*4 matrix of which the ((n+1)*(k-1)+j)-th row vector represents the j-th control point of the k-th generalized rational Bezier curve segment of G^2 Hermite interpolation spline
        int T = Mathf.FloorToInt(1 / tao + 1); //需要计算的每段Bezier曲线离散点的总个数是T  the number of the discrete points of every Bezier curve segment is T
        float[,] C = new float[(N - 1) * T, M]; //需要计算的每段Bezier曲线离散点的总个数是T=1/tao+1,组成样条的曲线的条数是N-1，所以C的行数是(N-1)*T，行向量是点的坐标  the number of rows of matrix C is (N-1)*T
        for (int i = 1; i <= N - 1; i++)
        {
            for (float t = 0f; t <= 1; t += tao)
            {
                float[,] control = new float[n + 1, M]; //第i段bezier曲线对应的控制点 the control points of the i-th generalized rational Bezier curve
                for (int j = 0; j < n + 1; j++)
                {
                    for (int z = 0; z < M; z++)
                    {
                        control[j, z] = X[(n + 1) * (i - 1) + j, z]; //t对应第i段bezier曲线的第k个离散点，取整  t corresponds to the k-th discrete point of the i-th generalized rational Bezier curve segment
                    }
                }
                int k = Mathf.RoundToInt(t / tao + 1); //t对应第i段bezier曲线的第k个离散点，取整
                float[] w = new float[n + 1];
                for (int z = 0; z < n + 1; z++)
                {
                    w[z] = W[i - 1, z];
                }
                for (int z = 0; z < M; z++)
                {
                    C[(i - 1) * T + k - 1, z] = RB(control, w, t)[z]; //计算第i段bezier曲线的第k个离散点，对应于矩阵C中的第(i-1)*T+k个行向量  compute the k-th discrete point of the i-th generalized rational Bezier curve segment and store it in the ((i-1)*T+k)-th row of matrix C
                }
            }
        }
        return C;
    }

    /// <summary>
    /// n次有理球面Bezier曲线在t时刻的取值  the value of the generalized rational Bezier curve of degree n at t
    /// </summary>
    /// <param name="X">X是储存n+1个曲线控制点的(n+1)*m维矩阵，行向量是控制点坐标  X is a (n+1)*m matrix of which the row vectors represent the control points of the generalized rational Bezier curve of degree n</param>
    /// <param name="w">w是储存n+1个第0级（初始）权因子的行向量 w is a row vector which stores n+1 initial weights </param>
    /// <param name="t">t是时间参数   time parameter </param>
    /// <returns>返回值是n次有理球面Bezier曲线在t时刻的取值  the value of the generalized rational Bezier curve of degree n at t</returns>
    static float[] RB(float[,] X, float[] w, float t)
    {
        int r = X.GetLength(1);
        int n = w.GetLength(0) - 1; //n+1个第0级（初始）权因子对应n次广义有理Bezier曲线 n+1 initial weights correspond to the generalized rational Bezier curve of degree n
        float[,] C = W(w, t); //矩阵C的第k行储存的是第k-1级权因子,k=1,...,n+1  the k-th row of matrix C stores the (k-1)-th weight
        float[,] A = new float[n, r]; //A是n次有理球面Bezier曲线在t时刻的取值 the value of the generalized rational Bezier curve of degree n at t
        float[] AA = new float[r];
        for (int j = 0; j < n; j++)
        {
            float[] x1 = new float[r];
            float[] x2 = new float[r];
            float[] c = new float[2];
            c[0] = C[0,j];
            c[1] = C[0,j+1];
            for (int z = 0; z < r; z++)
            {
                x1[z] = X[j, z];
                x2[z] = X[j + 1, z];
            }
            for (int z = 0; z < r; z++)
            {
                A[j, z] = RB1(x1, x2, c, t)[z]; //n次球面Bezier曲线的递归定义 the recursive definition of the generalized rational Bezier curve of degree n
            }
        }
        if (n > 1)
        {
            for (int k = 1; k < n; k++)
            {
                for (int j = 0; j < n - k; j++)
                {
                    float[] a1 = new float[r];
                    float[] a2 = new float[r];
                    float[] c = new float[2];
                    c[0] = C[k,j];
                    c[1] = C[k,j+1];
                    for (int z = 0; z < r; z++)
                    {
                        a1[z] = A[j, z];
                        a2[z] = A[j + 1, z];
                    }
                    for (int z = 0; z < r; z++)
                    {
                        A[j, z] = RB1(a1, a2, c, t)[z];
                    }
                }
            }
        }
        for (int z = 0; z < r; z++)
        {
            AA[z] = A[0, z];
        }
        return AA;
    }

    /// <summary>
    /// 连接球面两点X,Y的n=1次广义有理球面Bezier曲线RB1   RB1 is a generalized rational Bezier curve of degree n=1 which connects two spherical points X and Y
    /// </summary>
    /// <param name="w">w是储存n=2个第0级（初始）权因子的行向量 w is a row vector which stores n+1=2 initial weights </param>
    /// <param name="t">t是参数</param>
    static float[] RB1(float[] X, float[] Y, float[] w, float t)
    {
        int r = X.GetLength(0);
        float[,] C = W(w, t); //矩阵C的第k行储存的是第k-1级权因子,k=1,...,n+1  the k-th row of matrix C stores the (k-1)-th weights
        float[,] u = U(C, t); //矩阵u的第k行储存的是第k-1级比例系数,k=1,...,n  the k-th row of matrix u stores the (k-1)-th scale coefficient
        float d = Dot(X, Y);
        float a = Mathf.Acos(d); //a是曲线相邻控制点对应向量的夹角theta0 a is the angle between two neighbouring control points of the curve   
        float[] A = new float[r];
        if (d == -1)
        {
            for (int i = 0; i < r; i++)
            {
                A[i] = 100000f;
            }
        }
        else
        {
            for (int i = 0; i < r; i++)
            {
                A[i] = Mathf.Sin((1 - u[0, 0]) * a) / Mathf.Sin(a) * X[i] + Mathf.Sin(u[0, 0] * a) / Mathf.Sin(a) * Y[i];
            }
        }
        return A;
    }

    /// <summary>
    /// 计算n次广义有理Bezier曲线的各级比例系数u compute all the scale coefficient of the generalized rational Bezier curve of degree n
    /// </summary>
    /// <param name="W">矩阵W的第k行储存的是第k-1级共（n+2-k）个权因子，k=1,...,n+1  the k-th row of matrix W stores the (k-1)-th weights of which the number is n+2-k </param>
    /// <param name="t">t是参数</param>
    /// <returns>返回值的第k行储存的是第k-1级共(n+1-k)个比例系数，k=1,...,n  the k-th row of the returning matrix stores the (k-1)-th scale coefficient of which the number is n+1-k</returns>
    static float[,] U(float[,] W, float t)
    {
        int n = W.GetLength(1) - 1; //n+1个第0级（初始）权因子对应n次广义有理Bezier曲线  n+1 initial weights correspond to the generalized rational Bezier curve of degree n
        float[,] A = new float[n, n];
        for (int k = 0; k < n; k++)
        {
            for (int j = 0; j < n - k; j++)
            {
                A[k, j] = t * W[k, j + 1] / ((1 - t) * W[k, j] + t * W[k, j + 1]);
            }
        }
        return A;
    }

    /// <summary>
    /// 计算各级权因子的函数 the function computing all levels of the weights
    /// </summary>
    /// <param name="w">w是储存n次广义有理Bezier曲线的n+1个第0级（初始）权因子的行向量（w_0=1,w_1,...,w_n）w is a row vector which stores n+1=2 initial weights the generalized rational Bezier curve of degree n</param>
    /// <param name="t">t是参数</param>
    /// <returns>返回值的第k行储存的是第k-1级权因子，k=1,...,n+1 the k-th row of the returning matrix stores the (k-1)-th weights of which the number is n+2-k</returns>
    static float[,] W(float[] w, float t)
    {
        int n = w.GetLength(0) - 1; //n+1个第0级（初始）权因子对应n次广义有理Bezier曲线 n+1 initial weights correspond to the generalized rational Bezier curve of degree n
        float[,] A = new float[n + 1, n + 1];
        for (int j = 0; j < n + 1; j++)
        {
            A[0, j] = w[j]; //A的第一行储存的是0级（初始）权因子 the first row of matrix A stores the initial weights
        }
        for (int i = 1; i < n + 1; i++)
        {
            for (int j = 0; j < n + 1 - i; j++)
            {
                A[i, j] = (1 - t) * A[i - 1, j] + t * A[i - 1, j + 1]; //利用递归算法计算第i级权因子，储存到A的第i行中 compute the i-th weights by the ﻿recursive algorithm and store them in the i-th row of A
            }
        }
        return A;
    }

    /// <summary>
    /// 计算控制点坐标 compute the control points
    /// </summary>
    /// <param name="rotInterData">储存的是需要插值的N个四元素（表示旋转）的N*4矩阵，行向量是插值四元素坐标  A N*4 matrix of which the row vectors represent the orientational interpolation quaternions</param>
    /// <param name="vrotInterData">储存的是N个插值四元素处的单位切向量（表示速度）的N*4矩阵，行向量是单位切向量坐标  A N*4 matrix of which the row vectors represent the unit tangent quaternions at the orientational interpolation quaternions</param>
    /// <param name="crotInterData">储存的是N个插值四元素处的曲率向量（表示加速度）的N*4矩阵，行向量是曲率向量坐标  A N*4 matrix of which the row vectors represent the curvature quaternions at the orientational interpolation quaternions</param>
    /// <param name="W">W储存的是广义有理n次Bezier曲线的初始权因子的(N-1)*(n+1)矩阵，行向量是每段广义有理n次Bezier曲线的n+1个（初始）权因子（w_0,w_1,...,w_n）W is a (N-1)*(n+1) matrix of which the row vector stores n+1 initial weights of the generalized rational Bezier curve of degree n</param>
    /// <param name="beta2">beta2储存的是G^1Hermite插值的一阶及二阶几何连续形状参数的(N-1)*4矩阵，行向量是每段广义有理n次Bezier曲线在t=0和t=1处的一阶及t=0和t=1处的二阶几何连续的形状参数 A (N-1)*4 matrix of which the k-th row vector stores the four shape parameters of the k-th generalized rational Bezier curve segment of G^2 Hermite interpolation spline at the endpoints</param>
    /// <returns>返回值是G^2Hermite插值的广义有理n次Bezier样条(由N-1段广义有理n次Bezier曲线组成)的全部控制点的(n+1)(N-1)*4矩阵，行向量是控制点坐标  a (n+1)(N-1)*4 matrix of which the ((n+1)*(k-1)+j)-th row vector represents the j-th control point of the k-th generalized rational Bezier curve segment of G^2 Hermite interpolation spline</returns>
    static float[,] rotAllControl2(float[,] rotInterData, float[,] vrotInterData, float[,] crotInterData, float[,] W, float[,] beta2)
    {
        int n = 5; //球面Bezier曲线的次数是n=5(G^2Hermite插值)the degree of the generalized rational Bezier spline for G^2 Hermite interpolation is n=5
        int N = rotInterData.GetLength(0); //值点的个数是N，样条由N-1段曲线组成 the number of the positional interpolation vectors is N, the spline is composed of N-1 segments
        int M = rotInterData.GetLength(1); //维数 dimension
        float[,] x = new float[(n + 1) * (N - 1), M];
        for (int i = 1; i <= (N - 1); i++)
        {
            float[,] r = new float[2, M];
            float[,] v = new float[2, M];
            float[,] c = new float[2, M];
            float[] w = new float[n + 1];
            for (int z = 0; z < M; z++)
            {
                r[0, z] = rotInterData[i - 1, z];
                r[1, z] = rotInterData[i, z];
                v[0, z] = vrotInterData[i - 1, z];
                v[1, z] = vrotInterData[i, z];
                c[0, z] = crotInterData[i - 1, z];
                c[1, z] = crotInterData[i, z];
                w[z] = W[i - 1, z];
            }
            for (int j = 0; j < n + 1; j++)
            {
                for (int z = 0; z < M; z++)
                {
                    x[(n + 1) * (i - 1) + j, z] = Jscontrol2(r, v, c, w, beta2[i - 1, 0], beta2[i - 1, 1], beta2[i - 1, 2], beta2[i - 1, 3])[j, z];
                }
            }
        }
        return x;
    }

    /// <summary>
    /// 计算控制顶点  compute the control points
    /// 根据G2-Hermite插值条件计算广义有理Bezier曲线控制顶点的函数  the function for computing the control points of the generalized rational Bezier curve  of degree n according to the G^2 Hermite interpolation conditions
    /// </summary>
    /// <param name="Z">Z是储存2个插值点p_0,p_1的2*r维矩阵，行向量是插值点坐标 Z is a 2*r matrix of which the row vectors represent the orientational interpolation vectors</param>
    /// <param name="V1">V1是储存2个插值点处单位切向量v_0^1,v_1^1的2*r维矩阵，行向量是切向量坐标 V1 is a 2*r matrix of which the row vectors represent the unit tangent vectors at the orientational interpolation vectors</param>
    /// <param name="V2">V2是储存2个插值点处曲率向量v_0^2,v_1^2的2*r维矩，行向量是曲率向量坐标 V2 is a 2*r matrix of which the row vectors represent the curvature vectors at the orientational interpolation vectors</param>
    /// <param name="w">w是储存n+1个第0级（初始）权因子的行向量（w_0=1,w_1,...,w_n） w is a row vector which stores n+1=2 initial weights the generalized rational Bezier curve of degree n </param>
    /// <param name="beta01">beta01是t=0处一阶几何连续的形状参数  beta01 is the shape parameter of G^1 continuity at t=0</param>
    /// <param name="beta11">beta11是t=1处一阶几何连续的形状参数  beta11 is the shape parameter of G^1 continuity at t=1</param>
    /// <param name="beta02">beta02是t=0处二阶几何连续的形状参数  beta02 is the shape parameter of G^2 continuity at t=0</param>
    /// <param name="beta02">beta12是t=1处二阶几何连续的形状参数  beta12 is the shape parameter of G^2 continuity at t=1</param>
    /// <returns>返回值是储存n+1个曲线控制点（b_0,b_1,...,b_n）的(n+1)*r维矩阵，行向量是控制点坐标  a (n+1)*r matrix of which the row vectors represent the control points of the interpolation curve</returns>
    static float[,] Jscontrol2(float[,] Z, float[,] V1, float[,] V2, float[] w, float beta01, float beta11, float beta02, float beta12)
    {
        int r = Z.GetLength(1); //获取数组维数 r is the dimension
        float[,] C = new float[2, r]; //C是储存2个曲率向量向切平面的投影c_0,c_1的2*4维矩阵，行向量是投影向量坐标   C is a 2*4 matrix of which the row vectors represent the projection of the ordinary acceleration vectors onto the tangent space
        int n = 5; //球面Bezier曲线的次数是n次,n=5  the degree of the generalized rational Bezier spline for G^2 Hermite interpolation is n=5 
        float[,] x = new float[n + 1, r]; //x是储存n+1个曲线控制点（b_0,b_1,...,b_n）的(n+1)*r维矩阵，行向量是控制点坐标   x is a (n+1)*r matrix of which the row vectors represent the control points of the interpolation curve
        float[] theta = new float[n]; //theta是储存n次广义有理Bezier曲线n个相邻两控制点夹角（theta_0,theta_1,...,theta_n-1）的行向量  theta is a row vector of which the i-th element is the angle between the i-th and the (i+1)-th control points of the curve
        for (int i = 0; i < r; i++)
        {
            C[0, i] = beta01 * beta01 * Z[0, i] + beta02 * V1[0, i] + beta01 * beta01 * V2[0, i];
            C[1, i] = beta11 * beta11 * Z[1, i] + beta12 * V1[1, i] + beta11 * beta11 * V2[1, i];
        }
        theta[0] = beta01 / n / w[1]; //控制点b_0与b_1的夹角theta_0  theta_0 is the angle between the control points b_0 and b_1
        theta[n - 1] = beta11 / n / w[n - 1]; //控制点b_{n-1}与b_{n}的夹角theta_n  theta_n is the angle between the control points b_{n-1} and b_n
        for (int i = 0; i < r; i++)
        {
            x[0, i] = Z[0, i]; //根据广义球面有理Bezier曲线的端点插值性，计算首控制点b_0  compute the first control point b_0 by the endpoint interpolation property of the generalized rational Bezier curve
            x[n, i] = Z[1, i]; //根据广义球面有理Bezier曲线的端点插值性，计算末控制点b_n  compute the last control point b_n by the endpoint interpolation property of the generalized rational Bezier curve
            if (theta[0] < Mathf.PI)
            {
                x[1, i] = Mathf.Cos(theta[0]) * x[0, i] + Mathf.Sin(theta[0]) * V1[0, i]; //根据首端点处G^1插值条件求解出的第二个控制点b_1 compute the second control point b_1 by the endpoint G^1 Hermite interpolation conditions
            }
            if (theta[n - 1] < Mathf.PI)
            {
                x[n - 1, i] = Mathf.Cos(theta[n - 1]) * x[n, i] - Mathf.Sin(theta[n - 1]) * V1[1, i]; //根据末端点处G^1插值条件求解出的倒数第二个控制点b_{n-1} compute the second-last control point b_{n-1} by the endpoint G^1 Hermite interpolation conditions
            }
        }
        float[] x0 = new float[r];
        float[] x1 = new float[r];
        float[] xn0 = new float[r];
        float[] xn1 = new float[r];
        float[] c0 = new float[r];
        float[] c1 = new float[r];
        for (int i = 0; i < r; i++)
        {
            x0[i] = x[0, i];
            x1[i] = x[1, i];
            c0[i] = C[0, i];
            c1[i] = C[1, i];
            xn0[i] = x[n - 1, i];
            xn1[i] = x[n, i];
        }
        float[] v0 = DB10(x0, w[0], x1, w[1]); //v0是b_{0}^1(t)在t=0处的一阶导数 v0 is the first order derivative of b_{0}^1(t) at t=0 
        float[] v1 = DB11(x0, w[0], x1, w[1]); //v1是b_{0}^1(t)在t=1处的一阶导数 v1 is the first order derivative of b_{0}^1(t) at t=1 
        float[] v2 = DB11(xn0, w[n - 1], xn1, w[n]); //v2是b_{n-1}^1(t)在t=1处的一阶导数 v2 is the first order derivative of b_{n-1}^1(t) at t=1 
        float[] v3 = DB10(xn0, w[n - 1], xn1, w[n]); //v3是b_{n-1}^1(t)在t=0处的一阶导数 v3 is the first order derivative of b_{n-1}^1(t) at t=0
        float[] dX0 = new float[r]; //dX0是b_1^1(t)在t=0处的一阶导数 dX0 is the first order derivative of b_{1}^1(t) at t=0
        float[] dX1 = new float[r]; //dX1是b_{n-2}^1(t)在t=1处的一阶导数  dX1 is the first order derivative of b_{n-2}^1(t) at t=1
        for (int i = 0; i < r; i++)
        {
            dX0[i] = Mathf.Sin(theta[0]) * C[0, i] / (n - 1) / n / w[1] / theta[0] - Dot(c0, x1) * (v0[i] / (w[1] * w[1]) / theta[0] - v1[i] / Mathf.Sin(theta[0])) / (n - 1) / n / theta[0] - (2 * w[1] - 2 * n * w[1] * w[1] + (n - 1) * w[2]) * v1[i] / (n - 1);
            dX1[i] = -Mathf.Sin(theta[n - 1]) * C[1, i] / (n - 1) / n / w[n - 1] / theta[n - 1] - Dot(c1, xn0) * (v2[i] / (w[n - 1] * w[n - 1]) / theta[n - 1] - v3[i] / Mathf.Sin(theta[n - 1])) / (n - 1) / n / theta[n - 1] - (2 * w[n - 1] - 2 * n * w[n - 1] * w[n - 1] + (n - 1) * w[n - 2]) * v3[i] / (n - 1);
        }
        theta[1] = w[1] * Norm(dX0) / w[2]; //控制点b_1与b_2的夹角theta_1  theta_1 is the angle between the control points b_1 and b_2
        theta[n - 2] = w[n - 1] * Norm(dX1) / w[n - 2];
        for (int i = 0; i < r; i++)
        {
            if (theta[1] < Mathf.PI)
            {
                x[2, i] = Mathf.Sin(theta[1]) * dX0[i] / Norm(dX0) + Mathf.Cos(theta[1]) * x[1, i]; //根据首端点处G^2插值条件求解出的第三个控制点b_2 compute the third control point b_2 by the endpoint G^2 Hermite interpolation conditions
            }
            if (theta[n - 2] < Mathf.PI)
            {
                x[n - 2, i] = -Mathf.Sin(theta[n - 2]) * dX1[i] / Norm(dX1) + Mathf.Cos(theta[n - 2]) * x[n - 1, i]; //根据末端点处G^2插值条件求解出的倒数第三个控制点b_{n-2} ompute the third-last control point b_{n-2} by the endpoint G^2 Hermite interpolation conditions
            }
        }
        return x;
    }

    /// <summary>
    /// 连接球面两点X1,X2的1次广义有理Bezier曲线在t=0处的切向量函数  compute the tangent of the generalized rational Bezier curve of degree 1, which connect the spherical points X1 and X2, at t=0
    /// </summary>
    /// <param name="w1">w1是控制点X1处的第0级（初始）权因子  w1 is the initial weight of control point X1</param>
    /// <param name="w2">w2是控制点X2处的第0级（初始）权因子  w2 is the initial weight of control point X2</param>
    static float[] DB10(float[] X1, float w1, float[] X2, float w2)
    {
        float theta = Mathf.Acos(Dot(X1, X2)); //theta是向量X1,X2的夹角 theta is the angle between the vectors X1 and X2
        int n = X1.GetLength(0);
        float[] dA = new float[n];
        int isEqual = 0;
        for (int i = 0; i < n; i++)
        {
            if (X1[i] == X2[i])
            {
                isEqual++;
            }
        }
        if (isEqual != n)
        {
            for (int i = 0; i < n; i++)
            {
                dA[i] = w2 * theta / w1 / Mathf.Sin(theta) * (-Mathf.Cos(theta) * X1[i] + X2[i]); //求连接球面两点X1,X2的1次广义有理Bezier曲线在t=0处的切向量 compute the tangent vector of the generalized rational Bezier curve of degree 1, which connect the spherical points X1 and X2, at t=0
            }
        }
        return dA;
    }

    /// <summary>
    ///  连接球面两点X1,X2的1次广义有理Bezier曲线在t=1处的切向量函数 compute the tangent vector of the generalized rational Bezier curve of degree 1, which connect the spherical points X1 and X2, at t=1
    /// </summary>
    /// <param name="w1">w1是控制点X1处的第0级（初始）权因子  w1 is the initial weight of control point X1</param>
    /// <param name="w2">w2是控制点X2处的第0级（初始）权因子  w2 is the initial weight of control point X2</param>
    static float[] DB11(float[] X1, float w1, float[] X2, float w2)
    {
        float theta = Mathf.Acos(Dot(X1, X2)); //theta是向量X1,X2的夹角 theta is the angle between the vectors X1 and X2
        int n = X1.GetLength(0);
        float[] dA = new float[n];
        int isEqual = 0;
        for (int i = 0; i < n; i++)
        {
            if (X1[i] == X2[i])
            {
                isEqual++;
            }
        }
        if (isEqual != n)
        {
            for (int i = 0; i < n; i++)
            {
                dA[i] = w1 * theta / w2 / Mathf.Sin(theta) * (-X1[i] + Mathf.Cos(theta) * X2[i]); //求连接球面两点X1,X2的1次广义有理Bezier曲线在t=1处的切向量 compute the tangent vector of the generalized rational Bezier curve of degree 1, which connect the spherical points X1 and X2, at t=0
            }
        }
        return dA;
    }

    /// <summary>
    /// 计算两个向量点乘 compute the inner product of two vectors
    /// </summary>
    static float Dot(float[] X1, float[] X2)
    {
        float s = 0f;
        for (int i = 0; i < X1.GetLength(0); i++)
        {
            s += X1[i] * X2[i];
        }
        return s;
    }

    /// <summary>
    /// 求四元向量的二范数 compute the 2-norm of a quaternion
    /// </summary>
    static float Norm(float[] a)
    {
        return Mathf.Sqrt(a[0] * a[0] + a[1] * a[1] + a[2] * a[2] + a[3] * a[3]);
    }

    /// <summary>
    /// 四元向量标准化 normalize a quaternion
    /// </summary>
    static float[] Normr(float[] a)
    {
        float[] v = new float[4];
        float r = Norm(a);
        v[0] = a[0] / r;
        v[1] = a[1] / r;
        v[2] = a[2] / r;
        v[3] = a[3] / r;
        return v;
    }

    /// <summary>
    /// 将初始旋转数据转化成满足球面条件的插值数据  convert the initial orientional data into the interpolation data satisfying spherical conditions
    /// </summary>
    /// <param name="C">C的行向量储存的是插值点信息 the row vectors of matrix C represent the interpolation points</param>
    /// <param name="dC">dC的行向量储存的是插值点处的一阶导信息 the row vectors of matrix C represent the first order derivatives at the interpolation points</param>
    /// <param name="d2C">d2C的行向量储存的是插值点处的二阶导信息 the row vectors of matrix C represent the second order derivatives at the interpolation points</param>
    /// <returns>满足球面条件的插值数据 interpolation data satisfying spherical conditions</returns>
    static float[,] TransData(float[,] C, float[,] dC, float[,] d2C)
    {
        int N = C.GetLength(0); //N是矩阵C的行数，即为插值点个数 N is the number of rows of matrix C which is also the number of interpolation points
        int d = C.GetLength(1); //d是矩阵C的列数，即为插值点维数 d is the number of columns of matrix C which is also the dimension of interpolation points
        float[,] H = new float[N, d * 3]; //H储存返回值  H stores the returning values
        for (int i = 0; i < N; i++)
        {
            float[] c = new float[d];
            float[] dc = new float[d];
            float[] d2c = new float[d];
            float[] h1 = new float[d];
            float[] h2 = new float[d];
            float[] hNorm = new float[d];
            for (int z = 0; z < d; z++)
            {
                c[z] = C[i, z];
                dc[z] = dC[i, z];
                d2c[z] = d2C[i, z];
            }
            for (int z = 0; z < d; z++)
            {
                H[i, z] = Normr(c)[z];
                h1[z] = Normr(c)[z];                
            }
            for (int z = 0; z < d; z++)
            {
                hNorm[z] = dc[z] - Dot(h1, dc) * h1[z];                
            }
            for (int z = 0; z < d; z++)
            {
                H[i, d + z] = Normr(hNorm)[z]; //求球面插值点rot_inter_data(i,:)处的单位切向量vrot_inter_data(i,:)  compute the unit tangent vector vrot_inter_data(i,:) at the spherical interpolation point rot_inter_data(i,:)
                h2[z] = Normr(hNorm)[z];
            }
            float s1 = Norm(dc);
            float s2 = Dot(d2c, h2);
            for (int z = 0; z < d; z++)
            {
                H[i, 2 * d + z] = d2c[z] - s2 * h2[z] / (s1 * s1);
            }
        }
        return H;
    }

    /// <summary>
    /// 获取实际使用旋转数组 obtain the practical orientional data 
    /// </summary>
    /// <param name="rotIniData">原始输入点旋转链表  the links of the original given data </param>
    /// <returns>去除开始两点与末尾两点的n-4组旋转四元素 n-4 sets of coordinates without the first and the last two points</returns>
    static float[,] GiveBackRot(List<Quaternion> rotIniData)
    {
        int lengh = rotIniData.Count;
        float[,] rotInterData = new float[lengh - 4, 4];
        for (int i = 0; i < lengh - 4; i++)
        {
            rotInterData[i, 0] = rotIniData[i + 2][0];
            rotInterData[i, 1] = rotIniData[i + 2][1];
            rotInterData[i, 2] = rotIniData[i + 2][2];
            rotInterData[i, 3] = rotIniData[i + 2][3];
        }
        return rotInterData;
    }

    /// <summary>
    /// 获取旋转插值数据处的一阶导向量  compute the first order derivatives at the orientional interpolation points by using the Bert's method
    /// </summary>
    /// <param name="rotIniData">原始输入点旋转链表 the link of the original given data </param>
    /// <returns>旋转插值数据处的一阶导向量 the first order derivatives at the orientional interpolation points</returns>
    static float[,] GiveBackD1Rot(List<Quaternion> rotIniData)
    {
        float w1 = 1f;
        float w2 = 1f;
        int lengh = rotIniData.Count;
        float[,] d1RotInterDataM = new float[lengh - 2, 4];
        float[,] d1RotInterData = new float[lengh - 4, 4];
        for (int i = 0; i < lengh - 2; i++)
        {
            float[] r0 = new float[4];
            float[] r1 = new float[4];
            float[] r2 = new float[4];
            for (int z = 0; z < 4; z++)
            {
                r0[z] = rotIniData[i][z];
                r1[z] = rotIniData[i + 1][z];
                r2[z] = rotIniData[i + 2][z];
            }
            for (int z = 0; z < 4; z++)
            {
                d1RotInterDataM[i, z] = (DB11(r0, w1, r1, w2)[z] + DB10(r1, w1, r2, w2)[z]) / 2; //利用Bert的方法，计算旋转插值数据处的一阶导向量 compute the first order derivatives at the orientional interpolation points by using the Bert's method
            }
        }
        for (int i = 0; i < lengh - 4; i++)
        {
            for (int z = 0; z < 4; z++)
            {
                d1RotInterData[i, z] = d1RotInterDataM[i + 1, z];
            }
        }
        return d1RotInterData;
    }

    /// <summary>
    /// 获取旋转插值数据处的二阶导向量 compute the second order derivatives at the orientional interpolation points by using the Bert's method
    /// </summary>
    /// <param name="rotIniData">原始输入点旋转链表 the link of the original given data</param>
    /// <returns>旋转插值数据处的二阶导向量 the second order derivatives at the orientional interpolation points</returns>
    static float[,] GiveBackD2Rot(List<Quaternion> rotIniData)
    {
        float w1 = 1f;
        float w2 = 1f;
        int lengh = rotIniData.Count;
        float[,] d1RotInterDataM = new float[lengh - 2, 4];
        float[,] d1RotInterData = new float[lengh - 4, 4];
        float[,] d2RotInterData = new float[lengh - 4, 4];
        for (int i = 0; i < lengh - 2; i++)
        {
            float[] r0 = new float[4];
            float[] r1 = new float[4];
            float[] r2 = new float[4];
            for (int z = 0; z < 4; z++)
            {
                r0[z] = rotIniData[i][z];
                r1[z] = rotIniData[i + 1][z];
                r2[z] = rotIniData[i + 2][z];
            }
            for (int z = 0; z < 4; z++)
            {
                d1RotInterDataM[i, z] = (DB11(r0, w1, r1, w2)[z] + DB10(r1, w1, r2, w2)[z]) / 2; //利用Bert的方法，计算旋转插值数据处的一阶导向量 compute the first order derivatives at the orientional interpolation points by using the Bert's method
            }
        }
        for (int i = 0; i < lengh - 4; i++)
        {
            for (int z = 0; z < 4; z++)
            {
                d1RotInterData[i, z] = d1RotInterDataM[i + 1, z];
            }
        }
        for (int i = 0; i < lengh - 4; i++)
        {
            float[] rm0 = new float[4];
            float[] rm1 = new float[4];
            float[] rm2 = new float[4];
            float[] d1 = new float[4];
            float[] d2 = new float[4];
            float[] r = new float[4];
            for (int z = 0; z < 4; z++)
            {
                rm0[z] = d1RotInterDataM[i, z];
                rm1[z] = d1RotInterDataM[i + 1, z];
                rm2[z] = d1RotInterDataM[i + 2, z];
                d1[z] = d1RotInterData[i, z];
                r[z] = rotIniData[i + 2][z];
            }
            for (int z = 0; z < 4; z++)
            {
                d2[z] = (DB11(rm0, w1, rm1, w2)[z] + DB10(rm1, w1, rm2, w2)[z]) / 2; //利用Bert的方法，计算旋转插值数据处的二阶导向量 compute the second order derivatives at the orientional interpolation points by using the Bert's method
            }
            float k = -Dot(d1, d1) / Dot(d2, r);
            for (int z = 0; z < 4; z++)
            {
                d2RotInterData[i, z] = k * d2[z];
            }
        }
        return d2RotInterData;
    }

    /// <summary>
    /// 形状参数的选取  choose the shape parameters
    /// </summary>
    static float[,] GetBetaR(float[,] d1RotInterData, float[,] d2RotInterData, float[,] H)
    {
        int N = d1RotInterData.GetLength(0);
        float[,] betaR = new float[N - 1, 4];
        //对应旋转曲线  corresponding to orientional curves
        for (int i = 0; i < N - 1; i++)
        {
            float[] d10 = new float[4];
            float[] d11 = new float[4];
            float[] d20 = new float[4];
            float[] d21 = new float[4];
            float[] h0 = new float[4];
            float[] h1 = new float[4];
            for (int z = 0; z < 4; z++)
            {
                d10[z] = d1RotInterData[i, z];
                d11[z] = d1RotInterData[i + 1, z];
                d20[z] = d2RotInterData[i, z];
                d21[z] = d2RotInterData[i + 1, z];
                h0[z] = H[i, z + 4];
                h1[z] = H[i + 1, z + 4];
            }
            betaR[i, 0] = Norm(d10);
            betaR[i, 1] = Norm(d11);
            betaR[i, 2] = Dot(d20, h0);
            betaR[i, 3] = Dot(d21, h1);
        }
        return betaR;
    }

    /// <summary>
    /// 权重的选取,实际上对应的是noakes的方法 the selection of the weights corresponding to noakes' method
    /// </summary>
    /// <param name="l">初始点个数 the number of initial points</param>
    /// <returns>权重数组 a matrix stores the weights</returns>
    static float[,] GetW(int l)
    {
        float[,] w = new float[l - 5, 6];
        for (int i = 0; i < l - 6; i++)
        {
            for (int j = 0; j < 6; j++)
            {
                w[i, j] = 1f;
            }
        }
        w[l - 6, 0] = 1f;
        w[l - 6, 1] = 2f;
        w[l - 6, 2] = 4f;
        w[l - 6, 3] = 4f;
        w[l - 6, 4] = 2f;
        w[l - 6, 5] = 1f;
        return w;
    }

    /// <summary>
    /// 根据原始旋转状态返回插值后的旋转状态 orientations obtained from the interpolant of the original orientional data 
    /// </summary>
    /// <param name="rotationOld">原始旋转状态 the original orientional data </param>
    /// <param name="tao">步长 step length</param>
    /// <returns>插值后的旋转状态 orientations obtained from the interpolant of the original orientional data</returns>
    static public List<Quaternion> GetRotationRoute(List<Quaternion> rotationOld, float tao)
    {
        float[,] rotInterData = GiveBackRot(rotationOld);
        float[,] d1RotInterData = GiveBackD1Rot(rotationOld);
        float[,] d2RotInterData = GiveBackD2Rot(rotationOld);
        float[,] H = TransData(rotInterData, d1RotInterData, d2RotInterData);
        float[,] beta = GetBetaR(d1RotInterData, d2RotInterData, H);
        int l = rotationOld.Count;
        float[,] W = GetW(l);
        int n = H.GetLength(0);
        float[,] rData = new float[n, 4];
        float[,] vData = new float[n, 4];
        float[,] cData = new float[n, 4];
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                rData[i, j] = H[i, j];
                vData[i, j] = H[i, j + 4];
                cData[i, j] = H[i, j + 8];
            }
        }
        float[,] q = G2SrbSpline(rData, vData, cData, W, beta, tao);
        int m = q.GetLength(0);
        List<Quaternion> result = new List<Quaternion>();
        for (int i = 0; i < m; i++)
        {
			if(q[i, 0] == float.NaN && q[i, 1] == float.NaN && q[i, 2] == float.NaN && q[i, 3] == float.NaN && i > 0)
			{
				result.Add(result[i-1]);
			}
			else
			{
	            Quaternion rot = new Quaternion();
	            for (int z = 0; z < 4; z++)
	            {
	                rot[z] = q[i, z];
	            }
	            result.Add(rot);
			}
        }
        return result;
    }
}
