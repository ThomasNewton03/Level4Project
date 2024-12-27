// Copyright (c) Visometry GmbH.

/**
 * \file vlErrors.h
 * \brief This file contains error codes in an enum.
 *
 * You can use those error codes for comparison when receiving an error from a callback.
 */

#ifndef VL_ERRORS_H
#define VL_ERRORS_H

/*!
 * \ingroup VisionLibSDKNative
 * Error codes.
 */
typedef enum
{
    /*! An internal error occurred during initialization. */
    VL_INTERNAL_ERROR = 1,

    /*! No calibration available for device. The device ID is passed in the info field. Please be
       aware that some standard calibration might be used which will can harm the tracking quality
       massively. */
    VL_WARNING_CALIBRATION_MISSING_FOR_DEVICE = 2, /* Info: *DeviceName* */

    /*! There is maybe no camera connected to your system or no Camera/WebCam capabilities are set.
     */
    VL_ERROR_NO_CAMERA_CONNECTED = 4,

    /*! The camera may be removed or used by another application.*/
    VL_ERROR_NO_CAMERA_ACCESS = 5,

    /*! It was not possible to acquire an image from the camera.*/
    VL_ERROR_CAMERA_IMAGE_AQUISITION_FAILED = 6,

    /*! The calibration DB could not be loaded. */
    VL_WARNING_CALIBRATION_DB_LOAD_FAILED = 10, /* Info: *CalibrationDB_URI* */

    /*! The calibration DB is not valid JSON. */
    VL_WARNING_CALIBRATION_DB_INVALID = 11, /* Info: *CalibrationDB_URI* */

    /*! While loading a calibration data base an internal error occurred reading the database.
       Please review the error log of the VisionLib for more information! */
    VL_WARNING_CALIBRATION_DB_LOAD_ERROR = 12, /* Info: *CalibrationDB_URI**/

    /*! While loading a calibration data base the following deviceID has been overwritten. */
    VL_WARNING_CALIBRATION_DEVICE_ID_OVERWRITTEN_ON_LOAD = 13, /* Info: *deviceID Overwritten* */

    /*! While loading a calibration data base the following deviceID has been overwritten due to a
       defined alternative deviceID in 'Source'. */
    VL_WARNING_CALIBRATION_DEVICE_ID_OVERWRITTEN_BY_ALTERNATIVE_ID =
        14, /* Info: *deviceID Overwritten* - *deviceID Source* */

    /*! Will be issued when a parameter used in your tracking configuration is deprecated. It should
       be replaced by the one provided. */
    VL_DEPRECATION_WARNING = 20, /* Info: *deprecatedParameter* - *newParameter* */

    /*! File is valid json but a parameter is missing, has wrong type or is set to an invalid value.
     */
    VL_ERROR_JSON_FILE_INVALID_PARAMETER =
        90, /* Info: *FileURI* :: *parameterName* :: *parameterStructure* */

    /*! Syntax error encountered while parsing file. */
    VL_ERROR_FILE_SYNTAX_ERROR = 95, /* Info: *FileURI* */

    /*! Could not write into the specified file. */
    VL_ERROR_FILE_WRITING_FAILED = 96, /* Info: *FileURI* */

    /*! An optional permission is not set. The program might not run as expected.*/
    VL_WARNING_PERMISSION_NOT_SET = 97, /* Info: *PermissionName* */

    /*! The file specified could not be acquired or loaded. */
    VL_ERROR_FILE_READING_FAILED = 98, /* Info: *FileURI* */

    /*! No valid tracking configuration. The file can not be handled or has the wrong format. */
    VL_ERROR_FILE_INVALID = 99, /* Info: *FileURI* */

    /*! You have passed a file different to the ones allowed or it is not valid (e.g. only .json and
       .vl files allowed for the configuration). */
    VL_ERROR_FILE_FORMAT_NOT_ALLOWED = 100, /* Info: *FileExtension* */

    /*! Will be issued when the license is not a valid file. */
    VL_ERROR_LICENSE_INVALID = 101, /* Info: *LicenseFileURI* */

    /*! Will be issued when the license has been expired. In this case a black image will be shown
       with the watermark instead of the camera image. The extrinsic data cannot be acquired. */
    VL_ERROR_LICENSE_EXPIRED = 102, /* Info: *ExpirationDate* */

    /*! Will be issued when you have started the license more than 5 times without re-deploying. In
       this case a black image will be shown with the watermark instead of the camera image. The
       extrinsic data cannot be acquired. */
    VL_ERROR_LICENSE_EXCEEDS_RUNS = 103, /* Info: *NumberOfRunsSinceDeployment* */

    /*! Will be issued when no valid license for a model bound feature could be found. This will not
       load this model and will result in undefined tracking behaviour. You should contact your
       license provider for updating the model hash provided in the info string. */
    VL_WARNING_LICENSE_MODEL_BOUND_FEATURE_INVALD = 104, /* Info: *ModelHashFeature* */

    /*! Will be issued when the license is not registered for this id. If you have licensed a
       commercial or poc license you might send us this id for updating your license file. */
    VL_ERROR_LICENSE_INVALID_HOST_ID = 105, /* Info: *HostID* */

    /*! Will be issued when no license file or data has been set. */
    VL_ERROR_LICENSE_NOT_SET = 106,

    /*! The given license is not valid for running for the current platform (e.g. on HoloLens). */
    VL_ERROR_LICENSE_INVALID_PLATFORM = 107, /* Info: *CurrentPlatform* */

    /*! Will be issued when the loaded model has NOT been registered in the license. */
    VL_WARNING_LICENSE_USING_UNREGISTERED_MODELS = 108,

    /*! Will be issued when the specified license file has not been found at the specified location.
     */
    VL_ERROR_LICENSE_FILE_NOT_FOUND = 109, /* Info: *LicenseFileURI* */

    /*! Will be issued when the license is encountering a program version incompatibility (e.g.
       Lifetime License bound to certain versions). */
    VL_ERROR_LICENSE_INVALID_PROGRAM_VERSION = 110, /* Info: *LastAllowedBuildDate* */

    /*! Will be issued when the license is bound to a software protection dongle and does not work
       with the current seat. */
    VL_ERROR_LICENSE_INVALID_SEAT = 111,

    /*! Will be issued when a certain feature of the VisionLib is not available due to license
       restrictions. */
    VL_ERROR_LICENSE_INVALID_FEATURE = 112, /* Info: *featureBundleName* */

    /*! Will be issued when the license is only valid for less than a week. You should contact your
       license provider for obtaining a new license.*/
    VL_WARNING_LICENSE_EXPIRING_SOON = 113, /* Info: *daysLicenseIsValid* */

    /*! Will be issued when the license is not registered for this bundle id. If you have licensed a
       commercial or poc license you might send us this id for updating your license file. */
    VL_ERROR_LICENSE_INVALID_BUNDLE_ID = 114, /* Info: *bundleName* */

    /*! Will be issued when the number of allowed tracking anchors is exceeded. Info contains: the
       number of allowed anchors. */
    VL_ERROR_LICENSE_EXCEEDED_ALLOWED_NUMBER_OF_ANCHORS = 115, /* Info: *numberOfAllowedAnchors* */

    /*! A used scheme is not supported. Info holds the name of the scheme */
    VL_ERROR_UNSUPPORTED_SCHEME = 116, /* Info: *unsupportedScheme* :: *supportedSchemes* */

    /*! A required feature is not available on this system. Info holds the name of the feature */
    VL_ERROR_FEATURE_NOT_SUPPORTED = 200, /* Info: *FeatureName* */

    /*! The model name passed could not be loaded. Info holds the filename uri requested. */
    VL_ERROR_MODEL_LOAD_FAILED = 300, /* Info: *FileURI* */

    /*! The model downloaded by the specified fileURI could not be decoded. */
    VL_ERROR_MODEL_DECODE_FAILED = 301, /* Info: *FileURI* */

    /*! The bounding box of the model using the given metric is implausibly large or small. You
       should check the `metric` parameter. Info contains the dimensions of the bounding box.*/
    VL_WARNING_IMPLAUSIBLE_METRIC = 302, /* Info: *Boundingbox dimensions* */

    /*! The given modelname has been used twice or was used before. */
    VL_ERROR_DUPLICATE_MODEL_NAME = 303, /* Info: *modelName* */

    /*! Two models create the same modelID. */
    VL_ERROR_DUPLICATE_MODEL_ID = 304, /* Info: *modelName1* :: *modelName2* */

    /*! The model with the given name does not exist. */
    VL_ERROR_MODEL_NOT_FOUND = 305, /* Info: *modelName* */

    /*! The image passed in the poster tracker configuration could not be loaded. Info holds the
       filename uri requested. */
    VL_ERROR_POSTER_LOAD_FAILED = 350, /* Info: *FileURI* */

    /*! Will be issued when the poster quality is below a critical value. You should replace the
   poster in the tracker with one of a better quality.*/
    VL_WARNING_POSTER_QUALITY_CRITICAL = 351, /* Info: *PosterQuality* */

    /*! The setup of the graph failed with an unknown reason. */
    VL_ERROR_GRAPH_SETUP_FAILED_UNKNOWN_ERROR = 400,

    /*! Could not find the node with the given name. Info holds the name of the node.*/
    VL_ERROR_GRAPH_NODE_NOT_FOUND = 401, /* Info: *nodeName* */

    /*! The data path doesn't comply with the expected pattern `nodeName.dataName`. Info holds the
       data path that was invalid.*/
    VL_ERROR_GRAPH_INVALID_DATA_PATH = 402, /* Info: *dataPath* */

    /*! Could not find an input of a node. Info contains: The nodename :: The Key that has not been
       found :: The keys that are defined on that node. */
    VL_ERROR_GRAPH_INPUT_NOT_FOUND = 403, /* Info: *nodeName* :: *keyName* :: *awailableKeys* */

    /*! Could not find an output of a node. Info contains: The nodename :: The Key that has not been
       found :: The keys that are defined on that node. */
    VL_ERROR_GRAPH_OUTPUT_NOT_FOUND = 404, /* Info: *nodeName* :: *keyName* :: *awailableKeys* */

    /*! There is a cycle in the graph - so no order of execution could be determined. */
    VL_ERROR_GRAPH_HAS_CYCLES = 405,

    /*! There was no tracker defined. */
    VL_ERROR_GRAPH_TRACKERS_EMPTY = 406,

    /*! The same name has been used for two or more devices. */
    VL_ERROR_GRAPH_DUPLICATE_DEVICE_NAME = 407, /* Info: *deviceName* */

    /*! The same name has been used for two or more trackers. */
    VL_ERROR_GRAPH_DUPLICATE_TRACKER_NAME = 408, /* Info: *trackerName* */

    /*! The same name has been used for two or more anchors. */
    VL_ERROR_DUPLICATE_ANCHOR_NAME = 500, /* Info: *anchorName* */

    /*! The given anchor name does not exist. */
    VL_ERROR_ANCHOR_NAME_NOT_FOUND = 501, /* Info: *anchorName* */

    /*! The given parent anchor is a descendant of the anchor. */
    VL_ERROR_ANCHOR_IS_DESCENDANT = 502, /* Info: *anchorName* :: *parentAnchorName* */

    /*! The dll needed for using a device is not loaded.*/
    VL_ERROR_DLL_NOT_LOADED = 600, /* Info: *dllName* */

    /*! Could not load the dll. */
    VL_WARNING_DLL_LOAD_FAILED = 601, /* Info: *dllName* */

    /*! Could not find the dll needed as dependency. */
    VL_WARNING_DLL_NOT_FOUND = 602, /* Info: *dllName* */

    /*! The version of the dll is not compatible with the vlSDK. */
    VL_WARNING_DLL_VERSION_DIFFERENT = 603, /* Info: *dllName* */

    /*! The given command wasn't executed but canceled. */
    VL_ERROR_COMMAND_CANCELED = 700,

    /*! The given command is not supported by the current pipeline. */
    VL_ERROR_COMMAND_NOT_SUPPORTED = 701, /* Info: *activePipelines* */

    /*! The given command could not be executed because of problems inside VisionLib. */
    VL_ERROR_COMMAND_INTERNAL_PROBLEM = 702,

    /*! The given command could not be executed because the given parameter does not fit parameter
       structure. */
    VL_ERROR_COMMAND_INVALID_PARAMETER = 703, /* Info: *parameterName* :: *parameterStructure* */

    /*! The given command could not be executed because the given parameter value isn't supported by
       the current pipeline. */
    VL_ERROR_COMMAND_PARAMETER_VALUE_NOT_SUPPORTED =
        704, /* Info: *parameterName* :: *parameterValue* :: *activePipelines* */

    /*! Will be issued when trying to set a parameter, which has a fixed value for the current
       setup.*/
    VL_WARNING_PARAMETER_CONSTANT_VALUE =
        705, /* Info: *parameter* :: *constantValue* :: *requirement* */

    /*! The given command is currently not possible. */
    VL_ERROR_COMMAND_CURRENTLY_NOT_POSSIBLE = 706
} vlErrorCode;

#endif // VL_ERRORS_H
