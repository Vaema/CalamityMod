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
    // pixelate the effect
	float2 pixelationFactor = 2 / uImageSize0;
	float2 noiseCoords = floor(coords / pixelationFactor) * pixelationFactor;
    
	float3 greene = float3(0 / 255.0, 237 / 255.0, 28 / 255.0);
    
	float2 framedCoords = (noiseCoords * uImageSize0 - uSourceRect.xy) / uSourceRect.zw * 0.2;
    float2 modifiedCoords = framedCoords;
	modifiedCoords.y += uTime * 0.75; // Cause the effect to travel upwards
    
	float2 framedCoords2 = (noiseCoords * uImageSize0 - uSourceRect.xy) / uSourceRect.zw * 0.3;
	float2 modifiedCoords2 = framedCoords2;
	modifiedCoords2.y -= uTime * 0.5; // Cause the effect to travel upwards
    
	float4 noiseColor = tex2D(uImage1, frac(modifiedCoords));
	float4 noiseColor2 = tex2D(uImage1, frac(modifiedCoords2));
    float4 color = tex2D(uImage0, coords);    
    
	float3 finalColor = lerp(uColor, uSecondaryColor, noiseColor * 2); // Applies light blue to the noise
	float3 finalColor2 = lerp(uColor, uSecondaryColor, noiseColor2 * 2); // Applies light blue to the noise
    
	return float4((finalColor + finalColor2) / 2.0, 1) * color * sampleColor;
}
technique Technique1
{
    pass DyePass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}