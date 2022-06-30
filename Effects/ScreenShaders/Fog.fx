sampler uImage0 : register(s0);
sampler uImage1 : register(s1);
float3 uColor;
float3 uSecondaryColor;
float uOpacity;
float uSaturation;
float uRotation;
float uTime;
float4 uSourceRect;
float2 uWorldPosition;
float uDirection;
float3 uLightSource;
float2 uImageSize0;
float2 uImageSize1;
float4 uShaderSpecificData;

float Random(float2 coords)
{
    return frac(sin(dot(coords, float2(49, 81)) * 1000) * 1000);
}
float PerlinNoise(float2 coords)
{
    float2 integerPart = floor(coords);
    float2 fractionalPart = frac(coords);
    float a = Random(integerPart);
    float b = Random(integerPart + float2(1, 0));
    float c = Random(integerPart + float2(0, 1));
    float d = Random(integerPart + float2(1, 1));
    float2 cubic = fractionalPart * fractionalPart * (3 - fractionalPart * 2);
    return lerp(a, b, cubic.x) + (c - a) * cubic.y * (1 - cubic.x) + (d - b) * cubic.x * cubic.y;
}

float ScaledNoise(float2 coords)
{
    float result = 0;
    float2 coordsCopy = coords;
    float scale = 0.5;
    for (int i = 0; i < 4; i++)
    {
        result += PerlinNoise(coordsCopy) * scale;
        coordsCopy *= 2;
        scale *= 0.5;
    }
    return result;
}

float4 PixelShaderFunction(float4 sampleColor : TEXCOORD, float2 coords : TEXCOORD0) : COLOR0
{
    float motion = ScaledNoise(coords * uSaturation * 12 + float2(uTime * -0.1, uTime * 0.54));
    float2 motion2D = motion;
    float3 noise = ScaledNoise(coords * uSaturation * 15 + motion2D);
    return lerp(float4(noise, 1), float4(uColor, 1), 0.67) * uOpacity * 0.2;
}
technique Technique1
{
    pass DyePass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}