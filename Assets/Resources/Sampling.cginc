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