using System.Collections.Generic;
using System.Threading.Tasks;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.SceneManagement;
using Visometry.VisionLib.SDK.Core.API;
using Visometry.VisionLib.SDK.Core.API.Native;
using Visometry.VisionLib.SDK.Core.Details;

namespace Visometry.VisionLib.SDK.Core
{
    /// <summary>
    ///  The InitDataHandler contains all functions related to InitData.
    /// </summary>
    /// @ingroup Core
    [HelpURL(DocumentationLink.APIReferenceURI.Core + "init_data_handler.html")]
    public class InitDataHandler : MonoBehaviour, ISceneValidationCheck
    {
        /// <summary>
        /// If this option is enabled, the application will try to load the init data from the
        /// specified <see cref="initDataURI"/> every time the tracker starts.
        /// This way the init data can be used directly from the first frame.
        /// </summary>
        public bool loadInitDataOnTrackerStart = false;

        /// <summary>
        /// The initDataURI defines the path where the init data is stored. WriteInitData
        /// and ReadInitData will both use this uri.
        /// If this is not set, the default value is
        /// `local-storage-dir:VisionLib/InitData/InitData.binz`.
        /// </summary>
        public string initDataURI = null;

        private const string defaultInitDataLocation = "local-storage-dir:VisionLib/InitData/";
        private const string defaultInitDatafileName = "/InitData.binz";

        public void Start()
        {
            if (TrackingManager.DoesTrackerExistAndIsRunning())
            {
                OnTrackerRunning();
            }
        }

        public void OnEnable()
        {
            TrackingManager.OnTrackerRunning += OnTrackerRunning;
        }

        public void OnDisable()
        {
            TrackingManager.OnTrackerRunning -= OnTrackerRunning;
        }

        public async Task WriteInitDataAsync()
        {
            var writeURI = GetInitDataURI();
            await ModelTrackerCommands.WriteInitDataAsync(
                TrackingManager.Instance.Worker,
                writeURI);
            NotificationHelper.SendInfo(
                "Init data written to: '" + VLSDK.GetPhysicalPath(writeURI) + "'");
        }

        /// <summary>
        ///  Write the captured initialization data into the file specified in
        ///  <see cref="initDataURI"/>.
        /// </summary>
        /// <remarks> This function will be performed asynchronously.</remarks>
        /// <remarks>
        ///  In order to avoid having to use a different file path for each
        ///  platform, the "local-storage-dir" scheme can be used as file prefix.
        ///  This scheme points to different locations depending on the platform:
        ///  * Windows: Current users home directory
        ///  * MacOS: Current users document directory
        ///  * iOS / Android: The current applications document directory
        /// </remarks>
        public void WriteInitData()
        {
            TrackingManager.CatchCommandErrors(WriteInitDataAsync(), this);
        }

        public async Task ReadInitDataAsync()
        {
            await ModelTrackerCommands.ReadInitDataAsync(
                TrackingManager.Instance.Worker,
                GetInitDataURI());
            NotificationHelper.SendInfo("Init data read.");
        }

        /// <summary>
        ///  Loads stored initialization data from the file specified in <see cref="initDataURI"/>.
        /// </summary>
        /// <remarks> This function will be performed asynchronously.</remarks>
        /// <remarks>
        ///  In order to load init data at best use a static uri. A common way is for each
        ///  platform, is using  "local-storage-dir" scheme which can be used as file prefix.
        ///  This scheme points to different locations depending on the platform:
        ///  * Windows: Current users home directory
        ///  * MacOS: Current users document directory
        ///  * iOS / Android: The current applications document directory
        /// </remarks>
        public void ReadInitData()
        {
            TrackingManager.CatchCommandErrors(ReadInitDataAsync(), this);
        }

        public async Task ResetInitDataAsync()
        {
            await ModelTrackerCommands.ResetInitDataAsync(TrackingManager.Instance.Worker);
            NotificationHelper.SendInfo("Init data reset.");
        }

        /// <summary>
        ///  Reset the initialization data loaded via <see cref="ReadInitData"/>.
        /// </summary>
        /// <remarks> This function will be performed asynchronously.</remarks>
        /// <remarks>
        ///  This Function only resets int data previously loaded from files (static init data). It
        ///  does not affect init data learned on the fly (dynamic init data). To reset dynamic init
        ///  data, use `TrackingAnchor.ResetHard` function on the desired anchor(s) (or
        ///  `ModelTracker.ResetTrackingHard` if you use single model tracking). 
        /// </remarks>
        public void ResetInitData()
        {
            TrackingManager.CatchCommandErrors(ResetInitDataAsync(), this);
        }

        private void OnTrackerRunning()
        {
            if (this.loadInitDataOnTrackerStart)
            {
                ReadInitData();
            }
        }

        private string GetInitDataURI()
        {
            if (string.IsNullOrEmpty(this.initDataURI))
            {
                this.initDataURI = InitDataHandler.defaultInitDataLocation +
                                   SceneManager.GetActiveScene().name +
                                   InitDataHandler.defaultInitDatafileName;
            }
            return this.initDataURI;
        }

#if UNITY_EDITOR
        /// <summary>
        /// Creates a new GameObject containing the InitDataHandler component.
        /// See also <see cref="ValidateAddInitPoseHandler"/>.
        /// </summary>
        [MenuItem("GameObject/VisionLib/InitData/Add InitDataHandler", false, 3)]
        private static void AddInitPoseHandler(MenuCommand menuCommand)
        {
            var initDataHandlerGameObject = new GameObject("InitDataHandler");
            GameObjectUtility.SetParentAndAlign(
                initDataHandlerGameObject,
                menuCommand.context as GameObject);
            Undo.RegisterCreatedObjectUndo(initDataHandlerGameObject, $"Create InitDataHandler");
            initDataHandlerGameObject.AddComponentUndoable<InitDataHandler>();
        }

        /// <summary>
        /// Validation function for AddInitPoseHandler. Checks whether an InitDataHandler can be
        /// added to the current scene.
        /// </summary>
        /// <returns></returns>
        [MenuItem("GameObject/VisionLib/InitData/Add InitDataHandler", true, 3)]
        private static bool ValidateAddInitPoseHandler()
        {
            return !FindObjectOfType<InitDataHandler>();
        }

        public List<SetupIssue> GetSceneIssues()
        {
            var initDataHandlers = FindObjectsOfType<InitDataHandler>();
            if (initDataHandlers.Length > 1)
            {
                return new List<SetupIssue>
                {
                    new SetupIssue(
                        "Multiple InitDataHandler",
                        $"There are {initDataHandlers.Length} defined in this scene. " +
                        "This will lead to unexpected behaviour. " +
                        "Please remove all but one components of this type.",
                        SetupIssue.IssueType.Error,
                        this.gameObject,
                        new DestroyComponentAction(this))
                };
            }
            return SetupIssue.NoIssues();
        }
#endif
    }
}
