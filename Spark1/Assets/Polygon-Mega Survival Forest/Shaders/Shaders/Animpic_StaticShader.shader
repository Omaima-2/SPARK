// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Animpic/StaticShader"
{
	Properties
	{
		[SingleLineTexture][Header(Maps)][Space(7)]_OcclusionTexture("OcclusionTexture", 2D) = "white" {}
		[SingleLineTexture][Header(Maps)][Space(7)]_SurfaceAlbedo("Albedo", 2D) = "white" {}
		[Normal][SingleLineTexture]_SurfaceNormal("Normal", 2D) = "bump" {}
		[Header(Settings)][Space(5)]_SurfaceColor("Color", Color) = (1,1,1,0)
		_SurfaceSmoothness("Smoothness", Range( 0 , 1)) = 0
		[Header(Coverage)][Space(5)][Toggle(_COVERAGE_ON)] _COVERAGE("Enable", Float) = 0
		[SingleLineTexture]_CoverageAlbedo("Coverage Albedo", 2D) = "white" {}
		[Normal][SingleLineTexture]_CoverageNormal("Coverage Normal", 2D) = "gray" {}
		[SingleLineTexture]_CoverageMask("Coverage Mask", 2D) = "white" {}
		[KeywordEnum(World_Normal,Vertex_Position)] _CoverageOverlayType("Overlay Method", Float) = 0
		_CoverageColor("Color", Color) = (0,0,0,0)
		_TextureTileScale("TextureTileScale", Range( 0 , 300)) = 1
		_CoverageTileScale("Coverage Tile Scale", Range( 0 , 300)) = 0
		_CoverageMaskScale("Coverage Mask Scale", Range( 0 , 300)) = 0
		_CoverageLevel("Level", Float) = 0
		_CoverageFade("Fade", Range( -1 , 1)) = 0.5
		_CoverageSmoothness("Smoothness", Range( 0 , 1)) = 0
		_CoverageContrast("Contrast", Range( 0.03 , 1)) = 0.3
		_CoverageThicknessLevel("Thickness", Range( 0 , 1)) = 1
		[Toggle(_BLENDNORMALS_ON)] _BlendNormals("Blend Normals", Float) = 0
		[Header(Show)][Space(5)][Toggle(_SNOW_ON)] _Snow("Enable", Float) = 0
		[KeywordEnum(Vertex_Group,UV_Group)] _SnowOverlay("Snow Overlay", Float) = 0
		_TextureTile("TextureTile", Vector) = (1,1,0,0)
		[SingleLineTexture]_SnowTexture("Snow Texture", 2D) = "white" {}
		_SnowColor("Snow Color", Color) = (0.9,0.9,0.9,0)
		_SnowTiling1("Snow Tiling", Float) = 1
		_SnowLevel("SnowLevel", Float) = 0.59
		_SnowFade1("SnowFade", Range( -2 , 2)) = 0.3647059
		[HideInInspector] _texcoord2( "", 2D ) = "white" {}
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" }
		Cull Back
		CGPROGRAM
		#include "UnityStandardUtils.cginc"
		#include "UnityShaderVariables.cginc"
		#pragma target 3.0
		#pragma shader_feature_local _COVERAGE_ON
		#pragma shader_feature_local _BLENDNORMALS_ON
		#pragma shader_feature_local _COVERAGEOVERLAYTYPE_WORLD_NORMAL _COVERAGEOVERLAYTYPE_VERTEX_POSITION
		#pragma shader_feature_local _SNOW_ON
		#pragma shader_feature_local _SNOWOVERLAY_VERTEX_GROUP _SNOWOVERLAY_UV_GROUP
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows nometa noforwardadd dithercrossfade 
		struct Input
		{
			float2 uv_texcoord;
			float2 uv2_texcoord2;
			float3 worldPos;
			float3 worldNormal;
			INTERNAL_DATA
		};

		uniform sampler2D _SurfaceNormal;
		uniform float2 _TextureTile;
		uniform float _TextureTileScale;
		uniform sampler2D _CoverageNormal;
		uniform float _CoverageTileScale;
		uniform float _CoverageLevel;
		uniform float _CoverageFade;
		uniform sampler2D _CoverageMask;
		uniform float _CoverageMaskScale;
		uniform float _CoverageContrast;
		uniform float _CoverageThicknessLevel;
		uniform float4 _SurfaceColor;
		uniform sampler2D _SurfaceAlbedo;
		uniform float4 _CoverageColor;
		uniform sampler2D _CoverageAlbedo;
		uniform float4 _SnowColor;
		uniform float _SnowLevel;
		uniform sampler2D _SnowTexture;
		uniform float _SnowTiling1;
		uniform float _SnowFade1;
		uniform float _SurfaceSmoothness;
		uniform float _CoverageSmoothness;
		uniform sampler2D _OcclusionTexture;
		uniform float4 _OcclusionTexture_ST;


		float3 PerturbNormal107_g4( float3 surf_pos, float3 surf_norm, float height, float scale )
		{
			// "Bump Mapping Unparametrized Surfaces on the GPU" by Morten S. Mikkelsen
			float3 vSigmaS = ddx( surf_pos );
			float3 vSigmaT = ddy( surf_pos );
			float3 vN = surf_norm;
			float3 vR1 = cross( vSigmaT , vN );
			float3 vR2 = cross( vN , vSigmaS );
			float fDet = dot( vSigmaS , vR1 );
			float dBs = ddx( height );
			float dBt = ddy( height );
			float3 vSurfGrad = scale * 0.05 * sign( fDet ) * ( dBs * vR1 + dBt * vR2 );
			return normalize ( abs( fDet ) * vN - vSurfGrad );
		}


		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_TexCoord204 = i.uv_texcoord * ( _TextureTile * _TextureTileScale );
			float3 tex2DNode6 = UnpackNormal( tex2D( _SurfaceNormal, uv_TexCoord204 ) );
			float2 temp_output_181_0 = ( float2( 1,1 ) * _CoverageTileScale );
			float2 uv2_TexCoord179 = i.uv2_texcoord2 * temp_output_181_0;
			float3 tex2DNode72 = UnpackNormal( tex2D( _CoverageNormal, uv2_TexCoord179 ) );
			float3 temp_output_97_0 = BlendNormals( tex2DNode6 , tex2DNode72 );
			#ifdef _BLENDNORMALS_ON
				float3 staticSwitch158 = temp_output_97_0;
			#else
				float3 staticSwitch158 = tex2DNode72;
			#endif
			#ifdef _BLENDNORMALS_ON
				float3 staticSwitch160 = temp_output_97_0;
			#else
				float3 staticSwitch160 = tex2DNode72;
			#endif
			float3 ase_worldPos = i.worldPos;
			float3 surf_pos107_g4 = ase_worldPos;
			float3 ase_worldNormal = WorldNormalVector( i, float3( 0, 0, 1 ) );
			float3 surf_norm107_g4 = ase_worldNormal;
			float3 ase_vertex3Pos = mul( unity_WorldToObject, float4( i.worldPos , 1 ) );
			#if defined(_COVERAGEOVERLAYTYPE_WORLD_NORMAL)
				float staticSwitch164 = ase_worldNormal.y;
			#elif defined(_COVERAGEOVERLAYTYPE_VERTEX_POSITION)
				float staticSwitch164 = ase_vertex3Pos.y;
			#else
				float staticSwitch164 = ase_worldNormal.y;
			#endif
			float2 uv2_TexCoord187 = i.uv2_texcoord2 * ( float2( 1,1 ) * _CoverageMaskScale );
			float CoverageMask37 = saturate( ( ( ( ( staticSwitch164 + _CoverageLevel ) * ( _CoverageFade * 5 ) ) + tex2D( _CoverageMask, uv2_TexCoord187 ).r ) * ( _CoverageContrast * 15 ) ) );
			float height107_g4 = ( CoverageMask37 * ( _CoverageThicknessLevel * 10 ) );
			float scale107_g4 = 1.0;
			float3 localPerturbNormal107_g4 = PerturbNormal107_g4( surf_pos107_g4 , surf_norm107_g4 , height107_g4 , scale107_g4 );
			float3 ase_worldTangent = WorldNormalVector( i, float3( 1, 0, 0 ) );
			float3 ase_worldBitangent = WorldNormalVector( i, float3( 0, 1, 0 ) );
			float3x3 ase_worldToTangent = float3x3( ase_worldTangent, ase_worldBitangent, ase_worldNormal );
			float3 worldToTangentDir42_g4 = mul( ase_worldToTangent, localPerturbNormal107_g4);
			float3 lerpResult73 = lerp( tex2DNode6 , ( staticSwitch158 + BlendNormals( staticSwitch160 , worldToTangentDir42_g4 ) ) , CoverageMask37);
			#ifdef _COVERAGE_ON
				float3 staticSwitch156 = lerpResult73;
			#else
				float3 staticSwitch156 = tex2DNode6;
			#endif
			float3 Normal75 = staticSwitch156;
			o.Normal = Normal75;
			float4 temp_output_3_0 = ( _SurfaceColor * tex2D( _SurfaceAlbedo, uv_TexCoord204 ) );
			float4 lerpResult26 = lerp( temp_output_3_0 , ( _CoverageColor * tex2D( _CoverageAlbedo, uv2_TexCoord179 ) ) , CoverageMask37);
			#ifdef _COVERAGE_ON
				float4 staticSwitch136 = lerpResult26;
			#else
				float4 staticSwitch136 = temp_output_3_0;
			#endif
			#if defined(_SNOWOVERLAY_VERTEX_GROUP)
				float staticSwitch192 = (WorldNormalVector( i , Normal75 )).y;
			#elif defined(_SNOWOVERLAY_UV_GROUP)
				float staticSwitch192 = i.uv2_texcoord2.y;
			#else
				float staticSwitch192 = (WorldNormalVector( i , Normal75 )).y;
			#endif
			float2 temp_cast_0 = (_SnowTiling1).xx;
			float2 uv2_TexCoord191 = i.uv2_texcoord2 * temp_cast_0;
			float4 SnowMask149 = ( 1.0 - saturate( ( ( staticSwitch192 + _SnowLevel + tex2D( _SnowTexture, uv2_TexCoord191 ) ) * ( _SnowFade1 * 2 ) ) ) );
			float4 lerpResult154 = lerp( staticSwitch136 , _SnowColor , SnowMask149);
			#ifdef _SNOW_ON
				float4 staticSwitch155 = lerpResult154;
			#else
				float4 staticSwitch155 = staticSwitch136;
			#endif
			float4 Albedo19 = staticSwitch155;
			o.Albedo = Albedo19.rgb;
			float lerpResult70 = lerp( _SurfaceSmoothness , _CoverageSmoothness , CoverageMask37);
			#ifdef _COVERAGE_ON
				float staticSwitch157 = lerpResult70;
			#else
				float staticSwitch157 = _SurfaceSmoothness;
			#endif
			float Smoothness76 = staticSwitch157;
			o.Smoothness = Smoothness76;
			float2 uv_OcclusionTexture = i.uv_texcoord * _OcclusionTexture_ST.xy + _OcclusionTexture_ST.zw;
			o.Occlusion = tex2D( _OcclusionTexture, uv_OcclusionTexture ).r;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
}
/*ASEBEGIN
Version=18935
919.2453;156.6792;1049.66;616.5849;5546.068;727.2517;1;True;False
Node;AmplifyShaderEditor.CommentaryNode;40;-4236.528,991.4641;Inherit;False;1846.116;876.1783;;17;37;29;172;65;168;67;133;171;128;12;129;164;163;9;178;186;187;Coverage Mask;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;184;-4391.426,1695.911;Inherit;False;Property;_CoverageMaskScale;Coverage Mask Scale;13;0;Create;True;0;0;0;False;0;False;0;0;0;300;0;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;185;-4385.439,1564.307;Inherit;False;Constant;_Tile1;Tile;19;0;Create;True;0;0;0;False;0;False;1,1;1,1;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.PosVertexDataNode;163;-4152.777,1234.433;Inherit;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.WorldNormalVector;9;-4162.069,1066.901;Inherit;False;False;1;0;FLOAT3;0,0,1;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;12;-3888.225,1469.237;Inherit;False;Property;_CoverageFade;Fade;15;0;Create;False;0;0;0;False;0;False;0.5;-0.02;-1;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;129;-3799.043,1345.244;Inherit;False;Property;_CoverageLevel;Level;14;0;Create;False;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;164;-3909.553,1176.687;Inherit;False;Property;_CoverageOverlayType;Overlay Method;9;0;Create;False;0;0;0;False;0;False;0;0;0;True;;KeywordEnum;2;World_Normal;Vertex_Position;Create;True;True;All;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;186;-4108.229,1602.432;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;128;-3564.458,1251.746;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ScaleNode;171;-3583.102,1473.308;Inherit;False;5;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;187;-3944.152,1579.525;Inherit;False;1;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;40,40;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;65;-3480.072,1761.062;Inherit;False;Property;_CoverageContrast;Contrast;17;0;Create;False;0;0;0;False;0;False;0.3;1;0.03;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;133;-3373.707,1347.685;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;67;-3529.404,1561.653;Inherit;True;Property;_CoverageMask;Coverage Mask;8;1;[SingleLineTexture];Create;True;0;0;0;False;0;False;-1;None;None;True;1;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;1;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ScaleNode;172;-3194.476,1765.348;Inherit;False;15;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;168;-3139.477,1480.969;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;178;-2986.466,1590.994;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;180;-4997.87,-23.8998;Inherit;False;Constant;_Tile;Tile;19;0;Create;True;0;0;0;False;0;False;1,1;1,1;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.RangedFloatNode;206;-5029.383,-415.559;Inherit;False;Property;_TextureTileScale;TextureTileScale;11;0;Create;True;0;0;0;False;0;False;1;0;0;300;0;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;202;-4905.398,-595.4598;Inherit;False;Property;_TextureTile;TextureTile;22;0;Create;True;0;0;0;False;0;False;1,1;1,1;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.RangedFloatNode;182;-4999.87,104.1002;Inherit;False;Property;_CoverageTileScale;Coverage Tile Scale;12;0;Create;True;0;0;0;False;0;False;0;0;0;300;0;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;29;-2785.624,1591.101;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;205;-4681.254,-489.6626;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.CommentaryNode;80;-4239.22,82.26685;Inherit;False;2475.836;823.7727;;16;96;173;75;156;73;94;159;158;92;160;86;97;95;6;72;74;Normal;1,1,1,1;0;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;181;-4748.87,3.100204;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;179;-4562.657,-141.819;Inherit;False;1;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;40,40;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;96;-4185.028,784.5165;Inherit;False;Property;_CoverageThicknessLevel;Thickness;18;0;Create;False;0;0;0;False;0;False;1;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;204;-4522.903,-604.7146;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;40,40;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RegisterLocalVarNode;37;-2608.598,1587.197;Inherit;False;CoverageMask;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;6;-4101.941,183.3497;Inherit;True;Property;_SurfaceNormal;Normal;2;2;[Normal];[SingleLineTexture];Create;False;0;0;0;False;0;False;-1;None;None;True;1;True;bump;Auto;True;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ScaleNode;173;-3896.309,791.2147;Inherit;False;10;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;74;-3946.92,678.7665;Inherit;False;37;CoverageMask;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;72;-4099.849,384.6438;Inherit;True;Property;_CoverageNormal;Coverage Normal;7;2;[Normal];[SingleLineTexture];Create;True;0;0;0;False;0;False;-1;None;None;True;1;True;gray;Auto;True;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.BlendNormalsNode;97;-3741.605,289.0083;Inherit;False;0;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;95;-3708.03,725.5165;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;160;-3484.625,599.2416;Inherit;False;Property;_BlendNormals;Blend Normals;19;0;Create;True;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Reference;158;True;True;All;9;1;FLOAT3;0,0,0;False;0;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT3;0,0,0;False;5;FLOAT3;0,0,0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.FunctionNode;86;-3520.594,726.2181;Inherit;False;Normal From Height;-1;;4;1942fe2c5f1a1f94881a33d532e4afeb;0;2;20;FLOAT;0;False;110;FLOAT;1;False;2;FLOAT3;40;FLOAT3;0
Node;AmplifyShaderEditor.StaticSwitch;158;-2913.515,472.0008;Inherit;False;Property;_BlendNormals;Blend Normals;19;0;Create;True;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;FLOAT3;0,0,0;False;0;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT3;0,0,0;False;5;FLOAT3;0,0,0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.BlendNormalsNode;92;-3190.313,658.4233;Inherit;False;0;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;94;-2704.498,354.6852;Inherit;False;37;CoverageMask;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;159;-2616.252,539.8761;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.LerpOp;73;-2453.019,295.6774;Inherit;False;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.StaticSwitch;156;-2279.687,179.6532;Inherit;False;Property;_Keyword0;Keyword 0;5;0;Create;True;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Reference;136;True;True;All;9;1;FLOAT3;0,0,0;False;0;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT3;0,0,0;False;5;FLOAT3;0,0,0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.CommentaryNode;141;-4230.377,1963.836;Inherit;False;1817.06;911.9388;;15;200;199;198;197;196;193;194;195;191;188;143;192;190;145;149;Snow Mask;1,1,1,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;75;-1990.311,181.0033;Inherit;False;Normal;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;188;-4134.77,2616.005;Inherit;False;Property;_SnowTiling1;Snow Tiling;25;0;Create;True;0;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;143;-4190.989,2023.263;Inherit;False;75;Normal;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;190;-4006.799,2255.197;Inherit;False;1;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TextureCoordinatesNode;191;-3969.599,2597.005;Inherit;False;1;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.WorldNormalVector;145;-4011.881,2027.824;Inherit;True;False;1;0;FLOAT3;0,0,0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;195;-3709.86,2458.522;Inherit;False;Property;_SnowFade1;SnowFade;27;0;Create;True;0;0;0;False;0;False;0.3647059;0.8;-2;2;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;193;-3733.882,2568.772;Inherit;True;Property;_SnowTexture;Snow Texture;23;1;[SingleLineTexture];Create;True;0;0;0;False;0;False;-1;ab397318286c3d3408185b3651c4a5eb;ab397318286c3d3408185b3651c4a5eb;True;1;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;194;-3602.52,2377.944;Inherit;False;Property;_SnowLevel;SnowLevel;26;0;Create;False;0;0;0;False;0;False;0.59;-0.24;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;192;-3716.999,2199.733;Inherit;False;Property;_SnowOverlay;Snow Overlay;21;0;Create;False;0;0;0;False;0;False;0;0;0;True;;KeywordEnum;2;Vertex_Group;UV_Group;Create;True;True;All;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;196;-3427.006,2204.655;Inherit;True;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ScaleNode;197;-3410.201,2459.808;Inherit;True;2;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;41;-4238.76,-899.634;Inherit;False;2098.106;900.623;;15;19;155;154;138;136;153;26;46;3;42;2;1;45;24;207;Albedo;1,1,1,1;0;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;198;-3217.575,2352.213;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;45;-4072.99,-431.8915;Inherit;False;Property;_CoverageColor;Color;10;0;Create;False;0;0;0;False;0;False;0,0,0,0;0.2075471,0.2075471,0.2075471,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;1;-4077,-815.2811;Inherit;False;Property;_SurfaceColor;Color;3;0;Create;False;0;0;0;False;2;Header(Settings);Space(5);False;1,1,1,0;0.65,0.5223636,0.4549999,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;24;-4157.983,-247.7908;Inherit;True;Property;_CoverageAlbedo;Coverage Albedo;6;1;[SingleLineTexture];Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SaturateNode;199;-3025.879,2365.759;Inherit;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;2;-4160.389,-631.0305;Inherit;True;Property;_SurfaceAlbedo;Albedo;1;1;[SingleLineTexture];Create;False;0;0;0;False;2;Header(Maps);Space(7);False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;46;-3785.429,-344.7409;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;3;-3786.77,-733.1992;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;42;-3823.418,-512.2126;Inherit;False;37;CoverageMask;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;200;-2842.802,2378.531;Inherit;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;149;-2655.135,2374.287;Inherit;True;SnowMask;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.CommentaryNode;79;-2319.256,996.4626;Inherit;False;1126.105;401.0079;;6;76;157;70;69;54;71;Smoothness;1,1,1,1;0;0
Node;AmplifyShaderEditor.LerpOp;26;-3519.058,-554.4398;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.StaticSwitch;136;-3256.93,-738.9318;Inherit;False;Property;_COVERAGE;Enable;5;0;Create;False;0;0;0;False;2;Header(Coverage);Space(5);False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;COLOR;0,0,0,0;False;0;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;5;COLOR;0,0,0,0;False;6;COLOR;0,0,0,0;False;7;COLOR;0,0,0,0;False;8;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;71;-2184.376,1294.319;Inherit;False;37;CoverageMask;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;69;-2261.295,1186.549;Inherit;False;Property;_CoverageSmoothness;Smoothness;16;0;Create;False;0;0;0;False;0;False;0;0.1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;138;-3244.006,-354.445;Inherit;False;Property;_SnowColor;Snow Color;24;0;Create;True;0;0;0;False;0;False;0.9,0.9,0.9,0;0.9,0.9,0.9,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;54;-2262.323,1078.302;Inherit;False;Property;_SurfaceSmoothness;Smoothness;4;0;Create;False;0;0;0;False;0;False;0;0.35;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;153;-3242.98,-559.4428;Inherit;True;149;SnowMask;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;154;-2922.619,-557.3438;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;70;-1900.662,1166.826;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;157;-1676.103,1078.295;Inherit;False;Property;_Keyword1;Keyword 1;5;0;Create;True;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Reference;136;True;True;All;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;155;-2653.816,-737.3947;Inherit;False;Property;_Snow;Enable;20;0;Create;False;0;0;0;False;2;Header(Show);Space(5);False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;COLOR;0,0,0,0;False;0;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;5;COLOR;0,0,0,0;False;6;COLOR;0,0,0,0;False;7;COLOR;0,0,0,0;False;8;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;19;-2372.134,-737.6198;Inherit;False;Albedo;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;76;-1400.923,1077.988;Inherit;False;Smoothness;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;201;-4563.822,61.70771;Inherit;False;1;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;40,40;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;20;-1095.244,81.52013;Inherit;False;19;Albedo;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;77;-1094.706,191.4895;Inherit;False;75;Normal;1;0;OBJECT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;78;-1102.427,278.4375;Inherit;False;76;Smoothness;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;207;-3551.626,-207.2547;Inherit;True;Property;_OcclusionTexture;OcclusionTexture;0;1;[SingleLineTexture];Create;False;0;0;0;False;2;Header(Maps);Space(7);False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;-824.3199,131.1741;Float;False;True;-1;2;;0;0;Standard;Animpic/StaticShader;False;False;False;False;False;False;False;False;False;False;True;True;True;False;False;False;False;False;False;False;True;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;18;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;164;1;9;2
WireConnection;164;0;163;2
WireConnection;186;0;185;0
WireConnection;186;1;184;0
WireConnection;128;0;164;0
WireConnection;128;1;129;0
WireConnection;171;0;12;0
WireConnection;187;0;186;0
WireConnection;133;0;128;0
WireConnection;133;1;171;0
WireConnection;67;1;187;0
WireConnection;172;0;65;0
WireConnection;168;0;133;0
WireConnection;168;1;67;1
WireConnection;178;0;168;0
WireConnection;178;1;172;0
WireConnection;29;0;178;0
WireConnection;205;0;202;0
WireConnection;205;1;206;0
WireConnection;181;0;180;0
WireConnection;181;1;182;0
WireConnection;179;0;181;0
WireConnection;204;0;205;0
WireConnection;37;0;29;0
WireConnection;6;1;204;0
WireConnection;173;0;96;0
WireConnection;72;1;179;0
WireConnection;97;0;6;0
WireConnection;97;1;72;0
WireConnection;95;0;74;0
WireConnection;95;1;173;0
WireConnection;160;1;72;0
WireConnection;160;0;97;0
WireConnection;86;20;95;0
WireConnection;158;1;72;0
WireConnection;158;0;97;0
WireConnection;92;0;160;0
WireConnection;92;1;86;40
WireConnection;159;0;158;0
WireConnection;159;1;92;0
WireConnection;73;0;6;0
WireConnection;73;1;159;0
WireConnection;73;2;94;0
WireConnection;156;1;6;0
WireConnection;156;0;73;0
WireConnection;75;0;156;0
WireConnection;191;0;188;0
WireConnection;145;0;143;0
WireConnection;193;1;191;0
WireConnection;192;1;145;2
WireConnection;192;0;190;2
WireConnection;196;0;192;0
WireConnection;196;1;194;0
WireConnection;196;2;193;0
WireConnection;197;0;195;0
WireConnection;198;0;196;0
WireConnection;198;1;197;0
WireConnection;24;1;179;0
WireConnection;199;0;198;0
WireConnection;2;1;204;0
WireConnection;46;0;45;0
WireConnection;46;1;24;0
WireConnection;3;0;1;0
WireConnection;3;1;2;0
WireConnection;200;0;199;0
WireConnection;149;0;200;0
WireConnection;26;0;3;0
WireConnection;26;1;46;0
WireConnection;26;2;42;0
WireConnection;136;1;3;0
WireConnection;136;0;26;0
WireConnection;154;0;136;0
WireConnection;154;1;138;0
WireConnection;154;2;153;0
WireConnection;70;0;54;0
WireConnection;70;1;69;0
WireConnection;70;2;71;0
WireConnection;157;1;54;0
WireConnection;157;0;70;0
WireConnection;155;1;136;0
WireConnection;155;0;154;0
WireConnection;19;0;155;0
WireConnection;76;0;157;0
WireConnection;201;0;181;0
WireConnection;0;0;20;0
WireConnection;0;1;77;0
WireConnection;0;4;78;0
WireConnection;0;5;207;0
ASEEND*/
//CHKSM=0AEDDAEC699DF71108804C17AFF67FC3D81034C6