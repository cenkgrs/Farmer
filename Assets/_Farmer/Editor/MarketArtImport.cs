using UnityEditor;
using UnityEngine;
namespace Farmer.Editor
{
    // Keep reference pixels: no power-of-two resampling, mipmaps or lossy GPU compression.
    public sealed class MarketArtImport : AssetPostprocessor
    {
        private void OnPreprocessTexture()
        {
            if(!assetPath.StartsWith("Assets/_Farmer/Resources/MarketArt/"))return;
            var importer=(TextureImporter)assetImporter;
            importer.textureType=TextureImporterType.Default;
            importer.npotScale=TextureImporterNPOTScale.None;
            importer.mipmapEnabled=false;
            importer.textureCompression=TextureImporterCompression.Uncompressed;
            importer.maxTextureSize=2048;
            importer.filterMode=FilterMode.Bilinear;
            importer.wrapMode=TextureWrapMode.Clamp;
        }
    }
}
