using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[ExecuteAlways]
public class BakeSH : MonoBehaviour
{
    public Cubemap cubemap;
    public ComputeShader shader;
    public Material viz;

    private int kernelProjectL0;
    private int kernelProjectL1;
    private int kernelProjectL2;

    private const int threadGroupSize1D = 64;
    private const int threadGroupSize2D = 8;

    public struct SHL0
    {
        public Vector3 l0; // l0r, l0g, l0b
        public override string ToString() => $"{l0}";
    }

    public struct SHL1
    {
        public Vector4 l1r; // l10r, l11r, l12r, l0r
        public Vector4 l1g; // l10g, l11g, l12g, l0g
        public Vector4 l1b; // l10b, l11b, l12b, l0b
        public override string ToString() => $"{l1r}, {l1g}, {l1b}";
    };

    struct SHL2
    {
        public Vector4 l1r; // l10r, l11r, l12r, l0r
        public Vector4 l1g; // l10g, l11g, l12g, l0g
        public Vector4 l1b; // l10b, l11b, l12b, l0b
        public Vector4 l2r; // l20r, l21r, l22r, l23r
        public Vector4 l2g; // l20g, l21g, l22g, l23g
        public Vector4 l2b; // l20b, l21b, l22b, l23b
        public Vector4 l2c; // l24r, l24g, l24b, nil
        public override string ToString() => $"{l1r}, {l1g}, {l1b}, {l2r}, {l2g}, {l2b}, {l2c}";
    };

    public void BakeSHL0(int sampleCount = 1024)
    {
        kernelProjectL0 = shader.FindKernel("projectL0");
        kernelProjectL1 = shader.FindKernel("projectL1");
        kernelProjectL2 = shader.FindKernel("projectL2");

        int shCount = 1;

        shader.SetInt("sampleCount", sampleCount);
        shader.SetTexture(kernelProjectL0, "cubemap", cubemap);

        ComputeBuffer accumulator = new ComputeBuffer(shCount, sizeof(float) * 3);
        var arr = new SHL0[accumulator.count];
        accumulator.SetData(arr);
        shader.SetBuffer(kernelProjectL0, "accumulator", accumulator);

        shader.Dispatch(kernelProjectL0, Mathf.CeilToInt(shCount / (float)threadGroupSize1D), 1, 1);

        accumulator.GetData(arr);

        viz.SetVector("_L1r", new Vector4(0, 0, 0, arr[0].l0.x));
        viz.SetVector("_L1g", new Vector4(0, 0, 0, arr[0].l0.y));
        viz.SetVector("_L1b", new Vector4(0, 0, 0, arr[0].l0.z));

        accumulator.Release();
    }

    public void BakeSHL1(int sampleCount = 1024)
    {
        kernelProjectL0 = shader.FindKernel("projectL0");
        kernelProjectL1 = shader.FindKernel("projectL1");
        kernelProjectL2 = shader.FindKernel("projectL2");

        int shCount = 1;

        shader.SetInt("sampleCount", sampleCount);
        shader.SetTexture(kernelProjectL1, "cubemap", cubemap);

        ComputeBuffer accumulator = new ComputeBuffer(shCount, sizeof(float) * 12);
        var arr = new SHL1[accumulator.count];
        accumulator.SetData(arr);
        shader.SetBuffer(kernelProjectL1, "accumulator", accumulator);

        shader.Dispatch(kernelProjectL1, Mathf.CeilToInt(shCount / (float)threadGroupSize1D), 1, 1);

        accumulator.GetData(arr);

        viz.SetVector("_L1r", arr[0].l1r);
        viz.SetVector("_L1g", arr[0].l1g);
        viz.SetVector("_L1b", arr[0].l1b);

        accumulator.Release();
    }

    public void BakeSHL2(int sampleCount = 1024)
    {
        kernelProjectL0 = shader.FindKernel("projectL0");
        kernelProjectL1 = shader.FindKernel("projectL1");
        kernelProjectL2 = shader.FindKernel("projectL2");

        int shCount = 1;
        
        shader.SetInt("sampleCount", sampleCount);
        shader.SetTexture(kernelProjectL2, "cubemap", cubemap);

        ComputeBuffer accumulator = new ComputeBuffer(shCount, sizeof(float) * 28);
        var arr = new SHL2[accumulator.count];
        accumulator.SetData(arr);
        shader.SetBuffer(kernelProjectL2, "accumulator", accumulator);

        shader.Dispatch(kernelProjectL2, Mathf.CeilToInt(shCount / (float)threadGroupSize1D), 1, 1);

        accumulator.GetData(arr);

        viz.SetVector("_L1r", arr[0].l1r);
        viz.SetVector("_L1g", arr[0].l1g);
        viz.SetVector("_L1b", arr[0].l1b);
        viz.SetVector("_L2r", arr[0].l2r);
        viz.SetVector("_L2g", arr[0].l2g);
        viz.SetVector("_L2b", arr[0].l2b);
        viz.SetVector("_L2c", arr[0].l2c);

        accumulator.Release();
    }
}

[CustomEditor(typeof(BakeSH))]
[CanEditMultipleObjects]
public class BakeSHEditor : Editor
{
    private int shBand = 2;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

        var tar = target as BakeSH;
        string[] options = new string[]
        {
            "L0 (1 term per color channel)",
            "L1 (4 terms per color channel)",
            "L2 (9 terms per color channel)",
        };

        shBand = EditorGUILayout.Popup("SH Band", shBand, options);
        
        if (GUILayout.Button("Bake SH"))
        {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            switch (shBand)
            {
                case 0: tar.BakeSHL0(); break;
                case 1: tar.BakeSHL1(); break;
                case 2: tar.BakeSHL2(); break;
            }
            tar.viz.SetInt("_SHBand", shBand);

            sw.Stop();
            Debug.Log($"Bake took {sw.ElapsedMilliseconds} ms");
        }
    }
}