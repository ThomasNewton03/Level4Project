using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Visometry.Helpers;
using Visometry.VisionLib.SDK.Core.Details;
using Visometry.VisionLib.SDK.Core.API;
using Visometry.VisionLib.SDK.Core.Details.TrackingAnchorTreeView;
using static Visometry.VisionLib.SDK.Core.TrackingAnchorEditorSettings;

namespace Visometry.VisionLib.SDK.Core
{
    [CustomEditor(typeof(TrackingAnchor), true)]
    public class TrackingAnchorEditor : Editor
    {
        private enum TrackingType
        {
            FrameToFrameTracking,
            ExistenceCheck
        }

        private const int maxBoundingBoxes = 15;

        private TrackingAnchorTreeEditor trackingAnchorTreeEditor;

        internal class FoldoutValues
        {
            public bool showTree;
            public bool showTrackingStateEvents;
            public bool showParentChildContent;
            public bool showTrackingGeometry;
            public bool showInitPoseSettings;
            public bool showAugmentationSettings;
            public bool showInitPoseJsonConversion;
            public bool showParameterJsonConversion;
            public bool showAdvancedSettingsInKeepUpright;
            public bool showTrackingParameter;
            public bool showAdvancedParameters;
            public bool showAnchorEvents;
        }

        internal static FoldoutValues state = new FoldoutValues();

        private TrackingAnchor trackingAnchor;
        private SerializedProperty anchorRuntimeParametersProperty;

        private SerializedProperty anchorNameProperty;
        private SerializedProperty slamCameraProperty;
        private SerializedProperty parentAnchorProperty;
        private SerializedProperty childAnchorsProperty;
        private SerializedProperty useInitPoseProperty;
        private SerializedProperty workSpacesProperty;

        private SerializedProperty keepUprightProperty;
        private SerializedProperty worldUpVectorProperty;
        private SerializedProperty modelUpVectorProperty;

        private JsonInputField<InitPoseHelper.JsonInitPose> legacyJsonInitPoseField;
        private JsonInputField<AnchorRuntimeParametersConverter> legacyJsonParametersField;

        private SerializedProperty unitProperty;
        private SerializedProperty persistParametersFromPlayModeProperty;
        private SerializedProperty persistInitPoseFromPlayModeProperty;

        private SerializedProperty onTrackedEventProperty;
        private SerializedProperty onTrackingCriticalEventProperty;
        private SerializedProperty onTrackingLostEventProperty;
        private SerializedProperty onAnchorEnabledEventProperty;
        private SerializedProperty onAnchorDisabledEventProperty;
        private SerializedProperty showInitPoseGuideWhileDisabledProperty;

        private Bounds? anchorBoundingBox;
        private SetupIssueCache geometrySetupIssueCache;
        private bool drawChildBoundingBoxes;
        private bool hasRenderers;

        private void OnEnable()
        {
            this.anchorNameProperty = this.serializedObject.FindProperty("anchorName");
            this.useInitPoseProperty =
                this.serializedObject.FindProperty("initPoseHandler.useInitPose");
            this.slamCameraProperty = this.serializedObject.FindProperty("slamCamera");
            this.parentAnchorProperty = this.serializedObject.FindProperty("parentAnchor");
            this.childAnchorsProperty = this.serializedObject.FindProperty("childAnchors");
            this.keepUprightProperty =
                this.serializedObject.FindProperty("initPoseHandler.keepUpright");
            this.worldUpVectorProperty =
                this.serializedObject.FindProperty("initPoseHandler.worldUpVector");
            this.modelUpVectorProperty =
                this.serializedObject.FindProperty("initPoseHandler.modelUpVector");

            this.workSpacesProperty = this.serializedObject.FindProperty("workSpaces");
            this.unitProperty = this.serializedObject.FindProperty("unit");
            this.persistParametersFromPlayModeProperty =
                this.serializedObject.FindProperty("persistParametersFromPlayMode");
            this.persistInitPoseFromPlayModeProperty =
                this.serializedObject.FindProperty("persistInitPoseFromPlayMode");
            this.onTrackedEventProperty = this.serializedObject.FindProperty("OnTracked");
            this.onTrackingCriticalEventProperty =
                this.serializedObject.FindProperty("OnTrackingCritical");
            this.onTrackingLostEventProperty = this.serializedObject.FindProperty("OnTrackingLost");
            this.onAnchorEnabledEventProperty =
                this.serializedObject.FindProperty("OnAnchorEnabled");
            this.onAnchorDisabledEventProperty =
                this.serializedObject.FindProperty("OnAnchorDisabled");
            this.showInitPoseGuideWhileDisabledProperty =
                this.serializedObject.FindProperty("augmentationHandler.showInitPoseGuideWhileDisabled");

            this.trackingAnchor = this.serializedObject.targetObject as TrackingAnchor;
            if (!this.trackingAnchor)
            {
                LogTrackingAnchorAccessError();
                return;
            }
            if (string.IsNullOrEmpty(this.anchorNameProperty.stringValue))
            {
                this.anchorNameProperty.stringValue = TrackingAnchor.CreateUniqueName();
                this.serializedObject.ApplyModifiedProperties();
            }
            UpdateAnchorHierarchy();
            EditorApplication.hierarchyChanged += UpdateAnchorHierarchy;
            this.legacyJsonInitPoseField = new(
                "Paste Init Pose:",
                "Parsed init pose." +
                "When pressing the apply button, this initPose will be applied to the GameObjects transform.",
                InitPoseHelper.JsonInitPose.Parse,
                DocumentationLink.initPoseJson,
                jsonInitPose =>
                {
                    var pose = jsonInitPose?.ToPose();
                    if (pose.HasValue)
                    {
                        this.trackingAnchor.SetVLInitPose(pose.Value);
                    }
                });
            this.legacyJsonParametersField = new(
                "Paste anchor parameter section:",
                "Parameters extracted from the parameter section. " +
                "All parameters from this TrackingAnchor will be overwritten. " +
                "Therefore this also lists the final parameters, even if they are not defined in the pasted parameter section",
                AnchorRuntimeParametersConverter.ParseParameters,
                DocumentationLink.modelTrackerConfig,
                parameters =>
                {
                    TrackingManager.CatchCommandErrors(
                        parameters?.ApplyToAnchorAsync(this.trackingAnchor),
                        this.trackingAnchor);
                });
            this.trackingAnchor.GetAnchorRuntimeParameters();
            this.anchorRuntimeParametersProperty = this.serializedObject.FindProperty("parameters");

            RecalculateCachedData();

            // This variable has to be assigned after the required variables have already been
            // initialized
            this.geometrySetupIssueCache = new SetupIssueCache(
                () =>
                {
                    var trackingGeometryIssue = SceneValidator.AggregateSceneIssues(
                        this.trackingAnchor.gameObject.GetComponentsInChildren<TrackingMesh>());
                    trackingGeometryIssue.AddRange(
                        this.trackingAnchor.GetTrackingGeometryIssues(this.anchorBoundingBox));
                    return trackingGeometryIssue;
                });
            EditorApplication.hierarchyChanged += RecalculateCachedData;
        }

        public void OnDisable()
        {
            EditorApplication.hierarchyChanged -= UpdateAnchorHierarchy;
            EditorApplication.hierarchyChanged -= RecalculateCachedData;
        }

        public override void OnInspectorGUI()
        {
            if (!this.trackingAnchor)
            {
                LogTrackingAnchorAccessError();
                return;
            }

            this.trackingAnchor.GetGeneralIssues().Draw();

            this.serializedObject.Update();

            EditorGUI.BeginDisabledGroup(Application.isPlaying);

            EditorGUILayout.PropertyField(this.anchorNameProperty);

            EditorGUI.EndDisabledGroup();

            DrawSLAMCameraSection();
            DrawTrackingType();
            DrawParentChildSection();
            DrawInitPoseSection();

            DrawParametersSection();
            DrawTrackingGeometrySection();
            DrawAugmentedContentSection();
            DrawTrackingEventsSection();
            DrawAnchorEventsSection();

            this.serializedObject.ApplyModifiedProperties();
        }

        private void OnSceneGUI()
        {
            if (!this.anchorBoundingBox.HasValue)
            {
                return;
            }
            AxisColoredBoundingBoxGizmo.Draw(
                this.anchorBoundingBox.Value,
                this.trackingAnchor.transform,
                this.trackingAnchor.GetMetric());

            if (!this.drawChildBoundingBoxes)
            {
                return;
            }
            var models = this.trackingAnchor.GetComponentsInChildren<TrackingObject>().Where(
                trackingObject => trackingObject.enabled && !trackingObject.occluder);
            foreach (var model in models)
            {
                model.DrawAxisColoredBoundingBoxGizmo(false);
            }
        }

        private void RecalculateCachedData()
        {
            var trackingObjects = this.trackingAnchor.GetComponentsInChildren<TrackingObject>()
                .Where(trackingObject => trackingObject.enabled).ToList();

            this.anchorBoundingBox = trackingObjects
                .Where(trackingObject => !trackingObject.occluder)
                .Select(o => o.GetBoundingBoxInAnchorCoordinates()).Combine();
            this.drawChildBoundingBoxes = trackingObjects.Count() <
                                          TrackingAnchorEditor.maxBoundingBoxes;
            this.geometrySetupIssueCache?.Reset();
            this.hasRenderers = this.trackingAnchor.HasRenderers();
        }

        private void DrawInitPoseJsonConversionSection()
        {
            TrackingAnchorEditor.state.showInitPoseJsonConversion = EditorGUILayout.Foldout(
                TrackingAnchorEditor.state.showInitPoseJsonConversion,
                new GUIContent(
                    "Legacy JSON Init Pose",
                    "An init pose section copied from a tracking configuration file can be pasted into this field and then applied persistently to the TrackingAnchor.\n\n" +
                    "Example Format:\n" + "\"initPose\": {\n" + "    \"t\": [-0.1, -0.2, 6.1],\n" +
                    "    \"r\": [0.1, -0.1, 0.7, -0.2]\n" + "}"),
                true);

            if (!TrackingAnchorEditor.state.showInitPoseJsonConversion)
            {
                return;
            }

            EditorGUI.indentLevel++;
            this.legacyJsonInitPoseField.Draw();
            DrawLegacyVLInitPoseCopyButton();
            EditorGUI.indentLevel--;
        }

        private void DrawLegacyVLInitPoseCopyButton()
        {
            if (!ButtonParameters.ButtonWasClicked(copyLegacyVLInitPoseButtonParameters))
            {
                return;
            }

            var vlInitPose = this.trackingAnchor.GetVLInitPose();
            var jsonPose = JsonHelper.ToJson(
                new ModelTrackerCommands.InitPose(vlInitPose.position, vlInitPose.rotation));

            GUIUtility.systemCopyBuffer = jsonPose;
            LogHelper.LogDebug(
                $"Copied legacy VL Init Pose \"{jsonPose}\" to clipboard",
                this.trackingAnchor);
        }

        private void DrawKeepUprightSection()
        {
            EditorGUILayout.PropertyField(this.keepUprightProperty);
            if (this.keepUprightProperty.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(this.modelUpVectorProperty);
                EditorGUI.indentLevel++;
                TrackingAnchorEditor.state.showAdvancedSettingsInKeepUpright = EditorGUILayout.Foldout(
                    TrackingAnchorEditor.state.showAdvancedSettingsInKeepUpright,
                    new GUIContent(
                        "Advanced",
                        "Additional parameters whose default values work for most cases."),
                    true);
                if (TrackingAnchorEditor.state.showAdvancedSettingsInKeepUpright)
                {
                    EditorGUILayout.PropertyField(this.worldUpVectorProperty);
                }
                EditorGUI.indentLevel--;
                EditorGUI.indentLevel--;
            }
        }

        private void DrawSLAMCameraSection()
        {
            EditorGUI.BeginChangeCheck();
            {
                EditorGUILayout.PropertyField(this.slamCameraProperty);
            }
            if (EditorGUI.EndChangeCheck())
            {
                this.serializedObject.ApplyModifiedProperties();
            }
        }

        private void DrawTrackingType()
        {
            var disablePoseEstimationParameter = this.trackingAnchor.GetAnchorRuntimeParameters()
                .disablePoseEstimation;

            // Convert the boolean value to an enum
            var currentType = disablePoseEstimationParameter.GetValue()
                ? TrackingType.ExistenceCheck
                : TrackingType.FrameToFrameTracking;

            var newType = (TrackingType) EditorGUILayout.Popup(
                new GUIContent(
                    "Tracking Type",
                    "Frame To Frame Tracking: Estimating the pose of the TrackingAnchor starting from the tracking result of the parent.\n" +
                    "Existence Check: Validating if the TrackingAnchor exists at the predefined pose."),
                (int) currentType,
                Enum.GetNames(typeof(TrackingType)));

            if (currentType != newType)
            {
                this.trackingAnchor.SetDisablePoseEstimation(
                    newType == TrackingType.ExistenceCheck);
            }
        }

        private void DrawParentChildSection()
        {
            TrackingAnchorEditor.state.showParentChildContent = EditorGUILayout.Foldout(
                TrackingAnchorEditor.state.showParentChildContent,
                new GUIContent(
                    "Parent & Children",
                    "This section displays the parent-child relationship to other anchors in the scene hierarchy."),
                true);

            if (TrackingAnchorEditor.state.showParentChildContent)
            {
                DrawParentChildSectionContent();
            }
        }

        private void DrawParentChildSectionContent()
        {
            EditorGUILayout.HelpBox(
                "This section lists the parent Tracking Anchor and all children of this Tracking Anchor. " +
                "Move a Tracking Anchor GameObject below this GameObject in the scene hierarchy to add it as Child Anchor.",
                MessageType.None);
            ButtonParameters.DrawLinkButton(
                "Open Documentation",
                "Open VisionLib documentation on Nested Tracking.",
                DocumentationLink.nestedTracking);
            GUILayout.Space(5);

            if (this.trackingAnchor.HasParentAnchor())
            {
                var parentAnchor = this.parentAnchorProperty.objectReferenceValue as TrackingAnchor;
                if (parentAnchor)
                {
                    GUILayout.BeginVertical("Parent Anchor", "HelpBox");
                    GUILayout.Space(15);
                    EditorGUI.indentLevel++;
                    DrawGameObjectWithFindButton(parentAnchor.gameObject);
                    EditorGUI.indentLevel--;
                    GUILayout.EndVertical();
                }
            }

            if (this.childAnchorsProperty.arraySize >= 1)
            {
                GUILayout.BeginVertical("Children of this Anchor", "HelpBox");
                GUILayout.Space(15);
                EditorGUI.indentLevel++;
                for (var i = 0; i < this.childAnchorsProperty.arraySize; i++)
                {
                    var childAnchor =
                        this.childAnchorsProperty.GetArrayElementAtIndex(i).objectReferenceValue as
                            TrackingAnchor;
                    if (childAnchor == null)
                    {
                        continue;
                    }
                    DrawGameObjectWithFindButton(childAnchor.gameObject);
                }
                EditorGUI.indentLevel--;
                GUILayout.EndVertical();
            }
        }

        private static void DrawGameObjectWithFindButton(GameObject gameObject)
        {
            RevealInHierarchy.DrawButton(gameObject.name, gameObject);
        }

        private void DrawInitPoseSection()
        {
            TrackingAnchorEditor.state.showInitPoseSettings = EditorGUILayout.Foldout(
                TrackingAnchorEditor.state.showInitPoseSettings,
                new GUIContent(
                    "Init Pose",
                    "This section combines all options and helper functions pertaining to tracking initialization."),
                true);
            if (TrackingAnchorEditor.state.showInitPoseSettings)
            {
                EditorGUI.indentLevel++;
                DrawInitPoseSectionContent();
                EditorGUI.indentLevel--;
            }
        }

        private void DrawInitPoseSectionContent()
        {
            var hasParent = this.trackingAnchor.HasParentAnchor();
            if (hasParent)
            {
                EditorGUILayout.HelpBox(
                    "Since this TrackingAnchor is a child of another anchor, you cannot adjust the InitPose",
                    MessageType.Info);
            }
            EditorGUI.BeginDisabledGroup(hasParent);

            EditorGUILayout.PropertyField(this.useInitPoseProperty);
            EditorGUI.BeginDisabledGroup(!this.useInitPoseProperty.boolValue);

            EditorGUILayout.PropertyField(this.persistInitPoseFromPlayModeProperty);
            using (new EditorGUILayout.VerticalScope("HelpBox"))
            {
                var centerObject = new ButtonParameters
                {
                    label = "Center in Slam Camera",
                    labelTooltip =
                        "Pressing the button moves the TrackingAnchor to a position, in which it is visible inside the scene.",
                    buttonIcon = GUIHelper.Icons.MoveToolIcon
                };

                EditorGUI.BeginDisabledGroup(!this.trackingAnchor.HasTrackingObjects());
                if (ButtonParameters.ButtonWasClicked(centerObject))
                {
                    this.trackingAnchor.CenterInitPoseInSlamCamera();
                }
                EditorGUI.EndDisabledGroup();

                if (!this.trackingAnchor.GetComponent<InitPoseInteraction>())
                {
                    DrawAddInteractionButton();
                }
                else
                {
                    DrawRemoveInteractionButton();
                }
            }

            EditorGUI.BeginChangeCheck();
            {
                DrawKeepUprightSection();

                DrawInitPoseJsonConversionSection();
            }
            if (EditorGUI.EndChangeCheck())
            {
                this.serializedObject.ApplyModifiedProperties();
            }

            EditorGUI.EndDisabledGroup();
            EditorGUI.EndDisabledGroup();
            
            EditorGUILayout.PropertyField(this.workSpacesProperty);
        }

        private void DrawParameterJsonConversionSection()
        {
            TrackingAnchorEditor.state.showParameterJsonConversion = EditorGUILayout.Foldout(
                TrackingAnchorEditor.state.showParameterJsonConversion,
                new GUIContent(
                    "Legacy JSON Parameters",
                    "An anchor parameter section copied from a tracking configuration file can be pasted into this field and then applied persistently to the TrackingAnchor. This will overwrite all existing parameters.\n\n" +
                    "Example Format:\n" + "\"parameters\": {\n" + "    \"metric\": \"m\",\n" +
                    "    \"keyFrameDistance\": 5,\n" + "    \"laplaceThreshold\": 1,\n" +
                    "    \"normalThreshold\": 0.3,\n" + "}"),
                true);

            if (!TrackingAnchorEditor.state.showParameterJsonConversion)
            {
                return;
            }
            EditorGUI.indentLevel++;
            this.legacyJsonParametersField.Draw();
            EditorGUI.indentLevel--;
        }

        private void DrawParametersSection()
        {
            EditorGUI.BeginChangeCheck();
            var parameterIssues =
                ((AnchorRuntimeParameters) this.anchorRuntimeParametersProperty
                    .managedReferenceValue).GetSceneIssues(this.trackingAnchor.gameObject);

            if (!FoldoutWithRequiredAction(
                    ref TrackingAnchorEditor.state.showTrackingParameter,
                    "Tracking Parameters",
                    "This section allows the adjustment of the anchor's tracking parameters.",
                    parameterIssues.Count > 0))
            {
                return;
            }
            parameterIssues.Draw();

            EditorGUI.indentLevel++;
            DrawTrackingParametersHeader();

            EditorGUILayout.PropertyField(this.anchorRuntimeParametersProperty);

            if (EditorGUI.EndChangeCheck())
            {
                this.serializedObject.ApplyModifiedProperties();
                var anchorRuntimeParameters =
                    (AnchorRuntimeParameters) this.anchorRuntimeParametersProperty
                        .managedReferenceValue;
                TrackingManager.CatchCommandErrors(
                    anchorRuntimeParameters.UpdateParametersInBackendAsync(this.trackingAnchor),
                    this.trackingAnchor);
            }

            EditorGUI.indentLevel--;
        }

        private void DrawTrackingParametersHeader()
        {
            EditorGUILayout.HelpBox(
                "This section allows you to modify tracking parameters. " +
                "The advanced parameters already have default values that work for most use-cases." +
                "\n\nSee our documentation for more info on the tracking parameters.",
                MessageType.None);

            ButtonParameters.DrawLinkButton(
                "Open Documentation",
                "Open VisionLib documentation on tracking parameters",
                DocumentationLink.basicTrackingParameter);
            if (ButtonParameters.ButtonWasClicked(
                    new ButtonParameters()
                    {
                        label = "Reset Parameters",
                        labelTooltip = "Resets all parameters to their default values.",
                        buttonIcon = GUIHelper.Icons.RefreshIcon
                    }))
            {
                this.trackingAnchor.ResetParametersToDefault();
            }

            DrawParameterJsonConversionSection();
            
            EditorGUILayout.PropertyField(this.persistParametersFromPlayModeProperty);
        }

        private void DrawTrackingGeometrySection()
        {
            var hasTrackingObjects = this.trackingAnchor.HasTrackingObjects();
            var modelDimensionsString = this.anchorBoundingBox.HasValue
                ? $" ({this.anchorBoundingBox.Value.GetDimensionString(this.trackingAnchor.GetMetric())})"
                : "";

            EditorGUILayout.BeginHorizontal();
            EditorGUIUtility.SetIconSize(new Vector2(15, 15));
            var foldoutOpen = FoldoutWithRequiredAction(
                ref TrackingAnchorEditor.state.showTrackingGeometry,
                $"Tracking Geometry{modelDimensionsString}",
                "This section helps set up your tracking geometry. The dimensions shown " +
                "are the detected physical size of the combined tracking geometry. " +
                "This only considers TrackingMeshes and -URIs that are not occluders.",
                this.geometrySetupIssueCache.Count != 0);

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(
                this.unitProperty,
                GUIContent.none,
                GUILayout.MaxWidth(50));
            if (EditorGUI.EndChangeCheck())
            {
                this.trackingAnchor.SetMetric((Metric.Unit) this.unitProperty.intValue);
                // It is necessary to reset the TreeView, since the TreeViewElements labels
                // contain the metric and thus have to be recreated after the metric changes. 
                if (this.trackingAnchorTreeEditor != null)
                {
                    this.trackingAnchorTreeEditor.ResetTreeView();
                }
            }

            EditorGUILayout.EndHorizontal();
            if (!foldoutOpen)
            {
                return;
            }
            this.geometrySetupIssueCache.Draw();

            EditorGUI.indentLevel++;
            using (new EditorGUILayout.VerticalScope("HelpBox"))
            {
                EditorGUI.BeginDisabledGroup(!this.hasRenderers);
                DrawTrackingMeshButton(
                    TrackingMeshButtonMode.AddTrackingMeshes,
                    TrackingAnchorEditorSettings.addTrackingMeshButtonParameters);

                if (hasTrackingObjects)
                {
                    DrawTrackingMeshButton(
                        TrackingMeshButtonMode.RemoveTrackingMeshes,
                        TrackingAnchorEditorSettings.removeTrackingMeshButtonParameters);
                    EditorGUI.EndDisabledGroup();
                    DrawModelHashCopyButton();
                    EditorGUI.BeginDisabledGroup(!this.hasRenderers);
                }

                RenderedObjectEditorHelper.DrawEnableMeshRenderersButton(
                    this.trackingAnchor.gameObject);
                EditorGUI.EndDisabledGroup();
            }

            TrackingAnchorEditor.state.showTree = EditorGUILayout.Foldout(
                TrackingAnchorEditor.state.showTree,
                "Scene Hierarchy",
                true);
            if (TrackingAnchorEditor.state.showTree)
            {
                DrawTreeViewSection();
            }
            EditorGUI.indentLevel--;
        }

        private void DrawAugmentedContentSection()
        {
            var augmentedContentIssue = this.trackingAnchor.GetAugmentedContentIssues();

            EditorGUIUtility.SetIconSize(new Vector2(15, 15));
            if (!FoldoutWithRequiredAction(
                    ref TrackingAnchorEditor.state.showAugmentationSettings,
                    "Augmented Content",
                    "This section manages the content rendered during initialization and during tracking.",
                    augmentedContentIssue.Count != 0)) 
            {
                return;
            }

            if (!this.hasRenderers)
            {
                EditorGUILayout.HelpBox(
                    "This TrackingAnchor does not contain any Renderer.\n" +
                    "Therefore it is not possible to configure the augmented content from existing Renderer. Define the augmented contend manually by referencing a RenderedObject with this TrackingAnchor.",
                    MessageType.Info);
            }
            augmentedContentIssue.Draw();
            if (this.trackingAnchor.HasAugmentedContent())
            {
                DrawAugmentationSetupShorthands();
                RenderedObjectEditorHelper.DrawRenderedObjectsSection(
                    this.trackingAnchor.GetRegisteredRenderedObjects());
            }
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(this.showInitPoseGuideWhileDisabledProperty);
            if (EditorGUI.EndChangeCheck())
            {
                this.trackingAnchor.ShowInitPoseGuideWhileDisabled =
                    this.showInitPoseGuideWhileDisabledProperty.boolValue;
            }
        }

        private void DrawAddInteractionButton()
        {
            if (!ButtonParameters.ButtonWasClicked(addInteractionButtonParameters))
            {
                return;
            }
            this.trackingAnchor.AddInitPoseInteraction();
        }

        private void DrawRemoveInteractionButton()
        {
            if (!ButtonParameters.ButtonWasClicked(removeInteractionButtonParameters))
            {
                return;
            }
            if (EditorUtility.DisplayDialog(
                    "Delete Interaction?",
                    "Removing the interaction components will delete your " +
                    "interaction tuning settings and break any existing component references.",
                    "Delete",
                    "Cancel"))
            {
                this.trackingAnchor.RemoveInitPoseInteraction();
            }
        }

        private void DrawTrackingEventsSection()
        {
            var trackingEventIssues = this.trackingAnchor.GetTrackingEventIssues();

            if (!FoldoutWithRequiredAction(
                    ref TrackingAnchorEditor.state.showTrackingStateEvents,
                    "Tracking Events",
                    "React to changes in the tracking state of the TrackingAnchor.",
                    trackingEventIssues.Count() != 0)) 
            {
                return;
            }

            trackingEventIssues.Draw();

            EditorGUILayout.PropertyField(this.onTrackedEventProperty);
            EditorGUILayout.PropertyField(this.onTrackingCriticalEventProperty);
            EditorGUILayout.PropertyField(this.onTrackingLostEventProperty);
        }

        private void DrawAnchorEventsSection()
        {
            var anchorEventIssues = this.trackingAnchor.GetAnchorEventIssues();

            if (!FoldoutWithRequiredAction(
                    ref TrackingAnchorEditor.state.showAnchorEvents,
                    "Anchor Events",
                    "React to changes in the TrackingAnchor lifecycle.",
                    anchorEventIssues.Count() != 0))  
            {
                return;
            }

            EditorGUILayout.PropertyField(this.onAnchorEnabledEventProperty);
            EditorGUILayout.PropertyField(this.onAnchorDisabledEventProperty);
        }

        private void DrawModelHashCopyButton()
        {
            if (!ButtonParameters.ButtonWasClicked(copyModelHashButtonParameters))
            {
                return;
            }

            var licenseFeatureBlock = GetLicenseFeatureBlock();

            GUIUtility.systemCopyBuffer = licenseFeatureBlock;
            Debug.Log(
                $"Copied License Features \"{licenseFeatureBlock}\" to clipboard",
                this.trackingAnchor);
        }

        private void DrawTreeViewSection()
        {
            if (!this.trackingAnchorTreeEditor)
            {
                this.trackingAnchorTreeEditor = CreateInstance<TrackingAnchorTreeEditor>();
                this.trackingAnchorTreeEditor.TrackingAnchor = (TrackingAnchor) this.target;
            }
            EditorGUI.BeginChangeCheck();
            this.trackingAnchorTreeEditor.OnInspectorGUI();
            if (EditorGUI.EndChangeCheck())
            {
                RecalculateCachedData();
            }
        }

        private void DrawTrackingMeshButton(
            TrackingMeshButtonMode mode,
            TrackingMeshButtonParameters parameters)
        {
            if (!ButtonParameters.ButtonWasClicked(parameters.buttonParameters))
            {
                return;
            }
            try
            {
                switch (mode)
                {
                    case TrackingMeshButtonMode.AddTrackingMeshes:
                        TrackingObjectHelper.AddTrackingMeshesInSubTree(
                            this.trackingAnchor.gameObject);
                        break;
                    case TrackingMeshButtonMode.RemoveTrackingMeshes:
                        TrackingObjectHelper.RemoveTrackingMeshesInSubTree(
                            this.trackingAnchor.gameObject);
                        break;
                }
            }
            catch (TrackingObjectHelper.InvalidTargetException e)
            {
                Debug.LogWarning(
                    parameters.failureMessagePrefix + e.Message + "\".",
                    this.trackingAnchor.gameObject);
            }
        }

        private void DrawAugmentationSetupShorthands()
        {
            using (new EditorGUILayout.VerticalScope("HelpBox"))
            {
                using (new EditorGUI.DisabledGroupScope(!this.hasRenderers))
                {
                    GUILayout.Label(
                        "Configure augmented content automatically:",
                        EditorStyles.wordWrappedLabel);
                    EditorGUI.indentLevel++;
                    bool hasRenderedObjectOnTrackingAnchor =
                        this.trackingAnchor.GetComponent<RenderedObject>();
                    using (new EditorGUI.DisabledGroupScope(hasRenderedObjectOnTrackingAnchor))
                    {
                        if (ButtonParameters.ButtonWasClicked(
                                TrackingAnchorEditorSettings.setSelfAsAugmentationButtonParameters))
                        {
                            SetUpTrackingAnchorAsAugmentedContent();
                        }
                    }
                    DrawCloneAsRenderedObjectButton(
                        TrackingAnchorEditorSettings.cloneAsAugmentationButtonParameters,
                        RenderedObject.RenderMode.WhenTracking);
                    DrawCloneAsRenderedObjectButton(
                        TrackingAnchorEditorSettings.cloneAsInitPoseGuideButtonParameters,
                        RenderedObject.RenderMode.WhenInitializing);
                    DrawCloneAsRenderedObjectButton(
                        TrackingAnchorEditorSettings.cloneAsBothButtonParameters,
                        RenderedObject.RenderMode.WhenInitializingOrTracking);
                    EditorGUI.indentLevel--;
                }
            }
        }

        private void SetUpTrackingAnchorAsAugmentedContent()
        {
            if (!this.trackingAnchor.HasMeshes() || !this.trackingAnchor.HasRenderers())
            {
                LogHelper.LogWarning(
                    "No Renderers found in children. Skipped setting up " +
                    "TrackingAnchor as its own augmented content.");
                return;
            }

            TrackingAnchorHelper.CreateRenderedObjectAndLinkToTrackingAnchor(
                RenderedObject.RenderMode.Always,
                this.trackingAnchor,
                this.trackingAnchor.gameObject);
        }

        private void DrawCloneAsRenderedObjectButton(
            ButtonParameters buttonParameters,
            RenderedObject.RenderMode renderMode)
        {
            if (ButtonParameters.ButtonWasClicked(buttonParameters))
            {
                var cloneParent = this.trackingAnchor.CloneAsRenderedObject(renderMode);
#if !VL_USE_UNIVERSAL_RENDER_PIPELINE
                if (renderMode == RenderedObject.RenderMode.WhenInitializing)
                {
                    TrackingObjectHelper.SetMeshRendererMaterialsInSubtree(
                        cloneParent,
                        TrackingObjectHelper.LoadAsset<Material>(
                            TrackingObjectHelper.LoadableAsset.SemiTransparentDefaultMaterial));
                }
#endif
            }
        }

        private void UpdateAnchorHierarchy()
        {
            if (this.trackingAnchor == null)
            {
                return;
            }
            this.trackingAnchor.UpdateParentAnchor();
            for (var i = 0; i < this.childAnchorsProperty.arraySize; i++)
            {
                var childAnchor =
                    this.childAnchorsProperty.GetArrayElementAtIndex(i).objectReferenceValue as
                        TrackingAnchor;
                if (childAnchor == null)
                {
                    continue;
                }
                childAnchor.UpdateParentAnchor();
            }
            foreach (var childAnchor in
                     this.trackingAnchor.GetComponentsInChildren<TrackingAnchor>())
            {
                childAnchor.UpdateParentAnchor();
            }
        }

        private static bool FoldoutWithRequiredAction(
            ref bool foldout,
            string labelText,
            string tooltip,
            bool actionRequired)
        {
            if (actionRequired)
            {
                EditorGUILayout.Foldout(
                    true,
                    GUIHelper.GenerateGUIContentWithIcon(
                        GUIHelper.Icons.WarningIcon,
                        "Action Required! " + tooltip,
                        " " + labelText),
                    true);
                return true;
            }
            // Update foldout value
            foldout = EditorGUILayout.Foldout(
                foldout,
                new GUIContent(labelText, tooltip),
                true);
            return foldout;
        }

        private string GetLicenseFeatureBlock()
        {
            var hashes = this.trackingAnchor.GetModelHashes();
            return "-----BEGIN MODEL LICENSE FEATURES BLOCK-----\n" + hashes.Aggregate(
                       "",
                       (current, hash) => current + (hash + "\n")) +
                   "-----END MODEL LICENSE FEATURES BLOCK-----\n";
        }

        private static void LogTrackingAnchorAccessError()
        {
            Debug.LogError(
                "Cannot draw tracking anchor editor because " +
                "the trackingAnchor instance could not be accessed.");
        }
    }
}
