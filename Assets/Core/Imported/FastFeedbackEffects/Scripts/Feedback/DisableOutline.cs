using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FastFeedback
{
    /// <summary>
    /// Disable the outline on the specified game object, using the specified settings.
    /// </summary>
    [System.Serializable]
    public class DisableOutline : FeedbackItem
    {
        public GameObject outlineToDisable;
        public DisableSettings disableSettings;
        public int disablePriority = 1;

        public enum DisableSettings
        {
            DisableAllOutlines,
            DisableOutlinesWithReferenceKey,
            DisableOutlinesWithPriority,
            DisableOutlinesLessThanPriority,
            DisableOutlinesGreaterThanPriority
        }

        /// <summary>
        /// Activate the feedback effect, setting up all appropriate actions using the given settings.
        /// </summary>
        public override void Activate(GameObject target = null, GameObject origin = null, Vector3 targetPosition = new Vector3())
        {
            base.Activate(target, origin, targetPosition);
            GameObject obj = GetTargetGameObject(target);
            if (obj == null) return;
            EnableOutlineEffect outlineEffect = obj.GetComponent<EnableOutlineEffect>();
            if (outlineEffect == null) return;

            if (disableSettings == DisableSettings.DisableAllOutlines)
                outlineEffect.RemoveAllOutlines();
            else if (disableSettings == DisableSettings.DisableOutlinesWithReferenceKey)
                outlineEffect.RemoveOutlinesWithTag(effectTag);
            else if (disableSettings == DisableSettings.DisableOutlinesGreaterThanPriority)
                outlineEffect.RemoveOutlinesGreaterThanPriority(disablePriority);
            else if (disableSettings == DisableSettings.DisableOutlinesLessThanPriority)
                outlineEffect.RemoveOutlinesLessThanPriority(disablePriority);
            else if (disableSettings == DisableSettings.DisableOutlinesWithPriority)
                outlineEffect.RemoveOutlinesWithPriority(disablePriority);
        }

#if UNITY_EDITOR
        /// <summary>
        /// Draw the custom editor UI for the effect.
        /// </summary>
        public override void DrawUI(GameFeedback target)
        {
            base.DrawUI(target);

            // Options for selecting which object to affect.
            targetGameObjectSettings = (GameObjectSettings)EditorGUILayout.EnumPopup("Object with Outline", targetGameObjectSettings);
            if (RequiresTag(targetGameObjectSettings))
                targetGameObjectTag = EditorGUILayout.TextField(" ", targetGameObjectTag);
            EditorGUILayout.Space(SPACING_BETWEEN_ITEMS);

            // Options for specifying which outline(s) to disable.
            disableSettings = (DisableSettings)EditorGUILayout.EnumPopup("Disable Settings", disableSettings);
            EditorGUILayout.Space(SPACING_BETWEEN_ITEMS);
            if (disableSettings == DisableSettings.DisableOutlinesWithReferenceKey)
            {
                effectTag = EditorGUILayout.TextField(" ", effectTag);
            }
            else if (disableSettings == DisableSettings.DisableOutlinesWithPriority ||
                disableSettings == DisableSettings.DisableOutlinesLessThanPriority ||
                disableSettings == DisableSettings.DisableOutlinesGreaterThanPriority)
            {
                disablePriority = EditorGUILayout.IntSlider(" ", disablePriority, 1, 10);
            }

            // Error handling.
            hasError = false;
            if (RequiresTag(targetGameObjectSettings) && targetGameObjectTag.Length == 0)
            {
                hasError = true;
                EditorGUILayout.HelpBox("You must assign a valid GameObject tag.", MessageType.Error);
            }
            if (disableSettings == DisableSettings.DisableOutlinesWithReferenceKey && effectTag.Length == 0)
            {
                hasError = true;
                EditorGUILayout.HelpBox("You must assign an outline tag to disable.", MessageType.Error);
            }
        }

        /// <summary>
        /// Return the custom editor icon for the effect.
        /// </summary>
        public override string GetDescription()
        {
            return "Outline - Remove Object Outlines";
        }

        /// <summary>
        /// Return a short description of the effect.
        /// </summary>
        public override Texture2D GetIcon()
        {
            return (Texture2D)EditorGUIUtility.Load(@"LightProbeProxyVolume Gizmo");
        }
#endif
    }
}