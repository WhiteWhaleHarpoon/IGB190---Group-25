using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FastFeedback
{
    /// <summary>
    /// Enable an outline on the specified game object, using the specified settings.
    /// </summary>
    [System.Serializable]
    public class EnableOutlineWithColor : FeedbackItem
    {
        public GameObject outlineToEnable;
        public Color outlineColor = Color.yellow;

        public float outlineThickness = 3.0f;

        public DurationSettings durationSettings = DurationSettings.Forever;
        public float durationTime = 0.1f;

        public OutlineType outlineType;
        public float outlineFlashTime = 0.1f;

        public int priority = 4;

        public enum DurationSettings
        {
            Forever,
            ForXSeconds
        };

        public enum OutlineType
        {
            Solid,
            FlashEveryXSeconds
        };


        /// <summary>
        /// Activate the feedback effect, setting up all appropriate actions using the given settings.
        /// </summary>
        public override void Activate(GameObject target = null, GameObject origin = null, Vector3 targetPosition = new Vector3())
        {
            base.Activate(target, origin, targetPosition);

            // Guard clauses.
            GameObject obj = GetTargetGameObject(target);
            if (obj == null) return;

            // Feedback effects.
            EnableOutlineEffect effect = obj.GetComponent<EnableOutlineEffect>();
            if (effect == null) effect = obj.AddComponent<EnableOutlineEffect>();
            effect.Apply(this, target, origin);
        }

#if UNITY_EDITOR
        /// <summary>
        /// Draw the custom editor UI for the effect.
        /// </summary>
        public override void DrawUI(GameFeedback target)
        {
            base.DrawUI(target);

            // Options for choosing which game object to affect.
            targetGameObjectSettings = (GameObjectSettings)EditorGUILayout.EnumPopup("Object to Outline", targetGameObjectSettings);
            if (targetGameObjectSettings == GameObjectSettings.GameObjectWithTag)
                targetGameObjectTag = EditorGUILayout.TextField(" ", targetGameObjectTag);
            EditorGUILayout.Space(SPACING_BETWEEN_ITEMS);

            // Option for choosing the outline color.
            outlineColor = EditorGUILayout.ColorField("Color", outlineColor);
            EditorGUILayout.Space(SPACING_BETWEEN_ITEMS);

            // Option for choosing the outline size.
            outlineThickness = EditorGUILayout.Slider("Thickness", outlineThickness, 1.0f, 20.0f);
            EditorGUILayout.Space(SPACING_BETWEEN_ITEMS);

            // Option for choosing if the outline should flash.
            outlineType = (OutlineType)EditorGUILayout.EnumPopup("Type", outlineType);
            if (outlineType == OutlineType.FlashEveryXSeconds)
            {
                outlineFlashTime = EditorGUILayout.FloatField(" ", outlineFlashTime);
            }
            EditorGUILayout.Space(SPACING_BETWEEN_ITEMS);

            // Option for choosing the outline duration.
            durationSettings = (DurationSettings)EditorGUILayout.EnumPopup("Duration", durationSettings);
            if (durationSettings == DurationSettings.ForXSeconds)
            {
                durationTime = EditorGUILayout.FloatField(" ", durationTime);
            }
            EditorGUILayout.Space(SPACING_BETWEEN_ITEMS);

            // Option for choosing the outline priority.
            priority = EditorGUILayout.IntSlider("Priority", priority, 1, 10);
            EditorGUILayout.Space(SPACING_BETWEEN_ITEMS);

            // Option for choosing the outline tag.
            effectTag = EditorGUILayout.TextField("Reference Key", effectTag);
            EditorGUILayout.Space(SPACING_BETWEEN_ITEMS);

            // Error checking.
            hasError = false; 
            if (RequiresTag(targetGameObjectSettings) && targetGameObjectTag.Length == 0)
            {
                hasError = true;
                EditorGUILayout.HelpBox("You must assign a valid GameObject tag.", MessageType.Error);
            }
        }

        /// <summary>
        /// Return the custom editor icon for the effect.
        /// </summary>
        public override Texture2D GetIcon()
        {
            return (Texture2D)EditorGUIUtility.Load(@"LightProbeProxyVolume Gizmo");
        }

        /// <summary>
        /// Return a short description of the effect.
        /// </summary>
        public override string GetDescription()
        {
            return "Outline - Add Object Outline";
        }
#endif
    }
}