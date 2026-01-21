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
float2 uTargetPosition;
float4 uLegacyArmorSourceRect;
float2 uLegacyArmorSheetSize;

float BlendMode_ColorBurn(float base, float blend)
{
    if (base > 1.0)
        return 1.0;
    else if (blend <= 0.0)
        return 0.0;
    else
        return 1.0 - min(1.0, (1.0 - base) / blend);
}

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float2 framedCoords = (coords * uImageSize0 - uSourceRect.xy) / uSourceRect.zw * 0.1;
    float2 modifiedCoords = framedCoords;
    modifiedCoords.y += uTime * 0.02; // Cause the effect to travel upwards    

    float4 noiseColor = tex2D(uImage1, frac(modifiedCoords));
    float4 color = tex2D(uImage0, coords);

    return float4(BlendMode_ColorBurn(color.r, uColor.r), BlendMode_ColorBurn(color.g, uColor.g), BlendMode_ColorBurn(color.b, uColor.b), color.a) * noiseColor * sampleColor;
}

technique Technique1
{
    pass DyePass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}