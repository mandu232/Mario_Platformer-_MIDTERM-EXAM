// [UNITY-SKILL:SPRITEATLAS]
// AI-assisted implementation by OpenAI Codex, 2026-10-24.
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.U2D;
using UnityEngine;

namespace Midterm.Editor
{
    public sealed class TileAtlasPrebuild : IPreprocessBuildWithReport
    {
        const string AtlasPath = "Assets/Sprite/Tilemap/Terrain.spriteatlasv2";
        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report) => EnsureAtlas();

        // AiCode -------
        public static void EnsureAtlas()
        {
            EditorSettings.spritePackerMode = SpritePackerMode.SpriteAtlasV2;
            if (!System.IO.File.Exists(AtlasPath))
            {
                var atlas = new SpriteAtlasAsset();
                atlas.Add(new UnityEngine.Object[] {
                    AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Sprite/Tilemap/tilemap.png"),
                    AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Sprite/Tilemap/tilemap-backgrounds.png")
                });
                SpriteAtlasAsset.Save(atlas, AtlasPath);
                AssetDatabase.ImportAsset(AtlasPath);
            }

            var importer = (SpriteAtlasImporter)AssetImporter.GetAtPath(AtlasPath);
            var packing = importer.packingSettings;
            packing.padding = 4;
            packing.enableRotation = false;
            packing.enableTightPacking = false;
            packing.enableAlphaDilation = true;
            importer.packingSettings = packing;
            var texture = importer.textureSettings;
            texture.filterMode = FilterMode.Point;
            texture.generateMipMaps = false;
            importer.textureSettings = texture;
            var platform = importer.GetPlatformSettings("DefaultTexturePlatform");
            platform.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SetPlatformSettings(platform);
            importer.includeInBuild = true;
            importer.SaveAndReimport();
        }
        // AiCode -------
    }
}
