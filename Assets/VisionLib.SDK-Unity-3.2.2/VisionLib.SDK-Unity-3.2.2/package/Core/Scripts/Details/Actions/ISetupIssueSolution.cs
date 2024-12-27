#if UNITY_EDITOR

namespace Visometry.VisionLib.SDK.Core.Details
{
    /// <summary>
    /// Interface for Actions, which can be used inside SetupIssues to fix the corresponding issue.
    /// </summary>
    public interface ISetupIssueSolution
    {
        public void Invoke();

        public string GetMessage();
    }
}

#endif
