using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;
using System.Collections.Generic;

public class SpriteSlicingCopier : EditorWindow
{
    [MenuItem("Tools/复制 Sprite 切片数据")]
    public static void ShowWindow()
    {
        GetWindow<SpriteSlicingCopier>("Sprite 切片复制器");
    }

    public Texture2D sourceTexture;
    public Texture2D targetTexture;

    void OnGUI()
    {
        GUILayout.Label("将源纹理的 Sprite 切片复制到目标纹理", EditorStyles.boldLabel);
        sourceTexture = (Texture2D)EditorGUILayout.ObjectField("源纹理（角色，已切好）", sourceTexture, typeof(Texture2D), false);
        targetTexture = (Texture2D)EditorGUILayout.ObjectField("目标纹理（武器）", targetTexture, typeof(Texture2D), false);

        if (GUILayout.Button("复制切片数据", GUILayout.Height(40)))
        {
            CopySlicing();
        }
    }

    void CopySlicing()
    {
        if (sourceTexture == null || targetTexture == null)
        {
            EditorUtility.DisplayDialog("错误", "请指定源纹理和目标纹理", "确定");
            return;
        }

        string sourcePath = AssetDatabase.GetAssetPath(sourceTexture);
        string targetPath = AssetDatabase.GetAssetPath(targetTexture);

        TextureImporter sourceImporter = (TextureImporter)AssetImporter.GetAtPath(sourcePath);
        TextureImporter targetImporter = (TextureImporter)AssetImporter.GetAtPath(targetPath);

        // 新版 API：通过数据提供者工厂获取 Sprite 数据
        var factory = new SpriteDataProviderFactories();
        factory.Init();

        var sourceProvider = factory.GetSpriteEditorDataProviderFromObject(sourceImporter);
        sourceProvider.InitSpriteEditorDataProvider();

        var targetProvider = factory.GetSpriteEditorDataProviderFromObject(targetImporter);
        targetProvider.InitSpriteEditorDataProvider();

        // 读取源纹理的所有 Sprite 切片
        SpriteRect[] sourceSprites = sourceProvider.GetSpriteRects();

        if (sourceSprites == null || sourceSprites.Length == 0)
        {
            EditorUtility.DisplayDialog("错误", "源纹理没有 Sprite 切片数据，请先切好角色图", "确定");
            return;
        }

        // 逐帧复制 Rect、Pivot、名称、对齐方式
        List<SpriteRect> newSprites = new List<SpriteRect>();
        foreach (var src in sourceSprites)
        {
            SpriteRect dst = new SpriteRect
            {
                name = src.name,
                rect = src.rect,
                pivot = src.pivot,
                alignment = src.alignment,
                border = src.border
            };
            newSprites.Add(dst);
        }

        // 写入目标纹理并重新导入
        targetProvider.SetSpriteRects(newSprites.ToArray());
        targetProvider.Apply();

        targetImporter.textureType = TextureImporterType.Sprite;
        targetImporter.spriteImportMode = SpriteImportMode.Multiple;
        EditorUtility.SetDirty(targetImporter);
        targetImporter.SaveAndReimport();

        EditorUtility.DisplayDialog("完成",
            $"已将 {newSprites.Count} 个 Sprite 切片从 {sourceTexture.name} 复制到 {targetTexture.name}",
            "确定");
    }
}
