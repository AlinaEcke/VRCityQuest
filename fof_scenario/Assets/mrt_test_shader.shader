Shader "Test/MRT" {
    Properties {
        _MainTex ("", 2D) = "" {}
    }

CGINCLUDE

#include "UnityCG.cginc"

struct vb_input {
    float4 vertex: POSITION;
    float3 texcoord: TEXCOORD0;
};

struct vb_v2f {
    float4 pos: POSITION;
    float3 uv : TEXCOORD0;
	float4 cp : TEXCOORD1;
	float2 sp : TEXCOORD2;
};

struct f2r {
	fixed4 c0: COLOR0;
	fixed4 c1: COLOR1;
};

sampler2D _MainTex;
uniform float4 _MainTex_TexelSize;

sampler2D _CameraDepthTexture;

float4x4 _PrevViewProj;
float _VelocityMult;
float _BlurStrength;

vb_v2f vb_vert(vb_input v) {
	vb_v2f o;
	o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
	o.uv.xyz = v.texcoord.xyz;
	o.cp = o.pos;
	o.sp = (o.cp.xy / o.cp.w) * half2(0.5, 0.5) + half2(0.5, 0.5);
	return o;
}

f2r vb_frag(vb_v2f i) : COLOR {
	f2r o;

	float d = UNITY_SAMPLE_DEPTH(tex2D(_CameraDepthTexture, float2(i.sp.x, 1.0 - i.sp.y)));
	d = Linear01Depth(d);

	float4 viewPos = float4(i.uv.x * d, -i.uv.y * d, i.uv.z * d, 1.0);
	float4 prevClipPos = mul(_PrevViewProj, viewPos);

	float2 c0 = prevClipPos.xy / prevClipPos.w,
	       c1 = i.cp.xy / i.cp.w;
	float2 dx = c1 - c0;

	float2 v = dx * _VelocityMult;

	v = v * 0.5 + half2(0.5, 0.5);
	v = saturate(v);

	o.c0 = float4(v.x, v.y, 0.0, 1.0);

#if SHADER_API_D3D9 || SHADER_API_D3D11 || SHADER_API_XBOX360
#	if SHADER_API_D3D11
	float2 hp = half2(0.0, 0.0);
	c1.xy = c1.xy * half2(0.5, 0.5) + half2(0.5, 0.5);
#	else
	float2 hp = _MainTex_TexelSize * half2(0.5, -0.5);
	//c1.xy = c1.xy * half2(0.5, -0.5) + half2(0.5, 0.5);
	c1.xy = c1.xy * half2(0.5, 0.5) + half2(0.5, 0.5);
#	endif
#else
	float2 hp = half2(0.0, 0.0);
	c1.xy = c1.xy * half2(0.5, 0.5) + half2(0.5, 0.5);
#endif

	v = (v - half2(0.5, 0.5)) * 2.0;
	v = float2(v.x, -v.y) * _MainTex_TexelSize.xy * _BlurStrength;

	o.c1 = (tex2D(_MainTex, c1.xy + hp) +
			tex2D(_MainTex, c1.xy + hp + v) +
			tex2D(_MainTex, c1.xy + hp + v * 2.0) +
			tex2D(_MainTex, c1.xy + hp + v * 3.0)) * 0.25;
	o.c1.a = 1.0;

	return o;
}

ENDCG

	Subshader {
		Pass {
			ZTest Always Cull Off ZWrite Off
			Fog { Mode off }

			CGPROGRAM
			#pragma exclude_renderers flash
			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma vertex vb_vert
			#pragma fragment vb_frag
			ENDCG
		}
	}

	Fallback Off
}
