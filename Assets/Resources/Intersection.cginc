// Triangle data
RWStructuredBuffer<float3> verts;
RWStructuredBuffer<int3> indices;
int numVerts;
int numIndices;

// Properties
float pushOff;

// Acceleration structure
RWStructuredBuffer<int> grid;
int gridStride;
float3 gridMin;
float3 gridMax;
float3 gridSize;
float cellSize;
int cellCount;

// Adapted from raytri.c
bool mullerTrumbore(float3 ro, float3 rd, float3 a, float3 b, float3 c, inout float outT)
{
    outT = 0;

    float3 edge1 = b - a;
    float3 edge2 = c - a;

    // begin calculating determinant - also used to calculate U parameter
    float3 pv = cross(rd, edge2);

    // if determinant is near zero, ray lies in plane of triangle
    float det = dot(edge1, pv);

    if (abs(det) < 1e-6f)
        return false;

    float inv_det = 1.0f / det;

    // calculate distance from vert0 to ray origin
    float3 tv = ro - a;

    // calculate U parameter and test bounds
    float u = dot(tv, pv) * inv_det;
    if (u < 0.0f || u > 1.0f)
        return false;

    // prepare to test V parameter
    float3 qv = cross(tv, edge1);

    // calculate V parameter and test bounds
    float v = dot(rd, qv) * inv_det;
    if (v < 0.0f || u + v > 1.0f)
        return false;

    float t = dot(edge2, qv) * inv_det;
    if (t < 0.0f)
        return false;
    outT = t;

    return true;
}

bool trace(float3 ro, float3 rd, out float minT)
{
    const uint axisMap[8] = { 2, 1, 2, 1, 2, 2, 0, 0 };
    const float maxDistance = 1000000;
    minT = maxDistance;

    float3 start = ro - gridMin;
    float3 cell = floor(start / cellSize - 0.000000001);
    float3 step = sign(rd);
    float3 stepDelta = 0;
    float3 nextIntersection = 0;
    float3 exit = 0;
    [unroll(3)] for (uint i = 0; i < 3; i++)
    {
        if (rd[i] < 0)
        {
            stepDelta[i] = -cellSize / rd[i];
            nextIntersection[i] = (cell[i] * cellSize - start[i]) / rd[i];
            exit[i] = -1;
        }
        else
        {
            stepDelta[i] = cellSize / rd[i];
            nextIntersection[i] = ((cell[i] + 1) * cellSize - start[i]) / rd[i];
            exit[i] = gridSize[i];
        }
    }

    uint totalSteps = abs(dot(1, gridSize));
    for (i = 0; i < totalSteps; i++)
    {
        float minDistance = maxDistance;
        uint gridIdx = idx3DTo1D(gridSize.x, gridSize.y, cell.x, cell.y, cell.z);
        gridIdx *= gridStride;

        for (uint tri = 0; tri < uint(gridStride); tri++)
        {
            uint triIdx = grid[gridIdx + tri];
            [branch] if (triIdx >= 0 && triIdx < uint(numIndices))
            {
                int3 tri = indices[triIdx];
                float3 a = verts[tri.x];
                float3 b = verts[tri.y];
                float3 c = verts[tri.z];
                float t;
                [branch] if (mullerTrumbore(ro, rd, a, b, c, t))
                {
                    [branch] if (t > pushOff)
                    {
                        minT = min(minT, t);
                    }
                }
            }
        }
        [branch] if (minDistance < maxDistance)
        {
            return true;
        }

        uint bitA = (nextIntersection.x < nextIntersection.y) << 2;
        uint bitB = (nextIntersection.x < nextIntersection.z) << 1;
        uint bitC = (nextIntersection.y < nextIntersection.z);
        uint axis = axisMap[bitA + bitB + bitC];
        [forcecase] switch (axis)
        {
            case 0: 
                cell.x += step.x;
                [branch] if (cell.x == exit.x) return false;
                nextIntersection.x += stepDelta.x;
                break;
            case 1:
                cell.y += step.y;
                [branch] if (cell.y == exit.y) return false;
                nextIntersection.y += stepDelta.y;
                break;
            case 2:
                cell.z += step.z;
                [branch] if (cell.z == exit.z) return false;
                nextIntersection.z += stepDelta.z;
                break;
        }
    }

    return false;
}