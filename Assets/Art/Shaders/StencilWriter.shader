Shader "Custom/StencilWriter"
{
	Properties
	{
		_Ref("Ref", Range(0,255)) = 1
		[KeywordEnum(Never,Greater,GEqual,Less,LEqual,Equal,NotEqual,Always)]_Comp("Comp", Float) = 7
		[KeywordEnum(Keep,Zero,Replace,IncrSat,DecrSat,Invert,IncrWrap,DecrWrap)]_Pass("Pass", Float) = 2
	}
	SubShader
	{
		Tags { "Queue" = "Geometry-1" }
		ColorMask 0
		ZWrite Off

		Stencil
		{
			Ref [_Ref]
			Comp [_Comp]
			Pass [_Pass]
		}

		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Lambert

		struct Input
		{
			float3 worldPos;
		};

		// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
		// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
		// #pragma instancing_options assumeuniformscaling
		UNITY_INSTANCING_BUFFER_START(Props)
			// put more per-instance properties here
		UNITY_INSTANCING_BUFFER_END(Props)

		void surf(Input IN, inout SurfaceOutput o)
		{
			o.Albedo = fixed4(1,1,1,1);
		}
		ENDCG
	}
}