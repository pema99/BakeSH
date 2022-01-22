// --- SH stuff ---
// 1/2 * sqrt(1/π)
#define L0_NORMALIZATION 0.2820947917738781434740397257803862929220253146644994284220428608

// 1/2 * sqrt(3/π)
#define L1_NORMALIZATION 0.4886025119029199215863846228383470045758856081942277021382431574

// 1/2 * sqrt(15/π)
#define L2_NORMALIZATION_LEFT 1.0925484305920790705433857058026884026904329595042589753478516999

// 1/4 * sqrt(5/π)
#define L2_NORMALIZATION_ZONAL 0.3153915652525200060308936902957104933242475070484115878434078878

// 1/4 * sqrt(15/π)
#define L2_NORMALIZATION_RIGHT 0.5462742152960395352716928529013442013452164797521294876739258499

float y0()
{
    return L0_NORMALIZATION;
}

float y10(float3 direction)
{
    return L1_NORMALIZATION * direction.x;
}

float y11(float3 direction)
{
    return L1_NORMALIZATION * direction.y;
}

float y12(float3 direction)
{
    return L1_NORMALIZATION * direction.z;
}

float y20(float3 direction)
{
    return L2_NORMALIZATION_LEFT * direction.z * direction.x;
}

float y21(float3 direction)
{
    return L2_NORMALIZATION_LEFT * direction.x * direction.y;
}

float y22(float3 direction)
{
    return L2_NORMALIZATION_ZONAL * (3.0 * direction.y * direction.y - 1.0);
}

float y23(float3 direction)
{
    return L2_NORMALIZATION_LEFT * direction.z * direction.y;
}

float y24(float3 direction)
{
    return L2_NORMALIZATION_RIGHT * (direction.z * direction.z - direction.x * direction.x);
}

struct SHL0
{
    float3 l0; // l0r, l0g, l0b
};

struct SHL1
{
    float4 l1r; // l10r, l11r, l12r, l0r
    float4 l1g; // l10g, l11g, l12g, l0g
    float4 l1b; // l10b, l11b, l12b, l0b
};

struct SHL2
{
    float4 l1r; // l10r, l11r, l12r, l0r
    float4 l1g; // l10g, l11g, l12g, l0g
    float4 l1b; // l10b, l11b, l12b, l0b
    float4 l2r; // l20r, l21r, l22r, l23r
    float4 l2g; // l20g, l21g, l22g, l23g
    float4 l2b; // l20b, l21b, l22b, l23b
    float4 l2c; // l24r, l24g, l24b
};