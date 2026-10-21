// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net) and Silicon Studio Corp. (https://www.siliconstudio.co.jp)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Security.Cryptography;
using System.Runtime.InteropServices;
using Stride.TextureConverter.Requests;

namespace Stride.TextureConverter.Tests
{
    class TestTools
    {
        public static string ComputeSHA1(string filePath)
        {
            var file = new FileStream(filePath, FileMode.Open);
            SHA1 sha1 = new SHA1CryptoServiceProvider();
            byte[] retVal = sha1.ComputeHash(file);
            file.Close();

            var sb = new StringBuilder();
            for (int i = 0; i < retVal.Length; i++)
            {
                sb.Append(retVal[i].ToString("x2"));
            }
            return sb.ToString();
        }

        public static string ComputeSHA1(IntPtr input, int size)
        {
            byte[] array = new byte[size];
            Marshal.Copy(input, array, 0, size);

            SHA1 sha1 = new SHA1CryptoServiceProvider();
            byte[] retVal = sha1.ComputeHash(array);

            var sb = new StringBuilder();
            for (int i = 0; i < retVal.Length; i++)
            {
                sb.Append(retVal[i].ToString("x2"));
            }

            return sb.ToString();
        }

        public static TexImage Load(ITexLibrary library, string file)
        {
            var image = new TexImage();
            library.Execute(image, new FileLoadingRequest(Module.PathToInputImages + file, false));
            image.Name = file;
            image.CurrentLibrary = library;
            return image;
        }

        public Dictionary<String, String> Checksum { get; private set; }

        private TestTools() {
            Checksum = new Dictionary<string, string>
            {
                // StrideTexLibrary
                {"ExportTest_Texture3D_WMipMaps_BC3.sd", "b351c27d236ee7bd1dfd4c0e8a5a5c785a2b9c53"},
                {"ExportTest_TextureArray_WMipMaps_BC3.sd", "cb04d7f022623eaf16741dc8820f6354ffe101f4"},
                {"ExportTest_TextureCube_WMipMaps_RGBA8888.sd", "a1d07aced830ba66c965c2c7ab24fcc38a8ce9e7"},
                {"ExportMinMipMapTest_512_TextureArray_WMipMaps_BC3.sd", "b12768b45f9eef2978e8d508dd0706246ee1f346"},
                {"ExportMinMipMapTest_4_Texture3D_WMipMaps_BC3.sd", "b59b422af672e22f16d435f55b68203cf0121874"},
                {"ExportMinMipMapTest_16_TextureCube_WMipMaps_RGBA8888.sd", "1d3640be224834c1f84a84fa24340519d56f2291"},

                // DxtTexLib
                {"DecompressTest_TextureArray_WMipMaps_BC3.dds", "54e100f9fb5982a8e51984911918e2f663d21805"},
                {"DecompressTest_TextureCube_WMipMaps_BC3.dds", "78a23f5778fd66113bb1e71971e3708cbb8531b5"},
                {"CompressTest_BC3_UNorm_TextureArray_WMipMaps_BGRA8888.dds", "753f8c365a0d76ea4e4fbd2664621da705df2d84"},
                {"CompressTest_BC3_UNorm_TextureCube_WMipMaps_BGRA8888.dds", "12d4023f4a40a97c94ac246db3c06bf2c686f421"},
                {"CompressTest_BC3_UNorm_Texture3D_WMipMaps_BGRA8888.dds", "b1c0a1a3a438ef2327d0c3bfad2dd775e85892bd"},
                {"CompressTest_BC1_UNorm_TextureArray_WMipMaps_BGRA8888.dds", "0fa2202c848737e613e7ad3321cec3b94c37500d"},
                {"CompressTest_BC1_UNorm_TextureCube_WMipMaps_BGRA8888.dds", "1d14b32768318be15b3c5fc70ace7bfe07ee42b3"},
                {"CompressTest_BC1_UNorm_Texture3D_WMipMaps_BGRA8888.dds", "07a66d854b3375cf6415dc14980fd7ec14b08001"},
                {"GenerateMipMapTest_Linear_Texture3D_WOMipMaps_BC3.dds", "89352d9f6ae303458a2e65a8caa95fb5aeaed4c7"},
                {"GenerateMipMapTest_Nearest_Texture3D_WOMipMaps_BC3.dds", "89352d9f6ae303458a2e65a8caa95fb5aeaed4c7"},
                {"GenerateMipMapTest_Cubic_TextureCube_WOMipMaps_BC3.dds", "5e01b2d12a4c69bc4ae97adfc06b801a2cf1d076"},
                {"GenerateMipMapTest_Box_TextureArray_WOMipMaps_BC3.dds", "b446156ba6aa0a95a75d017b81a353eb109f63de"},
                {"GenerateNormalMapTest_TextureArray_WOMipMaps_BC3.dds", "b9960ef32989a69ba2e14a66fab8597541b792ad"},
                {"GenerateNormalMapTest_TextureCube_WOMipMaps_BC3.dds", "5b4f26d5fbd93742f43351ff3399067f551c2263"},
                {"GenerateNormalMapTest_Texture3D_WOMipMaps_BC3.dds", "8e8f43f8d7eaa46563eb8f22fe15310fa673f4e7"},
                {"FixedRescaleTest_Bilinear_TextureCube_WMipMaps_BGRA8888.dds", "d44db5629fb1b5f6accaa1101d930314f823d5c7"},
                {"FixedRescaleTest_Nearest_Texture3D_WMipMaps_BGRA8888.dds", "2d6777265300e1c26835cf3283f44deb2ac3c884"},
                {"FixedRescaleTest_Bicubic_TextureArray_WMipMaps_BGRA8888.dds", "0d997edf1d0713c61767b2e7a3da3313a4155359"},
                {"FactorRescaleTest_Bicubic_TextureArray_WMipMaps_BGRA8888.dds", "0d997edf1d0713c61767b2e7a3da3313a4155359"},
                {"FactorRescaleTest_Box_Texture3D_WMipMaps_BGRA8888.dds", "0b414656b8674da7f0d07ce2146323ec9e08c870"},
                {"FactorRescaleTest_Bilinear_TextureCube_WMipMaps_BGRA8888.dds", "d44db5629fb1b5f6accaa1101d930314f823d5c7"},
                {"ExportTest_TextureCube_WMipMaps_BC3.dds", "a19ba3dd681e0c6dbad292040b70119500e91b4b"},
                {"ExportTest_Texture3D_WMipMaps_BGRA8888.dds", "ddc133f9f0dd823298f2416f54df1e2c33491d0c"},
                {"ExportTest_TextureArray_WMipMaps_BC3.dds", "d4fa1aa605897dee0ba089040a7b363061c75533"},
                {"ExportMinMipMapTest_512_TextureArray_WMipMaps_BC3.dds", "774029b79bd274541f8c52b054c671374bac52b6"},
                {"ExportMinMipMapTest_4_Texture3D_WMipMaps_BGRA8888.dds", "e02b7b8908538b5af073d4d1e7bb447096bfa168"},
                {"ExportMinMipMapTest_8_TextureCube_WMipMaps_BC3.dds", "256a76c0a1e537eef07e72c2600e3138542898d8"},
                {"ExportMinMipMapTest_16_TextureArray_WMipMaps_BC3.dds", "c8ebd13bce480ea875493efa5902f975e32060be"},
                {"PreMultiplyAlphaTest_TextureCube_WMipMaps_BGRA8888.dds", "78a23f5778fd66113bb1e71971e3708cbb8531b5"},
                {"PreMultiplyAlphaTest_TextureArray_WMipMaps_BGRA8888.dds", "54e100f9fb5982a8e51984911918e2f663d21805"},
                {"PreMultiplyAlphaTest_Texture3D_WMipMaps_BGRA8888.dds", "dab24b680a0e1c01c2bd84003b53257ca019d5bf"},

                // FITexLib
                {"CorrectGammaTest_duck.jpg", "0bc9b3b0dae4db9b8acbd25ba1252c543e41bea6"},
                {"CorrectGammaTest_stones.png", "435b603796e2f899a481d6f24a4730143691a49f"},
                {"FlipTest_Horizontal_duck.jpg", "a83bb9292db4f75dbda4816827f608d7193d8bfa"},
                {"FlipTest_Vertical_stones.png", "e26be5c6f5c076de77e10ff7cac58d4248f1931b"},
                {"FixedRescaleTest_Bicubic_duck.jpg", "9f5f48196f8d57baecdd44a7f737165be1527ec7"},
                {"FixedRescaleTest_Box_duck.jpg", "e1305473784a288066ccb471677dc646aa38a505"},
                {"FixedRescaleTest_Bilinear_stones.png", "000de5831d2c497e442fdae47f4cdf7b6d094733"},
                {"FixedRescaleTest_BSpline_stones.png", "e970d600058132ce37b43c17453294af9d6259fa"},
                {"FixedRescaleTest_CatmullRom_duck.jpg", "ab787399596b5facfa6bd8b6342acbb1f1071e1c"},
                {"FixedRescaleTest_Lanczos3_stones.png", "0535281cf237668f36f79310b5f87f9fa9e0d3bb"},
                {"FactorRescaleTest_Bilinear_stones.png", "000de5831d2c497e442fdae47f4cdf7b6d094733"},
                {"FactorRescaleTest_Bicubic_stones.png", "4fcaad4d2c68a596bc0c79ea65d7cf00bc1b0b4c"},
                {"FactorRescaleTest_Box_stones.png", "ee3b22211ce7760838ba959c802bed7da971d004"},
                {"FactorRescaleTest_BSpline_stones.png", "e970d600058132ce37b43c17453294af9d6259fa"},
                {"FactorRescaleTest_CatmullRom_stones.png", "4645a50755a341603338d5edf9a81b27e2a828b9"},
                {"FactorRescaleTest_Lanczos3_stones.png", "0535281cf237668f36f79310b5f87f9fa9e0d3bb"},
                {"FactorRescaleTest_Lanczos3_Texture3D_WMipMaps_BGRA8888.dds", "f1f340b0f877023c3bd7d6f045a3ade2ba12f205"},
                {"SwitchChannelsTest_duck.jpg", "744947a2f35fd54f634c180865dd9df7e7307efd"},
                {"SwitchChannelsTest_stones.png", "92bb7a83ad6be031f14e35ca173d885885dfeb37"},
                {"ImageSharpTexLibTest_ExportArrayTest_16_TextureArray_WMipMaps_BGRA8888-ind_0-mip_0.png", "453aba8518f7af27b8b167f288cbf70aa5420df2"},
                {"ImageSharpTexLibTest_ExportArrayTest_16_TextureArray_WMipMaps_BGRA8888-ind_0-mip_1.png", "abe5900f913b1b2be914f3deb33eeb0280199692"},
                {"ImageSharpTexLibTest_ExportArrayTest_16_TextureArray_WMipMaps_BGRA8888-ind_0-mip_2.png", "3a38002cff510957e91d11d001da1de485776081"},
                {"ImageSharpTexLibTest_ExportArrayTest_16_TextureArray_WMipMaps_BGRA8888-ind_0-mip_3.png", "711d4db91599ea867efb5aef7abd56c0a822acf3"},
                {"ImageSharpTexLibTest_ExportArrayTest_16_TextureArray_WMipMaps_BGRA8888-ind_0-mip_4.png", "8116b2a3ab7ab5f358d020bbf0d04f8728f336cb"},
                {"ImageSharpTexLibTest_ExportArrayTest_16_TextureArray_WMipMaps_BGRA8888-ind_0-mip_5.png", "826b235917e7503913f04537dff0183770d470ab"},
                {"ImageSharpTexLibTest_ExportArrayTest_0_TextureArray_WMipMaps_BGRA8888-ind_0-mip_0.png", "453aba8518f7af27b8b167f288cbf70aa5420df2"},
                {"ImageSharpTexLibTest_ExportArrayTest_0_TextureArray_WMipMaps_BGRA8888-ind_0-mip_1.png", "abe5900f913b1b2be914f3deb33eeb0280199692"},
                {"ImageSharpTexLibTest_ExportArrayTest_0_TextureArray_WMipMaps_BGRA8888-ind_0-mip_2.png", "3a38002cff510957e91d11d001da1de485776081"},
                {"ImageSharpTexLibTest_ExportArrayTest_0_TextureArray_WMipMaps_BGRA8888-ind_0-mip_3.png", "711d4db91599ea867efb5aef7abd56c0a822acf3"},
                {"ImageSharpTexLibTest_ExportArrayTest_0_TextureArray_WMipMaps_BGRA8888-ind_0-mip_4.png", "8116b2a3ab7ab5f358d020bbf0d04f8728f336cb"},
                {"ImageSharpTexLibTest_ExportArrayTest_0_TextureArray_WMipMaps_BGRA8888-ind_0-mip_5.png", "826b235917e7503913f04537dff0183770d470ab"},
                {"ImageSharpTexLibTest_ExportArrayTest_0_TextureArray_WMipMaps_BGRA8888-ind_0-mip_6.png", "396857b2ed3e414eb510fdb1d69d2b1850eb772e"},
                {"ImageSharpTexLibTest_ExportArrayTest_0_TextureArray_WMipMaps_BGRA8888-ind_0-mip_7.png", "7ab7459d50319bb37fb3ecdba68136c357d3e5a8"},
                {"ImageSharpTexLibTest_ExportArrayTest_0_TextureArray_WMipMaps_BGRA8888-ind_0-mip_8.png", "3bd4d36c8b41852981408bce6d9a6bafbda1456e"},
                {"ImageSharpTexLibTest_ExportArrayTest_0_TextureArray_WMipMaps_BGRA8888-ind_0-mip_9.png", "ea5624117fe67c896b624e3b2f97304d8001b86c"},
                {"ImageSharpTexLibTest_ExportArrayTest_0_TextureArray_WMipMaps_BGRA8888-ind_1-mip_0.png", "72012385e208f9731aca6c6cfad0fca5a5ea1571"},
                {"ImageSharpTexLibTest_ExportArrayTest_0_TextureArray_WMipMaps_BGRA8888-ind_1-mip_1.png", "af33d585137f9eb967b0e3911a65ff8be5d64b62"},
                {"ImageSharpTexLibTest_ExportArrayTest_0_TextureArray_WMipMaps_BGRA8888-ind_1-mip_2.png", "e0cb45438e02d96bcd21949c616f110dfcd2f54a"},
                {"ImageSharpTexLibTest_ExportArrayTest_0_TextureArray_WMipMaps_BGRA8888-ind_1-mip_3.png", "34e3ccc0c83bbe1c5c2c7f4f117f5c1e8ef6e7c4"},
                {"ImageSharpTexLibTest_ExportArrayTest_0_TextureArray_WMipMaps_BGRA8888-ind_1-mip_4.png", "90a24d440bc40a254fd0ad281c167fc52f5c402c"},
                {"ImageSharpTexLibTest_ExportArrayTest_0_TextureArray_WMipMaps_BGRA8888-ind_1-mip_5.png", "98e16d84bbe44d0c85fb9c4ae5adcd6fdccf382e"},
                {"ImageSharpTexLibTest_ExportArrayTest_0_TextureArray_WMipMaps_BGRA8888-ind_1-mip_6.png", "73248fce1517729cf898bf1158d453c3ad14ee34"},
                {"ImageSharpTexLibTest_ExportArrayTest_0_TextureArray_WMipMaps_BGRA8888-ind_1-mip_7.png", "6466588a6da41936b9a2bc0bd655f893f83df700"},
                {"ImageSharpTexLibTest_ExportArrayTest_0_TextureArray_WMipMaps_BGRA8888-ind_1-mip_8.png", "f4ab59af05d62014f2c6dccc14c4a0c99e96b399"},
                {"ImageSharpTexLibTest_ExportArrayTest_0_TextureArray_WMipMaps_BGRA8888-ind_1-mip_9.png", "ea5624117fe67c896b624e3b2f97304d8001b86c"},

                // TextureTool
                {"TextureTool_Flip_Horizontal_stones.png", "ce7f4854e125ccf39f7c333c24d25ba2863f77e0"},
                {"TextureTool_Flip_Horizontal_TextureArray_WMipMaps_BC3.dds", "8a0a645b0ec08c384142156210dab27edc703422"},
                {"TextureTool_PreMultiplyAlpha_Texture3D_WOMipMaps_BC3.dds", "a26416c17a55f68f94ce3f26aabfcf5a91616f75"},
                {"TextureTool_Compress_BC3_UNorm_TextureArray_WMipMaps_BC3.dds", "db259e223c77b3f3c3591e6227e8c0c09c66e471"},
                {"TextureTool_Compress_BC3_UNorm_stones.png", "fed1dfaf60c16bb633a2b3f0038d9b339451d2f2"},
                {"TextureTool_GenerateMipMap_Linear_TextureArray_WMipMaps_BC3.dds", "83aa34d27e214e4fc9867dab96193a325e2bccd3"},
                {"TextureTool_GenerateMipMap_Box_stones.png", "4b22ac8a3ab3e7fe8134e64b928f258854c6d269"},
                {"TextureTool_CorrectGamma_TextureArray_WMipMaps_BC3.dds", "38b2c6a1dac0b0af878f4932b9216d2c87417f5d"},
                {"TextureTool_GenerateNormalMap_TextureArray_WMipMaps_BC3.dds", "a89bab99023323858c5409c063f53dc8f80505a8"},
                {"TextureTool_Rescale_TextureArray_WMipMaps_BC3.dds", "0d997edf1d0713c61767b2e7a3da3313a4155359"},
                {"TextureTool_SwitchChannel_TextureArray_WMipMaps_BC3.dds", "d4bd82c97e9a8d8bf4c16759c5d67d3aff585fb6"},
                {"TextureTool_ProcessingTest_NormalMapLanczos3_BC3_UNorm_TextureCube_WMipMaps_BC3.sd.dds", "63ece6917444d4b7c49fec447517c543a8219907"},
                {"TextureTool_ProcessingTest_Lanczos3_BC3_UNorm_TextureCube_WMipMaps_BC3.sd.dds", "168d7c218d666aa1acbe3de9eea5179c894f6ae6"},
                {"TextureTool_ProcessingTest_NormalMapBox_BC3_UNorm_duck.jpg.dds", "4c9d64c581d7351124e614d09f0eaf121f2f5b5d"},
                {"TextureTool_ProcessingTest_Box_BC3_UNorm_duck.jpg.dds", "b4f286270c09dbc611fba9f028098ec8b96e2616"},
                {"TextureTool_CreateAtlas", "e8d1e42cb1acc0d6d7fe6b2c16c7544f8cef56fd"},
                {"TextureTool_ExtractAtlas_atlas_WMipMaps.dds_stones.png", "06654c4fc44ef2a13bfb18a915e066e4cb5f213e"},
                {"TextureTool_UpdateAtlas_atlas_WMipMaps.dds_square256_2.png", "a3d2ebe9cba9443f3a0ef002565aa4ff425597cd"},

                {"TextureTool_CreateArray_stones256.png_square256.png", "aaa6861777bd6debefeec34a18694f37e32cfe99"},
                {"TextureTool_CreateCube_stones256.png_square256.png", "18ba65f9e50135549d871c645421e19261854b53"},
                {"TextureTool_Extract_4_array_WMipMaps.dds", "21b6bc92253be4f34b819f05b2ec39a7b0305f9d"},
                {"TextureTool_Insert_3_square256.png_array_WMipMaps.dds", "e7e9dbcc26a4c5cccda273735cfa9d182c90d0bc"},
                {"TextureTool_Remove_3_array_WMipMaps.dds", "2ad8b420964c4564a9cfa488e5a394345fc1f64e"},

                {"TextureTool_UpdateArray_array_WMipMaps.dds_0_square256_2.png", "c6d90a935a9a50172c9d62367fd5cbdca35b1eb1"},



                // AtlasTexLibrary
                {"AtlasTexLibrary_CreateAtlas_False_True", "09a08c14ed0378eb1118a9580e604df2658ba63a"},
                {"AtlasTexLibrary_CreateAtlas_True_False", "dfd002dd713d2a39d34b7c555ec35bf07d06a18d"},
                {"AtlasTexLibrary_CreateAtlas_False_False", "e8d1e42cb1acc0d6d7fe6b2c16c7544f8cef56fd"},
                {"AtlasTexLibrary_Extract_square256.dds", "2a477d5dea47cf304433d2defda2a9adee2ebb8b"},
                {"AtlasTexLibrary_Update_square256_2.png_atlas_WOMipMaps.png", "86540e19226f6cddcb31c9fc73b12b9a31ffff1d"},
                {"ExtractAll_duck.jpg", "abb6fa73ea0bededdf81ba50d95ca00490b5db18"},
                {"ExtractAll_rect100_128.png", "e657e463475d59fe7da38fae55dc76ae5ddccb67"},
                {"ExtractAll_rect128_100.png", "90be794b2485bc23dc9e4117ff7dd1e56bfaeab5"},
                {"ExtractAll_square128.png", "03e33dadd405636108aa1ef81037bc9dfdebe2b4"},
                {"ExtractAll_square256.png", "646b97fdb968d48ada93e451df49bd68f73b6e7d"},
                {"ExtractAll_square256_2.png", "646b97fdb968d48ada93e451df49bd68f73b6e7d"},
                {"ExtractAll_square512.png", "66d47702144982320acd08e949dbefe524f2a3ab"},
                {"ExtractAll_stones.png", "5f815194db5d6c7f8b4512f2140377258fdc2f08"},
                {"ExtractAll_stones256.png", "010e900ab2c42337355e25ee9e12ba71a4689285"},

                // ArrayTexLibrary
                {"ArrayTexLibrary_CreateArray_stones256.png_square256.png", "aaa6861777bd6debefeec34a18694f37e32cfe99"},
                {"ArrayTexLibrary_Extract_array_WMipMaps.dds", "c75dfbc71e26435da42762694a24f7c7d3764ec5"},
                {"ArrayTexLibrary_Update_0_array_WOMipMaps.dds", "aaa6861777bd6debefeec34a18694f37e32cfe99"},
                {"ArrayTexLibrary_Remove_3_array_WMipMaps.dds", "2ad8b420964c4564a9cfa488e5a394345fc1f64e"},
                {"ArrayTexLibrary_Insert_square256.png_3_array_WOMipMaps.dds", "c94b156dec8619bb833c90834d78e7cb84a6e5ff"},
                {"ArrayTexLibrary_CreateCube_stones256.png_square256.png", "18ba65f9e50135549d871c645421e19261854b53"},

            };
        }

        private static TestTools instance;

        public static TestTools GetInstance()
        {
            if (instance == null)
            {
                instance = new TestTools();
            }
            return instance;
        }
    }
}
