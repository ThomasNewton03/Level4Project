using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Visometry.VisionLib.SDK.Core.Details;

namespace Visometry.VisionLib.SDK.Core
{
    /// <summary>
    /// Editor Window allowing to display information about an operation that the user can accept or cancel.
    /// </summary>
    public class InfoDialogWindow : VisionLibDialogWindow
    {
        private List<LinkButtonContent> infoLinks;
        private bool dontShowAgain = false;
        private const int toggleMaxWidth = 30;
        public const string setupTrackingPlayerPrefKey = "VisionLib.DontShowSetupTrackingDialog";
        public const string useForTrackingPlayerPrefKey = "VisionLib.DontShowUseForTrackingDialog";

        protected override void DrawAdditionalContent()
        {
            DrawLinks();
        }

        protected override void DrawAdditionalOptions()
        {
            this.dontShowAgain = EditorGUILayout.Toggle(
                this.dontShowAgain,
                GUILayout.MaxWidth(InfoDialogWindow.toggleMaxWidth));
            GUILayout.Label("Do not ask me again");
        }

        public static bool OpenSetupTrackingDialog()
        {
            const string windowTitle = "VisionLib Info - Setup Tracking";
            const string message =
                "This operation will add a VLTracking Prefab to the current scene. This prefab " +
                "includes the VLTrackingConfiguration GameObject, VLCamera GameObject and all " +
                "components required for VisionLib Tracking.\n\nMark at least one mesh in your " +
                "scene with 'VisionLib > Use For Tracking' to set up model tracking.";

            var links = new List<LinkButtonContent>
            {
                new()
                {
                    linkButtonLabel = "Using Models from the Unity Hierarchy",
                    linkButtonTooltip = "After adding the basic VisionLib components to your scene, select your models as tracking geometry.",
                    linkURL = DocumentationLink.modelInjection
                }
            };

            return OpenDialog(
                windowTitle,
                message,
                links,
                InfoDialogWindow.setupTrackingPlayerPrefKey,
                "Setup Tracking");
        }

        public static bool OpenUseForTrackingDialog(string gameObjectName)
        {
            const string windowTitle = "VisionLib Info - Use For Tracking";
            var message =
                "This operation will add a TrackingMesh component to all GameObjects below '" +
                gameObjectName + "' that contain a Mesh Component. All meshes " +
                "marked as Tracking Mesh will be used for VisionLib's Model Tracking." +
                "\n\nGameObject '" + gameObjectName + "' will be re-parented " +
                "under a new Tracking Anchor GameObject. The Tracking Anchor " +
                "combines all its child meshes into a single tracking target during " +
                "tracking.\n\nA RenderedObject component will be added to the " +
                "Tracking Anchor GameObject. This component manages the visibility and other" +
                " rendering-related features of all meshes below itself.";

            var links = new List<LinkButtonContent>
            {
                new()
                {
                    linkButtonLabel = "Tracking Anchor",
                    linkButtonTooltip = "More details on the TrackingAnchor component - the core component of VisionLib's model tracking in Unity.",
                    linkURL = DocumentationLink.trackingAnchor
                },
                new()
                {
                    linkButtonLabel = "Tracking Mesh",
                    linkButtonTooltip = "More details on the TrackingMesh component - the representation of a single tracking geometry.",
                    linkURL = DocumentationLink.trackingMesh
                },
                new()
                {
                    linkButtonLabel = "Rendered Object",
                    linkButtonTooltip = "More details on the RenderedObject component - the key component that connects the augmented content meshes to a TrackingAnchor.",
                    linkURL = DocumentationLink.renderedObject
                }
            };

            return OpenDialog(
                windowTitle,
                message,
                links,
                InfoDialogWindow.useForTrackingPlayerPrefKey,
                "Use For Tracking");
        }

        private void DrawLinks()
        {
            if (this.infoLinks.Count < 1)
            {
                return;
            }

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(VisionLibDialogWindow.tabbingSpace);
            EditorGUILayout.LabelField("Learn more", CustomEditorStyles.boldWithWrappedLabel);
            EditorGUILayout.EndHorizontal();

            foreach (var linkDescription in this.infoLinks)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(VisionLibDialogWindow.tabbingSpace);
                ButtonParameters.DrawLinkButton(linkDescription);
                EditorGUILayout.EndHorizontal();
            }
        }

        private static bool OpenDialog(
            string windowTitle,
            string message,
            List<LinkButtonContent> links,
            string playerPrefKey,
            string acceptText)
        {
            if (PlayerPrefs.HasKey(playerPrefKey) && PlayerPrefs.GetInt(playerPrefKey) == 1)
            {
                return true;
            }

            var infoDialogWindow = CreateInstance<InfoDialogWindow>();

            infoDialogWindow.titleContent.text = windowTitle;
            infoDialogWindow.dialogMessage = message;
            infoDialogWindow.infoLinks = links;
            infoDialogWindow.infoAcceptText = acceptText;
            infoDialogWindow.Resize();

            infoDialogWindow.ShowModalUtility();

            if (infoDialogWindow.dontShowAgain)
            {
                PlayerPrefs.SetInt(playerPrefKey, 1);
            }

            var dialogResult = infoDialogWindow.dialogResult;
            infoDialogWindow.Destroy();
            return dialogResult;
        }

        protected override float CalculateWindowHeightFromContent()
        {
            return CalculateBaseWindow() +
                   this.infoLinks.Count * VisionLibDialogWindow.lineHeightFactor;
        }

        [MenuItem("VisionLib/Local Settings/Reset Show Info Dialog Preferences", false, 1010)]
        private static void DeleteShowInfoDialogPlayerPrefs()
        {
            PlayerPrefs.DeleteKey(InfoDialogWindow.setupTrackingPlayerPrefKey);
            PlayerPrefs.DeleteKey(InfoDialogWindow.useForTrackingPlayerPrefKey);
        }
    }
}
