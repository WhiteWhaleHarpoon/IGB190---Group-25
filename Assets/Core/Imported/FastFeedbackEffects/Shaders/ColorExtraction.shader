Shader "Screen Effects/ColorExtraction" {

	Properties{
		//_MainTex("Base (RGB)", 2D) = "white" {}
		_TargetColor("Target Color", Color) = (1,0,0,1)
	}

		SubShader{
			Pass {
				CGPROGRAM

				#pragma vertex vert
				#pragma fragment frag

				uniform sampler2D _MainTex;
	float threshold = 100000.0f;
	float4 _TargetColor;
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

	struct LABColor {
		float L;
		float A;
		float B;
	};

	LABColor ToLAB(float4 c) {
		LABColor lab;
		float D65x = 0.9505;
		float D65y = 1.0;
		float D65z = 1.0890;
		float rLinear = c.r;
		float gLinear = c.g;
		float bLinear = c.b;
		float r = (rLinear > 0.04045f) ? pow((rLinear + 0.055) / (1.055), 2.2) : (rLinear / 12.92);
		float g = (gLinear > 0.04045f) ? pow((gLinear + 0.055) / (1.055), 2.2) : (gLinear / 12.92);
		float b = (bLinear > 0.04045f) ? pow((bLinear + 0.055) / (1.055), 2.2) : (bLinear / 12.92);
		float x = (r * 0.4124f + g * 0.3576f + b * 0.1805f);
		float y = (r * 0.2126f + g * 0.7152f + b * 0.0722f);
		float z = (r * 0.0193f + g * 0.1192f + b * 0.9505f);
		x = (x > 0.9505) ? 0.9505 : ((x < 0) ? 0 : x);
		y = (y > 1.0) ? 1.0 : ((y < 0) ? 0 : y);
		z = (z > 1.089) ? 1.089 : ((z < 0) ? 0 : z);
		float fx = x / D65x;
		float fy = y / D65y;
		float fz = z / D65z;
		fx = ((fx > 0.008856) ? pow(fx, (1.0f / 3.0)) : (7.787 * fx + 16.0 / 116.0));
		fy = ((fy > 0.008856) ? pow(fy, (1.0f / 3.0)) : (7.787 * fy + 16.0 / 116.0));
		fz = ((fz > 0.008856) ? pow(fz, (1.0f / 3.0)) : (7.787 * fz + 16.0 / 116.0));
		lab.L = 116.0 * fy - 16;
		lab.A = 500.0 * (fx - fy);
		lab.B = 200.0 * (fy - fz);
		return lab;
	}

	float4 FromLAB(LABColor lab) {
		float D65x = 0.9505;
		float D65y = 1.0;
		float D65z = 1.0890;
		float delta = 6.0 / 29.0;
		float fy = (lab.L + 16) / 116.0;
		float fx = fy + (lab.A / 500.0);
		float fz = fy - (lab.B / 200.0);
		float x = (fx > delta) ? D65x * (fx * fx * fx) : (fx - 16.0 / 116.0) * 3.0 * (delta * delta) * D65x;
		float y = (fy > delta) ? D65y * (fy * fy * fy) : (fy - 16.0 / 116.0) * 3.0 * (delta * delta) * D65y;
		float z = (fz > delta) ? D65z * (fz * fz * fz) : (fz - 16.0 / 116.0) * 3.0 * (delta * delta) * D65z;
		float r = x * 3.2410 - y * 1.5374 - z * 0.4986;
		float g = -x * 0.9692 + y * 1.8760 - z * 0.0416;
		float b = x * 0.0556 - y * 0.2040 + z * 1.0570;
		r = (r <= 0.0031308) ? 12.92 * r : (1.055) * pow(r, (1.0 / 2.4)) - 0.055;
		g = (g <= 0.0031308) ? 12.92 * g : (1.055) * pow(g, (1.0 / 2.4)) - 0.055;
		b = (b <= 0.0031308) ? 12.92 * b : (1.055) * pow(b, (1.0 / 2.4)) - 0.055;
		r = (r < 0) ? 0 : r;
		g = (g < 0) ? 0 : g;
		b = (b < 0) ? 0 : b;
		return float4(r, g, b, 1.0);
	}

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

		//LABColor target = ToLAB(float4(1.0f, 0.0f, 0.0f, 1.0f));
		LABColor target = ToLAB(_TargetColor);

		LABColor lab = ToLAB(colour);

		float LDiff = (lab.L - target.L);
		float ADiff = (lab.A - target.A);
		float BDiff = (lab.B - target.B);

		//if (sqrt(LDiff * LDiff + ADiff * ADiff + BDiff * BDiff) < 250) {
		float4 newColor;
		if (   (   abs(LDiff * LDiff * LDiff) + abs(ADiff * ADiff * ADiff) + abs(BDiff * BDiff * BDiff)) < 500000) {

			//float factor = 0.5f;
			float factor = 1;
			LABColor c;
			c.L = (lab.L * factor + target.L * (1.0f - factor));
			//c.A = (lab.A * factor + target.A * (1.0f - factor));
			//c.B = (lab.B * factor + target.B * (1.0f - factor));
			c.L = (lab.L * factor + target.L * (1.0f - factor));

			//c.A = target.A;
			//c.B = target.B;

			c.L = lab.L;
			c.A = lab.A;
			c.B = lab.B;

			



			//return FromLAB(c);
			newColor = FromLAB(c);
			newColor = colour;

			//return float4((colour.rgb + float3(1.0f, 0.0f, 0.0f) / 2.0f), 1.0f);
		}
		else {
			float grayscale = 0.299 * colour.r + 0.587 * colour.g + 0.114 * colour.b;
			//grayscale *= 0.5f;
			grayscale *= 1.0;
			newColor = float4(grayscale, grayscale, grayscale, 1.0f);
		}

		return float4(colour.rgb * (1.0 - _Strength) + newColor.rgb * _Strength, 1.0f);

		// Image effect shader code here!
		
		
	}

	

	ENDCG
}
	}
}
