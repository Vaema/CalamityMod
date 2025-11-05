sampler baseTexture : register(s0);
sampler greenTexture : register(s1);
sampler purpleTexture : register(s2);
sampler glintTexture : register(s3);

float time;
float2 screenOffset;
float2 offscreenOffset;

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0, float4 screenPos : VPOS) : COLOR
{
    // Calculate world position
    float2 worldPos = screenPos.xy + screenOffset - offscreenOffset;
    
    // Calculate tile coordinates for fade2
    float tileX = worldPos.x / 16.0;
    float tileY = worldPos.y / 16.0;
    
    // Sample all three textures
    float4 blueColor = tex2D(baseTexture, coords);
    float4 purpleColor = tex2D(purpleTexture, coords);
    float4 greenColor = tex2D(greenTexture, coords);
    
    // Calculate fade values
    float fade1 = (sin(time * 0.2) + 1.0) / 2.0;
    float fade2 = (sin(time * 0.1 + tileX * 0.08 - tileY * 0.05) + 1.0) / 2.0;
    
    // Blend the colors
    float4 result = blueColor;
    result.rgb = lerp(result.rgb, greenColor.rgb, greenColor.a * fade1);
    result.rgb = lerp(result.rgb, purpleColor.rgb, purpleColor.a * fade2);
    
    return result * sampleColor;
}

technique Technique1
{
    pass SeaPrismBlendingPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}