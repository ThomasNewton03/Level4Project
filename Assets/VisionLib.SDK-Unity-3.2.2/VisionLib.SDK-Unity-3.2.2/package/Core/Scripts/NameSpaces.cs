// Summaries of namespaces for documentation purposes

/** @defgroup VisionLibSDKUnity VisionLib.SDK.Unity
 *  @brief The reference of the VisionLib.SDK.Unity functions.
 *
 * The VisionLib SDK for Unity is created using the C# language and reflects the native C-API of VisionLib.
 */

/** @defgroup Core VisionLib.SDK.Core
 *  @ingroup VisionLibSDKUnity
 *  @brief Contains components that are used to add tracking functionality and to setup or interact with VisionLib's tracking.
 */

/** @defgroup API VisionLib.SDK.Core.API
 *  @ingroup Core
 *  @brief  Contains classes that are used to directly communicate with the VisionLib engine, e.g. command classes.
 */

/** @defgroup Native VisionLib.SDK.Core.API.Native
 *  @ingroup Core
 *  @brief Contains classes that wrap the interface of the native VisionLib SDK.
 */

/// <summary>
/// Contains components that are used to add tracking functionality and
/// to setup or interact with VisionLib's tracking.
/// </summary>
namespace Visometry.VisionLib.SDK.Core {}

/// <summary>
/// Contains classes that are used to directly communicate with the
/// VisionLib engine, e.g. command classes.
/// </summary>
namespace Visometry.VisionLib.SDK.Core.API {}

/// <summary>
///  Contains classes that wrap the interface of the native VisionLib SDK.
/// </summary>
namespace Visometry.VisionLib.SDK.Core.API.Native {}

/** @defgroup WorkSpace WorkSpace
 *  @ingroup Core
 *  @brief The reference of the UnitySDK functions which correspond to WorkSpaces.
 */

/// <summary>
///  Namespace with classes for generating WorkSpace json. These will be used for calling
///  corresponding commands in VisionLib.SDK.Native.
///  **THIS IS SUBJECT TO CHANGE** Do not rely on this code in productive environments.
/// </summary>
namespace Visometry.VisionLib.SDK.Core.API.WorkSpace {}

/// <summary>
/// Internal use only.
/// </summary>
namespace Visometry.VisionLib.SDK.Core.Details {}

/// <summary>
/// Internal use only.
/// </summary>
namespace Visometry.VisionLib.SDK.Core.Details.TrackingAnchorTreeView {}

/** @defgroup Examples VisionLib.SDK.Examples
 *  @ingroup VisionLibSDKUnity
 *  @brief The reference of the UnitySDK functions which are only used in our examples.
 *
 *  Some of our scripts are only relevant for the provided examples. Therefore a separate namespace
 *  and documentation group has been created
 */

/// <summary>
/// Contains example scripts for the VisionLib SDK.
/// </summary>
namespace Visometry.VisionLib.SDK.Examples {}
