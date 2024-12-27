using System;
using System.Collections.Generic;

namespace Visometry.VisionLib.SDK.Core.Details
{
    /// <summary>
    /// This class can cache setup issues and only recalculate them if a button is pressed. 
    /// </summary> 
    /// <remarks>
    /// If there is an external way of solving the issues, it is necessary to call `Reset`
    /// accordingly. Therefore we recommend to only use this class when the advantage of caching the
    /// SetupIssues is higher than the disadvantage of possible misses.
    /// </remarks>
    public class SetupIssueCache
    {
        private readonly Func<List<SetupIssue>> validationCheck;
        private List<SetupIssue> issues;

        public SetupIssueCache(ISceneValidationCheck checkableClass)
        {
            this.validationCheck = checkableClass.GetSceneIssues;
            Reset();
        }

        public SetupIssueCache(Func<List<SetupIssue>> validationCheck)
        {
            this.validationCheck = validationCheck;
            Reset();
        }

        /// <summary>
        /// Draws the setup issues and recalculates them if a button has been pressed
        /// </summary>
        public void Draw()
        {
            if (this.issues.Draw())
            {
                Reset();
            }
        }

        /// <summary>
        /// Recalculates setup issues
        /// </summary>
        public void Reset()
        {
            this.issues = this.validationCheck.Invoke();
        }

        public int Count {
            get
            {
                return this.issues.Count;
            }
        }
    }
}
