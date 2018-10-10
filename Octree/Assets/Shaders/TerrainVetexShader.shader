Shader "Vertex/TerrainVetexShader"
{
	Properties
	{
	}
	SubShader
	{
		Tags { "RenderType"="Opaque"  "LightMode" = "ForwardBase"}
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			#pragma multi_compile_fog
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				//float2 uv : TEXCOORD0;
				float4 color: Color;
				float3 normal : NORMAL;

			};

			struct v2f
			{
				float4 pos : SV_POSITION;
				float4 color: Color;
			};

			v2f vert (appdata v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.color = v.color - (abs(v.normal.z) + abs(v.normal.x))*0.2;
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				return i.color;
			}
			ENDCG
		}
	}

	    Fallback "VertexLit"
}
