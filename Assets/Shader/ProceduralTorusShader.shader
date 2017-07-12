Shader "Unlit/ProceduralTorusShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Color("Color", Color) = (1,1,1,1)
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100
		Cull off
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#define UNITY_PASS_FORWARDBASE
			#include "UnityCG.cginc"
			#include "AutoLight.cginc"
		/*	#pragma multi_compile_fwdbase_fullshadows
			#pragma multi_compile_fog*/

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				uint vertexId : SV_VertexID;
				uint instanceId : SV_InstanceID;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 pos : SV_POSITION;
				float3 normalDir : TEXCOORD1;
				LIGHTING_COORDS(2, 3)
				UNITY_FOG_COORDS(4)
			};
			
			struct TorusVertex
			{
				float3 pos;
				float3 prev_pos;
				float3 normal;
				float3 prev_normal;
				float2 uv;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			int _NumVertexOfPerTorus;
			int _NumIndexOfPerTorus;
			float _T;

			uniform float4 _LightColor0;
			uniform float4 _Color;

			StructuredBuffer<int> _IndexBuffer;
			StructuredBuffer<TorusVertex> _VertexBuffer;

			v2f vert (appdata v)
			{
				v2f o;

				int idx = _IndexBuffer[v.vertexId];// +v.instanceId * _NumVertexOfPerTorus;

				float t = v.vertexId / _NumIndexOfPerTorus;

				idx = idx + v.instanceId * _NumVertexOfPerTorus;

				TorusVertex tv = _VertexBuffer[idx];

				o.pos = UnityObjectToClipPos(float4(tv.pos,1));
				o.normalDir = UnityObjectToWorldNormal(tv.normal);
				o.uv = tv.uv;

				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				
				//i.normalDir = normalize(i.normalDir);
				float3 normalDirection = i.normalDir;
				float3 lightDirection = normalize(_WorldSpaceLightPos0.xyz);
				float3 lightColor = float3(1, 1, 1);// _LightColor0.rgb;

				////// Lighting:
				float attenuation = LIGHT_ATTENUATION(i);
				float3 attenColor = attenuation * lightColor;// _LightColor0.xyz;

				/////// Diffuse:
				float NdotL = max(0.0, dot(normalDirection, lightDirection));
				float3 directDiffuse = max(0.0, NdotL) * attenColor;
				float3 indirectDiffuse = float3(0, 0, 0);
				indirectDiffuse += UNITY_LIGHTMODEL_AMBIENT.rgb; // Ambient Light
				float3 diffuseColor = _Color.rgb;
				float3 diffuse = (directDiffuse + indirectDiffuse) * diffuseColor;

				/// Final Color:
				float3 finalColor = diffuse;
				fixed4 finalRGBA = fixed4(finalColor, 1);

				return finalRGBA;
			
			}
			ENDCG
		}
	}
}
