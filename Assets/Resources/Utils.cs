#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace PSH
{
    public static class Utils
    {
        private static Material blitMat;

        public static void RenderMeshFlat(RenderTexture rt, Mesh mesh, int pass)
        {
            if (blitMat == null)
                blitMat = new Material(Shader.Find("Hidden/BlitVertPos"));

            Matrix4x4 flip = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(1, 1, -1));
            Matrix4x4 lookat = Matrix4x4.LookAt(Vector3.zero, Vector3.forward, Vector3.up).inverse;

            var cmd = new CommandBuffer();
            cmd.SetViewProjectionMatrices(Matrix4x4.identity, flip * lookat);
            cmd.SetRenderTarget(rt);
            cmd.DrawMesh(mesh, Matrix4x4.identity, blitMat, 0, pass);

            Graphics.ExecuteCommandBuffer(cmd);
            cmd.Dispose();
        }

        public static void RenderMeshVertPos(RenderTexture rt, Mesh mesh)
        {
            RenderMeshFlat(rt, mesh, 0);
        }

        public static void RenderMeshNormals(RenderTexture rt, Mesh mesh)
        {
            RenderMeshFlat(rt, mesh, 1);
        }

        public static void SaveTexture(RenderTexture rt)
        {
            byte[] bytes = ToTexture2D(rt).EncodeToEXR();
            string path = $"{Application.dataPath}/bake.exr";
            System.IO.File.WriteAllBytes(path, bytes);
            AssetDatabase.Refresh();
        }
        private static Texture2D ToTexture2D(RenderTexture rTex)
        {
            Texture2D tex = new Texture2D(rTex.width, rTex.height, TextureFormat.RGBAFloat, false);
            RenderTexture.active = rTex;
            tex.ReadPixels(new Rect(0, 0, rTex.width, rTex.height), 0, 0);
            tex.Apply();
            RenderTexture.active = null;
            return tex;
        }

        public static RenderTexture GetWritableTempRT(int textureSize)
        {
            var rtDesc = new RenderTextureDescriptor(textureSize, textureSize, RenderTextureFormat.ARGBFloat, 0);
            RenderTexture rt = new RenderTexture(rtDesc);
            rt.enableRandomWrite = true;
            rt.useMipMap = false;
            rt.Create();
            return rt;
        }

        public static bool TryGetMesh(this GameObject go, out Mesh mesh)
        {
            mesh = null;

            if (go.TryGetComponent<MeshFilter>(out var meshFilter))
            {
                mesh = meshFilter.sharedMesh;
                return true;
            }

            if (go.TryGetComponent<SkinnedMeshRenderer>(out var skinnedMesh))
            {
                mesh = new Mesh();
                skinnedMesh.BakeMesh(mesh);
                return true;
            }

            return false;
        }

        public static int Idx3DTo1D(int xMax, int yMax, int x, int y, int z)
        {
            return (z * xMax * yMax) + (y * xMax) + x;
        }

        public static Vector3Int Idx1DTo3D(int xMax, int yMax, int idx)
        {
            int z = idx / (xMax * yMax);
            idx -= (z * xMax * yMax);
            int y = idx / xMax;
            int x = idx % xMax;
            return new Vector3Int(x, y, z);
        }

        public static Vector3 FloorVec(Vector3 vec)
        {
            return new Vector3(
                Mathf.Floor(vec.x),
                Mathf.Floor(vec.y),
                Mathf.Floor(vec.z));
        }

        public static Vector3 CeilVec(Vector3 vec)
        {
            return new Vector3(
                Mathf.Ceil(vec.x),
                Mathf.Ceil(vec.y),
                Mathf.Ceil(vec.z));
        }
    }
}
#endif