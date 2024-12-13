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
	float2 framedCoords = (coords * uImageSize0 - uSourceRect.xy) / uSourceRect.zw ;
    float2 modifiedCoords = framedCoords;
    modifiedCoords += uTime * 0.1; // Cause the effect to travel upwards
    
    float4 noiseColor = tex2D(uImage1, frac(modifiedCoords));
    float4 color = tex2D(uImage0, coords);    
    
    float3 finalColor = lerp(uSecondaryColor, uColor, noiseColor); // Applies green to the noise
    
	return float4(finalColor, 1) * color * sampleColor;
}
technique Technique1
{
    pass DyePass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}