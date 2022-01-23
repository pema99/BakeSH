using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace PSH
{
    [ExecuteAlways]
    public class BakeSH : MonoBehaviour
    {
        [Range(1, 4096)] public int textureSize = 512;
        [Range(1, 8192)] public int samplesPerPixel = 512;
        [Range(0, 1)] public float rayPushOff = 0.02f;
        [Range(0, 10)] public float rayMaxDistance = 1f;
        [Range(0, 1)] public float occlusionStrength = 0.5f;
        public bool hemisphericalSampling = true;
        public bool stitchSeams = true;

        private ComputeShader shader;

        private int kernelProjectOcclusionL1;
        private int kernelNormalizeAndDilate;

        private const int threadGroupSize = 8;
        private const int totalWorkPerInvocation = 1024 * 1024 * 8;

        public void OnEnable()
        {
            shader = Resources.Load<ComputeShader>("BakeSH");

            kernelProjectOcclusionL1 = shader.FindKernel("projectOcclusionL1");
            kernelNormalizeAndDilate = shader.FindKernel("normalizeAndDilate");
        }

        public void BakeOcclusionSHL2(GameObject go)
        {
            if (!go.TryGetMesh(out Mesh mesh)) return;

            // Calc batch sizes
            int totalWork = textureSize * textureSize * samplesPerPixel;
            int invocations = Mathf.CeilToInt(totalWork / (float)totalWorkPerInvocation);
            int sampleCount = Mathf.CeilToInt((float)totalWorkPerInvocation / (textureSize * textureSize));

            // Blit vert pos to texture
            RenderTexture vertPosRt = Utils.GetWritableTempRT(textureSize);
            RenderTexture normalsRt = Utils.GetWritableTempRT(textureSize);
            Utils.RenderMeshVertPos(vertPosRt, mesh);
            Utils.RenderMeshNormals(normalsRt, mesh);

            // Make writeable target RT
            RenderTexture scratchRt1 = Utils.GetWritableTempRT(textureSize);
            RenderTexture scratchRt2 = Utils.GetWritableTempRT(textureSize);

            // Setup kernel properties
            shader.SetInt("sampleCount", sampleCount);
            shader.SetBool("hemisphericalSampling", hemisphericalSampling);
            shader.SetFloat("pushOff", rayPushOff);
            shader.SetFloat("maxDistance", rayMaxDistance);
            shader.SetFloat("strength", occlusionStrength);
            shader.SetMatrix("localToWorld", go.transform.localToWorldMatrix);

            shader.SetTexture(kernelProjectOcclusionL1, "origins", vertPosRt);
            shader.SetTexture(kernelProjectOcclusionL1, "normals", normalsRt);
            shader.SetTexture(kernelProjectOcclusionL1, "target", scratchRt1);

            // Extract scene
            ExtractScene.GetAllSceneGeometry(out Vector3[] verts, out Vector3Int[] indices);

            ComputeBuffer vertsBuf = new ComputeBuffer(verts.Length, sizeof(float) * 3);
            vertsBuf.SetData(verts);
            shader.SetBuffer(kernelProjectOcclusionL1, "verts", vertsBuf);
            shader.SetInt("numVerts", verts.Length);

            ComputeBuffer indicesBuf = new ComputeBuffer(indices.Length, sizeof(float) * 3);
            indicesBuf.SetData(indices);
            shader.SetBuffer(kernelProjectOcclusionL1, "indices", indicesBuf);
            shader.SetInt("numIndices", indices.Length);

            // Dispatch and readback
            for (int i = 0; i < invocations; i++)
            {
                if (EditorUtility.DisplayCancelableProgressBar("Baking", $"Group {i}/{invocations}", i / (float)invocations))
                {
                    vertsBuf.Release();
                    indicesBuf.Release();
                    vertPosRt.Release();
                    normalsRt.Release();
                    scratchRt1.Release();
                    scratchRt2.Release();
                    EditorUtility.ClearProgressBar();
                    return;
                }

                shader.SetInt("batchId", i);

                shader.Dispatch(kernelProjectOcclusionL1, Mathf.CeilToInt(scratchRt1.width / (float)threadGroupSize), Mathf.CeilToInt(scratchRt1.height / (float)threadGroupSize), 1);

                // This line is just here to force GPU <-> CPU sync point
                indicesBuf.GetData(indices);
            }

            // Dispatch final division and dilation
            shader.SetInt("sampleCount", sampleCount * invocations);
            shader.SetBool("dilate", stitchSeams);
            shader.SetTexture(kernelNormalizeAndDilate, "input", scratchRt1);
            shader.SetTexture(kernelNormalizeAndDilate, "target", scratchRt2);
            shader.Dispatch(kernelNormalizeAndDilate, Mathf.CeilToInt(scratchRt1.width / (float)threadGroupSize), Mathf.CeilToInt(scratchRt1.height / (float)threadGroupSize), 1);

            EditorUtility.ClearProgressBar();
            Utils.SaveTexture(scratchRt2);

            // Cleanup
            vertsBuf.Release();
            indicesBuf.Release();
            vertPosRt.Release();
            normalsRt.Release();
            scratchRt1.Release();
            scratchRt2.Release();
        }
    }

    [CustomEditor(typeof(BakeSH))]
    [CanEditMultipleObjects]
    public class BakeSHEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            var tar = target as BakeSH;

            if (GUILayout.Button("Bake SH"))
            {
                System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
                sw.Start();

                tar.BakeOcclusionSHL2(tar.gameObject);

                sw.Stop();
                Debug.Log($"Bake took {sw.ElapsedMilliseconds} ms.");
            }
        }
    }
}