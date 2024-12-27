using UnityEngine;
using UnityEditor;

namespace Visometry.VisionLib.SDK.HoloLens
{
    /// \deprecated HoloLensInitCamera is obsolete. Please use the new TrackingAnchor.
    [CustomEditor(typeof(HoloLensInitCamera))]
    [System.Obsolete("HoloLensInitCamera is obsolete. Please use the new TrackingAnchor.")]
    public class HoloLensInitCameraEditor : UnityEditor.Editor
    {
        private HoloLensInitCamera initCamera;

        void Reset()
        {
            this.initCamera = target as HoloLensInitCamera;
        }

        private void OnEnable()
        {
            this.initCamera = target as HoloLensInitCamera;
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            if (GUILayout.Button(
                    "Align with view",
                    GUILayout.Height(25)))
            {
                initCamera.AlignWithView();
            }

            // Only show the VisionLib initPose, if the InitCamera is selected in hierarchy
            if (!PrefabUtility.IsPartOfPrefabInstance(this.targets[0]))
            {
                return;
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
