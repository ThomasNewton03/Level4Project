using Visometry.VisionLib.SDK.Core.Details;

namespace Visometry.VisionLib.SDK.Core
{
    public static class RenderedObjectEditorSettings
    {
        public static readonly ButtonParameters enableMeshRenderersButtonParameters =
            new ButtonParameters
            {
                label = "Make all models visible in the game view",
                labelTooltip = "Enable the MeshRenderers on all models",
                buttonIcon = GUIHelper.Icons.VisibleOnIcon
            };

        public static readonly ButtonParameters disableMeshRenderersButtonParameters =
            new ButtonParameters
            {
                label = "Make all models invisible in the game view",
                labelTooltip = "Disable the MeshRenderers on all models",
                buttonIcon = GUIHelper.Icons.VisibleOffIcon
            };
    }
}
