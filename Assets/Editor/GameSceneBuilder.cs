using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.U2D.Sprites;
using UnityEngine;

/// <summary>
/// One-click setup for the playable scene: fixes the art import settings, builds the ship
/// animation, then creates the camera, background, managers and player in the open scene.
///
/// Everything it does is idempotent - running it twice does not create duplicates.
/// Menu: Tools > Chicken Invaders > Build Game Scene
/// </summary>
public static class GameSceneBuilder
{
    private const string ShipSheetPath   = "Assets/Art/Sprites/hero_ship.png";
    private const string StarfieldPath   = "Assets/Art/Backgrounds/PC _ Computer - Chicken Invaders 3 - Background - Starfield.png";

    private const int ShipFrameWidth  = 45;
    private const int ShipFrameHeight = 37;
    private const float ShipPixelsPerUnit = 40f;   // makes the ship ~1.1 units wide, next to a 1.28 unit chicken
    private const float CameraSize = 5f;           // 10 world units tall

    [MenuItem("Tools/Chicken Invaders/Build Game Scene")]
    public static void Build()
    {
        ConfigureShipSheet();
        ConfigureStarfield();
        BuildScene();

        AssetDatabase.SaveAssets();
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();

        Debug.Log("Game scene built. Press Play - A/D or the arrow keys move the ship.");
    }

    // ------------------------------------------------------------------ import settings

    /// <summary>
    /// Slices the 8-frame ship strip into a grid. Doing it in code means the frame size is stated
    /// once, here, instead of being dragged by hand in the Sprite Editor.
    /// </summary>
    private static void ConfigureShipSheet()
    {
        var importer = AssetImporter.GetAtPath(ShipSheetPath) as TextureImporter;
        if (importer == null)
        {
            Debug.LogError($"Ship sheet not found at {ShipSheetPath}");
            return;
        }

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Multiple;
        importer.spritePixelsPerUnit = ShipPixelsPerUnit;
        importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.mipmapEnabled = false;
        importer.alphaIsTransparency = true;

        var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(ShipSheetPath);
        var frameCount = texture.width / ShipFrameWidth;

        var factories = new SpriteDataProviderFactories();
        factories.Init();
        var provider = factories.GetSpriteEditorDataProviderFromObject(importer);
        provider.InitSpriteEditorDataProvider();

        var rects = new SpriteRect[frameCount];
        for (var i = 0; i < frameCount; i++)
        {
            rects[i] = new SpriteRect
            {
                name = $"hero_ship_{i}",
                spriteID = GUID.Generate(),
                rect = new Rect(i * ShipFrameWidth, 0, ShipFrameWidth, ShipFrameHeight),
                alignment = SpriteAlignment.Center,
                pivot = new Vector2(0.5f, 0.5f)
            };
        }

        provider.SetSpriteRects(rects);
        provider.Apply();
        importer.SaveAndReimport();
    }

    private static void ConfigureStarfield()
    {
        var importer = AssetImporter.GetAtPath(StarfieldPath) as TextureImporter;
        if (importer == null)
        {
            Debug.LogError($"Starfield not found at {StarfieldPath}");
            return;
        }

        importer.textureType = TextureImporterType.Sprite;
        // Single, not Multiple: this is one whole tile, not a sheet of separate pictures.
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = 100f;
        importer.wrapMode = TextureWrapMode.Repeat;
        importer.mipmapEnabled = false;

        var settings = new TextureImporterSettings();
        importer.ReadTextureSettings(settings);
        // Tiled Draw Mode needs Full Rect - Tight would cut the transparent border and the tiling
        // would show seams.
        settings.spriteMeshType = SpriteMeshType.FullRect;
        importer.SetTextureSettings(settings);

        importer.SaveAndReimport();
    }

    /// <summary>
    /// Sprites come back in asset order, so "_10" would sort before "_2" - sort by the trailing number.
    /// </summary>
    private static List<Sprite> LoadSortedSprites(string path)
    {
        return AssetDatabase.LoadAllAssetsAtPath(path)
            .OfType<Sprite>()
            .OrderBy(sprite =>
            {
                var underscore = sprite.name.LastIndexOf('_');
                return underscore >= 0 && int.TryParse(sprite.name[(underscore + 1)..], out var index) ? index : 0;
            })
            .ToList();
    }

    // ------------------------------------------------------------------ scene

    private static void BuildScene()
    {
        ConfigureCamera(out var halfHeight, out var halfWidth);
        BuildBackground();
        BuildManagers();
        BuildPlayer(halfHeight);
    }

    private static void ConfigureCamera(out float halfHeight, out float halfWidth)
    {
        var camera = Camera.main;
        if (camera == null)
        {
            var cameraObject = new GameObject("Main Camera") { tag = "MainCamera" };
            camera = cameraObject.AddComponent<Camera>();
            cameraObject.AddComponent<AudioListener>();
        }

        camera.orthographic = true;
        camera.orthographicSize = CameraSize;
        camera.backgroundColor = Color.black;
        camera.transform.position = new Vector3(0f, 0f, -10f);

        halfHeight = CameraSize;
        halfWidth = CameraSize * camera.aspect;
    }

    private static void BuildBackground()
    {
        var background = Find("Background") ?? new GameObject("Background");
        background.transform.position = Vector3.zero;
        // Never scaled - a background is made bigger by tiling it, not by stretching it.
        background.transform.localScale = Vector3.one;

        var renderer = GetOrAdd<SpriteRenderer>(background);
        renderer.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(StarfieldPath);
        renderer.drawMode = SpriteDrawMode.Tiled;
        renderer.tileMode = SpriteTileMode.Continuous;

        // Size MUST be a whole number of tiles. Tiled draw mode divides Size into an even count of
        // tiles, so a size of 32 over a 15.24 tile silently resizes every tile to 10.67 - and then
        // the scroller's snap-back is the wrong distance and the seam shows every few seconds.
        var tile = renderer.sprite.bounds.size;
        renderer.size = new Vector2(tile.x * 2f, tile.y * 3f);
        renderer.sortingOrder = -100;

        GetOrAdd<ScrollingBackground>(background);
    }

    private static void BuildManagers()
    {
        var managers = Find("Managers") ?? new GameObject("Managers");
        managers.transform.position = Vector3.zero;
        GetOrAdd<GameManager>(managers);
    }

    private static void BuildPlayer(float halfHeight)
    {
        var player = Find("Player") ?? new GameObject("Player");
        player.tag = Constants.PlayerTag;
        // Sitting one unit up from the bottom edge, derived from the camera rather than typed in.
        player.transform.position = new Vector3(0f, -halfHeight + 1f, 0f);
        player.transform.localScale = Vector3.one;

        var sprites = LoadSortedSprites(ShipSheetPath);

        var renderer = GetOrAdd<SpriteRenderer>(player);
        if (sprites.Count > 0) renderer.sprite = sprites[0];
        renderer.sortingOrder = 10;

        // No Animator on the ship. The eight frames are banking poses, not an animation loop -
        // PlayerController picks one from the current input instead.
        if (player.TryGetComponent<Animator>(out var strayAnimator))
        {
            Object.DestroyImmediate(strayAnimator);
        }

        var body = GetOrAdd<Rigidbody2D>(player);
        body.bodyType = RigidbodyType2D.Kinematic;
        body.gravityScale = 0f;

        var collider = GetOrAdd<BoxCollider2D>(player);
        if (sprites.Count > 0) collider.size = sprites[0].bounds.size * 0.8f;
        collider.isTrigger = true;

        var controller = GetOrAdd<PlayerController>(player);

        // Fill the pose list in frame order, so index 0 is the full left lean.
        var serialized = new SerializedObject(controller);
        var frames = serialized.FindProperty("_bankFrames");
        frames.arraySize = sprites.Count;
        for (var i = 0; i < sprites.Count; i++)
        {
            frames.GetArrayElementAtIndex(i).objectReferenceValue = sprites[i];
        }
        serialized.ApplyModifiedProperties();
    }

    // ------------------------------------------------------------------ helpers

    private static GameObject Find(string name)
    {
        return EditorSceneManager.GetActiveScene()
            .GetRootGameObjects()
            .FirstOrDefault(gameObject => gameObject.name == name);
    }

    private static T GetOrAdd<T>(GameObject target) where T : Component
    {
        return target.TryGetComponent<T>(out var existing) ? existing : target.AddComponent<T>();
    }
}
