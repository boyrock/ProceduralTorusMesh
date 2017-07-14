// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

Shader "Unlit/RimShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_RimColor("_RimColor", Color) = (1,1,1,1)
		_Color("_Color", Color) = (1,1,1,1)
		_SpecColor("SpecColor", Color) = (1.0, 1.0, 1.0, 1.0)
		_Shininess("Shininess", Float) = 10
		_RimPower("Rim Power", Range(0.1, 10.0)) = 3.0
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" "LightMode" = "ForwardBase" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			#pragma multi_compile_fog
			
			#include "UnityCG.cginc"
			#include "Lighting.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float3 normal : NORMAL;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 posWorld: TEXCOORD1;
				float4 vertex : SV_POSITION;
				float3 normalDir : NORMAL;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float4 _RimColor;
			float4 _Color;
			float _Shininess;
			float _RimPower;

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.posWorld = mul(unity_ObjectToWorld, v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.normalDir = UnityObjectToWorldNormal(v.normal);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				float3 normalDirection = i.normalDir;
				float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
				float3 lightDirection = normalize(_WorldSpaceLightPos0.xyz);
				float atten = 1.0;

				float3 diffuseReflection = atten * _LightColor0.xyz * saturate(dot(normalDirection, lightDirection));
				float3 specularReflection = atten * _SpecColor * _LightColor0.xyz * saturate(dot(normalDirection, lightDirection)) * pow(saturate(dot(reflect(-lightDirection, normalDirection), viewDirection)), _Shininess);

				float rim = 1 - saturate(dot(viewDirection, normalDirection));
				rim = pow(rim, _RimPower);
				float3 rimLighting = atten * _RimColor * saturate(dot(normalDirection, lightDirection)) * rim;

				float3 lightFinal = rimLighting + diffuseReflection + specularReflection + UNITY_LIGHTMODEL_AMBIENT.xyz;

				return float4(lightFinal * _Color.xyz, 1.0);
			}
			ENDCG
		}
	}
}
