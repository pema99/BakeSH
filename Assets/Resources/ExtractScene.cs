using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace PSH
{
    public class ExtractScene
    {
        public static void GetAllSceneGeometry(out Vector3[] verts, out Vector3Int[] indices, params GameObject[] except)
        {
            List<Vector3> vertsTmp = new List<Vector3>();
            List<Vector3Int> indicesTmp = new List<Vector3Int>();

            foreach (GameObject go in Resources.FindObjectsOfTypeAll(typeof(GameObject)) as GameObject[])
            {
                if (except.Contains(go))
                    continue;

                if (!(!EditorUtility.IsPersistent(go.transform.root.gameObject) && !(go.hideFlags == HideFlags.NotEditable || go.hideFlags == HideFlags.HideAndDontSave) && go.activeInHierarchy))
                    continue;

                if (!go.TryGetComponent<MeshFilter>(out var meshFilter))
                    continue;

                Mesh mesh = meshFilter.sharedMesh;
                Vector3[] meshVerts = mesh.vertices;
                int[] meshIndices = mesh.triangles;

                Vector3Int offset = Vector3Int.one * vertsTmp.Count;
                foreach (var vert in meshVerts)
                {
                    vertsTmp.Add(go.transform.localToWorldMatrix.MultiplyPoint(vert));
                }

                for (int i = 0; i < meshIndices.Length; i += 3)
                {
                    indicesTmp.Add(new Vector3Int(meshIndices[i], meshIndices[i + 1], meshIndices[i + 2]) + offset);
                }
            }

            verts = vertsTmp.ToArray();
            indices = indicesTmp.ToArray();
        }
    }
}