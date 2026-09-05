using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

/// <summary>
/// Rebuilds the chicken flap clip and its controller from the sliced chicken-wings sheet.
/// Menu: Tools > Chicken Invaders > Rebuild Flap Animation
///
/// The clip is written through Unity's own API rather than by hand, so the curve bindings are
/// always the ones the Animation window expects.
/// </summary>
public static class ChickenAnimationBuilder
{
    private const string WingsSheetPath = "Assets/Art/Sprites/chicken-wings.png";
    private const string ClipPath       = "Assets/Animations/ChickenFlap.anim";
    private const string ControllerPath = "Assets/Animations/ChickenAnimator.controller";

    private const int FrameStep = 4;
    private const float FrameRate = 24f;

    [MenuItem("Tools/Chicken Invaders/Rebuild Flap Animation")]
    public static void Rebuild()
    {
        var sprites = AssetDatabase.LoadAllAssetsAtPath(WingsSheetPath)
            .OfType<Sprite>()
            .OrderBy(SpriteIndex)
            .ToList();

        if (sprites.Count == 0)
        {
            Debug.LogError($"No sprites found at {WingsSheetPath}. Is the sheet sliced (Sprite Mode: Multiple)?");
            return;
        }

        // Frames 0..N are one continuous flap from wings-up to wings-down, so the loop plays out
        // and back again. Sub-sampling keeps the key count sane while staying smooth at 24 fps.
        var order = new List<int>();
        for (var i = 0; i < sprites.Count; i += FrameStep) order.Add(i);
        for (var i = order.Count - 2; i > 0; i--) order.Add(order[i]);

        var keys = new ObjectReferenceKeyframe[order.Count];
        for (var i = 0; i < order.Count; i++)
        {
            keys[i] = new ObjectReferenceKeyframe
            {
                time = i / FrameRate,
                value = sprites[order[i]]
            };
        }

        var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(ClipPath);
        if (clip == null)
        {
            clip = new AnimationClip();
            AssetDatabase.CreateAsset(clip, ClipPath);
        }

        // The asset is kept (deleting it would change its GUID and break the controller's
        // reference), but every existing curve is wiped first - that is what clears the broken
        // "Sprite (Missing!)" bindings left by the hand-written version.
        clip.ClearCurves();
        clip.frameRate = FrameRate;

        var binding = EditorCurveBinding.PPtrCurve(string.Empty, typeof(SpriteRenderer), "m_Sprite");
        AnimationUtility.SetObjectReferenceCurve(clip, binding, keys);

        var settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = true;
        AnimationUtility.SetAnimationClipSettings(clip, settings);

        EditorUtility.SetDirty(clip);

        var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
        if (controller == null)
        {
            controller = AnimatorController.CreateAnimatorControllerAtPathWithClip(ControllerPath, clip);
            controller.layers[0].stateMachine.states[0].state.name = "Flap";
        }
        else
        {
            var stateMachine = controller.layers[0].stateMachine;
            var state = stateMachine.states.Length > 0
                ? stateMachine.states[0].state
                : stateMachine.AddState("Flap");

            state.motion = clip;
            stateMachine.defaultState = state;
            EditorUtility.SetDirty(controller);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"ChickenFlap rebuilt: {keys.Length} keys, {keys.Length / FrameRate:0.00}s, " +
                  $"from {sprites.Count} sliced frames.");
    }

    /// <summary>
    /// Sprites come back in asset order, not numeric order, so "_10" would sort before "_2".
    /// </summary>
    private static int SpriteIndex(Sprite sprite)
    {
        var underscore = sprite.name.LastIndexOf('_');
        return underscore >= 0 && int.TryParse(sprite.name[(underscore + 1)..], out var index)
            ? index
            : 0;
    }
}
