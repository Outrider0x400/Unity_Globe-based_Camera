Shader "Custom/GlobeMatShader"
{
    Properties
    {

		_EdgeLength ("Edge length", Range(2,50)) = 15
		_Tess ("Tessellation", Range(1,256)) = 16
		_Phong ("Phong Strengh", Range(0,1)) = 0.5
		_Color ("Color", Color) = (1,1,1,1)
		_SpecTex ("Specular", 2D) = "white" {}
		_HeightMap ("Heightmap", 2D) = "black" {}
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
		_BumpMap ("Bumpmap", 2D) = "bump" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
		_Height ("Height", Range(0,0.5)) = 0.1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf StandardSpecular  fullforwardshadows vertex:vert  tessellate:tessFixed  tessphong:_Phong
		#include "Tessellation.cginc"

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 4.6

        sampler2D _MainTex;
		sampler2D _SpecTex;
		sampler2D _HeightMap;
		sampler2D _BumpMap;

		float _Tess;
		float _Phong;
		float4 tessFixed()
		{
			return _Tess;
		}

		float _EdgeLength;

		float4 tessEdge (appdata_full v0, appdata_full v1, appdata_full v2)
		{
			float minDist = 0.1;
			float maxDist = 2.0;
			return UnityDistanceBasedTess(v0.vertex, v1.vertex, v2.vertex, minDist, maxDist, _Tess);
		}

        struct Input
        {
			float2 uv_MainTex;
        };

		half _Height;

		Input vert (inout appdata_full v)
		{
			Input v2f;
			UNITY_INITIALIZE_OUTPUT(Input, v2f);

			const float3 unitLocalPosVec = normalize(v.vertex.xyz);	// We place the globe at global (0,0,0)
			const float polarAngleDeg = degrees(acos(unitLocalPosVec.y));
			const float azimuthAngleDeg = degrees(atan2(unitLocalPosVec.z, unitLocalPosVec.x)) + 180.0f;

			const float latitude = polarAngleDeg - 90.0f;
			const float longitude = azimuthAngleDeg;

			const float equirectangularX = longitude / 360.0f;
			const float equirectangularY = ((latitude + 90) / 180.0f);
			const float2 equirectangularUv = float2(equirectangularX, -equirectangularY);

			//const float height = tex2Dlod(_HeightMap, float4(equirectangularUv, 0, 0)).x * _Height + 1;

			//v.vertex.xyz *= height;
			v2f.uv_MainTex = equirectangularUv;
			return v2f;
		}

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandardSpecular o)
        {

            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = c.xyz;

            // Metallic and smoothness come from slider variables

            //o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
			o.Specular = tex2D (_SpecTex, IN.uv_MainTex)*c.xyz;
			o.Normal = UnpackNormal (tex2D (_BumpMap, IN.uv_MainTex));
            o.Alpha = 1.0f;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
