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


float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 color = tex2D(uImage0, coords);
    float a = color.a * sampleColor.a;
    if (any(color))
    {
        float2 pixelSize = 1 / uImageSize0;
        if (!any(tex2D(uImage0, coords + float2(pixelSize.x * 2, 0))) ||
            !any(tex2D(uImage0, coords + float2(-pixelSize.x * 2, 0))) ||
            !any(tex2D(uImage0, coords + float2(0, pixelSize.y * 2))) ||
            !any(tex2D(uImage0, coords + float2(0, -pixelSize.y * 2))))
        {
            return color * sampleColor;
        }
    }
    
    if (((color.r + color.g + color.b) / 3) > 0.3)    
        return float4(uColor, 1) + color * sampleColor;
    else
        return color * sampleColor;
}

technique Technique1
{
    pass DyePass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}