using UnityEditor;
using UnityEngine;

namespace Visometry.VisionLib.SDK.Core
{
    [CustomEditor(typeof(PosterAnalyzer))]
    public class PosterAnalyzerEditor : Editor
    {
        private PosterAnalyzer posterAnalyzer;
        private SerializedProperty textureProp;
        private double posterQuality;

        void OnEnable()
        {
            this.posterAnalyzer = target as PosterAnalyzer;
            this.textureProp = serializedObject.FindProperty("texture");
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(this.textureProp);

            if (EditorGUI.EndChangeCheck())
            {
                this.serializedObject.ApplyModifiedProperties();
                this.posterQuality = this.posterAnalyzer.GetPosterQuality();
            }

            if (this.posterQuality >= 0)
            {
                EditorGUILayout.LabelField("Quality", this.posterQuality.ToString());
            }
        }
    }
}
