using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Idempotent setup for regular chicken waves: prefab physics, tuning assets and scene references.
/// </summary>
public static class ChickenWaveSetup
{
    private const string BalancePath = "Assets/Config/GameBalance.asset";
    private const string WaveFolder = "Assets/Config/Waves";
    private const string BaseChickenPath = "Assets/Prefabs/Chickens/Chicken.prefab";
    private const string UnlitSpriteMaterialPath =
        "Packages/com.unity.render-pipelines.universal/Runtime/Materials/Sprite-Unlit-Default.mat";
    private static readonly string[] ChickenPrefabPaths =
    {
        "Assets/Prefabs/Chickens/ChickenRed.prefab",
        "Assets/Prefabs/Chickens/ChickenGreen.prefab",
        "Assets/Prefabs/Chickens/ChickenBlue.prefab"
    };

    [InitializeOnLoadMethod]
    private static void ScheduleSetupForOpenGameplayScene()
    {
        EditorApplication.delayCall += TrySetUpOpenGameplayScene;
    }

    private static void TrySetUpOpenGameplayScene()
    {
        if (Application.isPlaying || EditorApplication.isCompiling)
        {
            return;
        }

        var scene = SceneManager.GetActiveScene();
        var managers = FindRoot(scene, "Managers");
        var player = FindRoot(scene, "Player");
        var unlitMaterial = AssetDatabase.LoadAssetAtPath<Material>(UnlitSpriteMaterialPath);
        var playerNeedsUnlitMaterial = player != null &&
            player.TryGetComponent<SpriteRenderer>(out var playerRenderer) &&
            playerRenderer.sharedMaterial != unlitMaterial;
        if (scene.path == "Assets/Scenes/SampleScene.unity" && managers != null &&
            (managers.GetComponent<WaveManager>() == null ||
             SortingLayer.NameToID("Chickens") == 0 ||
             playerNeedsUnlitMaterial))
        {
            SetUpScene(saveOpenScenes: true);
        }
    }

    [MenuItem("Tools/Chicken Invaders/Set Up Chicken Waves")]
    public static void SetUpFromMenu()
    {
        SetUpScene(saveOpenScenes: true);
    }

    public static void SetUpScene(bool saveOpenScenes)
    {
        if (Application.isPlaying)
        {
            Debug.LogError("Exit Play Mode before setting up chicken waves.");
            return;
        }

        var scene = SceneManager.GetActiveScene();
        var managers = FindRoot(scene, "Managers");
        var player = FindRoot(scene, "Player");
        var unlitMaterial = AssetDatabase.LoadAssetAtPath<Material>(UnlitSpriteMaterialPath);
        if (managers == null)
        {
            Debug.LogError("The open scene needs a root object named Managers.");
            return;
        }

        var enemyLayer = EnsureLayer(Constants.EnemyLayer);
        EnsureSortingLayer("Chickens");
        EnsureSortingLayer("Player");
        if (enemyLayer < 0)
        {
            Debug.LogError("Could not create the Enemy physics layer.");
            return;
        }

        ConfigureChickenPrefab(enemyLayer);

        var balance = LoadOrCreateAsset<GameBalanceConfig>(BalancePath);
        var waves = CreateWaveAssets();
        var chickenPrefabs = ChickenPrefabPaths
            .Select(path => AssetDatabase.LoadAssetAtPath<GameObject>(path))
            .Where(prefab => prefab != null)
            .Select(prefab => prefab.GetComponent<Chicken>())
            .Where(chicken => chicken != null)
            .ToArray();

        if (balance == null || waves.Any(wave => wave == null) || chickenPrefabs.Length == 0)
        {
            Debug.LogError("Chicken wave setup could not load its prefabs or configuration assets.");
            return;
        }

        var factory = managers.GetComponent<ChickenFactory>() ?? Undo.AddComponent<ChickenFactory>(managers);
        var factoryProperties = new SerializedObject(factory);
        var prefabList = factoryProperties.FindProperty("_chickenPrefabs");
        prefabList.arraySize = chickenPrefabs.Length;
        for (var index = 0; index < chickenPrefabs.Length; index++)
        {
            prefabList.GetArrayElementAtIndex(index).objectReferenceValue = chickenPrefabs[index];
        }
        factoryProperties.ApplyModifiedProperties();

        var waveManager = managers.GetComponent<WaveManager>() ?? Undo.AddComponent<WaveManager>(managers);
        var managerProperties = new SerializedObject(waveManager);
        managerProperties.FindProperty("_balance").objectReferenceValue = balance;
        managerProperties.FindProperty("_factory").objectReferenceValue = factory;
        var waveList = managerProperties.FindProperty("_waves");
        waveList.arraySize = waves.Length;
        for (var index = 0; index < waves.Length; index++)
        {
            waveList.GetArrayElementAtIndex(index).objectReferenceValue = waves[index];
        }
        managerProperties.ApplyModifiedProperties();

        if (player != null && player.TryGetComponent<SpriteRenderer>(out var playerRenderer))
        {
            Undo.RecordObject(playerRenderer, "Set player sorting layer");
            playerRenderer.sortingLayerName = "Player";
            playerRenderer.sharedMaterial = unlitMaterial;
        }

        EditorUtility.SetDirty(managers);
        EditorSceneManager.MarkSceneDirty(scene);
        AssetDatabase.SaveAssets();
        if (saveOpenScenes)
        {
            EditorSceneManager.SaveOpenScenes();
        }

        Debug.Log("Chicken waves configured: four formations enter, drift, descend and accelerate.");
    }

    private static void ConfigureChickenPrefab(int enemyLayer)
    {
        var root = PrefabUtility.LoadPrefabContents(BaseChickenPath);
        if (root == null)
        {
            Debug.LogError($"Chicken prefab was not found at {BaseChickenPath}.");
            return;
        }

        try
        {
            root.layer = enemyLayer;
            var body = root.GetComponent<Rigidbody2D>();
            var collider = root.GetComponent<Collider2D>();
            var chicken = root.GetComponent<Chicken>() ?? root.AddComponent<Chicken>();
            if (body == null || collider == null)
            {
                Debug.LogError("The base Chicken prefab needs a Rigidbody2D and Collider2D.", root);
                return;
            }

            body.bodyType = RigidbodyType2D.Kinematic;
            body.gravityScale = 0f;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            var properties = new SerializedObject(chicken);
            properties.FindProperty("_rigidbody2D").objectReferenceValue = body;
            properties.FindProperty("_collider2D").objectReferenceValue = collider;
            properties.ApplyModifiedPropertiesWithoutUndo();

            foreach (var renderer in root.GetComponentsInChildren<SpriteRenderer>(true))
            {
                renderer.sortingLayerName = "Chickens";
            }

            PrefabUtility.SaveAsPrefabAsset(root, BaseChickenPath);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    private static WaveConfig[] CreateWaveAssets()
    {
        EnsureFolder("Assets", "Config");
        EnsureFolder("Assets/Config", "Waves");

        var waves = new WaveConfig[4];
        for (var index = 0; index < waves.Length; index++)
        {
            var wave = LoadOrCreateAsset<WaveConfig>($"{WaveFolder}/Wave{index + 1:00}.asset");
            if (wave == null) continue;

            var properties = new SerializedObject(wave);
            properties.FindProperty("_rows").intValue = index + 2;
            properties.FindProperty("_columns").intValue = 5;
            var variants = properties.FindProperty("_rowVariants");
            variants.arraySize = 3;
            variants.GetArrayElementAtIndex(0).intValue = 0;
            variants.GetArrayElementAtIndex(1).intValue = 1;
            variants.GetArrayElementAtIndex(2).intValue = 2;
            properties.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(wave);
            waves[index] = wave;
        }

        return waves;
    }

    private static T LoadOrCreateAsset<T>(string path) where T : ScriptableObject
    {
        var existing = AssetDatabase.LoadAssetAtPath<T>(path);
        if (existing != null) return existing;

        var folder = path.Substring(0, path.LastIndexOf('/'));
        var parentSlash = folder.LastIndexOf('/');
        EnsureFolder(folder.Substring(0, parentSlash), folder.Substring(parentSlash + 1));
        var asset = ScriptableObject.CreateInstance<T>();
        AssetDatabase.CreateAsset(asset, path);
        return asset;
    }

    private static GameObject FindRoot(Scene scene, string objectName)
    {
        return scene.GetRootGameObjects().FirstOrDefault(root => root.name == objectName);
    }

    private static int EnsureLayer(string layerName)
    {
        var existing = LayerMask.NameToLayer(layerName);
        if (existing >= 0) return existing;

        var tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        var layers = tagManager.FindProperty("layers");
        for (var index = 8; index < 32; index++)
        {
            var layer = layers.GetArrayElementAtIndex(index);
            if (!string.IsNullOrEmpty(layer.stringValue)) continue;
            layer.stringValue = layerName;
            tagManager.ApplyModifiedProperties();
            AssetDatabase.SaveAssets();
            return index;
        }

        return -1;
    }

    private static void EnsureSortingLayer(string layerName)
    {
        var tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        var sortingLayers = tagManager.FindProperty("m_SortingLayers");
        for (var index = 0; index < sortingLayers.arraySize; index++)
        {
            var existingLayer = sortingLayers.GetArrayElementAtIndex(index);
            if (existingLayer.FindPropertyRelative("name").stringValue == layerName)
            {
                var uniqueId = existingLayer.FindPropertyRelative("uniqueID");
                if (uniqueId.intValue == 0 && layerName != "Default")
                {
                    uniqueId.intValue = GetSortingLayerId(layerName);
                    tagManager.ApplyModifiedProperties();
                    AssetDatabase.SaveAssets();
                }
                return;
            }
        }

        var newIndex = sortingLayers.arraySize;
        sortingLayers.InsertArrayElementAtIndex(newIndex);
        var newSortingLayer = sortingLayers.GetArrayElementAtIndex(newIndex);
        newSortingLayer.FindPropertyRelative("name").stringValue = layerName;
        newSortingLayer.FindPropertyRelative("uniqueID").intValue = GetSortingLayerId(layerName);
        newSortingLayer.FindPropertyRelative("locked").boolValue = false;
        tagManager.ApplyModifiedProperties();
        AssetDatabase.SaveAssets();
    }

    private static int GetSortingLayerId(string layerName)
    {
        return layerName == "Chickens" ? 1769173620 : 1769173621;
    }

    private static void EnsureFolder(string parentPath, string childName)
    {
        var path = parentPath + "/" + childName;
        if (!AssetDatabase.IsValidFolder(path)) AssetDatabase.CreateFolder(parentPath, childName);
    }
}
