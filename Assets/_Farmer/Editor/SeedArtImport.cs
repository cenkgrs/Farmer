using UnityEditor;
using UnityEngine;
namespace Farmer.Editor
{
    public sealed class SeedArtImport : AssetPostprocessor
    {
        private void OnPreprocessTexture()
        {
            if(!assetPath.StartsWith("Assets/_Farmer/Resources/SeedArt/"))return;
            var importer=(TextureImporter)assetImporter;importer.textureType=TextureImporterType.Default;
            importer.alphaSource=TextureImporterAlphaSource.FromInput;importer.alphaIsTransparency=true;
            importer.mipmapEnabled=false;importer.textureCompression=TextureImporterCompression.Uncompressed;
            importer.maxTextureSize=2048;importer.npotScale=TextureImporterNPOTScale.None;importer.wrapMode=TextureWrapMode.Clamp;importer.filterMode=FilterMode.Bilinear;
        }
    }
}
