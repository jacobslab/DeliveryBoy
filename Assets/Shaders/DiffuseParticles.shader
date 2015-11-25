Shader "Particles/DiffuseTest2" {
 Properties {
     _Color ("Main Color", Color) = (1,1,1,1)
     _MainTex ("Base (RGB)", 2D) = "white" {}
 }
 
 
 SubShader {
     Tags {}
     LOD 200
 
 CGPROGRAM
 #pragma surface surf Lambert
 #pragma vertex vert
 
 sampler2D _MainTex;
 fixed4 _Color;
 
 struct Input {
     float2 uv_MainTex;
     float3 color;
 };
 
 void vert (inout appdata_full v, out Input o) {
   UNITY_INITIALIZE_OUTPUT(Input,o);
   o.color = v.color;
 }
 
 void surf (Input IN, inout SurfaceOutput o) {
     //fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
     fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * float4(IN.color.r,IN.color.g,IN.color.b,1);
     o.Albedo = c.rgb;
 }
 ENDCG
 }
 }
