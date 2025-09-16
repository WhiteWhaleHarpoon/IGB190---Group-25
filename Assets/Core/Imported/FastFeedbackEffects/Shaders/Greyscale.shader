Shader "Screen Effects/Grayscale" {

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
		float4 colour = tex2D(_MainTex, input.tex);

		float grayscale = 0.299 * colour.r + 0.587 * colour.g + 0.114 * colour.b;

		//return float4(grayscale, grayscale, grayscale, 1.0f);
		return float4(colour.rgb * (1.0 - _Strength) + grayscale.xxx * _Strength, 1.0f);

		// Image effect shader code here!


	}

	ENDCG
}
		}
}
