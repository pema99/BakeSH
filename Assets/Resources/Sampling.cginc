// --- Sampling ---
// https://www.reedbeta.com/blog/hash-functions-for-gpu-rendering/
uint pcgHash(uint input)
{
    uint state = input * 747796405u + 2891336453u;
    uint word = ((state >> ((state >> 28u) + 4u)) ^ state) * 277803737u;
    return (word >> 22u) ^ word;
}

float uniformSample(uint seed)
{
    uint hash = pcgHash(seed);
    return (float)hash / 4294967295.0;
}

// uniform square sample -> uniform sphere sample
float3 squareToSphere(float2 inputCoords)
{
    float theta = TWO_PI * inputCoords.x;
    float sinTheta, cosTheta;
    sincos(theta, sinTheta, cosTheta);

    float cosPhi = 1.0 - 2.0 * inputCoords.y;
    float sinPhi = sqrt(max(0.0, 1.0 - cosPhi * cosPhi));

    return float3(sinPhi * cosTheta, sinPhi * sinTheta, cosPhi);
}

// uniform square sample -> uniform hemisphere sample
float3 squareToHemisphere(float3 normal, float2 inputCoords)
{
    float3 dir = squareToSphere(inputCoords);
    if (dot(dir, normal) < 0)
        dir = -dir;
    return dir;
}

// uniform square sample -> cosine weighted hemisphere sample
float3 squareToCosineHemisphere(float3 normal, float2 inputCoords)
{
    // Get tangent space transformation
    float3 nt, nb;
    if (abs(normal.x) > abs(normal.y))
        nt = normalize(float3(normal.z, 0, -normal.x));
    else
        nt = normalize(float3(0, -normal.z, normal.y));
    nb = cross(normal, nt);
    float3x3 tbn = transpose(float3x3(nb, normal, nt));

    // Get cosine sample pointing up
    float theta = acos(sqrt(inputCoords.x));
    float phi = 2.0 * PI * inputCoords.y;
    float3 dir = float3(sin(theta) * cos(phi), cos(theta), sin(theta) * sin(phi));

    // Move into tangent space
    return normalize(mul(tbn, dir));
}