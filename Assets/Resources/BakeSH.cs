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

        private ComputeShader shader;

        private int kernelProjectOcclusionL1;
        private int kernelDivideSamples;

        private const int threadGroupSize = 8;
        private const int totalWorkPerInvocation = 1024 * 1024 * 8;

        public void OnEnable()
        {
            shader = Resources.Load<ComputeShader>("BakeSH");

            kernelProjectOcclusionL1 = shader.FindKernel("projectOcclusionL1");
            kernelDivideSamples = shader.FindKernel("divideSamples");
        }

        public void BakeOcclusionSHL2(GameObject go)
        {
            if (!go.TryGetComponent<MeshFilter>(out var meshFilter)) return;
            var mesh = meshFilter.sharedMesh;

            // Calc batch sizes
            int totalWork = textureSize * textureSize * samplesPerPixel;
            int invocations = Mathf.CeilToInt(totalWork / (float)totalWorkPerInvocation);
            int sampleCount = Mathf.CeilToInt((float)totalWorkPerInvocation / (textureSize * textureSize));

            // Blit vert pos to texture
            var rtDesc = new RenderTextureDescriptor(textureSize, textureSize, RenderTextureFormat.ARGBFloat, 0);
            RenderTexture vertPosRt = RenderTexture.GetTemporary(rtDesc);
            RenderTexture normalsRt = RenderTexture.GetTemporary(rtDesc);
            Utils.RenderMeshVertPos(vertPosRt, mesh);
            Utils.RenderMeshNormals(normalsRt, mesh);

            // Make writeable target RT
            RenderTexture targetRt = new RenderTexture(rtDesc);
            targetRt.enableRandomWrite = true;
            targetRt.useMipMap = false;
            targetRt.Create();

            // Setup kernel properties
            shader.SetInt("sampleCount", sampleCount);
            shader.SetBool("hemisphericalSampling", hemisphericalSampling);
            shader.SetFloat("pushOff", rayPushOff);
            shader.SetFloat("maxDistance", rayMaxDistance);
            shader.SetFloat("strength", occlusionStrength);
            shader.SetMatrix("localToWorld", go.transform.localToWorldMatrix);

            shader.SetTexture(kernelProjectOcclusionL1, "origins", vertPosRt);
            shader.SetTexture(kernelProjectOcclusionL1, "normals", normalsRt);
            shader.SetTexture(kernelProjectOcclusionL1, "target", targetRt);

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
                    RenderTexture.ReleaseTemporary(vertPosRt);
                    RenderTexture.ReleaseTemporary(normalsRt);
                    targetRt.Release();
                    EditorUtility.ClearProgressBar();
                    return;
                }

                shader.SetInt("batchId", i);

                shader.Dispatch(kernelProjectOcclusionL1, Mathf.CeilToInt(targetRt.width / (float)threadGroupSize), Mathf.CeilToInt(targetRt.height / (float)threadGroupSize), 1);

                // This line is just here to force GPU <-> CPU sync point
                indicesBuf.GetData(indices);
            }

            // Dispatch final division
            shader.SetInt("sampleCount", sampleCount * invocations);
            shader.SetTexture(kernelDivideSamples, "target", targetRt);
            shader.Dispatch(kernelDivideSamples, Mathf.CeilToInt(targetRt.width / (float)threadGroupSize), Mathf.CeilToInt(targetRt.height / (float)threadGroupSize), 1);

            EditorUtility.ClearProgressBar();
            Utils.SaveTexture(targetRt);

            // Cleanup
            vertsBuf.Release();
            indicesBuf.Release();
            RenderTexture.ReleaseTemporary(vertPosRt);
            RenderTexture.ReleaseTemporary(normalsRt);
            targetRt.Release();
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