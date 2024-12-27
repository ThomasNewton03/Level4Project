using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Visometry.VisionLib.SDK.Core.Details
{
    public static class SetupIssueEditorHelper
    {
        /// <summary>
        /// Collects all <see cref="SetupIssue"/>s and draws them in a HelpBox (if there are any). 
        /// </summary>
        public static void DrawSceneValidation()
        {
            using (new GUILayout.VerticalScope("", EditorStyles.helpBox))
            {
                var sceneErrors = SceneValidator.ValidateScene();
                DrawSceneValidationHeader(sceneErrors.Count);
                sceneErrors.Draw();
                GUILayout.FlexibleSpace();
            }
        }

        private static void DrawSceneValidationHeader(int numberOfErrors)
        {
            if (numberOfErrors == 0)
            {
                var icon = GUIHelper.GenerateGUIContentWithIcon(
                    GUIHelper.Icons.SuccessIcon,
                    "Scene integrity check found no issues.",
                    "Scene integrity check found no issues.");
                EditorGUILayout.LabelField(icon);
                return;
            }
            EditorGUILayout.LabelField(
                "Issues found in Scene (" + numberOfErrors + "):",
                CustomEditorStyles.boldWithWrappedLabel);
        }

        /// <summary>
        /// Helper function for drawing setup issues for a specific Object if there are any 
        /// </summary>
        public static void DrawErrorBox(ISceneValidationCheck check)
        {
            check.GetSceneIssues().Draw();
        }

        /// <summary>
        /// Helper function for drawing the <see cref="SetupIssue"/> in the Editor 
        /// </summary>
        /// <returns>true, if a solution has been selected</returns>
        public static bool Draw(this List<SetupIssue> setupIssues)
        {
            var anyButtonWasClicked = false;
            foreach (var issue in setupIssues)
            {
                anyButtonWasClicked |= issue.Draw();
            }
            return anyButtonWasClicked;
        }

        /// <summary>
        /// Helper function for drawing the <see cref="SetupIssue"/> in the Editor 
        /// </summary>
        /// <returns>true, if a solution has been selected</returns>
        public static bool Draw(this SetupIssue setupIssue)
        {
            if (setupIssue == null)
            {
                return false;
            }
            using (new GUILayout.VerticalScope("", EditorStyles.helpBox))
            {
                DrawIssueTitle(setupIssue);
                return DrawSolutionButtons(setupIssue);
            }
        }

        private const string autoResolveTooltip = "Press this button to auto-resolve the issue.";
        private const string manualResolveTooltip =
            "Press this button to open the corresponding GameObject.";

        private static GUIHelper.Icons GetIcon(SetupIssue.IssueType issueType)
        {
            return issueType switch
            {
                SetupIssue.IssueType.Info => GUIHelper.Icons.InfoIcon,
                SetupIssue.IssueType.Warning => GUIHelper.Icons.WarningIcon,
                SetupIssue.IssueType.Error => GUIHelper.Icons.ErrorIcon,
                _ => throw new ArgumentException("Unknown SetupIssue.IssueType: " + issueType)
            };
        }

        private static void DrawIssueTitle(this SetupIssue setupIssue)
        {
#if UNITY_2021_3_OR_NEWER
            using (new GUILayout.HorizontalScope("", GUIStyle.none))
            {
                var icon = GUIHelper.GenerateGUIContentWithIcon(
                    GetIcon(setupIssue.issueType),
                    setupIssue.message);
                EditorGUILayout.LabelField(icon, GUILayout.Width(20), GUILayout.MaxHeight(20));

                if (EditorGUILayout.LinkButton(
                        new GUIContent(setupIssue.title, setupIssue.message)))
                {
                    Selection.objects = new Object[] {setupIssue.sourceObject};
                }
            }
#else
            var buttonParameters = new ButtonParameters()
            {
                label = setupIssue.title,
                labelIcon = GetIcon(setupIssue.issueType),
                labelTooltip = setupIssue.message,
                buttonIcon = GUIHelper.Icons.None,
                buttonText = "Open",
                buttonTooltip = SetupIssueEditorHelper.manualResolveTooltip,
                guiStyle = new GUIStyle(EditorStyles.miniButton)
                {
                    fixedWidth = 70, 
                    fixedHeight = 20
                }
            };
            if (ButtonParameters.ButtonWasClicked(buttonParameters))
            {
                Selection.objects = new Object[] {setupIssue.sourceObject};
            }
#endif
        }

        private static bool DrawSolutionButtons(this SetupIssue setupIssue)
        {
            if (setupIssue.solveErrorFunctions == null)
            {
                return false;
            }

            EditorGUI.indentLevel++;
            var anyButtonWasClicked = false;
            foreach (var solution in setupIssue.solveErrorFunctions)
            {
                anyButtonWasClicked |= DrawSingleSolutionButton(solution);
            }
            EditorGUI.indentLevel--;
            return anyButtonWasClicked;
        }

        private static bool DrawSingleSolutionButton(this ISetupIssueSolution solution)
        {
            var buttonParameters = new ButtonParameters()
            {
                label = solution.GetMessage(),
                labelTooltip = SetupIssueEditorHelper.autoResolveTooltip,
                buttonIcon = GUIHelper.Icons.None,
                buttonText = "Fix",
                buttonTooltip = SetupIssueEditorHelper.autoResolveTooltip
            };
            if (ButtonParameters.ButtonWasClicked(buttonParameters))
            {
                solution.Invoke();
                return true;
            }
            return false;
        }
    }
}
