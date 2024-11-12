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
	float4 ret = color * sampleColor * float4(uColor, 1);
	if (any(color))
	{
		float2 pixelSize = 1 / uImageSize0;
		// Only the top 2 pixels before air are colored yellow
		if (!any(tex2D(uImage0, coords + float2(0, -pixelSize.y * 4))) ||
            !any(tex2D(uImage0, coords + float2(0, -pixelSize.y * 2))))
		{
			ret = color * sampleColor * float4(uColor.rgb, 1);
		}
		else
			ret = color * sampleColor * float4(uSecondaryColor.rgb, 1);
		return ret;
	}
	return float4(0, 0, 0, 0);
}
technique Technique1
{
    pass DyePass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}