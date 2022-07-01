// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "ProgressBar_sin"
{
	Properties
	{
		_TintColor ("Tint Color", Color) = (0.5,0.5,0.5,0.5)
		_MainTex ("Particle Texture", 2D) = "white" {}
		_InvFade ("Soft Particles Factor", Range(0.01,3.0)) = 1.0
		_waves("waves", Float) = 6.55
		_amplitude("amplitude", Float) = 0.05
		_speed("speed", Float) = 1
		_progress("progress", Float) = 0.39
		[Toggle(_MASK_OVER_ON)] _Mask_over("Mask_over", Float) = 0
		_Color0("Color 0", Color) = (1,1,1,1)
		_OverMask("OverMask", 2D) = "white" {}
		_OverMaskEmission("OverMaskEmission", Float) = 0.3
		_Emision("Emision", Float) = 1
		_MainTexture("MainTexture", 2D) = "white" {}
		_OverMask_Tiling("OverMask_Tiling", Vector) = (0,0,0,0)
		_OverMask_Speed("OverMask_Speed", Vector) = (0,0,0,0)
		[HideInInspector] _texcoord( "", 2D ) = "white" {}

	}


	Category 
	{
		SubShader
		{
		LOD 0

			Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" }
			Blend SrcAlpha OneMinusSrcAlpha
			ColorMask RGB
			Cull Off
			Lighting Off 
			ZWrite Off
			ZTest LEqual
			
			Pass {
			
				CGPROGRAM
				
				#ifndef UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX
				#define UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input)
				#endif
				
				#pragma vertex vert
				#pragma fragment frag
				#pragma target 2.0
				#pragma multi_compile_instancing
				#pragma multi_compile_particles
				#pragma multi_compile_fog
				#include "UnityShaderVariables.cginc"
				#pragma shader_feature_local _MASK_OVER_ON


				#include "UnityCG.cginc"

				struct appdata_t 
				{
					float4 vertex : POSITION;
					fixed4 color : COLOR;
					float4 texcoord : TEXCOORD0;
					UNITY_VERTEX_INPUT_INSTANCE_ID
					
				};

				struct v2f 
				{
					float4 vertex : SV_POSITION;
					fixed4 color : COLOR;
					float4 texcoord : TEXCOORD0;
					UNITY_FOG_COORDS(1)
					#ifdef SOFTPARTICLES_ON
					float4 projPos : TEXCOORD2;
					#endif
					UNITY_VERTEX_INPUT_INSTANCE_ID
					UNITY_VERTEX_OUTPUT_STEREO
					
				};
				
				
				#if UNITY_VERSION >= 560
				UNITY_DECLARE_DEPTH_TEXTURE( _CameraDepthTexture );
				#else
				uniform sampler2D_float _CameraDepthTexture;
				#endif

				//Don't delete this comment
				// uniform sampler2D_float _CameraDepthTexture;

				uniform sampler2D _MainTex;
				uniform fixed4 _TintColor;
				uniform float4 _MainTex_ST;
				uniform float _InvFade;
				uniform sampler2D _MainTexture;
				uniform float4 _MainTexture_ST;
				uniform float _OverMaskEmission;
				uniform sampler2D _OverMask;
				uniform float2 _OverMask_Speed;
				uniform float2 _OverMask_Tiling;
				uniform float4 _Color0;
				uniform float _Emision;
				uniform float _amplitude;
				uniform float _progress;
				uniform float _waves;
				uniform float _speed;


				v2f vert ( appdata_t v  )
				{
					v2f o;
					UNITY_SETUP_INSTANCE_ID(v);
					UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
					UNITY_TRANSFER_INSTANCE_ID(v, o);
					

					v.vertex.xyz +=  float3( 0, 0, 0 ) ;
					o.vertex = UnityObjectToClipPos(v.vertex);
					#ifdef SOFTPARTICLES_ON
						o.projPos = ComputeScreenPos (o.vertex);
						COMPUTE_EYEDEPTH(o.projPos.z);
					#endif
					o.color = v.color;
					o.texcoord = v.texcoord;
					UNITY_TRANSFER_FOG(o,o.vertex);
					return o;
				}

				fixed4 frag ( v2f i  ) : SV_Target
				{
					UNITY_SETUP_INSTANCE_ID( i );
					UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( i );

					#ifdef SOFTPARTICLES_ON
						float sceneZ = LinearEyeDepth (SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(i.projPos)));
						float partZ = i.projPos.z;
						float fade = saturate (_InvFade * (sceneZ-partZ));
						i.color.a *= fade;
					#endif

					float2 uv_MainTexture = i.texcoord.xy * _MainTexture_ST.xy + _MainTexture_ST.zw;
					float4 tex2DNode25 = tex2D( _MainTexture, uv_MainTexture );
					float2 uv028 = i.texcoord.xy * _OverMask_Tiling + float2( 2,0 );
					float2 panner29 = ( 1.0 * _Time.y * _OverMask_Speed + uv028);
					float temp_output_37_0 = saturate( ( _OverMaskEmission * tex2D( _OverMask, panner29 ).g ) );
					float4 temp_cast_0 = (temp_output_37_0).xxxx;
					float4 lerpResult38 = lerp( tex2DNode25 , temp_cast_0 , ( temp_output_37_0 * tex2DNode25 ));
					#ifdef _MASK_OVER_ON
					float4 staticSwitch39 = ( lerpResult38 * _Color0 * _Emision );
					#else
					float4 staticSwitch39 = tex2DNode25;
					#endif
					float2 uv01 = i.texcoord.xy * float2( 1,1 ) + float2( 0,0 );
					float temp_output_13_0 = ( _amplitude * 2.0 );
					float lerpResult16 = lerp( ( 0.0 - temp_output_13_0 ) , ( 1.0 + temp_output_13_0 ) , _progress);
					

					fixed4 col = ( staticSwitch39 * ceil( saturate( ( ( 1.0 - ( uv01.x - lerpResult16 ) ) + ( sin( ( ( uv01.y * _waves ) + ( _Time.y * _speed ) ) ) * _amplitude ) ) ) ) );
					UNITY_APPLY_FOG(i.fogCoord, col);
					return col;
				}
				ENDCG 
			}
		}	
	}
	CustomEditor "ASEMaterialInspector"
	
	
}
/*ASEBEGIN
Version=18100
0;240;1536;563;946.6404;1150.737;2.843952;True;True
Node;AmplifyShaderEditor.Vector2Node;44;93.27588,-1069.772;Inherit;False;Property;_OverMask_Tiling;OverMask_Tiling;10;0;Create;True;0;0;False;0;False;0,0;13.03,2;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.RangedFloatNode;3;-1571.678,317.4276;Inherit;False;Property;_waves;waves;0;0;Create;True;0;0;False;0;False;6.55;6.88;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;1;-1742.214,-589.7708;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.Vector2Node;45;435.2759,-884.772;Inherit;False;Property;_OverMask_Speed;OverMask_Speed;11;0;Create;True;0;0;False;0;False;0,0;0,-0.8;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.RangedFloatNode;7;-316.8505,391.5767;Inherit;False;Property;_amplitude;amplitude;1;0;Create;True;0;0;False;0;False;0.05;0.01;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;28;291.3524,-1068.641;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;10,2;False;1;FLOAT2;2,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;13;-913.8762,69.38953;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;2;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;17;-1078.727,-120.9824;Inherit;False;Constant;_Float0;Float 0;1;0;Create;True;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;18;-950.2825,-68.65347;Inherit;False;Constant;_Float1;Float 1;1;0;Create;True;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;5;-1608.284,452.5953;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;8;-1351.845,219.7702;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;6;-1602.819,577.895;Inherit;False;Property;_speed;speed;2;0;Create;True;0;0;False;0;False;1;7;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.PannerNode;29;596.6908,-1004.084;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,-0.81;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.ComponentMaskNode;11;-1150.771,183.7826;Inherit;True;True;True;True;True;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;14;-794.036,-191.5269;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;4;-1304.227,488.6616;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;24;-823.9421,-346.9742;Inherit;False;Property;_progress;progress;3;0;Create;True;0;0;False;0;False;0.39;-0.42;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;30;837.472,-1051.193;Inherit;True;Property;_OverMask;OverMask;6;0;Create;True;0;0;False;0;False;-1;None;68f00f87b51393441a4fb7564aab9728;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;36;1354.683,-872.5068;Inherit;False;Property;_OverMaskEmission;OverMaskEmission;7;0;Create;True;0;0;False;0;False;0.3;1.28;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;15;-662.0608,-71.68742;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;35;1518.793,-774.101;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;16;-439.0684,-235.5185;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;10;-897.77,312.5249;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;37;1689.658,-905.0652;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;25;1064.28,-586.615;Inherit;True;Property;_MainTexture;MainTexture;9;0;Create;True;0;0;False;0;False;-1;None;54917aa487d8351448c354257126d27e;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SinOpNode;9;-593.6376,320.8444;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;20;-181.2776,-664.5483;Inherit;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;12;-209.2556,57.59904;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;27;101.8106,-713.1099;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;31;1863.291,-752.3146;Inherit;True;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;34;2399.804,-1049.263;Inherit;False;Property;_Color0;Color 0;5;0;Create;True;0;0;False;0;False;1,1,1,1;1,1,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;38;2156.509,-977.0717;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;42;2362.269,-652.8328;Inherit;False;Property;_Emision;Emision;8;0;Create;True;0;0;False;0;False;1;1.1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;21;214.1284,-321.9456;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;41;2672.348,-814.4994;Inherit;False;3;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SaturateNode;22;491.7059,-317.3575;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;39;3077.918,-563.739;Inherit;False;Property;_Mask_over;Mask_over;4;0;Create;True;0;0;False;0;False;0;0;1;True;;Toggle;2;Key0;Key1;Create;True;9;1;COLOR;0,0,0,0;False;0;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;5;COLOR;0,0,0,0;False;6;COLOR;0,0,0,0;False;7;COLOR;0,0,0,0;False;8;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.CeilOpNode;23;780.8516,-271.7549;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SinTimeNode;19;-628.688,-402.3836;Inherit;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TemplateShaderPropertyNode;48;677.6053,-651.7884;Inherit;False;0;0;_MainTex;Shader;0;5;SAMPLER2D;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;26;2324.907,-307.5186;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.TemplateShaderPropertyNode;49;580.6949,-1143.092;Inherit;False;0;0;_MainTex;Shader;0;5;SAMPLER2D;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;0;2912.274,-307.9059;Float;False;True;-1;2;ASEMaterialInspector;0;7;ProgressBar_sin;0b6a9f8b4f707c74ca64c0be8e590de0;True;SubShader 0 Pass 0;0;0;SubShader 0 Pass 0;2;True;2;5;False;-1;10;False;-1;0;1;False;-1;0;False;-1;False;False;True;2;False;-1;True;True;True;True;False;0;False;-1;False;True;2;False;-1;True;3;False;-1;False;True;4;Queue=Transparent=Queue=0;IgnoreProjector=True;RenderType=Transparent=RenderType;PreviewType=Plane;False;0;False;False;False;False;False;False;False;False;False;False;True;0;0;;0;0;Standard;0;0;1;True;False;;0
WireConnection;28;0;44;0
WireConnection;13;0;7;0
WireConnection;8;0;1;2
WireConnection;8;1;3;0
WireConnection;29;0;28;0
WireConnection;29;2;45;0
WireConnection;11;0;8;0
WireConnection;14;0;17;0
WireConnection;14;1;13;0
WireConnection;4;0;5;0
WireConnection;4;1;6;0
WireConnection;30;1;29;0
WireConnection;15;0;18;0
WireConnection;15;1;13;0
WireConnection;35;0;36;0
WireConnection;35;1;30;2
WireConnection;16;0;14;0
WireConnection;16;1;15;0
WireConnection;16;2;24;0
WireConnection;10;0;11;0
WireConnection;10;1;4;0
WireConnection;37;0;35;0
WireConnection;9;0;10;0
WireConnection;20;0;1;1
WireConnection;20;1;16;0
WireConnection;12;0;9;0
WireConnection;12;1;7;0
WireConnection;27;0;20;0
WireConnection;31;0;37;0
WireConnection;31;1;25;0
WireConnection;38;0;25;0
WireConnection;38;1;37;0
WireConnection;38;2;31;0
WireConnection;21;0;27;0
WireConnection;21;1;12;0
WireConnection;41;0;38;0
WireConnection;41;1;34;0
WireConnection;41;2;42;0
WireConnection;22;0;21;0
WireConnection;39;1;25;0
WireConnection;39;0;41;0
WireConnection;23;0;22;0
WireConnection;26;0;39;0
WireConnection;26;1;23;0
WireConnection;0;0;26;0
ASEEND*/
//CHKSM=1FA496EE86F7253C7BE4F9081B8C41E2B847ECF0