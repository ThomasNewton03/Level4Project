using System;
using UnityEditor;
using UnityEngine;

namespace Visometry.VisionLib.SDK.Core.Details
{
    public static class GUIHelper
    {
        private struct IconResourceNames
        {
            //built in
            public const string successIconName = "Progress";
            public const string infoIconName = "console.infoicon.sml";
            public const string warningIconName = "console.warnicon.sml";
            public const string errorIconName = "console.erroricon.sml";
            public const string occluderEnabledIconName = "OcclusionArea Icon";
            public const string meshRendererEnabledIconName = "MeshRenderer Icon";
            public const string modelIconName = "PrefabModel Icon";
            public const string searchIconName = "Search Icon";
            public const string duplicateIconName = "TreeEditor.Duplicate";
            public const string combineMeshesIconName = "BlendTree Icon";
            public const string visibleOnIconName = "animationvisibilitytoggleon";
            public const string visibleOffIconName = "animationvisibilitytoggleoff";
            public const string plusIconName = "Toolbar Plus";
            public const string minusIconName = "Toolbar Minus";
            public const string linkIconName = "Linked";
            public const string webIconName = "BuildSettings.Web.Small";
            public const string moveToolIconName = "MoveTool";
            public const string importIconName = "Import";
            public const string saveIconName = "SaveActive";
            public const string refreshIconName = "Refresh";
            
            //custom
            public static string TrackingObjectIconName
            {
                get
                {
                    return EditorGUIUtility.isProSkin
                        ? "d_TrackingObject Icon"
                        : "TrackingObject Icon";
                }
            }

            public static string TrackingObjectWithEyeIconName
            {
                get
                {
                    return EditorGUIUtility.isProSkin
                        ? "d_TrackingObjectEye Icon"
                        : "TrackingObjectEye Icon";
                }
            }
        }

        public enum Icons
        {
            None,
            SuccessIcon,
            InfoIcon,
            WarningIcon,
            ErrorIcon,
            OccluderEnabledIcon,
            MeshRendererEnabledIcon,
            ModelIcon,
            SearchIcon,
            DuplicateIcon,
            CombineMeshesIcon,
            VisibleOnIcon,
            VisibleOffIcon,
            PlusIcon,
            MinusIcon,
            ImportIcon,
            TrackingObjectIcon,
            EnabledForTrackingIcon,
            LinkIcon,
            WebIcon,
            MoveToolIcon,
            SaveIcon,
            RefreshIcon
        }

        public static GUIContent GenerateGUIContentWithIcon(
            Icons icon,
            string tooltip,
            string text = "")
        {
            return icon switch
            {
                Icons.None => GenerateContentWithoutIcon(tooltip, text),

                Icons.SuccessIcon => GenerateIconContentWithBuiltInIcon(
                    IconResourceNames.successIconName,
                    tooltip,
                    text),
                Icons.InfoIcon => GenerateIconContentWithBuiltInIcon(
                    IconResourceNames.infoIconName,
                    tooltip,
                    text),
                Icons.WarningIcon => GenerateIconContentWithBuiltInIcon(
                    IconResourceNames.warningIconName,
                    tooltip,
                    text),
                Icons.ErrorIcon => GenerateIconContentWithBuiltInIcon(
                    IconResourceNames.errorIconName,
                    tooltip,
                    text),
                Icons.OccluderEnabledIcon => GenerateIconContentWithBuiltInIcon(
                    IconResourceNames.occluderEnabledIconName,
                    tooltip,
                    text),
                Icons.MeshRendererEnabledIcon => GenerateIconContentWithBuiltInIcon(
                    IconResourceNames.meshRendererEnabledIconName,
                    tooltip,
                    text),
                Icons.ModelIcon => GenerateIconContentWithBuiltInIcon(
                    IconResourceNames.modelIconName,
                    tooltip,
                    text),
                Icons.SearchIcon => GenerateIconContentWithBuiltInIcon(
                    IconResourceNames.searchIconName,
                    tooltip,
                    text),
                Icons.DuplicateIcon => GenerateIconContentWithBuiltInIcon(
                    IconResourceNames.duplicateIconName,
                    tooltip,
                    text),
                Icons.CombineMeshesIcon => GenerateIconContentWithBuiltInIcon(
                    IconResourceNames.combineMeshesIconName,
                    tooltip,
                    text),
                Icons.VisibleOnIcon => GenerateIconContentWithBuiltInIcon(
                    IconResourceNames.visibleOnIconName,
                    tooltip,
                    text),
                Icons.VisibleOffIcon => GenerateIconContentWithBuiltInIcon(
                    IconResourceNames.visibleOffIconName,
                    tooltip,
                    text),
                Icons.PlusIcon => GenerateIconContentWithBuiltInIcon(
                    IconResourceNames.plusIconName,
                    tooltip,
                    text),
                Icons.MinusIcon => GenerateIconContentWithBuiltInIcon(
                    IconResourceNames.minusIconName,
                    tooltip,
                    text),
                Icons.LinkIcon => GenerateIconContentWithBuiltInIcon(
                    IconResourceNames.linkIconName,
                    tooltip,
                    text),
                Icons.WebIcon => GenerateIconContentWithBuiltInIcon(
                    IconResourceNames.webIconName,
                    tooltip,
                    text),
                Icons.MoveToolIcon => GenerateIconContentWithBuiltInIcon(
                    IconResourceNames.moveToolIconName,
                    tooltip,
                    text),
                Icons.ImportIcon => GenerateIconContentWithBuiltInIcon(
                    IconResourceNames.importIconName,
                    tooltip,
                    text),
                Icons.TrackingObjectIcon => GenerateIconContentWithCustomIcon(
                    IconResourceNames.TrackingObjectIconName,
                    tooltip,
                    text),
                Icons.EnabledForTrackingIcon => GenerateIconContentWithCustomIcon(
                    IconResourceNames.TrackingObjectWithEyeIconName,
                    tooltip,
                    text),
                Icons.SaveIcon => GenerateIconContentWithBuiltInIcon(
                    IconResourceNames.saveIconName,
                    tooltip,
                    text),
                Icons.RefreshIcon => GenerateIconContentWithBuiltInIcon(
                    IconResourceNames.refreshIconName,
                    tooltip,
                    text),
                _ => throw new ArgumentException("Unknown icon:" + icon.ToString())
            };
        }

        private static GUIContent GenerateContentWithoutIcon(string tooltip, string text)
        {
            return new GUIContent(text, tooltip);
        }

        private static GUIContent GenerateIconContentWithBuiltInIcon(
            string builtInIconName,
            string tooltip,
            string text)
        {
            //Directly searching for built-in icon texture assets by name can fail for some icons.
            //Fetching the IconContent always works assuming you provide the correct name for an
            //existing icon.
            //This IconContent is a singleton. Changes to it manifest themselves everywhere it is
            //used. Therefore, copy via the GUIContent constructor and only ever edit the copy.
            var content = new GUIContent(EditorGUIUtility.IconContent(builtInIconName))
            {
                text = text, tooltip = tooltip
            };
            return content;
        }

        private static GUIContent GenerateIconContentWithCustomIcon(
            string iconResourceName,
            string tooltip,
            string text)
        {
            var icon = Resources.Load<Texture2D>(iconResourceName);
            return new GUIContent(text, icon, tooltip);
        }
    }
}
