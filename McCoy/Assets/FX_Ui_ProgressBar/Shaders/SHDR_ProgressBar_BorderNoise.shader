// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "ProgressBar_BorderNoise"
{
	Properties
	{
		_TintColor ("Tint Color", Color) = (0.5,0.5,0.5,0.5)
		_MainTex ("Particle Texture", 2D) = "white" {}
		_InvFade ("Soft Particles Factor", Range(0.01,3.0)) = 1.0
		_DissolveEdge("DissolveEdge", Float) = -0.49
		_Noise("Noise", 2D) = "white" {}
		_ProgressBar("ProgressBar", Float) = 1
		_MainTexture("MainTexture", 2D) = "white" {}
		_OverMask("OverMask", 2D) = "white" {}
		[Toggle(_MASK_OVER_ON)] _Mask_over("Mask_over", Float) = 0
		_ColorMask("ColorMask", Color) = (1,1,1,1)
		_OverMaskEmission("OverMaskEmission", Float) = 0.3
		_Emision("Emision", Float) = 1
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
				uniform float _Emision;
				uniform float4 _ColorMask;
				uniform float _DissolveEdge;
				uniform float _ProgressBar;
				uniform sampler2D _Noise;
				uniform float4 _Noise_ST;


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
					float4 tex2DNode14 = tex2D( _MainTexture, uv_MainTexture );
					float2 uv023 = i.texcoord.xy * _OverMask_Tiling + float2( 2,0 );
					float2 panner24 = ( 1.0 * _Time.y * _OverMask_Speed + uv023);
					float temp_output_27_0 = saturate( ( _OverMaskEmission * tex2D( _OverMask, panner24 ).r ) );
					float4 temp_cast_0 = (temp_output_27_0).xxxx;
					float4 lerpResult29 = lerp( tex2DNode14 , temp_cast_0 , ( temp_output_27_0 * tex2DNode14 ));
					#ifdef _MASK_OVER_ON
					float4 staticSwitch31 = ( lerpResult29 * _Emision * _ColorMask );
					#else
					float4 staticSwitch31 = tex2DNode14;
					#endif
					float2 uv02 = i.texcoord.xy * float2( 1,1 ) + float2( 0,0 );
					float smoothstepResult4 = smoothstep( uv02.x , ( uv02.x + _DissolveEdge ) , ( 0.4 * ( _ProgressBar + _DissolveEdge ) ));
					float temp_output_10_0 = saturate( smoothstepResult4 );
					float2 uv_Noise = i.texcoord.xy * _Noise_ST.xy + _Noise_ST.zw;
					float lerpResult5 = lerp( tex2D( _Noise, uv_Noise ).r , temp_output_10_0 , temp_output_10_0);
					

					fixed4 col = ( staticSwitch31 * saturate( ( temp_output_10_0 * lerpResult5 ) ) );
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
112.8;220;1536;556;2271.063;1281.185;2.569589;True;True
Node;AmplifyShaderEditor.Vector2Node;21;-1691.943,-1257.313;Inherit;False;Property;_OverMask_Tiling;OverMask_Tiling;9;0;Create;True;0;0;False;0;False;0,0;10,2;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.Vector2Node;22;-1349.943,-1072.313;Inherit;False;Property;_OverMask_Speed;OverMask_Speed;10;0;Create;True;0;0;False;0;False;0,0;0,-0.7;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.TextureCoordinatesNode;23;-1493.867,-1256.182;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;10,2;False;1;FLOAT2;2,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;7;-1100.778,289.7155;Inherit;False;Property;_DissolveEdge;DissolveEdge;0;0;Create;True;0;0;False;0;False;-0.49;0.12;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;11;-1014.856,442.0817;Inherit;False;Property;_ProgressBar;ProgressBar;2;0;Create;True;0;0;False;0;False;1;2.17;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.PannerNode;24;-1188.528,-1191.625;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,-0.81;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;26;-725.4043,-969.95;Inherit;False;Property;_OverMaskEmission;OverMaskEmission;7;0;Create;True;0;0;False;0;False;0.3;0.4;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;25;-947.7473,-1238.734;Inherit;True;Property;_OverMask;OverMask;4;0;Create;True;0;0;False;0;False;-1;None;e199ee8112362864f831425463b09fda;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;12;-823.1754,274.5262;Inherit;False;Constant;_Float1;Float 1;1;0;Create;True;0;0;False;0;False;0.4;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;9;-682.4479,397.8178;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;2;-1241.587,-88.64677;Inherit;True;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;3;-822.9228,64.63546;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;8;-587.6066,245.8537;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;28;-508.0543,-887.9255;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;14;-676.7985,-370.5701;Inherit;True;Property;_MainTexture;MainTexture;3;0;Create;True;0;0;False;0;False;-1;29ff51df72b94ab449fc0666f9fd8d71;54917aa487d8351448c354257126d27e;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SaturateNode;27;-365.8573,-1051.652;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode;4;-512.3105,-3.82571;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;10;-288.9117,-6.928516;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;30;-259.0023,-734.9744;Inherit;True;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;13;-401.6294,338.5388;Inherit;True;Property;_Noise;Noise;1;0;Create;True;0;0;False;0;False;-1;85b544dc6e75c0a4f891ca44a816b09a;89f1cb2bc600fd0418bf263ac3f3931d;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;29;-50.5354,-984.416;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;18;142.9387,-553.6968;Inherit;False;Property;_Emision;Emision;8;0;Create;True;0;0;False;0;False;1;1.19;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;5;-97.43449,165.1564;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;19;244.8988,-1022.605;Inherit;False;Property;_ColorMask;ColorMask;6;0;Create;True;0;0;False;0;False;1,1,1,1;1,1,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;20;453.0176,-715.3633;Inherit;False;3;3;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;6;115.5089,89.93906;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;16;394.6473,146.4366;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;31;488.9405,-383.1383;Inherit;False;Property;_Mask_over;Mask_over;5;0;Create;True;0;0;False;0;False;0;0;1;True;;Toggle;2;Key0;Key1;Create;True;9;1;COLOR;0,0,0,0;False;0;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;5;COLOR;0,0,0,0;False;6;COLOR;0,0,0,0;False;7;COLOR;0,0,0,0;False;8;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.CeilOpNode;17;629.0483,277.1976;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;15;552.8194,-82.85507;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;0;763.9775,-157.9642;Float;False;True;-1;2;ASEMaterialInspector;0;7;ProgressBar_BorderNoise;0b6a9f8b4f707c74ca64c0be8e590de0;True;SubShader 0 Pass 0;0;0;SubShader 0 Pass 0;2;True;2;5;False;-1;10;False;-1;0;1;False;-1;0;False;-1;False;False;True;2;False;-1;True;True;True;True;False;0;False;-1;False;True;2;False;-1;True;3;False;-1;False;True;4;Queue=Transparent=Queue=0;IgnoreProjector=True;RenderType=Transparent=RenderType;PreviewType=Plane;False;0;False;False;False;False;False;False;False;False;False;False;True;0;0;;0;0;Standard;0;0;1;True;False;;0
WireConnection;23;0;21;0
WireConnection;24;0;23;0
WireConnection;24;2;22;0
WireConnection;25;1;24;0
WireConnection;9;0;11;0
WireConnection;9;1;7;0
WireConnection;3;0;2;1
WireConnection;3;1;7;0
WireConnection;8;0;12;0
WireConnection;8;1;9;0
WireConnection;28;0;26;0
WireConnection;28;1;25;1
WireConnection;27;0;28;0
WireConnection;4;0;8;0
WireConnection;4;1;2;1
WireConnection;4;2;3;0
WireConnection;10;0;4;0
WireConnection;30;0;27;0
WireConnection;30;1;14;0
WireConnection;29;0;14;0
WireConnection;29;1;27;0
WireConnection;29;2;30;0
WireConnection;5;0;13;1
WireConnection;5;1;10;0
WireConnection;5;2;10;0
WireConnection;20;0;29;0
WireConnection;20;1;18;0
WireConnection;20;2;19;0
WireConnection;6;0;10;0
WireConnection;6;1;5;0
WireConnection;16;0;6;0
WireConnection;31;1;14;0
WireConnection;31;0;20;0
WireConnection;15;0;31;0
WireConnection;15;1;16;0
WireConnection;0;0;15;0
ASEEND*/
//CHKSM=397F2FB37A052A6DECDC2356D52B06AA7BCC4824