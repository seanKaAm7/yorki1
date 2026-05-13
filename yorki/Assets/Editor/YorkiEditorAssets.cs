using UnityEditor;
using UnityEngine;

public static class YorkiEditorAssets
{
    public const string UIFontPath = "Assets/Fonts/Moneygraphy-Pixel.ttf";

    public static Font LoadUIFont()
    {
        Font font = AssetDatabase.LoadAssetAtPath<Font>(UIFontPath);
        return font != null ? font : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
    }

    public static TextureImporter ConfigureSprite(string path, string warningPrefix)
    {
        var importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null)
        {
            Debug.LogWarning(warningPrefix + path);
            return null;
        }

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.mipmapEnabled = false;
        importer.alphaIsTransparency = true;
        importer.SaveAndReimport();
        return importer;
    }
}
