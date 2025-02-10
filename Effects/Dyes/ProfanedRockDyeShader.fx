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
    float time = (sin(uTime) * 0.5 + 0.25);
    float4 color = tex2D(uImage0, coords);
    
    float4 brown = float4(uColor, 1);
	float4 orange = float4(uSecondaryColor, 1);    
    
	float2 framedCoords = (coords * uImageSize0 - uSourceRect.xy) / uSourceRect.zw * 0.75;
    float2 modifiedCoords = framedCoords; 

    float4 noiseColor = tex2D(uImage1, frac(modifiedCoords));
    
	float4 lerped = lerp(brown, orange, noiseColor + time); // shift between brown and orange according to the noise
    float4 dotted = dot(color.rgb, 0.5); // grayscale
    
    return float4(dotted * lerped.rgb, color.a) * sampleColor;
}

technique Technique1
{
    pass DyePass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}