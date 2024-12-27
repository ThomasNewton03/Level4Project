using System;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace Visometry.VisionLib.SDK.Core.Details
{
    /// <summary>
    /// Class containing all checks to be performed before each Build with target platform Android.
    /// </summary>
    public abstract class PreBuildChecks : IPreprocessBuildWithReport
    {
        public int callbackOrder
        {
            get
            {
                return 0;
            }
        }
        public abstract void OnPreprocessBuild(BuildReport report);

        /// <summary>
        /// Displays a dialog, when checkValid is false. If the ok-Button is pressed, the solution
        /// will be executed. Otherwise the build will be canceled. 
        /// </summary>
        protected static void Check(
            bool checkValid,
            Action solution,
            string title,
            string message,
            string okString)
        {
            if (checkValid)
            {
                return;
            }

            if (EditorUtility.DisplayDialog(
                    $"VisionLib: Malconfigured Build – {title}",
                    message,
                    okString,
                    "Cancel build"))
            {
                solution();
                return;
            }
            CancelBuild();
        }
        
        /// <summary>
        /// Displays a dialog, when checkValid is false. If the ok-Button is pressed, the solution
        /// will be executed. If the continue button is pressed, nothing will be done. If the dialog
        /// is canceled the build will be canceled, too. 
        /// </summary>
        protected static void CheckOptional(
            bool checkValid,
            Action solution,
            string title,
            string message,
            string okString)
        {
            if (checkValid)
            {
                return;
            }

            var option = EditorUtility.DisplayDialogComplex(
                $"VisionLib: Malconfigured Build – {title}",
                message,
                okString,
                "Cancel build",
                "Continue build without adjustment");
            switch (option)
            {
                // okay button
                case 0:
                    solution();
                    return;
                // Continue build button
                case 2:
                    return;
            }
            CancelBuild();
        }

        protected static void CancelBuild()
        {
            throw new BuildFailedException("Build was canceled by the user.");
        }
    }
}
