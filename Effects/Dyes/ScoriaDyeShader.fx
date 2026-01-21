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

float betterSaturate(float var)
{
    if (var > 1)
        return 1;
    else if (var < 0)
        return 0;
    else 
        return var;
}

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float time = (sin(uTime * 2) * 0.5);
    float timeSaturated = betterSaturate(time);
    float4 color = tex2D(uImage0, coords);
    
    float4 brown = float4(uColor, 1);
	float4 orange = float4(uSecondaryColor, 1);
	float4 blue = float4(85 / 255.0, 140 / 255.0, 194 / 245.0, 1);
    
    
	float2 framedCoords = (coords * uImageSize0 - uSourceRect.xy) / uSourceRect.zw * 0.75;
	float2 modifiedCoords = framedCoords + float2(uTime * 0.02, 0);
    
	float2 framedCoords2 = (coords * uImageSize0 - uSourceRect.xy) / uSourceRect.zw * 0.5;
	float2 modifiedCoords2 = framedCoords2 + float2(uTime * 0.02 + 1, 1);

	float4 noiseColor = tex2D(uImage1, frac(modifiedCoords));
	float4 noiseColor2 = tex2D(uImage1, frac(modifiedCoords2));
    
    float a = color.a * sampleColor.a;
    
	float4 lerped = lerp(brown, orange, noiseColor - 0.5); // shift between brown and orange according to the noise
	float4 finallerp = lerp(lerped, blue, noiseColor2 - 0.5); // shift between blue and brorange according to the other noise
    float4 dotted = dot(color.rgb, 0.5); // grayscale
    
	return float4(dotted * finallerp.rgb, color.a) * sampleColor;
}

technique Technique1
{
    pass DyePass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}