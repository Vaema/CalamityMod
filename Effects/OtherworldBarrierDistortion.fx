sampler uImage0 : register(s0);
sampler uImage1 : register(s1);
float3 uColor;
float3 uSecondaryColor; 
float uOpacity; //Opacity works as the intensity of the filter
float uSaturation; //Saturation works as the speed of the noise movement
float uRotation;
float uTime;
float4 uSourceRect;
float2 uWorldPosition;
float uDirection;
float3 uLightSource;
float2 uImageSize0;
float2 uImageSize1;
float4 uShaderSpecificData;

float2 RotateUV(float2 uv, float radians)
{
    // Translate UV so center is origin
    float2 centered = uv - 0.5;

    // Apply rotation
    float cosTheta = cos(radians);
    float sinTheta = sin(radians);
    float2 rotated;
    rotated.x = centered.x * cosTheta - centered.y * sinTheta;
    rotated.y = centered.x * sinTheta + centered.y * cosTheta;

    // Translate back
    return rotated + 0.5;
}

float4 AdjustPixelPosition(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{

    //Drift to offset the noisemap. X multiplier is a prime number to ensure the "loop time" of this is very high, it takes 100 uTime to return to the original state
    float2 drift = float2(0.23 * uTime * uSaturation, uTime * uSaturation);
    
    //double the noise so that it like... looks less like it's just moving in one direction or something? idk how to do this best. Also applying this one in a perpendicular direction.
    float offsetToHave1 = tex2D(uImage1, coords + drift.yx).x;

    //Gets the offset from the red channel of whatever pixel this should get from the noisemap. This is fine because the noisemap is greyscale.
    float offsetToHave = tex2D(uImage1, coords + offsetToHave1).x;

    
    //Run a second noise check to make it look less like a texture moving in a straight line
    offsetToHave = tex2D(uImage1, coords + drift).x;
    
    //This is the uv distance from the center of the texture
    float2 uvOffsetFromCenter = (coords - 0.5);

    float2 modifiedCoords = coords + uvOffsetFromCenter * offsetToHave * uOpacity;
    
    //This mask is to prevent weird stretching effects at the edge of the tecture
    float mask = step(0, modifiedCoords.x) * step(modifiedCoords.y, 1) *
             step(0, modifiedCoords.y) * step(modifiedCoords.x, 1);
    
    //This makes a greyscale, "additive" draw input image transparent
    // I would like to just draw additive but i've been unable to figure that out. Yay.
    float4 distortedColor = tex2D(uImage0, modifiedCoords);
    distortedColor.a = distortedColor.x;

    return distortedColor * sampleColor * mask;
}


technique Technique1
{
    pass DistortionPass
    {
        PixelShader = compile ps_2_0 AdjustPixelPosition();
    }
}