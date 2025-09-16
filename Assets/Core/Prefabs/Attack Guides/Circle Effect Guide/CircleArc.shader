Shader "Custom/CircleArcWithTextureAndTintEdges" {
    Properties {
        _Angle("Angle (degrees)", Range(0.0, 360.0)) = 45.0
        _Radius("Radius", Float) = 0.5
        _LineWidth("Line Width", Float) = 0.05
        _EdgeThreshold("Edge Threshold", Float) = 0.1 // Edge thickness threshold
        _TintColor("Tint Color", Color) = (1,1,1,1) // Tint color for the edges
        _MainTex("Texture", 2D) = "white" {}
    }
    SubShader {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
        LOD 100

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        AlphaTest Greater 0.01
        Cull Off

        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            float _Angle;
            float _Radius;
            float _LineWidth;
            float _EdgeThreshold;
            fixed4 _TintColor;
            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
                float2 uv = i.uv * 2.0 - 1.0; // Transform to -1 to 1 space
                float angle = atan2(uv.y, uv.x) * 57.29578; // Convert to degrees
                if (angle < 0) angle += 360.0; // Normalize angle to 0-360 range

                float radius = length(uv);
                float arcStart = 0.0; // Starting angle of the arc
                float arcEnd = _Angle; // Ending angle of the arc

                float innerRadius = _Radius - _LineWidth / 2.0;
                float outerRadius = _Radius + _LineWidth / 2.0;

                // Determine if within edge threshold
                bool isWithinEdgeThreshold = (angle <= arcEnd && angle >= (arcEnd - _EdgeThreshold)) || (angle >= arcStart && angle <= (arcStart + _EdgeThreshold));

                if (uv.x * uv.x + uv.y * uv.y > 1) return fixed4(0, 0, 0, 0);
                

                if (radius >= innerRadius && radius <= outerRadius) {
                    if (isWithinEdgeThreshold) {
                        // Color the edge with the tint color
                        return _TintColor;
                    }
                    else if (angle >= arcStart && angle <= arcEnd) {
                        // Sample the texture for the rest of the arc
                        if (uv.x * uv.x + uv.y * uv.y > 0.95) return _TintColor;

                        return fixed4(_TintColor.rgb, 0.2f);

                        return tex2D(_MainTex, i.uv);
                    }
                }

                return fixed4(0, 0, 0, 0); // Transparent outside the arc
            }
            ENDCG
        }
    }
    FallBack "Transparent"
}