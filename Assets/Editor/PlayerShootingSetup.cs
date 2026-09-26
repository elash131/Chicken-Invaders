using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Idempotent editor setup for the approved player-shot slice: named layers, one Ion-bullet
/// prefab, the player muzzle and the pool references in the currently open scene.
/// </summary>
public static class PlayerShootingSetup
{
    private const string BulletSheetPath = "Assets/Art/Sprites/bulletIon.png";
    private const string BulletSpriteName = "bulletIon_0";
    private const string ProjectileFolderPath = "Assets/Prefabs/Projectiles";
    private const string BulletPrefabPath = ProjectileFolderPath + "/PlayerBullet.prefab";
    private const string UnlitSpriteMaterialPath =
        "Packages/com.unity.render-pipelines.universal/Runtime/Materials/Sprite-Unlit-Default.mat";

    [MenuItem("Tools/Chicken Invaders/Set Up Player Shooting")]
    public static void SetUpFromMenu()
    {
        SetUpScene(saveOpenScenes: true);
    }

    public static void SetUpScene(bool saveOpenScenes)
    {
        if (Application.isPlaying)
        {
            Debug.LogError("Exit Play Mode before setting up player shooting.");
            return;
        }

        var scene = SceneManager.GetActiveScene();
        var player = FindRoot(scene, "Player");
        var managers = FindRoot(scene, "Managers");

        if (player == null || managers == null)
        {
            Debug.LogError("The open scene needs root objects named Player and Managers.");
            return;
        }

        var playerLayer = EnsureLayer(Constants.PlayerLayer);
        var projectileLayer = EnsureLayer(Constants.PlayerProjectileLayer);
        EnsureLayer(Constants.EnemyLayer);
        EnsureSortingLayer("Projectiles");

        if (playerLayer < 0 || projectileLayer < 0)
        {
            Debug.LogError("Could not create the player projectile physics layers.");
            return;
        }

        EnsureFolder("Assets/Prefabs", "Projectiles");
        var bulletPrefab = BuildBulletPrefab(projectileLayer);
        if (bulletPrefab == null)
        {
            return;
        }

        Undo.RecordObject(player, "Set player physics layer");
        player.layer = playerLayer;

        var playerController = player.GetComponent<PlayerController>();
        var playerRenderer = player.GetComponent<SpriteRenderer>();
        if (playerController == null || playerRenderer == null)
        {
            Debug.LogError("Player needs PlayerController and SpriteRenderer components.", player);
            return;
        }

        var firePoint = player.transform.Find("FirePoint");
        if (firePoint == null)
        {
            var firePointObject = new GameObject("FirePoint");
            Undo.RegisterCreatedObjectUndo(firePointObject, "Create player fire point");
            firePoint = firePointObject.transform;
            firePoint.SetParent(player.transform, false);
        }

        var muzzleHeight = playerRenderer.sprite != null
            ? playerRenderer.sprite.bounds.extents.y + 0.12f
            : 0.6f;
        Undo.RecordObject(firePoint, "Position player fire point");
        firePoint.localPosition = new Vector3(0f, muzzleHeight, 0f);
        firePoint.localRotation = Quaternion.identity;
        firePoint.localScale = Vector3.one;

        var projectilePool = managers.GetComponent<ProjectilePool>();
        if (projectilePool == null)
        {
            projectilePool = Undo.AddComponent<ProjectilePool>(managers);
        }

        var poolProperties = new SerializedObject(projectilePool);
        poolProperties.FindProperty("_bulletPrefab").objectReferenceValue = bulletPrefab;
        poolProperties.FindProperty("_prewarmCount").intValue = 12;
        poolProperties.FindProperty("_maxRetained").intValue = 32;
        poolProperties.ApplyModifiedProperties();

        var playerProperties = new SerializedObject(playerController);
        playerProperties.FindProperty("_firePoint").objectReferenceValue = firePoint;
        playerProperties.FindProperty("_projectilePool").objectReferenceValue = projectilePool;
        playerProperties.ApplyModifiedProperties();

        Physics2D.IgnoreLayerCollision(projectileLayer, playerLayer, true);
        Physics2D.IgnoreLayerCollision(projectileLayer, projectileLayer, true);

        EditorUtility.SetDirty(player);
        EditorUtility.SetDirty(managers);
        EditorSceneManager.MarkSceneDirty(scene);
        AssetDatabase.SaveAssets();

        if (saveOpenScenes)
        {
            EditorSceneManager.SaveOpenScenes();
        }

        Debug.Log("Player shooting configured: hold Space or gamepad south to fire Ion bullets.");
    }

    private static Projectile BuildBulletPrefab(int projectileLayer)
    {
        var bulletSprite = AssetDatabase.LoadAllAssetsAtPath(BulletSheetPath)
            .OfType<Sprite>()
            .FirstOrDefault(sprite => sprite.name == BulletSpriteName);

        if (bulletSprite == null)
        {
            Debug.LogError($"Sprite {BulletSpriteName} was not found in {BulletSheetPath}.");
            return null;
        }

        var unlitMaterial = AssetDatabase.LoadAssetAtPath<Material>(UnlitSpriteMaterialPath);
        if (unlitMaterial == null)
        {
            Debug.LogError($"Unlit sprite material was not found at {UnlitSpriteMaterialPath}.");
            return null;
        }

        var bulletObject = new GameObject("PlayerBullet");
        try
        {
            bulletObject.layer = projectileLayer;
            bulletObject.transform.rotation = Quaternion.Euler(0f, 0f, 90f);

            var renderer = bulletObject.AddComponent<SpriteRenderer>();
            renderer.sprite = bulletSprite;
            // Laser bolts emit their own light; scene lighting must not recolour the source art.
            renderer.sharedMaterial = unlitMaterial;
            renderer.sortingLayerName = "Projectiles";

            var body = bulletObject.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Dynamic;
            body.gravityScale = 0f;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            body.constraints = RigidbodyConstraints2D.FreezeRotation;

            var collider = bulletObject.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            collider.size = new Vector2(
                bulletSprite.bounds.size.x * 0.8f,
                bulletSprite.bounds.size.y * 0.65f);

            var projectile = bulletObject.AddComponent<Projectile>();
            var projectileProperties = new SerializedObject(projectile);
            projectileProperties.FindProperty("_rigidbody2D").objectReferenceValue = body;
            projectileProperties.ApplyModifiedProperties();

            var prefab = PrefabUtility.SaveAsPrefabAsset(bulletObject, BulletPrefabPath);
            return prefab != null ? prefab.GetComponent<Projectile>() : null;
        }
        finally
        {
            Object.DestroyImmediate(bulletObject);
        }
    }

    private static GameObject FindRoot(Scene scene, string objectName)
    {
        return scene.GetRootGameObjects().FirstOrDefault(root => root.name == objectName);
    }

    private static int EnsureLayer(string layerName)
    {
        var existing = LayerMask.NameToLayer(layerName);
        if (existing >= 0)
        {
            return existing;
        }

        var tagManager = new SerializedObject(
            AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        var layers = tagManager.FindProperty("layers");

        for (var index = 8; index < 32; index++)
        {
            var layer = layers.GetArrayElementAtIndex(index);
            if (!string.IsNullOrEmpty(layer.stringValue))
            {
                continue;
            }

            layer.stringValue = layerName;
            tagManager.ApplyModifiedProperties();
            AssetDatabase.SaveAssets();
            return index;
        }

        return -1;
    }

    private static void EnsureSortingLayer(string layerName)
    {
        var tagManager = new SerializedObject(
            AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        var sortingLayers = tagManager.FindProperty("m_SortingLayers");

        for (var index = 0; index < sortingLayers.arraySize; index++)
        {
            if (sortingLayers.GetArrayElementAtIndex(index)
                .FindPropertyRelative("name").stringValue == layerName)
            {
                return;
            }
        }

        var newIndex = sortingLayers.arraySize;
        sortingLayers.InsertArrayElementAtIndex(newIndex);
        var sortingLayer = sortingLayers.GetArrayElementAtIndex(newIndex);
        sortingLayer.FindPropertyRelative("name").stringValue = layerName;
        sortingLayer.FindPropertyRelative("uniqueID").intValue = 1769173619;
        sortingLayer.FindPropertyRelative("locked").boolValue = false;
        tagManager.ApplyModifiedProperties();
        AssetDatabase.SaveAssets();
    }

    private static void EnsureFolder(string parentPath, string childName)
    {
        var path = parentPath + "/" + childName;
        if (!AssetDatabase.IsValidFolder(path))
        {
            AssetDatabase.CreateFolder(parentPath, childName);
        }
    }
}
