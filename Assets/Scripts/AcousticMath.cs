using System;

public class AcousticMath
{
    /// <summary>
    /// 畳み込み計算
    /// </summary>
    /// <param name="sound_signals">4点マイク</param>
    /// <param name="tsp_signal">tsp</param>
    /// <param name="length_bit">4点マイク配列ビット数</param>
    public static double[] Convolution(double[] sound_signals, double[] tsp_signal, int length_bit)
    {
        //サンプル長を求める
        long sampleLength = 1 << length_bit;

        //tspFFT
        double[] tspfftRe = new double[sampleLength];
        double[] tspfftIm = new double[sampleLength];
        double[] outtspfftIm = new double[sampleLength];
        FFT(length_bit, tsp_signal, tspfftIm, out tspfftRe, out outtspfftIm);

        double[] fftRe = new double[sampleLength];
        double[] fftIm = new double[sampleLength];
        double[] outfftIm = new double[sampleLength];
        double[] ifftRe = new double[sampleLength];
        double[] ifftIm = new double[sampleLength];
        double[] outifftIm = new double[sampleLength];
        double[] out_signal = new double[sampleLength];

        //FFT
        FFT(length_bit, sound_signals, fftIm, out fftRe, out outfftIm);

        //畳み込み
        for (int j = 0; j < sampleLength; j++)
        {
            ifftRe[j] = fftRe[j] * tspfftRe[j] - outfftIm[j] * outtspfftIm[j];
            ifftIm[j] = fftRe[j] * outtspfftIm[j] + outfftIm[j] * tspfftRe[j];
        }

        //IFFT
        IFFT(length_bit, ifftRe, ifftIm, out out_signal, out outifftIm);
        
        return out_signal;
    }

    /// <summary>
    /// FFT
    /// </summary>
    /// <param name="bitSize">ビット数</param>
    /// <param name="inputRe">入力(実数)</param>
    /// <param name="inputIm">入力(虚数) -> なければoutputImと一緒でOK</param>
    /// <param name="outputRe">結果(実数)</param>
    /// <param name="outputIm">結果(虚数)</param>
    public static void FFT(int bitSize, double[] inputRe, double[] inputIm, out double[] outputRe, out double[] outputIm)
    {
        int dataSize = 1 << bitSize;
        int[] reverseBitArray = BitScrollArray(dataSize);

        outputRe = new double[dataSize];
        outputIm = new double[dataSize];

        // バタフライ演算のための置き換え
        for (int i = 0; i < dataSize; i++)
        {
            outputRe[i] = inputRe[reverseBitArray[i]];
            outputIm[i] = inputIm[reverseBitArray[i]];
        }

        // バタフライ演算
        for (int stage = 1; stage <= bitSize; stage++)
        {
            int butterflyDistance = 1 << stage;
            int numType = butterflyDistance >> 1;
            int butterflySize = butterflyDistance >> 1;

            double wRe = 1.0;
            double wIm = 0.0;
            double uRe = System.Math.Cos(System.Math.PI / butterflySize);
            double uIm = -System.Math.Sin(System.Math.PI / butterflySize);

            for (int type = 0; type < numType; type++)
            {
                for (int j = type; j < dataSize; j += butterflyDistance)
                {
                    int jp = j + butterflySize;
                    double tempRe = outputRe[jp] * wRe - outputIm[jp] * wIm;
                    double tempIm = outputRe[jp] * wIm + outputIm[jp] * wRe;
                    outputRe[jp] = outputRe[j] - tempRe;
                    outputIm[jp] = outputIm[j] - tempIm;
                    outputRe[j] += tempRe;
                    outputIm[j] += tempIm;
                }
                double tempWRe = wRe * uRe - wIm * uIm;
                double tempWIm = wRe * uIm + wIm * uRe;
                wRe = tempWRe;
                wIm = tempWIm;
            }
        }
    }

    /// <summary>
    /// IFFT
    /// </summary>
    /// <param name="bitSize">ビット数</param>
    /// <param name="inputRe">入力(実数)</param>
    /// <param name="inputIm">入力(虚数) -> なければoutputImと一緒でOK？</param>
    /// <param name="outputRe">結果(実数)</param>
    /// <param name="outputIm">結果(虚数)</param>
    public static void IFFT(int bitSize, double[] inputRe, double[] inputIm, out double[] outputRe, out double[] outputIm)
    {
        int dataSize = 1 << bitSize;
        outputRe = new double[dataSize];
        outputIm = new double[dataSize];

        for (int i = 0; i < dataSize; i++)
        {
            inputIm[i] = -inputIm[i];
        }
        FFT(bitSize, inputRe, inputIm, out outputRe, out outputIm);
        for (int i = 0; i < dataSize; i++)
        {
            outputRe[i] /= (double)dataSize;
            outputIm[i] /= (double)(-dataSize);
        }
    }

    /// <summary>
    /// ビットを左右反転した配列を返す
    /// </summary>
    /// <param name="arraySize"></param>
    /// <returns></returns>
    private static int[] BitScrollArray(int arraySize)
    {
        int[] reBitArray = new int[arraySize];
        int arraySizeHarf = arraySize >> 1;

        reBitArray[0] = 0;
        for (int i = 1; i < arraySize; i <<= 1)
        {
            for (int j = 0; j < i; j++)
                reBitArray[j + i] = reBitArray[j] + arraySizeHarf;
            arraySizeHarf >>= 1;
        }
        return reBitArray;
    }

    /// <summary>
    /// 窓関数を掛ける
    /// </summary>
    public static double[] Windowing(double[] data, string windowFunc)
    {
        int size = data.Length;
        double[] windata = new double[size];

        for (int i = 0; i < size; i++)
        {
            double winValue = 0;
            // 各々の窓関数
			switch (windowFunc)
			{
                case "Hamming":
                    winValue = 0.54 - 0.46 * Math.Cos(2 * Math.PI * i / (size - 1));
                    break;
                case "Hanning":
                    winValue = 0.5 - 0.5 * Math.Cos(2 * Math.PI * i / (size - 1));
                    break;
                case "Blackman":
                    winValue = 0.42 - 0.5 * Math.Cos(2 * Math.PI * i / (size - 1)) + 0.08 * Math.Cos(4 * Math.PI * i / (size - 1));
                    break;
                case "Rectangular":
                    winValue = 1.0;
                    break;
                default:
                    winValue = 1.0;
                    break;
            }
            // 窓関数を掛け算
            windata[i] = data[i] * winValue;
        }
        return windata;
    }
}
