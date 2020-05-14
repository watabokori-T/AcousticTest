using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

public class EditArray
{
    /// <summary>
	/// int->float変換
	/// </summary>
	/// <param name="intArray">int型配列</param>
	/// <returns></returns>
    public static float[] int2float(int[] intArray)
	{
        float[] floatArray = new float[intArray.Length];
        Parallel.For(0, intArray.Length, i =>
        {
            floatArray[i] = (float)intArray[i];
        });
        return floatArray;
    }

    /// <summary>
	/// int->double変換
	/// </summary>
	/// <param name="intArray">int型配列</param>
	/// <returns></returns>
    public static double[] int2double(int[] intArray)
	{
        double[] doubleArray = new double[intArray.Length];
        Parallel.For(0, intArray.Length, i =>
        {
            doubleArray[i] = (double)intArray[i];
        });
        return doubleArray;
    }
    public static double[][] int2double(int[][] intArray)
    {
        double[][] doubleArray = new double[intArray.Length][];
        Parallel.For(0, intArray.Length, i =>
        {
            doubleArray[i] = new double[intArray[i].Length];
            for (int j = 0; j < intArray[i].Length; j++)
            {
                doubleArray[i][j] = (double)intArray[i][j];
            }
        });
        return doubleArray;
    }

    /// <summary>
	/// double->float
	/// </summary>
	/// <param name="doubleArray">double型配列</param>
	/// <returns></returns>
    public static float[] double2float(double[] doubleArray)
    {
        float[] floatArray = new float[doubleArray.Length];
        Parallel.For(0, doubleArray.Length, i =>
        {
            floatArray[i] = (float)doubleArray[i];
        });
        return floatArray;
    }
    public static float[][] double2float(double[][] doubleArray)
    {
        float[][] floatArray = new float[doubleArray.Length][];
        Parallel.For(0, doubleArray.Length, i =>
        {
            floatArray[i] = new float[doubleArray[i].Length];
            for (int j = 0; j < doubleArray[i].Length; j++)
            {
                floatArray[i][j] = (float)doubleArray[i][j];
            }
        });
        return floatArray;
    }

    /// <summary>
	/// double->short
	/// </summary>
	/// <param name="doubleArray">double型配列</param>
	/// <returns></returns>
    public static short[] double2short(double[] doubleArray)
	{
        short[] shortArray = new short[doubleArray.Length];
        normalize(doubleArray, Int16.MaxValue);
        Parallel.For(0, doubleArray.Length, i =>
        {
            shortArray[i] = (short)doubleArray[i];
        });
        return shortArray;
    }

    /// <summary>
	/// 正規化
	/// </summary>
	/// <param name="array">正規化する配列</param>
	/// <param name="max">幅</param>
    public static void normalize(float[] array, int max)
	{
        float mx = array.Max();
        mx = max / mx;
        Parallel.For(0, array.Length, i =>
        {
            array[i] *= mx;
        });
    }
    public static void normalize(double[] array, int max)
    {
        double mx = array.Max();
        mx = max / mx;
        Parallel.For(0, array.Length, i =>
        {
            array[i] *= mx;
        });
    }
    public static void normalize(float[][] array, int max)
	{
        float[] mxArray = new float[array.Length];
        Parallel.For(0, array.Length, i =>
        {
            mxArray[i] = array[i].Max();
        });
        float mx = mxArray.Max();
        mx = max / mx;
        Parallel.For(0, array.Length, i =>
        {
            for (int j = 0; j < array[i].Length; j++)
            {
                array[i][j] *= mx;
            }
        });
    }
}
