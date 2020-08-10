using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PositionChange
{
    /// <summary>
    /// 计算飞行位置轨迹  Compute the trajectory of the positional part of the flight process
    /// </summary>
    /// <param name="posInterData">储存的是需要插值的N个空间向量（表示位置）的N*3矩阵，行向量是插值向量坐标  A N*3 matrix of which the row vectors represent the positional interpolation vectors</param>
    /// <param name="vposInterData">储存的是N个插值空间向量处的单位切向量（表示速度）的N*3矩阵，行向量是单位切向量坐标  A N*3 matrix of which the row vectors represent the unit tangent vectors at the positional interpolation vectors</param>
    /// <param name="cposInterData">储存的是N个插值空间向量处的曲率向量（表示加速度）的N*3矩阵，行向量是曲率向量坐标 A N*3 matrix of which the row vectors represent the curvature vectors at the positional interpolation vectors</param>
    /// <param name="beta2">beta2储存的是G^1Hermite插值的一阶及二阶几何连续形状参数的(N-1)*3矩阵，行向量是每段n次Bezier曲线在t=0和t=1处的一阶及t=0和t=1处的二阶几何连续的形状参数  A (N-1)*4 matrix of which the k-th row vector stores the four shape parameters of the k-th Bezier curve segment of G^2 Hermite interpolation spline at the endpoints</param>
    /// <param name="tao">步长  step length</param>
    /// <returns>飞行位置轨迹点组   trajectory points of the positional part of the flight process </returns>
    static float[,] G2CSpline(float[,] posInterData, float[,] vposInterData, float[,] cposInterData, float[,] beta2, float tao)
    {
        int n = 5; //Bezier曲线的次数是n=5(G^2Hermite插值)  the degree of the Bezier spline for G^2 Hermite interpolation is n=5 
        int N = posInterData.GetLength(0); //N是插值点个数  the number of the positional interpolation vectors is N
        int M = posInterData.GetLength(1); 
        float[,] x = new float[(n + 1) * (N - 1), M]; //x储存的是G^2Hermite插值的n次Bezier样条(由N-1段n次Bezier曲线组成)的全部控制点的(n+1)(N-1)*3矩阵，行向量是控制点坐标  a (n+1)(N-1)*3 matrix of which the ((n+1)*(k-1)+j)-th row vector represents the j-th control point of the k-th Bezier curve segment of G^2 Hermite interpolation spline
        x = PosAllControl2C(posInterData, vposInterData, cposInterData, beta2); //将组成样条的N-1段Bezier曲线的所有控制顶点储存在x中 store all the control points of the Bezier spline in x 
        int T = Mathf.FloorToInt(1 / tao + 1); //需要计算的每段Bezier曲线离散点的总个数是T  the number of the discrete points of every Bezier curve segment is T
        float[,] c = new float[(N - 1) * T, M];
        for (int i = 1; i < N; i++)
        {
            for (float t = 0f; t <= 1; t += tao)
            {
                float[,] control = new float[n + 1, M]; //第i段bezier曲线对应的控制点  the control points of the i-th Bezier curve
                for (int j = 0; j < n + 1; j++)
                {
                    for (int z = 0; z < M; z++)
                    {
                        control[j, z] = x[(n + 1) * (i - 1) + j, z]; // t对应第i段bezier曲线的第k个离散点，取整  t corresponds to the k-th discrete point of the i-th Bezier curve segment
                    }
                }
                int k = Mathf.RoundToInt(t / tao + 1);
                for (int z = 0; z < M; z++)
                {
                    c[(i - 1) * T + k - 1, z] = CBezier(control, t)[z]; //计算第i段bezier曲线的第k个离散点，对应于矩阵C中的第(i-1)*T+k个行向量  compute the k-th discrete point of the i-th Bezier curve segment and store it in the ((i-1)*T+k)-th row of matrix C
                }
            }
        }
        return c;
    }

    /// <summary>
    /// 计算全部控制点坐标  compute all the control points
    /// </summary>
    /// <param name="posInterData">储存的是需要插值的N个空间向量（表示位置）的N*3矩阵，行向量是插值向量坐标  A N*3 matrix of which the row vectors represent the positional interpolation vectors</param>
    /// <param name="vposInterData">储存的是N个插值空间向量处的单位切向量（表示速度）的N*3矩阵，行向量是单位切向量坐标  A N*3 matrix of which the row vectors represent the unit tangent vectors at the positional interpolation vectors</param>
    /// <param name="cposInterData">储存的是N个插值空间向量处的曲率向量（表示加速度）的N*3矩阵，行向量是曲率向量坐标   A N*3 matrix of which the row vectors represent the curvature vectors at the positional interpolation vectors</param>
    /// <param name="beta2">beta2储存的是G^1Hermite插值的一阶及二阶几何连续形状参数的(N-1)*3矩阵，行向量是每段n次Bezier曲线在t=0和t=1处的一阶及t=0和t=1处的二阶几何连续的形状参数 A (N-1)*4 matrix of which the k-th row vector stores the four shape parameters of the k-th Bezier curve segment of G^2 Hermite interpolation spline at the endpoints</param>
    /// <returns>返回值是G^2Hermite插值的n次Bezier样条(由N-1段n次Bezier曲线组成)的全部控制点的(n+1)(N-1)*3矩阵，行向量是控制点坐标  a (n+1)(N-1)*3 matrix of which the ((n+1)*(k-1)+j)-th row vector represents the j-th control point of the k-th Bezier curve segment of G^2 Hermite interpolation spline</returns>
    static float[,] PosAllControl2C(float[,] posInterData, float[,] vposInterData, float[,] cposInterData, float[,] beta2)
    {
        int n = 5; //Bezier曲线的次数是n=5(G^2Hermite插值)  the degree of the Bezier spline for G^2 Hermite interpolation is n=5 
        int N = posInterData.GetLength(0); //插值点的个数是N，样条由N-1段曲线组成  the number of the positional interpolation vectors is N and the spline is composed of (N-1) segments
        int M = posInterData.GetLength(1);
        float[,] x = new float[(n + 1) * (N - 1), M]; //存储返回值  
        for (int i = 1; i < N; i++)
        {
            float[,] p = new float[2,M];
            float[,] v = new float[2,M];
            float[,] c = new float[2,M];
            for (int z = 0; z < M; z++)
            {
                p[0,z] = posInterData[i-1,z];
                p[1,z] = posInterData[i,z];
                v[0,z] = vposInterData[i-1,z];
                v[1,z] = vposInterData[i,z];
                c[0,z] = cposInterData[i-1,z];
                c[1,z] = cposInterData[i,z];
            }
            for (int j = 0; j < n + 1; j++)
            {
                for (int z = 0; z < M; z++)
                {
                    x[(n + 1) * (i - 1) + j , z] = Jscontrol2C(p, v, c, beta2[i - 1, 0], beta2[i - 1, 1], beta2[i - 1, 2], beta2[i - 1, 3])[j, z];
                }
            }
        }
        return x;
    }

    /// <summary>
    /// 计算控制顶点   compute the control points
    /// 根据G2-Hermite插值条件计算n次Bezier曲线控制顶点的函数   the function for computing the control points of the Bezier curve  of degree n according to the G^2 Hermite interpolation conditions
    /// </summary>
    /// <param name="z">z是储存2个插值点p_0,p_1的2*r维矩阵，行向量是插值点坐标 a 2*r matrix of which the row vectors represent the positional interpolation vectors </param>  
    /// <param name="v1">v1是储存2个插值点处单位切向量v_0^1,v_1^1的2*r维矩阵，行向量是切向量坐标 a 2*r matrix of which the row vectors represent the unit tangent vectors at the positional interpolation vectors </param> 
    /// <param name="v2">v2是储存2个插值点处曲率向量v_0^2,v_1^2的2*r维矩，行向量是曲率向量坐标 a 2*r matrix of which the row vectors represent the curvature vectors at the positional interpolation vectors</param>
    /// <param name="beta01">beta01是t=0处一阶几何连续的形状参数 beta01 is the shape parameter of G^1 continuity at t=0</param>
    /// <param name="beta11">beta11是t=1处一阶几何连续的形状参数 beta11 is the shape parameter of G^1 continuity at t=1</param>
    /// <param name="beta02">beta02是t=0处二阶几何连续的形状参数 beta02 is the shape parameter of G^2 continuity at t=0</param>
    /// <param name="beta12">beta12是t=1处二阶几何连续的形状参数 beta12 is the shape parameter of G^2 continuity at t=1</param>
    /// <returns>返回值储存n+1个曲线控制点（b_0,b_1,...,b_n）的(n+1)*r维矩阵，行向量是控制点坐标 a (n+1)*r matrix of which the row vectors represent the control points of the interpolation curve</returns>
    static float[,] Jscontrol2C(float[,] z, float[,] v1, float[,] v2, float beta01,float beta11,float beta02,float beta12)
    {     
        int n = 5; //Bezier曲线的次数是n次,n=5  the degree of the Bezier curve is n=5  
        int r = z.GetLength(1); //r是维度       r is the dimension
        float[,] x = new float[n+1,r]; //存储返回值
        for (int i = 0; i < r; i++)
        {
            x[0, i] = z[0, i]; //根据Bezier曲线的端点插值性，计算首控制点b_0  compute the first control point b_0 by the endpoint interpolation property of the Bezier curve
            x[n, i] = z[1, i]; //根据Bezier曲线的端点插值性，计算末控制点b_n  compute the last control point b_n by the endpoint interpolation property of the Bezier curve
            x[1, i] = x[0, i] + beta01 * v1[0, i] / n; //根据首端点处G^1插值条件求解出的第二个控制点b_1 compute the second control point b_1 by the endpoint G^1 Hermite interpolation conditions
            x[n - 1, i] = x[n, i] - beta11 * v1[1, i] / n; //根据末端点处G^1插值条件求解出的第二个控制点b_{n-1} compute the second-last control point b_{n-1} by the endpoint G^1 Hermite interpolation conditions
            x[2, i] = (beta02 * v1[0, i] + beta01 * beta01 * v2[0, i]) / n / (n - 1) + 2 * x[1, i] - x[0, i]; //根据首端点处G^2插值条件求解出的第三个控制点b_2 compute the third control point b_2 by the endpoint G^2 Hermite interpolation conditions
            x[n - 2, i] = (beta12 * v1[1, i] + beta11 * beta11 * v2[1, i]) / n / (n - 1) + 2 * x[n - 1, i] - x[n, i]; //根据末端点处G^2插值条件求解出的第三个控制点b_{n-2} compute the third-last control point b_{n-2} by the endpoint G^2 Hermite interpolation conditions
        }
        return x;
    }

    /// <summary>
    /// 返回n次Bezier曲线在t时刻的值  the value of the Bezier curve of degree n at t
    /// </summary>
    /// <param name="control">control是储存n次Bezier曲线的控制点的(n+1)*r矩阵，行向量是控制点坐标 a (n+1)*r matrix of which the row vectors represent the control points of the Bezier curve of degree n </param>
    /// <param name="t">t是时间   time </param>
    /// <returns>返回值是n次Bezier曲线在t时刻的值   the value of the Bezier curve of degree n at t </returns>
    static float[] CBezier(float[,] control,float t)
    {
        int n = control.GetLength(0) - 1; //Bezier曲线的次数是n次,n=5  the degree of the Bezier curve is n=5 
        int m = control.GetLength(1); //Bezier曲线所在空间维数是m   m is the dimension
        float[] c = new float[m]; 
        for (int k = 0; k < n + 1; k++)
        {
            for (int i = 0; i < m; i++)
            {
                c[i] = (Nchoosek(n, k) * Mathf.Pow(t, k) * Mathf.Pow(1 - t, n - k)) * control[k, i] + c[i];
            }
        }
        return c;
    }

    /// <summary>
    /// 求组合数C(n,k)   compute the combinatorial number C(n,k)
    /// </summary>
    static int Nchoosek(int n, int k)
    {
        if (k != 0)
        {
            int a = k;
            int b = n;
            for (int i = 1; i < k; i++)
            {
                a = a * (k - i);
                b = b * (n - i);
            }
            return b / a;
        }
        else
        {
            return 1;
        }
    }
    
    /// <summary>
    /// 求3元向量叉积  compute the tensor product of two space vectors
    /// </summary>
    static float[] Cross(float[] a, float[] b)
    {
        float[] c = new float[3];
        c[0] = a[1] * b[2] - a[2] * b[1];
        c[1] = a[2] * b[0] - a[0] * b[2];
        c[2] = a[0] * b[1] - a[1] * b[0];
        return c;
    }

    /// <summary>
    /// 求三元向量的而范数   compute the norm of the space vector
    /// </summary>
    static float Norm(float[] a)
    {
        return Mathf.Sqrt(a[0] * a[0] + a[1] * a[1] + a[2] * a[2]);
    }

    /// <summary>
    /// 三元向量标准化    normalize a vector  
    /// </summary>
    static float[] Normr(float[] a)
    {
        float[] v = new float[3];
        float r = Norm(a);
        v[0] = a[0] / r;
        v[1] = a[1] / r;
        v[2] = a[2] / r;
        return v;
    }

    /// <summary>
    /// 将初始位置数据转化成满足球面条件的插值数据  convert the initial positional data into the interpolation data satisfying spherical conditions
    /// </summary>
    /// <param name="C">C的行向量储存的是插值点信息  the row vectors of matrix C represent the interpolation points </param>
    /// <param name="dC">dC的行向量储存的是插值点处的一阶导信息</param>  the row vectors of matrix C represent the first order derivatives at the interpolation points
    /// <param name="d2C">d2C的行向量储存的是插值点处的二阶导信息</param>  the row vectors of matrix C represent the second order derivatives at the interpolation points
    /// <returns>满足球面条件的插值数据   interpolation data satisfying spherical conditions  </returns>  
    static float[,] TransData(float[,] C, float[,] dC, float[,] d2C)
    {
        int N = C.GetLength(0); //N是矩阵C的行数，即为插值点个数  N is the number of rows of matrix C which is also the number of interpolation points
        int d = C.GetLength(1); //d是矩阵C的列数，即为插值点维数  d is the number of columns of matrix C which is also the dimension of interpolation points
        float[,] H = new float[N, d * 3]; //H储存返回值  H stores the returning values
        for(int j = 0; j < N; j++)
        {
            for (int i = 0; i < d; i++)
            {
                H[j, i] = C[j, i];
            }
        }
        //求球面插值点处的单位切向量   compute the unit tangent vectors at the spherical interpolation points
        for (int j = 0; j < N; j++)
        {
            float[] n = new float[d];
            for (int z = 0; z < d; z++)
            {
                n[z] = dC[j, z];
            }
            for (int i = 0; i < d; i++)
            {
                H[j, d + i] = Normr(n)[i];
            }
        }
        //求球面插值点处的曲率向量  compute the curvature vectors at the spherical interpolation points
        for (int j = 0; j < N; j++)
        {
            float[] a = new float[d];
            float[] n1 = new float[d];
            float[] n2 = new float[d];
            for (int z = 0; z < d; z++)
            {
                a[z] = H[j, d + z];
                n1[z] = dC[j, z];
                n2[z] = d2C[j, z];
            }
            float[] r = new float[d];
            r = Normr(Cross(n1, n2));
            float k;
            k = Norm(Cross(n1, n2)) / Mathf.Pow(Norm(n1),3);
            float[] b = new float[d];
            b = Cross(r, a);
            for(int z = 0; z < d; z++)
            {
                H[j, 2 * d + z] = k * b[z];
            }
        }
        return H;
    }

    /// <summary>
    /// 获取实际使用位置数组   obtain the practical positional data 
    /// </summary>
    /// <param name="posIniData">原始输入点位置链表   the links of the original given data </param>
    /// <returns>去除开始两点与末尾两点的n-4组坐标值  n-4 sets of coordinates without the first and the last two points</returns>
    static float[,] GiveBackPos(List<Vector3> posIniData)
    {
        int lengh = posIniData.Count;
        float[,] posInterData = new float[lengh - 4, 3];
        for (int i = 0; i < lengh - 4; i++)
        {
            posInterData[i, 0] = posIniData[i + 2].x;
            posInterData[i, 1] = posIniData[i + 2].y;
            posInterData[i, 2] = posIniData[i + 2].z;
        }
        return posInterData;
    }

    /// <summary>
    /// 利用Bert的方法，计算位置插值数据处的一阶导向量 compute the first order derivatives at the positional interpolation points by using the Bert's method
    /// </summary>
    /// <param name="posIniData">原始输入点位置链表  the link of the original given data </param>
    /// <returns>去除开始两点与末尾两点的n-4组三维速度值 n-4 sets of velocity vectors without the first and the last two points </returns>
    static float[,] GiveBackV(List<Vector3> posIniData)
    {
        int lengh = posIniData.Count;
        float[,] vposInterData = new float[lengh - 4, 3];
        for (int i = 0; i < lengh - 4; i++)
        {
            vposInterData[i, 0] = (posIniData[i + 3].x - posIniData[i + 1].x) / 2;
            vposInterData[i, 1] = (posIniData[i + 3].y - posIniData[i + 1].y) / 2;
            vposInterData[i, 2] = (posIniData[i + 3].z - posIniData[i + 1].z) / 2;
        }
        return vposInterData;
    }

    /// <summary>
    /// 利用Bert的方法，计算位置插值数据处的二阶导向量 compute the second order derivatives at the positional interpolation points by using the Bert's method
    /// </summary>
    /// <param name="posIniData">原始输入点位置链表 the link of the original given data</param>
    /// <returns>去除开始两点与末尾两点的n-4组三维加速度值 n-4 sets of acceleration vectors without the first and the last two points</returns>
    static float[,] GiveBackA(List<Vector3> posIniData)
    {
        int lengh = posIniData.Count;
        float[,] aposInterData = new float[lengh - 4, 3];
        for (int i = 0; i < lengh - 4; i++)
        {
            aposInterData[i, 0] = (posIniData[i + 4].x - 2 * posIniData[i + 2].x + posIniData[i].x) / 4;
            aposInterData[i, 1] = (posIniData[i + 4].y - 2 * posIniData[i + 2].y + posIniData[i].y) / 4;
            aposInterData[i, 2] = (posIniData[i + 4].z - 2 * posIniData[i + 2].z + posIniData[i].z) / 4;
        }
        return aposInterData;
    }

    /// <summary>
    /// 形状参数的选取  choose the shape parameters
    /// </summary>
    static float[,] GetBetaC(float[,] vposInterData, float[,] aposInterData,float[,] p)
    {
        int N = vposInterData.GetLength(0);
        float[,] betaC = new float[N - 1, 4];
        //对应位置曲线      corresponding to positional curves
        for (int i = 0; i < N - 1; i++)
        {
            float[] n11 = new float[3]; 
            float[] n21 = new float[3];
            float[] n12 = new float[3];
            float[] n22 = new float[3];
            float[] n31 = new float[3];
            float[] n32 = new float[3]; 
            for(int z = 0; z < 3; z++)
            {
                n11[z] = vposInterData[i,z];
                n21[z] = aposInterData[i,z];
                n12[z] = vposInterData[i + 1, z];
                n22[z] = aposInterData[i + 1, z];
            }
            betaC[i, 0] = Norm(n11);
            betaC[i, 1] = Norm(n12);
            for (int z = 0; z < 3; z++)
            {
                n31[z] = n21[z] - betaC[i, 0] * betaC[i, 0] * p[i, 6 + z];
                n32[z] = n22[z] - betaC[i, 1] * betaC[i, 1] * p[i + 1, 6 + z];
            }
            betaC[i, 2] = Norm(n31);
            betaC[i, 3] = Norm(n32);
        }
        return betaC;
    }

    /// <summary>
    /// 根据原始位置返回插值后的路线点  trajectory points obtained from the interpolant of the original positional data 
    /// </summary>
    /// <param name="positionOld">原始位置点 original positional points </param>
    /// <param name="tao">步长 step length</param>
    /// <returns>插值后的路线点  trajectory points obtained from the interpolant of the original positional data </returns>
    static public List<Vector3> GetPositionRoute(List<Vector3> positionOld,float tao)
    {
        float[,] posInterData = GiveBackPos(positionOld);
        float[,] vposInterData = GiveBackV(positionOld);
        float[,] aposInterData = GiveBackA(positionOld);
        float[,] p = TransData(posInterData, vposInterData, aposInterData);
        float[,] beta = GetBetaC(vposInterData, aposInterData,p);
        int n = p.GetLength(0);
        float[,] pData = new float[n, 3];
        float[,] vData = new float[n, 3];
        float[,] aData = new float[n, 3];
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                pData[i, j] = p[i, j];
                vData[i, j] = p[i, j + 3];
                aData[i, j] = p[i, j + 6];
            }
        }
        float[,] c = G2CSpline(pData, vData, aData, beta, tao);
        int m = c.GetLength(0);
        List<Vector3> result = new List<Vector3>();
        for (int i = 0; i < m; i++)
        {
			if(c[i, 0] == float.NaN && c[i, 1] == float.NaN && c[i, 2] == float.NaN && i > 0)
			{
				if(i > 0)
					result.Add(result[i - 1]);
			}
			else
			{
	            Vector3 v = new Vector3();
	            v.Set(c[i, 0], c[i, 1], c[i, 2]);
	            result.Add(v);
			}
        }
        return result;
    }
}
