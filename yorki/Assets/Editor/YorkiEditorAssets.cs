using UnityEditor;
using UnityEngine;

public static class YorkiEditorAssets
{
    public const string UIFontPath = "Assets/Fonts/Moneygraphy-Pixel.ttf";
    public const string TalkScenePath = "Assets/Scenes/TalkScene.unity";
    public const string DrawingScenePath = "Assets/Scenes/SceneA.unity";
    public const string CustomerEpisode01Path = "Assets/Data/Dialogues/CustomerEpisodes/CustomerEpisode_01_Goro.asset";
    public const string CustomerEpisode02Path = "Assets/Data/Dialogues/CustomerEpisodes/CustomerEpisode_02_Hailey.asset";
    public const string CustomerEpisode03Path = "Assets/Data/Dialogues/CustomerEpisodes/CustomerEpisode_03_Winter.asset";
    public const string IntroMonologuePath = "Assets/Data/Dialogues/Sequences/IntroMonologue.asset";

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
