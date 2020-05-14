using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using System;
using System.IO;
using System.Threading.Tasks;

public class Cubes : MonoBehaviour
{
    [SerializeField]
    string wav_name = "/WavFiles/impulseResponce.wav";

    [SerializeField]
    int width = 1024;

    [SerializeField]
    int shift = 8;

    //音関連
    public static float[][] soundSignal;
    public static double[][] fftSignal;

    void Start()
    {
        StartFFT();
        CreateCubes();
        /*
        Parallel.Invoke(
            () => CreateCubes(),
            () => StartFFT()
        );
        */
    }

    void CreateCubes()
    {
        var manager = World.Active.EntityManager;
        // Entity が持つ Components を設計（Prefabとして）
        var archetype = manager.CreateArchetype(
            ComponentType.ReadOnly<Prefab>(),
            ComponentType.ReadWrite<LocalToWorld>(),
            ComponentType.ReadWrite<Translation>(),
            ComponentType.ReadOnly<RenderMesh>());
        // 上記の Components を持つ Entity を作成
        var prefab = manager.CreateEntity(archetype);
        // Entity の Component の値をセット（位置）
        manager.SetComponentData(prefab, new Translation() { Value = new float3(0, 1, 0) });

        // キューブオブジェクトの作成
        var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);

        // Entity の Component の値をセット（描画メッシュ）
        manager.SetSharedComponentData(prefab, new RenderMesh()
        {
            mesh = cube.GetComponent<MeshFilter>().sharedMesh,
            material = cube.GetComponent<MeshRenderer>().sharedMaterial,
            subMesh = 0,
            castShadows = UnityEngine.Rendering.ShadowCastingMode.Off,
            receiveShadows = false
        });

        // キューブオブジェクトの削除
        Destroy(cube);

        const int SIDE = 100;
        using (NativeArray<Entity> entities = new NativeArray<Entity>(SIDE * SIDE, Allocator.Temp, NativeArrayOptions.UninitializedMemory))
        {
            // Prefab Entity をベースに 10000 個の Entity を作成
            manager.Instantiate(prefab, entities);
            // 平面に敷き詰めるように Translation を初期化
            for (int x = 0; x < SIDE; x++)
            {
                for (int z = 0; z < SIDE; z++)
                {
                    int index = x + z * SIDE;
                    manager.SetComponentData(entities[index], new Translation
                    {
                        Value = new float3(x, 0, z)
                    });
                }
            }
        }
    }

    public void StartFFT()
    {
        //wav読込
        string waveFilePath1 = Application.dataPath + wav_name;
        Debug.Log(waveFilePath1);
        if (!File.Exists(waveFilePath1))
        {
            Debug.Log("ファイルどこじゃ？");
        }
        WavRead.ReadWave(waveFilePath1);

        //double変換、二次元配列化
        //width長のがlength-width個
        //重くなるからとりまwidth長がshiftずらし
        fftSignal = new double[WavRead.valuesR.Length / shift][];
        Parallel.For(0, fftSignal.Length, i =>
        {
            fftSignal[i] = new double[width];
            for (int j = 0; j < fftSignal[i].Length; j++)
            {
                fftSignal[i][j] = (double)WavRead.valuesR[i + j * shift];
            }
        });

        //窓
        Parallel.For(0, fftSignal.Length, i =>
        {
            fftSignal[i] = AcousticMath.Windowing(fftSignal[i], "Hamming");
        });

        //fft
        double length_bit_do = Math.Log(width, 2);
        int length_bit = (int)length_bit_do;

        double[][] fftRe = new double[fftSignal.Length][];
        double[][] fftIm = new double[fftSignal.Length][];
        double[][] outfftIm = new double[fftSignal.Length][];
        double[][] outfft = new double[fftSignal.Length][];
        Parallel.For(0, fftSignal.Length, i =>
        {
            fftRe[i] = new double[width];
            fftIm[i] = new double[width];
            outfftIm[i] = new double[width];
            outfft[i] = new double[width / 2];
            AcousticMath.FFT(length_bit, fftSignal[i], fftIm[i], out fftRe[i], out outfftIm[i]);

            for (int j = 0; j < fftSignal[i].Length / 2; j++)
            {
                outfft[i][j] = Math.Sqrt(fftRe[i][j] * fftRe[i][j] + outfftIm[i][j] * outfftIm[i][j]);
            }
        });

        //float変換
        soundSignal = EditArray.double2float(outfft);
        
        //画面に収まるよに正規化
        EditArray.normalize(soundSignal, 50);
    }
}