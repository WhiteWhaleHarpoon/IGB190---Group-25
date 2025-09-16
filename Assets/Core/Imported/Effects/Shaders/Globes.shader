Shader "Custom/ResourceGlobe"
{
    Properties
    {
        // Main Texture
        [PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}

        // Globe Fill Properties
        [Space]
        _Fill("Globe Fill", Range(0, 1)) = 0.257

        // Globe Coloring Properties
        [Space]
        _MainGlobeColor("Main Globe Color", Color) = (1, 1, 1, 1)
        _AccentGlobeColor("Accent Globe Color", Color) = (1, 1, 1, 1)
        _EdgeGlobeColor("Edge Globe Color", Color) = (0, 0.07343698, 1, 1)

        // Globe Waviness Properties
        [Space]
        _WaveCount("Wave Count", Range(0, 128)) = 20.103
        _WaveHeight("Wave Height", Range(0, 1)) = 0.13
        _WaveSpeed("Wave Speed", Range(-2, 2)) = 1

        // Globe Scrolling Properties
        [Space]
        _AccentScrollSpeedX("Accent Scroll Speed (X)", Range(-1, 1)) = 0.214
        _AccentScrollSpeedY("Accent Scroll Speed (Y)", Range(-1, 1)) = 0.214

        // Material Textures
        [Space]
        _AccentTexture("Accent Texture", 2D) = "white" {}
        _GlobeMask("Globe Mask", 2D) = "white" {}
        _FilledMask("Filled Mask", 2D) = "white" {}
        _EdgeMask("Edge Mask", 2D) = "white" {}
        _BorderTexture("Border Texture", 2D) = "white" {}

        // UI Masking (Hidden from Inspector)
        [HideInInspector] _StencilComp("Stencil Comparison", Float) = 8
        [HideInInspector] _Stencil("Stencil ID", Float) = 0
        [HideInInspector] _StencilOp("Stencil Operation", Float) = 0
        [HideInInspector] _StencilWriteMask("Stencil Write Mask", Float) = 255
        [HideInInspector] _StencilReadMask("Stencil Read Mask", Float) = 255
        [HideInInspector] _ColorMask("Color Mask", Float) = 15
    }

    SubShader
    {
        Tags {"Queue" = "Transparent" "IgnoreProjector" = "true" "RenderType" = "Transparent" "PreviewType"="Plane" "CanUseSpriteAtlas"="True"}
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma fragmentoption ARB_precision_hint_fastest
            #include "UnityCG.cginc"

            // Structure Definitions
            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float2 texcoord : TEXCOORD0;
                float4 vertex   : SV_POSITION;
                float4 color    : COLOR;
            };

            // Shader Properties
            sampler2D _MainTex;
            float _WaveCount;
            float _WaveHeight;
            float _WaveSpeed;
            float _AccentScrollSpeedX;
            float _AccentScrollSpeedY;
            sampler2D _AccentTexture;
            float4 _AccentGlobeColor;
            sampler2D _GlobeMask;
            float _Fill;
            sampler2D _FilledMask;
            sampler2D _EdgeMask;
            float4 _EdgeGlobeColor;
            float4 _MainGlobeColor;
            sampler2D _BorderTexture;

            // Vertex Shader
            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color;
                return OUT;
            }

            // Helper Functions
            float2 ApplyDistortion(float2 uv)
            {
                float speed = _Time * 100 * _WaveSpeed;
                uv.x += sin(uv.y * _WaveCount + speed) * _WaveHeight * 0.05;
                uv.y += cos(uv.x * _WaveCount + speed) * _WaveHeight * 0.05;
                return uv;
            }

            float4 ApplyTint(float4 textureColor, float4 tintColor)
            {
                float3 tint = dot(textureColor.rgb, float3(.222, .707, .071));
                tint.rgb *= tintColor.rgb;
                textureColor.rgb = lerp(textureColor.rgb, tint.rgb, tintColor.a);
                return textureColor;
            }

            float2 ApplyZoom(float2 uv)
            {
                float2 center = float2(0.5, 0.5);
                uv -= center;
                uv *= 2.347;
                uv += center;
                return uv;
            }

            float4 BlendOverlay(float4 baseColor, float4 overlayColor)
            {
                float4 result = baseColor;
                result.a = overlayColor.a + baseColor.a * (1 - overlayColor.a);
                result.rgb = (overlayColor.rgb * overlayColor.a + baseColor.rgb * baseColor.a * (1 - overlayColor.a)) * (result.a + 0.0000001);
                result.a = saturate(result.a);
                return result;
            }

            float2 ResizeUV(float2 uv)
            {
                uv += float2(1, 1);
                uv = fmod(uv * float2(0.64, 0.64), 1);
                return uv;
            }

            float2 ApplyFishEye(float2 uv)
            {
                float2 m = float2(0.5, 0.5);
                float2 d = uv - m;
                float r = sqrt(dot(d, d));
                float power = (2.0 * 3.141592 / (2.0 * sqrt(dot(m, m)))) * (0.221 + 0.001);
                float bind = sqrt(dot(m, m));
                uv = m + normalize(d) * tan(r * power) * bind / tan(bind * power);
                return uv;
            }

            float4 CreateHDR(float4 color)
            {
                if (color.r > 0.98) color.r = 2;
                if (color.g > 0.98) color.g = 2;
                if (color.b > 0.98) color.b = 2;
                return lerp(saturate(color), color, 0.256);
            }

            float2 AdjustUVPosition(float2 uv)
            {
                //uv += float2(0, 1 - (_Fill + 2) / 3.5);
                uv += float2(0, 1.55);
                uv += float2(0, -_Fill * 3.1);


                return uv;
            }

            float4 ApplyFade(float4 color)
            {
                return float4(color.rgb, color.a * 0.3);
            }

            float4 ApplyDisplacement(float2 uv)
            {
                float t = _Time.y;
                float2 mov = float2(_AccentScrollSpeedX * t, _AccentScrollSpeedY * t) * 1;
                float2 mov2 = float2(_AccentScrollSpeedX * t * 2, _AccentScrollSpeedY * t * 2) * 1;
                float4 baseColor = tex2D(_AccentTexture, uv + mov);
                float4 overlayColor = tex2D(_AccentTexture, uv + mov2);
                float r = (overlayColor.r + overlayColor.g + overlayColor.b) / 3;
                r *= overlayColor.a;
                uv += mov2 * 0.25;
                return tex2D(_AccentTexture, lerp(uv, uv + float2(baseColor.r * _AccentScrollSpeedX, baseColor.g * _AccentScrollSpeedY), r));
            }

            // Fragment Shader
            float4 frag(v2f i) : COLOR
            {
                // Apply distortions and effects
                float2 distortedUV = ApplyDistortion(i.texcoord);
                float2 zoomedUV = ApplyZoom(distortedUV);
                float2 fishEyeUV = ApplyFishEye(zoomedUV);
                float2 resizedUV = ResizeUV(fishEyeUV);
                float4 displacement = ApplyDisplacement(resizedUV);

                // Apply color tint and fade
                float4 tintedColor = ApplyTint(displacement, _AccentGlobeColor);
                float4 fadedColor = ApplyFade(displacement);
                tintedColor *= fadedColor;

                // Additional texture blending
                float4 globeMaskTex = tex2D(_GlobeMask, i.texcoord);
                float2 positionedUV = AdjustUVPosition(fishEyeUV);
                float4 filledMaskTex = tex2D(_FilledMask, positionedUV);
                globeMaskTex = lerp(globeMaskTex, globeMaskTex * filledMaskTex, 1);

                // Mask and blend operations
                float4 maskColor = tintedColor;
                maskColor.a = lerp(globeMaskTex.r * tintedColor.a, (1 - globeMaskTex.r) * tintedColor.a, 0);

                float4 edgeMaskTex = tex2D(_EdgeMask, positionedUV);
                float4 edgeTintColor = ApplyTint(edgeMaskTex, _EdgeGlobeColor);
                maskColor = lerp(maskColor, maskColor * maskColor.a + edgeTintColor * edgeTintColor.a, 1);

                float4 mainGlobeTintColor = ApplyTint(filledMaskTex, _MainGlobeColor);
                maskColor = lerp(maskColor, maskColor * maskColor.a + mainGlobeTintColor * mainGlobeTintColor.a, 1);

                // Final blend and HDR
                globeMaskTex = lerp(globeMaskTex, globeMaskTex * filledMaskTex, 1);
                float4 finalMask = maskColor;
                finalMask.a = lerp(globeMaskTex.r, 1 - globeMaskTex.r, 0);

                float4 borderTex = tex2D(_BorderTexture, i.texcoord);
                float4 blendedResult = BlendOverlay(finalMask, borderTex);

                float4 hdrResult = CreateHDR(blendedResult);

                // Final output color
                float4 finalColor = hdrResult;
                finalColor.rgb *= i.color.rgb;

                return finalColor;
            }

            ENDCG
        }
    }

    Fallback "Sprites/Default"
}