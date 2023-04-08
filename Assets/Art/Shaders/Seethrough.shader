Shader "Unlit/Seethrough"
{
	Properties
	{
		_Color("Color 1", Color) = (0,0,0,1)
		_Color2("Color 2", Color) = (0,0,0,0)
		_Factor("Factor", Int) = 1
		[Enum(Off,0,On,1)]_ZWrite("ZWrite", Int) = 0
	}
		SubShader
	{
		Tags { "RenderType" = "Transparent" "Queue" = "Transparent" }
		Blend SrcAlpha OneMinusSrcAlpha
		LOD 100
		ZWrite [_ZWrite]

		Pass
		{
			CGPROGRAM
			#pragma vertex vert 
			#pragma fragment frag
			#pragma target 3.0

			float4 vert(float4 vertexPosition:POSITION) : POSITION
			{
				return UnityObjectToClipPos(vertexPosition);
			}
			
			fixed4 _Color;
			fixed4 _Color2;
			int _Factor;

			float4 frag(UNITY_VPOS_TYPE screenPos : VPOS) : SV_Target
			{
				return lerp(_Color, _Color2,
					step(0,
						fmod(screenPos.x/_Factor + screenPos.y/_Factor,2.0)-1.0)
					);
			}
		   ENDCG
		}
	}
}
