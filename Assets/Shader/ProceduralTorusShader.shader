Shader "Unlit/ProceduralTorusShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_RimColor("_RimColor", Color) = (1,1,1,1)
		_SpecColor("SpecColor", Color) = (1.0, 1.0, 1.0, 1.0)
		_Shininess("Shininess", Float) = 10
		_RimPower("Rim Power", Range(0.1, 10.0)) = 3.0
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" "LightMode" = "ForwardBase" }
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
			#include "Lighting.cginc"
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
				float3 posWorld : TEXCOORD2;
				float3 color : TEXCOORD3;

				LIGHTING_COORDS(2, 3)
				UNITY_FOG_COORDS(4)
			};
			
			struct TorusVertex
			{
				float3 pos;
				float3 normal;
				float2 uv;
			};


			float4 _RimColor;
			float _Shininess;
			float _RimPower;

			sampler2D _MainTex;
			float4 _MainTex_ST;
			int _NumVertexOfPerTorus;
			int _NumIndexOfPerTorus;

			StructuredBuffer<int> _IndexBuffer;
			StructuredBuffer<TorusVertex> _VertexBuffer;
			StructuredBuffer<float3> _ColorBuffer;

			float rand(float n) { return frac(sin(n) * 43758.5453123); }
			float noise(float p)
			{
				float fl = floor(p);
				float fc = frac(p);
				return lerp(rand(fl), rand(fl + 1.0), fc);
			}

			v2f vert (appdata v)
			{
				v2f o;

				int idx = _IndexBuffer[v.vertexId];// +v.instanceId * _NumVertexOfPerTorus;

				float t = v.vertexId / _NumIndexOfPerTorus;

				idx = idx + v.instanceId * _NumVertexOfPerTorus;

				TorusVertex tv = _VertexBuffer[idx];

				float4 vertex = float4(tv.pos, 1);
				o.pos = UnityObjectToClipPos(vertex);
				o.posWorld = mul(unity_ObjectToWorld, vertex);
				o.normalDir = UnityObjectToWorldNormal(tv.normal);
				o.uv = tv.uv;


				int indexOfSegment = idx / 10;
				int _MaxSegment = 300;

				float tt = (sin(_Time.y * 0.5 + v.instanceId * 0.002 + (indexOfSegment / (_MaxSegment + 1)) * 0.001) + 1) * 0.5;
				o.color = _ColorBuffer[tt * 99];

				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				float3 normalDirection = i.normalDir;
				float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
				float3 lightDirection = normalize(_WorldSpaceLightPos0.xyz);
				float3 lightColor = 1;// _LightColor0.rgb;

				/// Lighting:
				float attenuation = 1.0;// LIGHT_ATTENUATION(i);
				float3 attenColor = attenuation * lightColor;

				/// Diffuse:
				float3 diffuseReflection = attenuation * lightColor * saturate(dot(normalDirection, lightDirection));

				/// Specular:
				float3 specularReflection = attenuation * _SpecColor * lightColor * saturate(dot(normalDirection, lightDirection)) * pow(saturate(dot(reflect(-lightDirection, normalDirection), viewDirection)), _Shininess);

				/// RimLight:
				float3 rimColor = _RimColor;
				half rim = 1.0 - saturate(dot(normalize(viewDirection), normalDirection));
				rim = pow(rim, _RimPower);
				float3 rimLighting = attenuation * lightColor * saturate(dot(normalDirection, lightDirection)) * rim * rimColor.rgb;

				/// FinalLight:
				float3 lightFinal = rimLighting + diffuseReflection + specularReflection + UNITY_LIGHTMODEL_AMBIENT.xyz;

				/// Final Color:
				float3 finalColor = lightFinal * i.color;
				fixed4 finalRGBA = fixed4(finalColor, 1);

				return finalRGBA;
			
			}
			ENDCG
		}
	}
}
