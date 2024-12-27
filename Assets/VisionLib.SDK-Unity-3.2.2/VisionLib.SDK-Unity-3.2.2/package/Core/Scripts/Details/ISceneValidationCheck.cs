using System.Collections.Generic;

namespace Visometry.VisionLib.SDK.Core.Details
{
    /// <summary>
    /// Interface for classes that require validation as part of a tracking scene setup. 
    /// For <see cref="MonoBehaviour"/>s that implement this interface, <see cref="Validate"/> will automatically be executed periodically in the edit mode as well as before each build.
    /// </summary>
    public interface ISceneValidationCheck
    {
        
#if UNITY_EDITOR
        /// <summary>
        /// Determine setup issues for the component.
        /// This should only be implemented for use in Editor. Surround it with
        /// #if UNITY_EDITOR
        /// </summary>
        /// <returns>
        ///     IEnumerable of SetupIssue which may provide a solution for the problem.
        ///     If no issue is inside this component, the IEnumerable will be empty.
        /// </returns>
        public List<SetupIssue> GetSceneIssues();
#endif
    }
}
