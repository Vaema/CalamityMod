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

float2 Pixelate(float2 uv)
{
    int multi = sin(uTime * 0.1) * 200 + 200;
    int2 var = int2(uv.x * multi, uv.y * multi);
    //var = round(var);
    return float2((float) var.x / multi, (float) var.y / multi);
}

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    // pixelate the coordinates
    float2 pixelationFactor = 2 / uImageSize0;
    float2 noiseCoords = floor(coords / pixelationFactor) * pixelationFactor; 
    
    float time = sin(uTime) * 0.5;
	float4 color = tex2D(uImage0, coords);
    
    float4 black = float4(0, 0, 0, 1);
    float4 blue = float4(89 / 255.0, 70 / 255.0, 111 / 255.0, 1);
    
    // coords scale in size and position over time
    float2 framedCoords = (noiseCoords * uImageSize0 - uSourceRect.xy) / uSourceRect.zw * (0.4 + time * 0.05);
    float2 modifiedCoords = framedCoords + float2(uTime * 0.02, 1 + sin(uTime) * 0.02);

    float4 noiseColor = tex2D(uImage1, frac(modifiedCoords)) * 0.5; // black noise

    float4 noiseColor2 = tex2D(uImage1, frac(modifiedCoords)) * 0.25; // green noise
	
    if (color.a == 0)
        return 0;
    
    // outlines are black
    if (any(color))
    {
        float2 pixelSize = 1 / uImageSize0;
        if (!any(tex2D(uImage0, coords + float2(pixelSize.x * 2, 0))) ||
            !any(tex2D(uImage0, coords + float2(-pixelSize.x * 2, 0))) ||
            !any(tex2D(uImage0, coords + float2(0, pixelSize.y * 2))) ||
            !any(tex2D(uImage0, coords + float2(0, -pixelSize.y * 2))))
        {
            return color * sampleColor * black;
        }
    }
    
    float4 lerped = lerp(black, float4(uColor.rgb, 1), noiseColor); // shift between brown and orange according to the noise
	float4 dotted = dot(color.rgb, 0.5); // grayscale
    float3 blackandblue = (lerped.rgb + blue.rgb) / 4; // blue and shifting black combined
    float4 greentoblack = float4(lerp(blackandblue * dotted.rgb, uSecondaryColor.rgb, noiseColor2.rgb), color.a); // shift between the blue black mix and green
    
    return float4(greentoblack.rgb, color.a) * sampleColor;
}

technique Technique1
{
	pass DyePass
	{
		PixelShader = compile ps_3_0 PixelShaderFunction();
	}
}