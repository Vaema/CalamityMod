sampler uImage0 : register(s0);
float uTime;
float uHoverIntensity;
float uPixel;
float uColorResolution;
float uGrayness;
float uSpeed;
float4 uSource;
float3 uInColor;

// variations of this need to be purpose built because of hlsl limitations
// this one only does a basic gaussian bloom at fixed parameters

float gaussian(float2 i, float sigma2, float pisigma2) {
    float top = exp(-((i.x * i.x) + (i.y * i.y)) / sigma2);
    float bot = pisigma2;
    return top / bot;
}

float2 AspectCorrectedGBlurScale(float2 resolution, float blurIntensity) {
    return (1.0f.xx / resolution) * blurIntensity;
}

float4 gauss_blur(sampler sp, float2 uv, float2 scale, int samples) {
    const float pi = radians(180.);
    const float sigma = float(samples) * 0.25;
    const float sigma2 = 2. * sigma * sigma;
    const float pisigma2 = pi * sigma2;

    float2 offset = float2(0, 0);
    float weight = gaussian(offset, sigma2, pisigma2);
    float4 color_av = tex2D(sp, uv) * weight;
    float accum = weight;
    
    for (int x = 0; x <= samples / 2; ++x) {
        for (int y = 1; y <= samples / 2; ++y) {
            offset = float2(x, y);
            weight = gaussian(offset, sigma2, pisigma2);
            color_av += tex2D(sp, uv + scale * offset) * weight;
            accum += weight;

            color_av += tex2D(sp, uv - scale * offset) * weight;
            accum += weight;

            offset = float2(-y, x);
            color_av += tex2D(sp, uv + scale * offset) * weight;
            accum += weight;

            color_av += tex2D(sp, uv - scale * offset) * weight;
            accum += weight;
        }
    }
    float4 final = color_av / accum;
    // gamma correction
    final = pow(final, (1.0 / 2.2).xxxx);
    return final;
}

float4 gauss_bloom(sampler sp, float2 uv, float2 scale, int samples, float4 exposure, float thresh) {
    float4 bright = clamp(gauss_blur(sp, uv, scale, samples) - thresh.xxxx, 0.0f.xxxx, 1.0f.xxxx) * (1.0f / (1.0f - thresh));
    return bright * exposure;
}

float4 main(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    return gauss_bloom(uImage0, coords, AspectCorrectedGBlurScale(uSource, 4.0f), 8, 1., 0.3f);
}

technique Technique1
{
    pass BloomShader
    {
        PixelShader = compile ps_3_0 main();
    }
}