// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Grid"
{
	Properties
	{
		_TextureSample0("Texture Sample 0", 2D) = "white" {}
		_Color2("Color 2", Color) = (0,0.9586205,1,0)
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Transparent+0" "IgnoreProjector" = "True" "IsEmissive" = "true"  }
		Cull Back
		CGINCLUDE
		#include "UnityShaderVariables.cginc"
		#include "UnityPBSLighting.cginc"
		#include "Lighting.cginc"
		#pragma target 3.0
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform float4 _Color2;
		uniform sampler2D _TextureSample0;
		uniform float4 _TextureSample0_ST;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_TexCoord8 = i.uv_texcoord * float2( 1,1 ) + float2( 0,0 );
			float temp_output_9_0 = ( uv_TexCoord8.x + 0.0 );
			float4 temp_output_22_0 = ( ( ( 1.0 - temp_output_9_0 ) * float4(1,0,0.931035,0) ) + ( temp_output_9_0 * float4(0,1,0.8758622,0) ) );
			float mulTime30 = _Time.y * 0.5;
			float cos33 = cos( mulTime30 );
			float sin33 = sin( mulTime30 );
			float2 rotator33 = mul( temp_output_22_0.rg - float2( 0,0 ) , float2x2( cos33 , -sin33 , sin33 , cos33 )) + float2( 0,0 );
			float4 temp_output_35_0 = ( temp_output_22_0 + float4( rotator33, 0.0 , 0.0 ) );
			float smoothstepResult40 = smoothstep( 3.0 , 0.0 , temp_output_35_0.r);
			float4 temp_output_50_0 = ( temp_output_35_0 * ( 1.0 - smoothstepResult40 ) );
			o.Albedo = temp_output_50_0.rgb;
			float2 uv_TextureSample0 = i.uv_texcoord * _TextureSample0_ST.xy + _TextureSample0_ST.zw;
			float4 tex2DNode43 = tex2D( _TextureSample0, uv_TextureSample0 );
			o.Emission = ( ( ( temp_output_50_0 + ( ( _Color2 * tex2DNode43 ) + 0.0 ) ) * float4(1,1,1,0) ) * 2.0 ).rgb;
			o.Alpha = tex2DNode43.r;
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf Standard alpha:fade keepalpha fullforwardshadows exclude_path:deferred noambient novertexlights nolightmap  nodynlightmap nodirlightmap 

		ENDCG
		Pass
		{
			Name "ShadowCaster"
			Tags{ "LightMode" = "ShadowCaster" }
			ZWrite On
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#pragma multi_compile_shadowcaster
			#pragma multi_compile UNITY_PASS_SHADOWCASTER
			#pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2
			#include "HLSLSupport.cginc"
			#if ( SHADER_API_D3D11 || SHADER_API_GLCORE || SHADER_API_GLES3 || SHADER_API_METAL || SHADER_API_VULKAN )
				#define CAN_SKIP_VPOS
			#endif
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "UnityPBSLighting.cginc"
			sampler3D _DitherMaskLOD;
			struct v2f
			{
				V2F_SHADOW_CASTER;
				float2 customPack1 : TEXCOORD1;
				float3 worldPos : TEXCOORD2;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};
			v2f vert( appdata_full v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID( v );
				UNITY_INITIALIZE_OUTPUT( v2f, o );
				UNITY_TRANSFER_INSTANCE_ID( v, o );
				Input customInputData;
				float3 worldPos = mul( unity_ObjectToWorld, v.vertex ).xyz;
				fixed3 worldNormal = UnityObjectToWorldNormal( v.normal );
				o.customPack1.xy = customInputData.uv_texcoord;
				o.customPack1.xy = v.texcoord;
				o.worldPos = worldPos;
				TRANSFER_SHADOW_CASTER_NORMALOFFSET( o )
				return o;
			}
			fixed4 frag( v2f IN
			#if !defined( CAN_SKIP_VPOS )
			, UNITY_VPOS_TYPE vpos : VPOS
			#endif
			) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				Input surfIN;
				UNITY_INITIALIZE_OUTPUT( Input, surfIN );
				surfIN.uv_texcoord = IN.customPack1.xy;
				float3 worldPos = IN.worldPos;
				fixed3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
				SurfaceOutputStandard o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutputStandard, o )
				surf( surfIN, o );
				#if defined( CAN_SKIP_VPOS )
				float2 vpos = IN.pos;
				#endif
				half alphaRef = tex3D( _DitherMaskLOD, float3( vpos.xy * 0.25, o.Alpha * 0.9375 ) ).a;
				clip( alphaRef - 0.01 );
				SHADOW_CASTER_FRAGMENT( IN )
			}
			ENDCG
		}
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=14401
2243;173;1391;632;-1209.039;1760.069;1;True;True
Node;AmplifyShaderEditor.TextureCoordinatesNode;8;-949.7549,-1422.429;Float;True;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;9;-538.8523,-1317.173;Float;True;2;2;0;FLOAT;0.0;False;1;FLOAT;0.0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;16;-120.5369,-1565.638;Float;False;Constant;_Color0;Color 0;3;0;Create;True;1,0,0.931035,0;0,0,0,0;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;20;-134.636,-1139.994;Float;False;Constant;_Color1;Color 1;3;0;Create;True;0,1,0.8758622,0;0,0,0,0;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.OneMinusNode;14;-317.951,-1528.314;Float;True;1;0;FLOAT;0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;31;330.5126,-948.3876;Float;False;Constant;_Float2;Float 2;4;0;Create;True;0.5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;17;146.7659,-1650.295;Float;True;2;2;0;FLOAT;0,0;False;1;COLOR;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;19;132.9641,-1276.018;Float;True;2;2;0;FLOAT;0,0;False;1;COLOR;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleTimeNode;30;611.2437,-945.2555;Float;False;1;0;FLOAT;1.0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;22;423.6838,-1402.498;Float;True;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RotatorNode;33;840.1835,-1128.319;Float;True;3;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;2;FLOAT;1.0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;41;1010.399,-858.9348;Float;False;Constant;_Float0;Float 0;4;0;Create;True;3;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;35;1256.194,-1587.266;Float;True;2;2;0;COLOR;0,0,0,0;False;1;FLOAT2;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;44;1291.607,-627.7979;Float;False;Property;_Color2;Color 2;1;0;Create;True;0,0.9586205,1,0;0,0.2965517,1,0;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SmoothstepOpNode;40;1388.678,-1017.158;Float;True;3;0;FLOAT;0.0;False;1;FLOAT;0,0,0,0;False;2;FLOAT;0.0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;43;1228.482,-364.8719;Float;True;Property;_TextureSample0;Texture Sample 0;0;0;Create;True;None;062a71cc1dd96c5488202d624203bbec;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0.0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1.0;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;45;1694.145,-571.8542;Float;True;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;52;2045.276,-561.4982;Float;False;Constant;_Float1;Float 1;4;0;Create;True;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;48;1788.22,-969.1276;Float;True;1;0;FLOAT;0.0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;50;2854.617,-1331.86;Float;True;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0.0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;51;2311.7,-772.9897;Float;True;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0.0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;64;3660.597,-622.2648;Float;False;Constant;_Color3;Color 3;3;0;Create;True;1,1,1,0;0,0,0,0;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;61;3453.973,-1022.898;Float;True;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0.0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;67;4191.967,-613.5358;Float;False;Constant;_Float4;Float 4;3;0;Create;True;2;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;63;3944.305,-776.1736;Float;True;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0.0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;36;1839.634,-1666.249;Float;True;Property;_alphatest;alpha test;2;0;Create;True;062a71cc1dd96c5488202d624203bbec;062a71cc1dd96c5488202d624203bbec;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0.0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1.0;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;68;4535.751,-792.7698;Float;True;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0.0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;37;4813.83,-1598.714;Float;True;2;2;0;COLOR;0.0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;5656.014,-779.84;Float;False;True;2;Float;ASEMaterialInspector;0;0;Standard;Grid;False;False;False;False;True;True;True;True;True;False;False;False;False;False;True;False;False;Back;0;0;False;0;0;False;0;Transparent;1;True;True;0;False;Transparent;;Transparent;ForwardOnly;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;False;0;255;255;0;0;0;0;0;0;0;0;False;2;15;10;25;False;0.5;True;2;SrcAlpha;OneMinusSrcAlpha;4;One;One;OFF;Add;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;0;0;False;0;0;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0.0;False;4;FLOAT;0.0;False;5;FLOAT;0.0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0.0;False;9;FLOAT;0.0;False;10;FLOAT;0.0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;9;0;8;1
WireConnection;14;0;9;0
WireConnection;17;0;14;0
WireConnection;17;1;16;0
WireConnection;19;0;9;0
WireConnection;19;1;20;0
WireConnection;30;0;31;0
WireConnection;22;0;17;0
WireConnection;22;1;19;0
WireConnection;33;0;22;0
WireConnection;33;2;30;0
WireConnection;35;0;22;0
WireConnection;35;1;33;0
WireConnection;40;0;35;0
WireConnection;40;1;41;0
WireConnection;45;0;44;0
WireConnection;45;1;43;0
WireConnection;48;0;40;0
WireConnection;50;0;35;0
WireConnection;50;1;48;0
WireConnection;51;0;45;0
WireConnection;51;1;52;0
WireConnection;61;0;50;0
WireConnection;61;1;51;0
WireConnection;63;0;61;0
WireConnection;63;1;64;0
WireConnection;68;0;63;0
WireConnection;68;1;67;0
WireConnection;37;0;35;0
WireConnection;37;1;36;0
WireConnection;0;0;50;0
WireConnection;0;2;68;0
WireConnection;0;9;43;1
ASEEND*/
//CHKSM=01A8B2C2272E9D51056FDDDA991AEFDEB4688D41