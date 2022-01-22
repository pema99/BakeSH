RWStructuredBuffer<float3> verts;
RWStructuredBuffer<int3> indices;
int numVerts;
int numIndices;
float pushOff;

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
    const float maxDist = 1000000;
    minT = maxDist;
    for (uint j = 0; j < uint(numIndices); j++)
    {
        int3 tri = indices[j];
        float3 a = verts[tri.x];
        float3 b = verts[tri.y];
        float3 c = verts[tri.z];
        float t;
        if (mullerTrumbore(ro, rd, a, b, c, t))
        {
            if (t > pushOff)
            {
                minT = min(minT, t);
            }
        }
    }
    return minT < maxDist;
}