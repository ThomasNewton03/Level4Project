using System.Collections.Generic;
using UnityEditor;
using Visometry.VisionLib.SDK.Core.Details;

namespace Visometry.VisionLib.SDK.Core
{
    public abstract class PrefabObsoleteMarkerEditorBase : Editor
    {
        protected SerializedProperty obsoletePrefabNameProperty;
        protected SerializedProperty hasReplacementProperty;
        protected SerializedProperty replacementPrefabNameProperty;
        protected SerializedProperty documentationLinksProperty;
        protected PrefabObsoleteMarker prefabObsoleteMarker;

        private void OnEnable()
        {
            this.obsoletePrefabNameProperty =
                this.serializedObject.FindProperty("obsoletePrefabName");
            this.hasReplacementProperty = this.serializedObject.FindProperty("hasReplacement");
            this.replacementPrefabNameProperty =
                this.serializedObject.FindProperty("replacementPrefabName");
            this.documentationLinksProperty =
                this.serializedObject.FindProperty("docsLinkDescriptions");
            this.prefabObsoleteMarker = this.serializedObject.targetObject as PrefabObsoleteMarker;
        }

        public override void OnInspectorGUI()
        {
            this.serializedObject.Update();
            EditorGUILayout.HelpBox(
                this.prefabObsoleteMarker.GetWarningMessage(),
                MessageType.Error);

            DrawDocumentationLinkButtons();

            this.serializedObject.ApplyModifiedProperties();
        }

        protected void DrawDocumentationLinkButtons()
        {
            if (this.documentationLinksProperty.arraySize > 0)
            {
                EditorGUILayout.LabelField(
                    "For help with upgrading from this obsolete prefab see: ");
            }
            for (var i = 0; i < this.documentationLinksProperty.arraySize; i++)
            {
                var docsLinkDescription = this.documentationLinksProperty.GetArrayElementAtIndex(i);
                
                ButtonParameters.DrawLinkButton(
                    docsLinkDescription.FindPropertyRelative("linkButtonLabel").stringValue,
                    docsLinkDescription.FindPropertyRelative("linkButtonTooltip").stringValue,
                    docsLinkDescription.FindPropertyRelative("linkURL").stringValue);
                    
            }
        }
    }
}
