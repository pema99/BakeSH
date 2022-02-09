#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PSH
{
    public class AccelerationStructure
    {
        private Vector3 aabbMin;
        private Vector3 aabbMax;
        private float cellSize;
        private Vector3 gridSize;
        private List<int>[] grid;

        public void Build(float gridLambda, Vector3[] verts, Vector3Int[] indices)
        {
            // Calculate AABB of all triangles
            aabbMin = verts[0];
            aabbMax = verts[0];
            foreach (var vert in verts)
            {
                aabbMin = Vector3.Min(aabbMin, vert);
                aabbMax = Vector3.Max(aabbMax, vert);
            }

            // Get grid and cell size via cleary's approximation
            float gridTerm = Mathf.Pow((gridLambda * indices.Length) / ((aabbMax.x - aabbMin.x) * (aabbMax.y - aabbMin.y) * (aabbMax.z - aabbMin.z)), 1.0f / 3.0f);
            gridSize = new Vector3(Mathf.Floor((aabbMax.x - aabbMin.x) * gridTerm), Mathf.Floor((aabbMax.y - aabbMin.y) * gridTerm), Mathf.Floor((aabbMax.z - aabbMin.z) * gridTerm));
            cellSize = (aabbMax.x - aabbMin.x) / gridSize.x;

            grid = new List<int>[(int)(gridSize.x * gridSize.y * gridSize.z)];
            for (int i = 0; i < grid.Length; i++)
            {
                grid[i] = new List<int>();
            }

            // Put tris into grid
            for (int i = 0; i < indices.Length; i++)
            {
                Vector3Int tri = indices[i];
                Vector3 a = verts[tri.x];
                Vector3 b = verts[tri.y];
                Vector3 c = verts[tri.z];
                Vector3 triMin = Vector3.Min(a, Vector3.Min(b, c)) - aabbMin;
                Vector3 triMax = Vector3.Max(a, Vector3.Max(b, c)) - aabbMin;
                triMin = Utils.FloorVec(triMin / cellSize);
                triMax = Utils.CeilVec(triMax / cellSize);

                // TODO: Should be <= ?
                for (int x = (int)triMin.x; x < (int)triMax.x; x++)
                {
                    for (int y = (int)triMin.y; y < (int)triMax.y; y++)
                    {
                        for (int z = (int)triMin.z; z < (int)triMax.z; z++)
                        {
                            if (x >= gridSize.x || y >= gridSize.y || z >= gridSize.z) continue;
                            grid[Utils.Idx3DTo1D((int)gridSize.x, (int)gridSize.y, x, y, z)].Add(i);
                        }
                    }
                }
            }
        }

        public ComputeBuffer Bind(ComputeShader shader, int kernelIndex)
        {
            int maxTriCount = grid.Max(x => x.Count);
            int cellCount = (int)(gridSize.x * gridSize.y * gridSize.z);
            ComputeBuffer buffer = new ComputeBuffer(cellCount * maxTriCount, sizeof(int));

            int[] arr = new int[cellCount * maxTriCount];
            for (int i = 0; i < cellCount; i++)
            {
                for (int j = 0; j < maxTriCount; j++)
                {
                    if (j < grid[i].Count)
                        arr[i * maxTriCount + j] = grid[i][j];
                    else
                        arr[i * maxTriCount + j] = -1;
                }
            }

            buffer.SetData(arr);

            shader.SetBuffer(kernelIndex, "grid", buffer);
            shader.SetInt("gridStride", maxTriCount);
            shader.SetVector("gridMin", aabbMin);
            shader.SetVector("gridMax", aabbMax);
            shader.SetVector("gridSize", gridSize);
            shader.SetFloat("cellSize", cellSize);
            shader.SetInt("cellCount", cellCount);
            
            return buffer;
        }

        public void Draw()
        {
            if (grid == null)
                return;

            for (int x = 0; x < gridSize.x; x++)
            {
                for (int y = 0; y < gridSize.y; y++)
                {
                    for (int z = 0; z < gridSize.z; z++)
                    {
                        var len = grid[Utils.Idx3DTo1D((int)gridSize.x, (int)gridSize.y, x, y, z)].Count;
                        if (len == 0) continue;

                        Gizmos.color = Color.Lerp(Color.green, Color.red, (len / 40f));
                        Gizmos.DrawWireCube(aabbMin + cellSize * new Vector3(x, y, z) + cellSize * Vector3.one * 0.5f, Vector3.one * cellSize);
                    }
                }
            }
        }
    }
}
#endif