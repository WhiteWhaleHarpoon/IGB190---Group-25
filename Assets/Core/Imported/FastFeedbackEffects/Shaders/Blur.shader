Shader "Screen Effects/Blur" {

	Properties{
		_MainTex("Base (RGB)", 2D) = "white" {}
		_Strength("Strength", Float) = 1.0
	}

		SubShader{
			Pass {
				CGPROGRAM

				#pragma vertex vert
				#pragma fragment frag

				uniform sampler2D _MainTex;
	uniform float _Strength;

	// Vertex shader input struct.
	struct VSInput {
		float4 pos : POSITION;
		float2 tex : TEXCOORD0;
	};

	// Vertex shader output struct.
	struct VSOutput {
		float4 pos : SV_POSITION;
		float2 tex : TEXCOORD0;
	};

	// Vertex shader.
	VSOutput vert(VSInput input) {
		VSOutput output;
		output.pos = UnityObjectToClipPos(input.pos);
		output.tex = input.tex;
		return output;
	}

	// Fragment shader.
	float4 frag(VSOutput input) : SV_Target
	{
		float offset = 0.003;
		
		float3 base = tex2D(_MainTex, input.tex);

		_Strength = 1;
		float3 colour = base;
		colour += tex2D(_MainTex, input.tex + float2(offset, 0)) * 0.5;
		colour += tex2D(_MainTex, input.tex + float2(-offset, 0)) * 0.5;
		colour += tex2D(_MainTex, input.tex + float2(0, offset)) * 0.5;
		colour += tex2D(_MainTex, input.tex + float2(0, -offset)) * 0.5;
		colour += tex2D(_MainTex, input.tex + float2(offset, offset)) * 0.25;
		colour += tex2D(_MainTex, input.tex + float2(-offset, offset)) * 0.25;
		colour += tex2D(_MainTex, input.tex + float2(offset, -offset)) * 0.25;
		colour += tex2D(_MainTex, input.tex + float2(-offset, -offset)) * 0.25;
		colour /= 4.0;

		return float4(base * (1.0 - _Strength) + colour * _Strength, 1.0f);


		//float grayscale = 0.299 * colour.r + 0.587 * colour.g + 0.114 * colour.b;

		//return float4(grayscale, grayscale, grayscale, 1.0f);
		//return float4(colour.rgb * (1.0 - _Strength) + grayscale.xxx * _Strength, 1.0f);

		// Image effect shader code here!


	}

	ENDCG
}
		}
}
