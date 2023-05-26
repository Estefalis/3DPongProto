#ifndef NOVA_SHADER_TYPES
#define NOVA_SHADER_TYPES
#if 1 //////////////////////// Types ////////////////////////
	#if 1 //////////////////////// UIBlock2D ////////////////////////
		struct SubQuadVert
		{
			float2 Pos;
			uint BlockDataIndex;
			float EdgeSoftenMask;

			float2 UVZoom;
			float2 CenterUV;
		};

		struct UIBlock2DData
		{
			float2 QuadSize;
			float2 GradientCenter;

			float2 GradientSizeReciprocal;
			float2 GradientRotationSinCos;

			float2 ShadowOffset;
			NovaColor PrimaryColor;
			NovaColor GradientColor;

			NovaColor ShadowColor;
			float CornerRadius;
			float ShadowWidth;
			float ShadowBlur;

			uint TransformIndex;
			uint TexturePackSlice;
			NovaColor BorderColor;
			float BorderWidth;

			float2 RadialFillCenter;
			float RadialFillRotation;
			float RadialFillAngle;
		};

		struct PerInstanceDropShadowShaderData
		{
			float2 Offset;
			float2 HalfBlockQuadSize;

			NovaColor Color;
			float Width;
			float Blur;
			float BlockClipRadius;

			uint TransformIndex;
			float EdgeSoftenMask;
			float2 RadialFillCenter;

			float RadialFillRotation;
			float RadialFillAngle;
			float2 _padding;
		};

		struct PerQuadDropShadowShaderData
		{
			float2 PositionInNode;
			float2 QuadSize;
		};
	#endif

	#if 1 //////////////////////// UIBlock3D ////////////////////////
		struct UIBlock3DData
		{
			float3 Size;
			float CornerRadius;

			float EdgeRadius;
			NovaColor Color;
			uint TransformIndex;
			float _padding;
		};
	#endif

	#if 1 //////////////////////// TextBlock ////////////////////////
		struct PerVertTextData
		{
			float3 Position;
			uint TransformIndex;

			float2 Texcoord0;
			float2 Texcoord1;

			NovaColor Color;
			float ScaleMultiplier;
			float2 _padding;
		};
	#endif

	#if 1 //////////////////////// Lighting ////////////////////////
		struct BlinnPhongData
		{
			float Specular;
			float Gloss;
			float2 _padding;
		};

		struct StandardData
		{
			float Smoothness;
			float Metallic;
			float2 _padding;
		};

		struct StandardSpecularData
		{
			NovaColor SpecularColor;
			float Smoothness;
			float2 _padding;
		};
	#endif
#endif


#if 1 //////////////////////// Access ////////////////////////
	#if defined(NOVA_FALLBACK_RENDERING)
		#if 1 //////////////////////// UIBlock2D ////////////////////////
			#define NOVA_GET_BUFFER_ITEM_SubQuadVert(name, index, bufferName) \
				float4 bufferName##_UV = GetTextureBufferUV(index, 2, bufferName##_TexelSize); \
				SubQuadVert name; \
				float4 bufferName##_Temp; \
				\
				bufferName##_Temp = tex2Dlod(bufferName, bufferName##_UV); \
				name.Pos = bufferName##_Temp.xy; \
				name.BlockDataIndex = asuint(bufferName##_Temp.z); \
				name.EdgeSoftenMask = bufferName##_Temp.w; \
				\
				bufferName##_UV.x += bufferName##_TexelSize.x; \
				bufferName##_Temp = tex2Dlod(bufferName, bufferName##_UV); \
				name.UVZoom = bufferName##_Temp.xy; \
				name.CenterUV = bufferName##_Temp.zw; 

			#define NOVA_GET_BUFFER_ITEM_UIBlock2DData(name, index, bufferName) \
				float4 bufferName##_UV = GetTextureBufferUV(index, 6, bufferName##_TexelSize); \
				UIBlock2DData name; \
				float4 bufferName##_Temp; \
				\
				bufferName##_Temp = tex2Dlod(bufferName, bufferName##_UV); \
				name.QuadSize = bufferName##_Temp.xy; \
				name.GradientCenter = bufferName##_Temp.zw; \
				\
				bufferName##_UV.x += bufferName##_TexelSize.x; \
				bufferName##_Temp = tex2Dlod(bufferName, bufferName##_UV); \
				name.GradientSizeReciprocal = bufferName##_Temp.xy; \
				name.GradientRotationSinCos = bufferName##_Temp.zw; \
				\
				bufferName##_UV.x += bufferName##_TexelSize.x; \
				bufferName##_Temp = tex2Dlod(bufferName, bufferName##_UV); \
				name.ShadowOffset = bufferName##_Temp.xy; \
				name.PrimaryColor.Packed = asuint(bufferName##_Temp.z); \
				name.GradientColor.Packed = asuint(bufferName##_Temp.w); \
				\
				bufferName##_UV.x += bufferName##_TexelSize.x; \
				bufferName##_Temp = tex2Dlod(bufferName, bufferName##_UV); \
				name.ShadowColor.Packed = asuint(bufferName##_Temp.x); \
				name.CornerRadius = bufferName##_Temp.y; \
				name.ShadowWidth = bufferName##_Temp.z; \
				name.ShadowBlur = bufferName##_Temp.w; \
				\
				bufferName##_UV.x += bufferName##_TexelSize.x; \
				bufferName##_Temp = tex2Dlod(bufferName, bufferName##_UV); \
				name.TransformIndex = asuint(bufferName##_Temp.x); \
				name.TexturePackSlice = asuint(bufferName##_Temp.y); \
				name.BorderColor.Packed = asuint(bufferName##_Temp.z); \
				name.BorderWidth = bufferName##_Temp.w; \
				\
				bufferName##_UV.x += bufferName##_TexelSize.x; \
				bufferName##_Temp = tex2Dlod(bufferName, bufferName##_UV); \
				name.RadialFillCenter = bufferName##_Temp.xy; \
				name.RadialFillRotation = bufferName##_Temp.z; \
				name.RadialFillAngle = bufferName##_Temp.w; 

			#define NOVA_GET_BUFFER_ITEM_PerInstanceDropShadowShaderData(name, index, bufferName) \
				float4 bufferName##_UV = GetTextureBufferUV(index, 4, bufferName##_TexelSize); \
				PerInstanceDropShadowShaderData name; \
				float4 bufferName##_Temp; \
				\
				bufferName##_Temp = tex2Dlod(bufferName, bufferName##_UV); \
				name.Offset = bufferName##_Temp.xy; \
				name.HalfBlockQuadSize = bufferName##_Temp.zw; \
				\
				bufferName##_UV.x += bufferName##_TexelSize.x; \
				bufferName##_Temp = tex2Dlod(bufferName, bufferName##_UV); \
				name.Color.Packed = asuint(bufferName##_Temp.x); \
				name.Width = bufferName##_Temp.y; \
				name.Blur = bufferName##_Temp.z; \
				name.BlockClipRadius = bufferName##_Temp.w; \
				\
				bufferName##_UV.x += bufferName##_TexelSize.x; \
				bufferName##_Temp = tex2Dlod(bufferName, bufferName##_UV); \
				name.TransformIndex = asuint(bufferName##_Temp.x); \
				name.EdgeSoftenMask = bufferName##_Temp.y; \
				name.RadialFillCenter = bufferName##_Temp.zw; \
				\
				bufferName##_UV.x += bufferName##_TexelSize.x; \
				bufferName##_Temp = tex2Dlod(bufferName, bufferName##_UV); \
				name.RadialFillRotation = bufferName##_Temp.x; \
				name.RadialFillAngle = bufferName##_Temp.y; 

			#define NOVA_GET_BUFFER_ITEM_PerQuadDropShadowShaderData(name, index, bufferName) \
				float4 bufferName##_UV = GetTextureBufferUV(index, 1, bufferName##_TexelSize); \
				PerQuadDropShadowShaderData name; \
				float4 bufferName##_Temp; \
				\
				bufferName##_Temp = tex2Dlod(bufferName, bufferName##_UV); \
				name.PositionInNode = bufferName##_Temp.xy; \
				name.QuadSize = bufferName##_Temp.zw; 
		#endif

		#if 1 //////////////////////// UIBlock3D ////////////////////////
			#define NOVA_GET_BUFFER_ITEM_UIBlock3DData(name, index, bufferName) \
				float4 bufferName##_UV = GetTextureBufferUV(index, 2, bufferName##_TexelSize); \
				UIBlock3DData name; \
				float4 bufferName##_Temp; \
				\
				bufferName##_Temp = tex2Dlod(bufferName, bufferName##_UV); \
				name.Size = bufferName##_Temp.xyz; \
				name.CornerRadius = bufferName##_Temp.w; \
				\
				bufferName##_UV.x += bufferName##_TexelSize.x; \
				bufferName##_Temp = tex2Dlod(bufferName, bufferName##_UV); \
				name.EdgeRadius = bufferName##_Temp.x; \
				name.Color.Packed = asuint(bufferName##_Temp.y); \
				name.TransformIndex = asuint(bufferName##_Temp.z); 
		#endif

		#if 1 //////////////////////// TextBlock ////////////////////////
			#define NOVA_GET_BUFFER_ITEM_PerVertTextData(name, index, bufferName) \
				float4 bufferName##_UV = GetTextureBufferUV(index, 3, bufferName##_TexelSize); \
				PerVertTextData name; \
				float4 bufferName##_Temp; \
				\
				bufferName##_Temp = tex2Dlod(bufferName, bufferName##_UV); \
				name.Position = bufferName##_Temp.xyz; \
				name.TransformIndex = asuint(bufferName##_Temp.w); \
				\
				bufferName##_UV.x += bufferName##_TexelSize.x; \
				bufferName##_Temp = tex2Dlod(bufferName, bufferName##_UV); \
				name.Texcoord0 = bufferName##_Temp.xy; \
				name.Texcoord1 = bufferName##_Temp.zw; \
				\
				bufferName##_UV.x += bufferName##_TexelSize.x; \
				bufferName##_Temp = tex2Dlod(bufferName, bufferName##_UV); \
				name.Color.Packed = asuint(bufferName##_Temp.x); \
				name.ScaleMultiplier = bufferName##_Temp.y; 
		#endif

		#if 1 //////////////////////// Lighting ////////////////////////


		#endif
	#else
		#if 1 //////////////////////// UIBlock2D ////////////////////////
			#define NOVA_GET_BUFFER_ITEM_SubQuadVert(name, index, bufferName) \
				SubQuadVert name = bufferName[index];

			#define NOVA_GET_BUFFER_ITEM_UIBlock2DData(name, index, bufferName) \
				UIBlock2DData name = bufferName[index];

			#define NOVA_GET_BUFFER_ITEM_PerInstanceDropShadowShaderData(name, index, bufferName) \
				PerInstanceDropShadowShaderData name = bufferName[index];

			#define NOVA_GET_BUFFER_ITEM_PerQuadDropShadowShaderData(name, index, bufferName) \
				PerQuadDropShadowShaderData name = bufferName[index];
		#endif

		#if 1 //////////////////////// UIBlock3D ////////////////////////
			#define NOVA_GET_BUFFER_ITEM_UIBlock3DData(name, index, bufferName) \
				UIBlock3DData name = bufferName[index];
		#endif

		#if 1 //////////////////////// TextBlock ////////////////////////
			#define NOVA_GET_BUFFER_ITEM_PerVertTextData(name, index, bufferName) \
				PerVertTextData name = bufferName[index];
		#endif

		#if 1 //////////////////////// Lighting ////////////////////////


		#endif
	#endif
#endif
#endif
