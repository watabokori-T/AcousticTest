using System;
using System.IO;
using System.Text;
using UnityEngine;

/// <summary>WAVE ヘッダ情報構造体</summary>
public struct WaveHeaderArgs
{
    /// <summary>RIFF ヘッダ</summary>
    public string RiffHeader;
    /// <summary>ファイルサイズ</summary>
    public int FileSize;
    /// <summary>WAVE ヘッダ</summary>
    public string WaveHeader;
    /// <summary>フォーマットチャンク</summary>
    public string FormatChunk;
    /// <summary>フォーマットチャンクサイズ</summary>
    public int FormatChunkSize;
    /// <summary>フォーマット ID</summary>
    public short FormatID;
    /// <summary>チャンネル数</summary>
    public short Channel;
    /// <summary>サンプリングレート</summary>
    public int SampleRate;
    /// <summary>1秒あたりのデータ数、サンプリング周波数*ブロックサイズ</summary>
    public int BytePerSec;
    /// <summary>ブロックサイズ、チャンネル数*1サンプルあたりのビット数/8</summary>
    public short BlockSize;
    /// <summary>1サンプルあたりのビット数</summary>
    public short BitPerSample;
    /// <summary>Data チャンク</summary>
    public string DataChunk;
    /// <summary>波形データのバイト数</summary>
    public int DataChunkSize;
    /// <summary>再生時間(msec)</summary>
    public int PlayTimeMsec;
}

public class WavRead
{
    /// <summary>WAVE ヘッダ情報</summary>
    public static WaveHeaderArgs waveHeader = new WaveHeaderArgs();
    /// <summary>WAVE データ配列</summary>
    public static byte[] waveData { get; private set; } = null;
    public static int[] valuesR, valuesL;

    /// <summary>
    /// wav読込
    /// </summary>
    public static bool ReadWave(string waveFilePath)
    {
        // ファイルの存在を確認する
        if (!File.Exists(waveFilePath))
        {
            Debug.Log("ファイルどこじゃ？");
            return false;
        }

        using (FileStream fs = new FileStream(waveFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
        {
            try
            {
                BinaryReader br = new BinaryReader(fs);
                waveHeader.RiffHeader = Encoding.GetEncoding(20127).GetString(br.ReadBytes(4));
                waveHeader.FileSize = BitConverter.ToInt32(br.ReadBytes(4), 0);
                waveHeader.WaveHeader = Encoding.GetEncoding(20127).GetString(br.ReadBytes(4));

                bool readFmtChunk = false;
                bool readDataChunk = false;
                while (!readFmtChunk || !readDataChunk)
                {
                    // ChunkIDを取得する
                    string chunk = Encoding.GetEncoding(20127).GetString(br.ReadBytes(4));
                    if (chunk.ToLower().CompareTo("fmt ") == 0)
                    {
                        // fmtチャンクの読み込み
                        waveHeader.FormatChunk = chunk;
                        waveHeader.FormatChunkSize = BitConverter.ToInt32(br.ReadBytes(4), 0);
                        waveHeader.FormatID = BitConverter.ToInt16(br.ReadBytes(2), 0);
                        waveHeader.Channel = BitConverter.ToInt16(br.ReadBytes(2), 0);
                        waveHeader.SampleRate = BitConverter.ToInt32(br.ReadBytes(4), 0);
                        waveHeader.BytePerSec = BitConverter.ToInt32(br.ReadBytes(4), 0);
                        waveHeader.BlockSize = BitConverter.ToInt16(br.ReadBytes(2), 0);
                        waveHeader.BitPerSample = BitConverter.ToInt16(br.ReadBytes(2), 0);
                        readFmtChunk = true;
                    }

                    else if (chunk.ToLower().CompareTo("data") == 0)

                    {
                        // dataチャンクの読み込み
                        waveHeader.DataChunk = chunk;
                        waveHeader.DataChunkSize = BitConverter.ToInt32(br.ReadBytes(4), 0);

                        // バッファに読み込み
                        waveData = br.ReadBytes(waveHeader.DataChunkSize);

                        // 再生時間を算出する
                        int bytesPerSec = waveHeader.SampleRate * waveHeader.BlockSize;
                        waveHeader.PlayTimeMsec = (int)(((double)waveHeader.DataChunkSize / (double)bytesPerSec) * 1000);

                        convertWaveData();
                        readDataChunk = true;
                    }
                    else
                    {
                        // 不要なチャンクの読み捨て
                        Int32 size = BitConverter.ToInt32(br.ReadBytes(4), 0);
                        if (0 < size)
                        {
                            br.ReadBytes(size);
                        }
                    }
                }
            }
            catch
            {
                fs.Close();
                Debug.Log("読み込めなんだ");
                return false;
            }
        }
        return true;
    }

    public static void convertWaveData()
    {
        try
        {
            // 音声データの取得
            valuesR = new int[(waveHeader.DataChunkSize / waveHeader.Channel) / (waveHeader.BitPerSample / 8)];
            valuesL = new int[(waveHeader.DataChunkSize / waveHeader.Channel) / (waveHeader.BitPerSample / 8)];

            // 1標本分の値を取得
            int frameIndex = 0;
            int chanelIndex = 0;

            for (int i = 0; i < waveHeader.DataChunkSize / (waveHeader.BitPerSample / 8); i++)
            {
                byte[] data = new byte[2];
                int work = 0;

                switch (waveHeader.BitPerSample)
                {
                    case 8:
                        work = (int)waveData[frameIndex];
                        frameIndex += 1;
                        break;
                    case 16:
                        Array.Copy(waveData, frameIndex, data, 0, 2);
                        work = (int)BitConverter.ToInt16(data, 0);
                        frameIndex += 2;
                        break;
                    default:
                        Debug.Log("波形解析できん");
                        break;
                }

                if (waveHeader.Channel == 1)
                {
                    valuesR[i] = work;
                }
                else
                {
                    if (chanelIndex == 0)
                    {
                        chanelIndex = 1;
                        valuesR[i / 2] = work;
                    }
                    else
                    {
                        chanelIndex = 0;
                        valuesL[i / 2] = work;
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.Log(e.ToString());
        }
    }
}
