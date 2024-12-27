// Copyright (c) Visometry GmbH.

/**
 * \file vlSDK.h
 * \brief This file contains all functions of the VisionLib SDK.
 *
 * Include this file for using the VisionLib in your software.
 *
 * \see http://www.visionlib.com
 */
 /**
  * \defgroup VisionLibSDKNative VisionLib.SDK.Native
  * \brief General C-Interface for the vlSDK, which works on all devices.
  */
/**@{*/

#ifndef VL_SDK_H
#define VL_SDK_H

#include "vlSDKDef.h"
#include "vlSDKVersion.h"

struct vlImageWrapper_s;
/*!
 * \brief Type which represents an ImageWrapper.
 *
 * Please use the \ref image "vlImageWrapper_..." functions to manipulate
 * instances of it.
 */
typedef struct vlImageWrapper_s vlImageWrapper_t;

struct vlExtrinsicDataWrapper_s;
/*!
 * \brief Type which represents an ExtrinsicDataWrapper.
 *
 * Please use the \ref extrinsic "vlExtrinsicDataWrapper_..." functions to
 * manipulate instances of it.
 */
typedef struct vlExtrinsicDataWrapper_s vlExtrinsicDataWrapper_t;

struct vlSimilarityTransformWrapper_s;
/*!
 * \brief Type which represents a SimilarityTransformWrapper.
 *
 * Please use the \ref extrinsic "vlSimilarityTransformWrapper_..." functions to
 * manipulate instances of it.
 */
typedef struct vlSimilarityTransformWrapper_s vlSimilarityTransformWrapper_t;

struct vlIntrinsicDataWrapper_s;
/*!
 * \brief Type which represents an IntrinsicDataWrapper.
 *
 * Please use the \ref intrinsic "vlIntrinsicDataWrapper_..." functions to manipulate
 * instances of it.
 */
typedef struct vlIntrinsicDataWrapper_s vlIntrinsicDataWrapper_t;

struct vlCalibratedImageWrapper_s;
/*!
* \brief Type which represents a CalibratedImageWrapper.
* 
* A calibrated image contains an image together with the corresponding intrinsic data and
* the transformation form the (camera) device to the calibrated image.
* Please use the \ref calibratedImage "vlCalibratedImageWapper_..." functions to manipulate
* instances of it.
*/
typedef struct vlCalibratedImageWrapper_s vlCalibratedImageWrapper_t;

struct vlWorker_s;
/*!
 * \brief Type which represents a Worker.
 *
 * Please use the \ref worker "vlWorker_..." functions to manipulate instances of
 * it.
 */
typedef struct vlWorker_s vlWorker_t;

/*!
 * Log levels.
 */
typedef enum
{
    /*! No logs. */
    VL_LOG_MUTE = 0,
    /*! Error level. */
    VL_LOG_ERROR = 1,
    /*! Warning level. */
    VL_LOG_WARNING = 2,
    /*! Debug level. */
    VL_LOG_DEBUG = 3
} vlLogLevel;

/*!
 * Screen orientations.
 */
typedef enum
{
    VL_RENDER_ROTATION_CCW_0 = 0,
    VL_RENDER_ROTATION_CCW_90 = 2,
    VL_RENDER_ROTATION_CCW_180 = 1,
    VL_RENDER_ROTATION_CCW_270 = 3,
} vlRenderRotation;

/*!
 * Internal image formats.
 */
typedef enum
{
    /*! Unsupported image format. */
    VL_IMAGE_FORMAT_UNDEFINED = 0,
    /*! Grey value image. */
    VL_IMAGE_FORMAT_GREY = 1,
    /*! Image with a red, green and blue channel. */
    VL_IMAGE_FORMAT_RGB = 2,
    /*! Image with a red, green, blue and alpha channel. */
    VL_IMAGE_FORMAT_RGBA = 3,
    /*! Image with one float channel describing the distance in meters. */
    VL_IMAGE_FORMAT_DEPTH = 4,
} vlImageFormat;

/*!
 * Modes of scaling images while maintaining a constant aspect ratio.
 */
typedef enum
{
    /*! Let the image cover the available space. */
    VL_FITTING_MODE_COVER = 0,
    /*! Contain the image inside the available space.*/
    VL_FITTING_MODE_CONTAIN = 1
} vlFittingMode;

/*!
 * \brief A pointer to a callback function which receives a zero terminated
 *        string as parameter.
 *
 * \param data Zero terminated string. The meaning depends on the context.
 * \param clientData Pointer value which was initially received from the user.
 *        This can be used to invoke a member function.
 */
typedef void (VL_CALLINGCONVENTION *vlCallbackZString)(
    const char data[], void* clientData);

/*!
 * \brief A pointer to a callback function which receives two zero terminated
 * string which contain JSON data.
 *
 * \param error Zero terminated string with JSON data. This will be NULL, if no
 *        error occurred. The JSON format of the error is 
 *          - "errorCode" (see \ref tracking-init-issues)
 *          - "command" (the name of the command triggering the error)
 *          - "info" (detailed information of the specific error)
 *          - "message"
 * \param data Zero terminated string with JSON data. This could be NULL, if an
 *        error occurred. The JSON format of the data object depends on the
 *        context.
 * \param clientData Pointer value which was initially received from the user.
 *        This can be used to invoke a member function.
 */
typedef void (VL_CALLINGCONVENTION *vlCallbackJsonString)(
    const char error[], const char data[], void* clientData);

/*!
 * \brief A pointer to a callback function which receives a zero terminated
 * result string (usually in JSON), a zero terminated error string and a binary
 * buffer which plain binary data.
 * Please note that the use of this function might change in future and is considered as BETA!
 *
 * NOTE: The passed data pointer should be released using the vlReleaseBinaryBuffer function.
 *
 * \param error Zero terminated string with JSON data. This will be NULL, if no
 *        error occurred. The JSON format of the error is 
 *          - "errorCode" (see \ref tracking-init-issues)
 *          - "command" (the name of the command triggering the error)
 *          - "info" (detailed information of the specific error)
 *          - "message"
 * \param result Zero terminated string with JSON data.
 *        The JSON format of the  object depends on the context.
 * \param data Binary data pointer (might be Zero). The context specific description
 *        might point to offsets within the binary data pointer.
 * \param size Size of the binary data.
 * \param clientData Pointer value which was initially received from the user.
 *        This can be used to invoke a member function.
 */
typedef void (VL_CALLINGCONVENTION *vlCallbackJsonAndBinaryString)(
    const char error[],
    const char result[],
    const char data[],
    unsigned int size,
    void* clientData);

/*!
 * \brief A pointer to a callback function which receives a pointer to an
 *        \ref image "ImageWrapper" object as parameter.
 *
 * \param image Pointer to an ImageWrapper object. You can use the
 *        \ref image "vlImageWrapper_..."" functions to work with it. Please
 *        notice, that the object is only valid inside the callback and it
 *        will automatically get deleted afterwards.
 * \param clientData Pointer value which was initially received from the user.
 *        This can be used to invoke a member function.
 */
typedef void (VL_CALLINGCONVENTION *vlCallbackImageWrapper)(
    vlImageWrapper_t* image, void* clientData);

/*!
 * \brief A pointer to a callback function which receives a pointer to an
 *        \ref extrinsic "ExtrinsicDataWrapper" object as parameter. Please
 *        notice, that the object is only valid inside the callback and it
 *        will automatically get deleted afterwards.
 *
 * \param extrinsicData Pointer to an ExtrinsicDataWrapper object. You can use
 *        the \ref extrinsic "vlExtrinsicDataWrapper_..."" functions to work
 *        with it.
 * \param clientData Pointer value which was initially received from the user.
 *        This can be used to invoke a member function.
 */
typedef void (VL_CALLINGCONVENTION *vlCallbackExtrinsicDataWrapper)(
    vlExtrinsicDataWrapper_t* extrinsicData, void* clientData);
    
/*!
 * \brief A pointer to a callback function which receives a pointer to an
 *        \ref extrinsic "SimilarityTransformWrapper" object as parameter. Please
 *        notice, that the object is only valid inside the callback and it
 *        will automatically get deleted afterwards.
 *
 * \param extrinsicData Pointer to a SimilarityTransformWrapper object. You can use
 *        the \ref extrinsic "vlSimilarityTransformWrapper_..."" functions to work
 *        with it.
 * \param clientData Pointer value which was initially received from the user.
 *        This can be used to invoke a member function.
 */
typedef void (VL_CALLINGCONVENTION *vlCallbackSimilarityTransformWrapper)(
    vlSimilarityTransformWrapper_t* similarityTransform, void* clientData);

/*!
 * \brief A pointer to a callback function which receives a pointer to an
 *        \ref intrinsic "IntrinsicDataWrapper" object as parameter. Please
 *        notice, that the object is only valid inside the callback and it
 *        will automatically get deleted afterwards.
 *
 * \param intrinsicData Pointer to an IntrinsicDataWrapper object. You can use
 *        the \ref intrinsic "vlIntrinsicDataWrapper_..."" functions to work
 *        with it.
 * \param clientData Pointer value which was initially received from the user.
 *        This can be used to invoke a member function.
 */
typedef void (VL_CALLINGCONVENTION *vlCallbackIntrinsicDataWrapper)(
    vlIntrinsicDataWrapper_t* intrinsicData, void* clientData);

/*!
 * \brief A pointer to a callback function which receives a pointer to a
 *        \ref calibratedImage "CalibratedImageWrapper" object as parameter. Please
 *        notice, that the object is only valid inside the callback and it
 *        will automatically get deleted afterwards.
 *
 * \param calibratedImage Pointer to a CalibratedImageWrapper object. You can use
 *        the \ref calibratedImage "vlCalibratedImageWrapper_..."" functions to work
 *        with it.
 * \param clientData Pointer value which was initially received from the user.
 *        This can be used to invoke a member function.
 */
typedef void(VL_CALLINGCONVENTION* vlCallbackCalibratedImageWrapper)(
    vlCalibratedImageWrapper_t* calibratedImage,
    void* clientData);

#ifdef __cplusplus
extern "C"
{
#endif // __cplusplus
    /*!
     * \defgroup global Global
     *
     * \brief Global functions which can be called without a corresponding
     * object which handles the request.
     */

    /*!
     * \ingroup global
     * \brief Returns the major version number of the VisionLib plugin.
     */
    VL_SDK_API unsigned int VL_CALLINGCONVENTION vlGetVersionMajor();

    /*!
     * \ingroup global
     * \brief Returns the minor version number of the VisionLib plugin.
     */
    VL_SDK_API unsigned int VL_CALLINGCONVENTION vlGetVersionMinor();

    /*!
     * \ingroup global
     * \brief Returns the revision version number of the VisionLib plugin.
     */
    VL_SDK_API unsigned int VL_CALLINGCONVENTION vlGetVersionRevision();
    /*!
     * \ingroup global
     * \brief Copies the version postfix of the VisionLib plugin into a buffer.
     *
     * \param postfix Buffer for storing the version postfix.
     * \param maxSize The size of the buffer.
     * \returns \c true, on success; \c false otherwise.
     */
    VL_SDK_API bool VL_CALLINGCONVENTION vlGetVersionPostfix(
        char postfix[], unsigned int maxSize);
    /*!
     * \ingroup global
     * \brief Copies the version string of the VisionLib plugin into a buffer.
     *
     * \param version Buffer for storing the version string.
     * \param maxSize The size of the buffer.
     * \returns \c true, on success; \c false otherwise.
     */
    VL_SDK_API bool VL_CALLINGCONVENTION vlGetVersionString(
        char version[], unsigned int maxSize);

    /*!
    * \ingroup global
    * \brief Copies the version hash of the VisionLib plugin into a buffer.
    *
    * \param version Buffer for storing the version hash.
    * \param maxSize The size of the buffer.
    * \returns \c true, on success; \c false otherwise.
    */
    VL_SDK_API bool VL_CALLINGCONVENTION vlGetVersionHashString(
        char version[], unsigned int maxSize);

    /*!
    * \ingroup global
    * \brief Copies the version timestamp of the VisionLib plugin into a buffer.
    *
    * \param version Buffer for storing the version timestamp.
    * \param maxSize The size of the buffer.
    * \returns \c true, on success; \c false otherwise.
    */
    VL_SDK_API bool VL_CALLINGCONVENTION vlGetVersionTimestampString(
        char versionTimeStamp[], unsigned int maxSize);
    
    /*!
     * \ingroup global
     * \brief Copies the host ID of the current application into the provided
     *        buffer as zero terminated string.
     *
     * The host ID is necessary for generating a license file.
     *
     * \param hostIdBuffer Buffer for storing the host ID as zero terminated
     *        string. The string will be empty if no host ID is available on
     *        the system.
     * \param maxSize The size of the buffer.
     * \returns \c true, on success; \c false otherwise.
     */
    VL_SDK_API bool VL_CALLINGCONVENTION vlGetHostId(
        char hostIdBuffer[], unsigned int maxSize);
    
    /*!
     * \ingroup global
     * \brief Copies the bundle ID of the current application into the provided
     *        buffer as zero terminated string.
     *
     * The bundle ID is necessary for generating a license file.
     *
     * \param bundleIdBuffer Buffer for storing the bundle ID as zero terminated
     *        string. The string will be empty if no bundle ID is available on
     *        the system.
     * \param maxSize The size of the buffer.
     * \returns \c true, on success; \c false otherwise.
     */
    VL_SDK_API bool VL_CALLINGCONVENTION vlGetBundleId(
        char bundleIdBuffer[], unsigned int maxSize);

    /*!
     * \ingroup global
     * \brief Registers a log listener.
     *
     * \param fn Listener function which will receive all VisionLib log
     *        messages.
     * \param clientData The listener will be notified with the given pointer
     *        value. This can be used to invoke a member function.
     * \returns \c true, on success; \c false otherwise.
     */
    VL_SDK_API bool VL_CALLINGCONVENTION vlAddLogListener(
        vlCallbackZString fn, void* clientData);

    /*!
     * \ingroup global
     * \brief Unregisters a log listener.
     *
     * \param fn Listener, which should be removed.
     * \param clientData Pointer value which was used during the registration.
     * \returns \c true, on success; \c false otherwise.
     */
    VL_SDK_API bool VL_CALLINGCONVENTION vlRemoveLogListener(
        vlCallbackZString fn, void* clientData);

    /*!
     * \ingroup global
     * \brief Removes all log listeners.
     *
     * \returns \c true, on success; \c false otherwise.
     */
    VL_SDK_API bool VL_CALLINGCONVENTION vlClearLogListeners();

    /*!
     * \ingroup global
     * \brief Enables log buffering.
     *
     * If log buffering is enabled, then log messages will not get dispatched
     * immediately. Instead they will get buffered and one must continuously
     * call the vlFlushLogBuffer function in order to dispatch the buffered log
     * messages to the registered listeners.
     *
     * This has the advantage, that registered listeners will not get notified
     * from some arbitrary thread, which would require proper thread
     * synchronization.
     *
     * By default log buffering is disabled.
     */
    VL_SDK_API void VL_CALLINGCONVENTION vlEnableLogBuffer();

    /*!
     * \ingroup global
     * \brief Disables log buffering.
     *
     * If log buffering is disabled, then log messages will get dispatched
     * immediately. This might happen from a different thread. Therefore one
     * must make sure, that registered log listeners are thread-safe.
     *
     * With disabled log buffering, calling the vlFlushLogBuffer function is
     * not necessary.
     *
     * By default log buffering is disabled.
     */
    VL_SDK_API void VL_CALLINGCONVENTION vlDisableLogBuffer();

    /*!
     * \ingroup global
     * \brief Sets the maximum number of log messages in the log buffer.
     *
     * If log buffering is enabled, then log messages will get buffered. In
     * order to not allocate too much memory, the size of the log buffer is
     * limited to a certain number of entries. If there are too many log
     * messages in the buffer, then the oldest message will get removed.
     *
     * By default the maximum number of buffer entries is 32.
     */
    VL_SDK_API void VL_CALLINGCONVENTION vlSetLogBufferSize(
        unsigned int maxEntries);

    /*!
     * \ingroup global
     * \brief Notifies registered log listeners of all buffered log messages.
     *
     * If log buffering is enabled, then log messages will not get dispatched
     * immediately. Instead they will get buffered and one must continuously
     * call the vlFlushLogBuffer function in order to dispatch the buffered log
     * messages to the registered listeners.
     *
     * Failing to call vlFlushLogBuffer with enabled log buffering will have
     * the effect, that registered log listeners will not get notified of any
     * log messages and old log messages will be lost forever.
     *
     * \returns \c true, on success; \c false otherwise.
     */
    VL_SDK_API bool VL_CALLINGCONVENTION vlFlushLogBuffer();

    /*!
     * \ingroup global
     * \brief Gets the current log level.
     *
     * \returns The log level (0: mute, 1: error, 2: warning, 3: debug).
     */
    VL_SDK_API int VL_CALLINGCONVENTION vlGetLogLevel();

    /*!
     * \ingroup global
     * \brief Sets the log level.
     *
     * It is recommended to set the log level  to 2 (warning) during
     * development, otherwise there will be too many messages. Only for
     * debugging purposes it might be useful to increase the log level. Before
     * deploying your application you should set the log level to 0 (mute) or
     * 1 (error).
     *
     * \param level New log level. (0: mute, 1: error, 2: warning, 3: debug).
     * \returns \c true, on success; \c false otherwise.
     */
    VL_SDK_API bool VL_CALLINGCONVENTION vlSetLogLevel(int level);

    /*!
     * \ingroup global
     * \brief Logs the given message as VisionLib log.
     *
     * \param message Zero terminated string with the message.
     * \param level Log level (0: log, 1: error, 2: warning, 3: debug).
     * \returns \c true, on success; \c false otherwise.
     */
    VL_SDK_API bool VL_CALLINGCONVENTION vlLog(
        const char message[], int level);

    /*!
     * \defgroup image ImageWrapper
     *
     * \brief Functions for managing ImageWrapper objects.
     *
     * The ImageWrapper is a wrapper for an Image object.
     */

    /*!
     * \ingroup image
     * \brief Creates a new Image object and returns a pointer to it.
     *
     * This pointer must be released using ::vlDelete_ImageWrapper.
     * \param imageFormat The format of the image to create.
     * \returns A pointer to the new ImageData.
     */
    VL_SDK_API vlImageWrapper_t* VL_CALLINGCONVENTION vlNew_ImageWrapper(vlImageFormat imageFormat);

    /*!
     * \ingroup image
     * \brief Creates a copy of the image and returns a pointer to it.
     *
     * This pointer must be released using ::vlDelete_ImageWrapper.
     * \param imageWrapper Pointer to an ImageWrapper object.
     * \returns A pointer to the new ImageWrapper object.
     */
    VL_SDK_API vlImageWrapper_t* VL_CALLINGCONVENTION
        vlImageWrapper_Clone(vlImageWrapper_t* imageWrapper);

    /*!
     * \ingroup image
     * \brief Deletes an ImageWrapper object.
     *
     * Call this function if you used the vlNew_ImageWrapper function to create
     * the object and you are now done using it.
     *
     * \param imageWrapper Pointer to an ImageWrapper object.
     */
    VL_SDK_API void VL_CALLINGCONVENTION vlDelete_ImageWrapper(
        vlImageWrapper_t* imageWrapper);

    /*!
     * \ingroup image
     * \brief Returns the internal type of the image.
     *
     * \param imageWrapper Pointer to an ImageWrapper object.
     * \returns Internal type of the image. The value can be cast into a
     *          ::vlImageFormat enumeration.
     */
    VL_SDK_API unsigned int VL_CALLINGCONVENTION vlImageWrapper_GetFormat(
        vlImageWrapper_t* imageWrapper);

    /*!
     * \ingroup image
     * \brief Returns the number of bytes per pixel.
     *
     * \param imageWrapper Pointer to an ImageWrapper object.
     * \returns The number of bytes per pixel.
     */
    VL_SDK_API unsigned int VL_CALLINGCONVENTION
        vlImageWrapper_GetBytesPerPixel(
            vlImageWrapper_t* imageWrapper);

    /*!
     * \ingroup image
     * \brief Returns the width of the image.
     *
     * \param imageWrapper Pointer to an ImageWrapper object.
     * \returns The width in pixels.
     */
    VL_SDK_API unsigned int VL_CALLINGCONVENTION vlImageWrapper_GetWidth(
        vlImageWrapper_t* imageWrapper);

    /*!
     * \ingroup image
     * \brief Returns the height of the image.
     *
     * \param imageWrapper Pointer to an ImageWrapper object.
     * \returns The height in pixels.
     */
    VL_SDK_API unsigned int VL_CALLINGCONVENTION vlImageWrapper_GetHeight(
        vlImageWrapper_t* imageWrapper);


    /*!
     * \ingroup image
     * \brief Copies the VisionLib image into the given buffer.
     *
     * Please make sure, that the buffer is large enough for storing the whole
     * image date (width * height * bytesPerPixel). The number of bytes per
     * pixel an be acquired using the ::vlImageWrapper_GetBytesPerPixel
     * function.
     *
     * \param imageWrapper Pointer to an ImageWrapper object.
     * \param buffer Buffer with width * height * bytesPerPixel bytes of
     *        memory.
     * \param bufferSize Size of the buffer.
     * \returns \c true, on success; \c false otherwise.
     */
    VL_SDK_API bool VL_CALLINGCONVENTION vlImageWrapper_CopyToBuffer(
        vlImageWrapper_t* imageWrapper,
        unsigned char buffer[], unsigned int bufferSize);

    /*!
     * \ingroup image
     * \brief Copies the given buffer into the VisionLib image
     *
     * The VisionLib image will be resized according to the width and height
     * parameter.
     *
     * Please make sure, that the data stored in the buffer has the same format
     * as the image. The image format can be acquired using the
     * ::vlImageWrapper_GetFormat function.
     *
     * \param imageWrapper Pointer to an ImageWrapper object.
     * \param buffer Buffer with the raw image data.
     * \param width New width of the image.
     * \param height New height of the image.
     * \returns \c true, on success; \c false otherwise.
     */
    VL_SDK_API bool VL_CALLINGCONVENTION vlImageWrapper_CopyFromBuffer(
        vlImageWrapper_t* imageWrapper, const unsigned char buffer[],
        unsigned int width, unsigned int height);

    /*!
     * \ingroup image
     * \brief Copies the given an formated buffer into the VisionLib image
     *
     * The VisionLib image will be resized according to the width and height and format
     * parameter.
     *
     * The image will be converted into internally into a RGBA format.
     *
     * \param imageWrapper Pointer to an ImageWrapper object.
     * \param buffer Buffer with the raw image data.
     * \param width New width of the image.
     * \param height New height of the image.
     * \param imageFormat The format of the image.
     * \returns \c true, on success; \c false otherwise.
     */
    VL_SDK_API bool VL_CALLINGCONVENTION vlImageWrapper_CopyFromBufferWithFormat(
        vlImageWrapper_t* imageWrapper, const unsigned char buffer[],
        unsigned int width, unsigned int height,vlImageFormat imageFormat);
    
    /*!
     * \defgroup extrinsic ExtrinsicDataWrapper
     *
     * \brief Functions for managing ExtrinsicDataWrapper objects.
     *
     * The ExtrinsicDataWrapper is a wrapper for an ExtrinsicData object.
     * ExtrinsicData objects represent the extrinsic camera parameters
     * (position and orientation).
     */

    /*!
     * \ingroup extrinsic
     * \brief Creates a new ExtrinsicDataWrapper object and returns a pointer to it.
     *
     * This pointer must be released using ::vlDelete_ExtrinsicDataWrapper.
     *
     * \returns A pointer to a new ExtrinsicDataWrapper object.
     */
    VL_SDK_API vlExtrinsicDataWrapper_t* VL_CALLINGCONVENTION vlNew_ExtrinsicDataWrapper();

    /*!
     * \ingroup extrinsic
     * \brief Creates a copy of the ExtrinsicDataWrapper object and returns a pointer to it.
     *
     * This pointer must be released using ::vlDelete_ExtrinsicDataWrapper.
     * \param extrinsicDataWrapper Pointer to an ExtrinsicDataWrapper object.
     * \returns A pointer to the new ExtrinsicDataWrapper object.
     */
    VL_SDK_API vlExtrinsicDataWrapper_t* VL_CALLINGCONVENTION
        vlExtrinsicDataWrapper_Clone(vlExtrinsicDataWrapper_t* extrinsicDataWrapper);

    /*!
     * \ingroup extrinsic
     * \brief Deletes an ExtrinsicDataWrapper object.
     *
     * Call this function if you used the vlNew_ExtrinsicDataWrapper function
     * to create the object and you are now done using it.
     *
     * \param extrinsicDataWrapper Pointer to an ExtrinsicDataWrapper object.
     */
    VL_SDK_API void VL_CALLINGCONVENTION vlDelete_ExtrinsicDataWrapper(
        vlExtrinsicDataWrapper_t* extrinsicDataWrapper);

    /*!
     * \ingroup extrinsic
     * \brief Returns whether the current tracking pose is valid (the tracking
     *        was successful).
     *
     * \param extrinsicDataWrapper Pointer to an ExtrinsicDataWrapper object.
     * \returns \c true, if the current tracking pose is valid;
     *          \c false otherwise.
     */
    VL_SDK_API bool VL_CALLINGCONVENTION vlExtrinsicDataWrapper_GetValid(
        vlExtrinsicDataWrapper_t* extrinsicDataWrapper);

    /*!
     * \ingroup extrinsic
     * \brief Sets the valid flag of the given ExtrinsicData.
     *
     * \param extrinsicDataWrapper Pointer to an ExtrinsicDataWrapper object.
     * \param value The new value of the valid flag.
     * \returns \c true, if setting worked;
     *          \c false otherwise
     */
    VL_SDK_API bool VL_CALLINGCONVENTION vlExtrinsicDataWrapper_SetValid(
        vlExtrinsicDataWrapper_t* extrinsicDataWrapper, bool value);

    /*!
     * \ingroup extrinsic
     * \brief Returns the current camera pose as model-view matrix.
     *
     *  The returned matrix assumes a right-handed coordinate system and is
     *  stored in the following order (column-major order):
     *  \f[
     *   \begin{bmatrix}
     *    0 & 4 &  8 & 12\\
     *    1 & 5 &  9 & 13\\
     *    2 & 6 & 10 & 14\\
     *    3 & 7 & 11 & 15\\
     *   \end{bmatrix}
     *  \f]
     *
     * \param extrinsicDataWrapper Pointer to an ExtrinsicDataWrapper object.
     * \param matrix Float array with 16 elements for storing the
     *        model-view matrix.
     * \param matrixElementCount Number of elements in the given array.
     *        This should be 16.
     * \returns \c true, on success; \c false otherwise.
     */
    VL_SDK_API bool VL_CALLINGCONVENTION
        vlExtrinsicDataWrapper_GetModelViewMatrix(
            vlExtrinsicDataWrapper_t* extrinsicDataWrapper, float* matrix,
            unsigned int matrixElementCount);

    /*!
     * \ingroup extrinsic
     * \brief Returns the translation \f$t\f$ from the world coordinate system
     *        to the camera coordinate system.
     *
     * Please notice, that \f$(R,t)\f$ represents the transformation of a 3D
     * point \f$P_w\f$ from world coordinates into a 3D point \f$P_c\f$ in
     * camera coordinates: \f$P_c = RP_w + t\f$.
     *
     * \param extrinsicDataWrapper Pointer to an ExtrinsicDataWrapper object.
     * \param t Float array with 3 elements \f$(x,y,z)\f$ for storing the
     *        translation.
     * \param elementCount Number of elements in the given array.
     *        This should be 3.
     * \returns \c true, on success; \c false otherwise.
     */
    VL_SDK_API bool VL_CALLINGCONVENTION vlExtrinsicDataWrapper_GetT(
        vlExtrinsicDataWrapper_t* extrinsicDataWrapper, float* t,
        unsigned int elementCount);

    /*!
     * \ingroup extrinsic
     * \brief Sets the translation \f$t\f$ from the world coordinate system to
     *        the camera coordinate system.
     *
     * Please notice, that \f$(R,t)\f$ represents the transformation of a 3D
     * point \f$P_w\f$ from world coordinates into a 3D point \f$P_c\f$ in
     * camera coordinates: \f$P_c = RP_w + t\f$.
     *
     * \param extrinsicDataWrapper Pointer to an ExtrinsicDataWrapper object.
     * \param t Float array with 3 elements \f$(x,y,z)\f$, which contain the
     *        translation.
     * \param elementCount Number of elements in the given array.
     *        This should be 3.
     * \returns \c true, on success; \c false otherwise.
     */
    VL_SDK_API bool VL_CALLINGCONVENTION vlExtrinsicDataWrapper_SetT(
        vlExtrinsicDataWrapper_t* extrinsicDataWrapper, const float* t,
        unsigned int elementCount);

    /*!
     * \ingroup extrinsic
     * \brief Returns the rotation \f$R\f$ from the world coordinate system to
     *        the camera coordinate system.
     *
     * Please notice, that \f$(R,t)\f$ represents the transformation of a 3D
     * point \f$P_w\f$ from world coordinates into a 3D point \f$P_c\f$ in
     * camera coordinates: \f$P_c = RP_w + t\f$.
     *
     * \param extrinsicDataWrapper Pointer to an ExtrinsicDataWrapper object.
     * \param q Float array with 4 elements \f$(x,y,z,w)\f$ for storing the
     *        rotation as quaternion.
     * \param elementCount Number of elements in the given array.
     *        This should be 4.
     * \returns \c true, on success; \c false otherwise.
     */
    VL_SDK_API bool VL_CALLINGCONVENTION vlExtrinsicDataWrapper_GetR(
        vlExtrinsicDataWrapper_t* extrinsicDataWrapper, float* q,
        unsigned int elementCount);

    /*!
     * \ingroup extrinsic
     * \brief Sets the rotation \f$R\f$ from the world coordinate system to the
     *        camera coordinate system.
     *
     * Please notice, that \f$(R,t)\f$ represents the transformation of a 3D
     * point \f$P_w\f$ from world coordinates into a 3D point \f$P_c\f$ in
     * camera coordinates: \f$P_c = RP_w + t\f$.
     *
     * \param extrinsicDataWrapper Pointer to an ExtrinsicDataWrapper object.
     * \param q Float array with 4 elements \f$(x,y,z,w)\f$, which contains the
     *          rotation as quaternion.
     * \param elementCount Number of elements in the given array.
     *        This should be 4.
     * \returns \c true, on success; \c false otherwise.
     */
    VL_SDK_API bool VL_CALLINGCONVENTION vlExtrinsicDataWrapper_SetR(
        vlExtrinsicDataWrapper_t* extrinsicDataWrapper, const float* q,
        unsigned int elementCount);

    /*!
     * \ingroup extrinsic
     * \brief Returns the position \f$P_{cam}\f$ of the camera in world
     *        coordinates.
     *
     * Internally the position \f$P_{cam}\f$ will be computed from the
     * transformation \f$(R,t)\f$ which transforms a 3D point from world
     * coordinates into camera coordinates (\f$P_{cam} = -R^{-1}t\f$).
     *
     * \param extrinsicDataWrapper Pointer to an ExtrinsicDataWrapper object.
     * \param t Float array with 3 elements \f$(x,y,z)\f$ for storing the
     *        position.
     * \param elementCount Number of elements in the given array.
     *        This should be 3.
     * \returns \c true, on success; \c false otherwise.
     */
    VL_SDK_API bool VL_CALLINGCONVENTION vlExtrinsicDataWrapper_GetCamPosWorld(
        vlExtrinsicDataWrapper_t* extrinsicDataWrapper, float* t,
        unsigned int elementCount);

    /*!
     * \ingroup extrinsic
     * \brief Sets the position \f$P_{cam}\f$ of the camera in world
     *        coordinates.
     *
     * Internally this will be stored as a transformation \f$(R,t)\f$ of a 3D
     * point from world coordinates into camera coordinates
     * (\f$t = -RP_{cam}\f$).
     *
     * \param extrinsicDataWrapper Pointer to an ExtrinsicDataWrapper object.
     * \param t Float array with 3 elements \f$(x,y,z)\f$, which contains the
     *        position.
     * \param elementCount Number of elements in the given array.
     *        This should be 3.
     * \returns \c true, on success; \c false otherwise.
     */
    VL_SDK_API bool VL_CALLINGCONVENTION vlExtrinsicDataWrapper_SetCamPosWorld(
        vlExtrinsicDataWrapper_t* extrinsicDataWrapper, const float* t,
        unsigned int elementCount);

    /*!
     * \defgroup similarityTransform SimilarityTransformWrapper
     *
     * \brief Functions for managing SimilarityTransformWrapper objects.
     *
     * The SimilarityTransformWrapper is a wrapper for a SimilarityTransform object.
     * SimilarityTransform objects represent a transform that scales in addition to rotation and translation.
     */

    /*!
     * \ingroup similarityTransform
     * \brief Creates a new SimilarityTransformWrapper object and returns a pointer to it.
     *
     * This pointer must be released using ::vlDelete_SimilarityTransformWrapper.
     *
     * \returns A pointer to a new SimilarityTransformWrapper object.
     */
    VL_SDK_API vlSimilarityTransformWrapper_t* VL_CALLINGCONVENTION vlNew_SimilarityTransformWrapper();

    /*!
     * \ingroup similarityTransform
     * \brief Creates a copy of the SimilarityTransformWrapper object and returns a pointer to it.
     *
     * This pointer must be released using ::vlDelete_SimilarityTransformWrapper.
     * \param similarityTransformWrapper Pointer to a SimilarityTransformWrapper object.
     * \returns A pointer to the new SimilarityTransformWrapper object.
     */
    VL_SDK_API vlSimilarityTransformWrapper_t* VL_CALLINGCONVENTION
        vlSimilarityTransformWrapper_Clone(vlSimilarityTransformWrapper_t* similarityTransformWrapper);

    /*!
     * \ingroup similarityTransform
     * \brief Deletes a SimilarityTransformWrapper object.
     *
     * Call this function if you used the vlNew_SimilarityTransformWrapper function
     * to create the object and you are now done using it.
     *
     * \param similarityTransformWrapper Pointer to a SimilarityTransformWrapper object.
     */
    VL_SDK_API void VL_CALLINGCONVENTION vlDelete_SimilarityTransformWrapper(
        vlSimilarityTransformWrapper_t* similarityTransformWrapper);

    /*!
     * \ingroup similarityTransform
     * \brief Returns whether the contained transform is valid (the tracking
     *        was successful).
     *
     * \param similarityTransformWrapper Pointer to a SimilarityTransformWrapper object.
     * \returns \c true, if the contained transform is valid;
     *          \c false otherwise.
     */
    VL_SDK_API bool VL_CALLINGCONVENTION vlSimilarityTransformWrapper_GetValid(
        vlSimilarityTransformWrapper_t* similarityTransformWrapper);

    /*!
     * \ingroup similarityTransform
     * \brief Sets the valid flag of the given SimilarityTransform.
     *
     * \param similarityTransformWrapper Pointer to a SimilarityTransformWrapper object.
     * \param value The new value of the valid flag.
     * \returns \c true, if setting worked;
     *          \c false otherwise
     */
    VL_SDK_API bool VL_CALLINGCONVENTION vlSimilarityTransformWrapper_SetValid(
        vlSimilarityTransformWrapper_t* similarityTransformWrapper, bool value);

    /*!
     * \ingroup similarityTransform
     * \brief Returns the translational part of the contained transform.
     *
     * Please notice, that \f$(s,R,t)\f$ represents the transformation of a 3D point as follows: 
     * \f$y = s * Rx + t\f$.
     *
     * \param similarityTransformWrapper Pointer to a SimilarityTransformWrapper object.
     * \param t Float array with 3 elements \f$(x,y,z)\f$ for storing the
     *        translation.
     * \param elementCount Number of elements in the given array.
     *        This should be 3.
     * \returns \c true, on success; \c false otherwise.
     */
    VL_SDK_API bool VL_CALLINGCONVENTION vlSimilarityTransformWrapper_GetT(
        vlSimilarityTransformWrapper_t* similarityTransformWrapper, float* t,
        unsigned int elementCount);

    /*!
     * \ingroup similarityTransform
     * \brief Sets the translational part of the contained transform.
     *
     * Please notice, that \f$(s,R,t)\f$ represents the transformation of a 3D point as follows: 
     * \f$y = s * Rx + t\f$.
     *
     * \param similarityTransformWrapper Pointer to a SimilarityTransformWrapper object.
     * \param t Float array with 3 elements \f$(x,y,z)\f$, which contain the
     *        translation.
     * \param elementCount Number of elements in the given array.
     *        This should be 3.
     * \returns \c true, on success; \c false otherwise.
     */
    VL_SDK_API bool VL_CALLINGCONVENTION vlSimilarityTransformWrapper_SetT(
        vlSimilarityTransformWrapper_t* similarityTransformWrapper, const float* t,
        unsigned int elementCount);

    /*!
     * \ingroup similarityTransform
     * \brief Returns the rotation of the contained transform.
     *
     * Please notice, that \f$(s,R,t)\f$ represents the transformation of a 3D point as follows: 
     * \f$y = s * Rx + t\f$.
     *
     * \param similarityTransformWrapper Pointer to a SimilarityTransformWrapper object.
     * \param q Float array with 4 elements \f$(x,y,z,w)\f$ for storing the
     *        rotation as quaternion.
     * \param elementCount Number of elements in the given array.
     *        This should be 4.
     * \returns \c true, on success; \c false otherwise.
     */
    VL_SDK_API bool VL_CALLINGCONVENTION vlSimilarityTransformWrapper_GetR(
        vlSimilarityTransformWrapper_t* similarityTransformWrapper, float* q,
        unsigned int elementCount);

    /*!
     * \ingroup similarityTransform
     * \brief Sets the rotation of the contained transform
     *
     * Please notice, that \f$(s,R,t)\f$ represents the transformation of a 3D point as follows: 
     * \f$y = s * Rx + t\f$.
     *
     * \param similarityTransformWrapper Pointer to a SimilarityTransformWrapper object.
     * \param q Float array with 4 elements \f$(x,y,z,w)\f$, which contains the
     *          rotation as quaternion.
     * \param elementCount Number of elements in the given array.
     *        This should be 4.
     * \returns \c true, on success; \c false otherwise.
     */
    VL_SDK_API bool VL_CALLINGCONVENTION vlSimilarityTransformWrapper_SetR(
        vlSimilarityTransformWrapper_t* similarityTransformWrapper, const float* q,
        unsigned int elementCount);

    /*!
     * \ingroup similarityTransform
     * \brief Sets the scale factor \f$s\f$ of the given SimilarityTransform.
     *
     * Please notice, that \f$(s,R,t)\f$ represents the transformation of a 3D point as follows: 
     * \f$y = s * Rx + t\f$.
     * 
     * \param similarityTransformWrapper Pointer to a SimilarityTransformWrapper object.
     * \param s The scale factor
     * \returns \c true, on success; \c false otherwise.
     */
    VL_SDK_API bool VL_CALLINGCONVENTION vlSimilarityTransformWrapper_SetS(
        vlSimilarityTransformWrapper_t* similarityTransformWrapper, float s);

    /*!
     * \ingroup similarityTransform
     * \brief Returns the scale factor \f$s\f$ of the given SimilarityTransform.
     *
     * Please notice, that \f$(s,R,t)\f$ represents the transformation of a 3D point as follows: 
     * \f$y = s * Rx + t\f$.
     * 
     * \param similarityTransformWrapper Pointer to a SimilarityTransformWrapper object.
     * \returns The scale factor, on success; \c -1 otherwise
     */
    VL_SDK_API float VL_CALLINGCONVENTION vlSimilarityTransformWrapper_GetS(
        vlSimilarityTransformWrapper_t* similarityTransformWrapper);

    /*!
     * \defgroup intrinsic IntrinsicDataWrapper
     *
     * \brief Functions for managing IntrinsicDataWrapper objects.
     *
     * The IntrinsicDataWrapper is a wrapper for an IntrinsicData object.
     * IntrinsicData objects represent the intrinsic camera parameters
     * (focal length, principal point, skew and distortion parameters).
     */

    /*!
     * \ingroup intrinsic
     * \brief Creates a new IntrinsicDataWrapper object and returns a pointer to it.
     *
     * This pointer must be released using ::vlDelete_IntrinsicDataWrapper.
     *
     * \param width The width of the intrinsic
     * \param height The height of the intrinsic.
     * \param fxNorm The normalized focal length in x direction of the intrinsic.
     * \param fyNorm The normalized focal length in y direction of the intrinsic.
     * \param cxNorm The normalized x-component of the principal point direction of the intrinsic.
     * \param cyNorm The normalized y-component of the principal point direction of the intrinsic.
     * \param skewNorm The normalized skew of the intrinsic.
     * \returns A pointer to the new IntrinsicDataWrapper object.
     */
    VL_SDK_API vlIntrinsicDataWrapper_t* VL_CALLINGCONVENTION vlNew_IntrinsicDataWrapper(
        unsigned int width,
        unsigned int height,
        double fxNorm,
        double fyNorm,
        double cxNorm,
        double cyNorm,
        double skewNorm);

    /*!
     * \ingroup intrinsic
     * \brief Creates a copy of the IntrinsicDataWrapper object and returns a pointer to it.
     *
     * This pointer must be released using ::vlDelete_IntrinsicDataWrapper.
     * \param intrinsicDataWrapper Pointer to an IntrinsicDataWrapper object.
     * \returns A pointer to the new IntrinsicDataWrapper object.
     */
    VL_SDK_API vlIntrinsicDataWrapper_t* VL_CALLINGCONVENTION
        vlIntrinsicDataWrapper_Clone(vlIntrinsicDataWrapper_t* intrinsicDataWrapper);

    /*!
     * \ingroup intrinsic
     * \brief Deletes an IntrinsicDataWrapper object.
     *
     * Call this function if you used the vlNew_IntrinsicDataWrapper function
     * to create the object and you are now done using it.
     *
     * \param intrinsicDataWrapper Pointer to an IntrinsicDataWrapper object.
     */
    VL_SDK_API void VL_CALLINGCONVENTION vlDelete_IntrinsicDataWrapper(
        vlIntrinsicDataWrapper_t* intrinsicDataWrapper);

    /*!
     * \ingroup intrinsic
     * \brief Returns the width of the intrinsic camera calibration.
     *
     * \param intrinsicDataWrapper Pointer to an IntrinsicDataWrapper object.
     * \returns The width in pixels.
     */
    VL_SDK_API unsigned int VL_CALLINGCONVENTION vlIntrinsicDataWrapper_GetWidth(
        vlIntrinsicDataWrapper_t* intrinsicDataWrapper);

    /*!
     * \ingroup intrinsic
     * \brief Returns the height of the intrinsic camera calibration.
     *
     * \param intrinsicDataWrapper Pointer to an IntrinsicDataWrapper object.
     * \returns The height in pixels.
     */
    VL_SDK_API unsigned int VL_CALLINGCONVENTION vlIntrinsicDataWrapper_GetHeight(
        vlIntrinsicDataWrapper_t* intrinsicDataWrapper);

    /*!
     * \ingroup intrinsic
     * \brief Returns the normalized focal length of the intrinsic camera
     *        calibration in x direction.
     *
     * The focal length in x direction was normalized through a division by
     * the width of the camera calibration.
     *
     * \param intrinsicDataWrapper Pointer to an IntrinsicDataWrapper object.
     * \returns Normalized focal length in x direction.
     */
    VL_SDK_API double VL_CALLINGCONVENTION vlIntrinsicDataWrapper_GetFxNorm(
        vlIntrinsicDataWrapper_t* intrinsicDataWrapper);

    /*!
     * \ingroup intrinsic
     * \brief Returns the normalized focal length of the intrinsic camera
     *        calibration in y direction.
     *
     * The focal length in y direction was normalized through a division by
     * the height of the camera calibration.
     *
     * \param intrinsicDataWrapper Pointer to an IntrinsicDataWrapper object.
     * \returns Normalized focal length in y direction.
     */
    VL_SDK_API double VL_CALLINGCONVENTION vlIntrinsicDataWrapper_GetFyNorm(
        vlIntrinsicDataWrapper_t* intrinsicDataWrapper);

    /*!
     * \ingroup intrinsic
     * \brief Returns the normalized skew of the intrinsic camera calibration.
     *
     * The skew was normalized through a division by the width of the camera
     * calibration.
     *
     * \param intrinsicDataWrapper Pointer to an IntrinsicDataWrapper object.
     * \returns Normalized skew.
     */
    VL_SDK_API double VL_CALLINGCONVENTION vlIntrinsicDataWrapper_GetSkewNorm(
        vlIntrinsicDataWrapper_t* intrinsicDataWrapper);

    /*!
     * \ingroup intrinsic
     * \brief Returns the normalized x-component of the principal point.
     *
     * The x-component was normalized through a division by the width of the
     * camera calibration.
     *
     * \param intrinsicDataWrapper Pointer to an IntrinsicDataWrapper object.
     * \returns Normalized x-component of the principal point.
     */
    VL_SDK_API double VL_CALLINGCONVENTION vlIntrinsicDataWrapper_GetCxNorm(
        vlIntrinsicDataWrapper_t* intrinsicDataWrapper);

    /*!
     * \ingroup intrinsic
     * \brief Returns the normalized y-component of the principal point.
     *
     * The y-component was normalized through a division by the height of the
     * camera calibration.
     *
     * \param intrinsicDataWrapper Pointer to an IntrinsicDataWrapper object.
     * \returns Normalized y-component of the principal point.
     */
    VL_SDK_API double VL_CALLINGCONVENTION vlIntrinsicDataWrapper_GetCyNorm(
        vlIntrinsicDataWrapper_t* intrinsicDataWrapper);

    /*!
     * \ingroup intrinsic
     * \brief Returns whether the intrinsic parameters are valid.
     *
     * A intrinsic camera calibration used for tracking should always be valid.
     *
     * \param intrinsicDataWrapper Pointer to an IntrinsicDataWrapper object.
     * \returns \c true, if the intrinsic calibration is valid;
     *          \c false otherwise.
     */
    VL_SDK_API bool VL_CALLINGCONVENTION vlIntrinsicDataWrapper_GetCalibrated(
        vlIntrinsicDataWrapper_t* intrinsicDataWrapper);

    /*!
     * \ingroup intrinsic
     * \brief Returns the calibration error.
     *
     * The reprojection error in pixel. This is interesting for evaluating the
     * quality of a camera calibration.
     *
     * \param intrinsicDataWrapper Pointer to an IntrinsicDataWrapper object.
     * \returns The reprojection error in pixel.
     */
    VL_SDK_API double VL_CALLINGCONVENTION
        vlIntrinsicDataWrapper_GetCalibrationError(
            vlIntrinsicDataWrapper_t* intrinsicDataWrapper);

    /*!
     * \ingroup intrinsic
     * \brief Retrieves the radial and tangential distortion parameters.
     *
     * \param intrinsicDataWrapper Pointer to an IntrinsicDataWrapper object.
     * \param k Double array with 5 elements for storing the distortion
     *        parameters.
     * \param elementCount Number of elements in the given array.
     *        This should be 5.
     * \returns \c true, on success; \c false otherwise.
     */
    VL_SDK_API bool VL_CALLINGCONVENTION
        vlIntrinsicDataWrapper_GetDistortionParameters(
            vlIntrinsicDataWrapper_t* intrinsicDataWrapper, double k[],
            unsigned int elementCount);

    /*! this function is deprecated, use vlIntrinsicDataWrapper_SetDistortionParameters instead.*/
    VL_SDK_API bool VL_CALLINGCONVENTION
        vlIntrinsicDataWrapper_GetRadialDistortion(
            vlIntrinsicDataWrapper_t* intrinsicDataWrapper, double k[],
            unsigned int elementCount);

    /*!
     * \ingroup intrinsic
     * \brief Sets the radial and tangential distortion parameters.
     *
     * \param intrinsicDataWrapper Pointer to an IntrinsicDataWrapper object.
     * \param k Double array with 5 elements, which contains the distortion
     *        parameters.
     * \param elementCount Number of elements in the given array.
     *        This should be 5.
     * \returns \c true, on success; \c false otherwise.
     */
    VL_SDK_API bool VL_CALLINGCONVENTION
        vlIntrinsicDataWrapper_SetDistortionParameters(
            vlIntrinsicDataWrapper_t* intrinsicDataWrapper, const double k[],
            unsigned int elementCount);

    /*! this function is deprecated, use vlIntrinsicDataWrapper_SetDistortionParameters instead.*/
    VL_SDK_API bool VL_CALLINGCONVENTION vlIntrinsicDataWrapper_SetRadialDistortion(
        vlIntrinsicDataWrapper_t* intrinsicDataWrapper,
        const double k[],
        unsigned int elementCount);

    /*!
     * \ingroup intrinsic
     * \brief Computed the projection matrix from the intrinsic camera
     *        parameters.
     *
     * The returned matrix is stored in the following order
     * (column-major order):
     * \f[
     *  \begin{bmatrix}
     *   0 & 4 &  8 & 12\\
     *   1 & 5 &  9 & 13\\
     *   2 & 6 & 10 & 14\\
     *   3 & 7 & 11 & 15\\
     *  \end{bmatrix}
     * \f]
     *
     * \param intrinsicDataWrapper Pointer to an IntrinsicDataWrapper object.
     * \param nearFact Value for the near clipping plane.
     * \param farFact Value for the far clipping plane.
     * \param screenWidth Width of the screen.
     * \param screenHeight Height of the screen.
     * \param renderRotation How the rendering is rotated relative to the orientation of the images
     *        received from the VisionLib. E.g., if the image will be rendered in
     *        landscape-left mode and the images are also in landscape-left mode,
     *        then ::VL_RENDER_ROTATION_CCW_0 should be used. If the image will be rendered in
     *        portrait mode, but the images are in landscape-left mode, then
     *        VL_RENDER_ROTATION_CCW_270 should be used.
     * \param mode The mode defines how to handle mismatching aspect ratios.
     *        Use ::VL_FITTING_MODE_COVER to scale the projection surface so that it
     *        covers the whole screen.
     *        Use ::VL_FITTING_MODE_CONTAIN to scale the projection surface so that it
     *        is completely contained inside the screen.
     * \param matrix Float array with 16 elements for storing the projection
     *        matrix.
     * \param matrixElementCount Number of elements in the given array.
     *        This should be 16.
     * \returns \c true, on success; \c false otherwise.
     */
    VL_SDK_API bool VL_CALLINGCONVENTION
        vlIntrinsicDataWrapper_GetProjectionMatrix(
            vlIntrinsicDataWrapper_t* intrinsicDataWrapper,
            float nearFact, float farFact, unsigned int screenWidth,
            unsigned int screenHeight, unsigned int renderRotation,
            unsigned int mode, float* matrix, unsigned int matrixElementCount);

    /*!
     * \defgroup calibratedImage CalibratedImageWrapper
     *
     * \brief Functions for managing CalibratedImageWrapper objects.
     *
     * The CalibratedImageWrapper is a wrapper for an CalibratedImage object.
     * CalibratedImage objects pack an image with its intrinsic and extrinsic parameters.
     */

    /*!
     * \ingroup calibratedImage
     * \brief Creates a new CalibratedImageWrapper object and returns a pointer to it.
     *
     * This pointer must be released using ::vlDelete_CalibratedImageWrapper.
     *
     * \returns A pointer to the new CalibratedImageWrapper object.
     */
    VL_SDK_API vlCalibratedImageWrapper_t* VL_CALLINGCONVENTION vlNew_CalibratedImageWrapper();

    /*!
     * \ingroup calibratedImage
     * \brief Creates a copy of the CalibratedImageWrapper object and returns a pointer to it.
     *
     * This pointer must be released using ::vlDelete_CalibratedImageWrapper.
     * \param calibratedImageWrapper Pointer to a CalibratedImageWrapper object.
     * \returns A pointer to the new CalibratedImageWrapper object.
     */
    VL_SDK_API vlCalibratedImageWrapper_t* VL_CALLINGCONVENTION
        vlCalibratedImageWrapper_Clone(vlCalibratedImageWrapper_t* calibratedImageWrapper);

    /*!
     * \ingroup calibratedImage
     * \brief Deletes an CalibratedImageWrapper object.
     *
     * Call this function if you used the vlNew_CalibratedImageWrapper function
     * to create the object and you are now done using it.
     *
     * \param calibratedImageWrapper Pointer to an CalibratedImageWrapper object.
     */
    VL_SDK_API void VL_CALLINGCONVENTION
        vlDelete_CalibratedImageWrapper(vlCalibratedImageWrapper_t* calibratedImageWrapper);

    /*!
    * \ingroup calibratedImage
    * \brief Returns a pointer to the ImageWrapper object of the calibrated image.
    * 
    * \param calibratedImageWrapper Pointer to a CalibratedImageWrapper object.
    */
    VL_SDK_API vlImageWrapper_t* VL_CALLINGCONVENTION
        vlCalibratedImageWrapper_GetImage(vlCalibratedImageWrapper_t* calibratedImageeWrapper);

    /*!
     * \ingroup calibratedImage
     * \brief Returns a pointer to the IntriniscDataWrapper object of the intrinsic 
     *        of the image.
     *
     * \param calibratedImageWrapper Pointer to a CalibratedImageWrapper object.
     */
    VL_SDK_API vlIntrinsicDataWrapper_t* VL_CALLINGCONVENTION
        vlCalibratedImageWrapper_GetIntrinsicData(
            vlCalibratedImageWrapper_t* calibratedImageWrapper);

    /*!
     * \ingroup calibratedImage
     * \brief Returns a pointer to the ExtrinsicDataWrapper object of the extrinsic
     *        from the device to the image coordinates.
     *
     * \param calibratedImageWrapper Pointer to a CalibratedImageWrapper object.
     */
    VL_SDK_API vlExtrinsicDataWrapper_t* VL_CALLINGCONVENTION
        vlCalibratedImageWrapper_GetImageFromDeviceTransform(
            vlCalibratedImageWrapper_t* calibratedImageWrapper);

    /*!
     * \defgroup worker Worker
     *
     * \brief Functions for managing Worker objects.
     *
     * A Worker object controls the tracking thread.
     */

    /*!
     * \ingroup worker
     * \brief Creates a Worker object.
     *
     * Use ::vlDelete_Worker after usage to avoid memory leaks.
     *
     * \returns Pointer to an Worker object. Use vlDelete_Worker after usage to
     *          avoid memory leaks.
     */
    VL_SDK_API vlWorker_t* VL_CALLINGCONVENTION vlNew_Worker();

    /*!
     * \ingroup worker
     * \brief Creates a synchronous Worker object.
     *
     * A synchronous Worker object doesn't create a new thread. Instead one
     * has to explicitly tell the Worker when to do his work by calling the
     * vlWorker_RunOnceSync function. This has the advantage, that we know
     * exactly when the tracking is running, when it's finished and when we
     * can get the results.
     *
     * Use ::vlDelete_Worker after usage to avoid memory leaks.
     *
     * \returns Pointer to a synchronous Worker object. Use vlDelete_Worker
                after usage to avoid memory leaks.
     */
    VL_SDK_API vlWorker_t* VL_CALLINGCONVENTION vlNew_SyncWorker();

    /*!
     * \ingroup worker
     * \brief Deletes a Worker object.
     *
     * \param worker Pointer to a Worker object.
     */
    VL_SDK_API void VL_CALLINGCONVENTION vlDelete_Worker(vlWorker_t* worker);

    /*!
     * \ingroup worker
     * \brief Starts the tracking thread.
     *
     * \param worker Pointer to a Worker object.
     * \returns \c true, on success; \c false otherwise.
     */
    VL_SDK_API bool VL_CALLINGCONVENTION vlWorker_Start(vlWorker_t* worker);

    /*!
     * \ingroup worker
     * \brief Stops the tracking thread.
     *
     * \param worker Pointer to a Worker object.
     * \returns \c true, on success; \c false otherwise.
     */
    VL_SDK_API bool VL_CALLINGCONVENTION vlWorker_Stop(vlWorker_t* worker);

    /*!
     * \ingroup worker
     * \brief Processes the enqueued commands and the tracking once.
     *
     * This function only works, if the Worker was created as synchronous
     * instance (vlNew_SyncWorker). The target number of FPS will get ignored.
     * After calling this function you should call vlWorker_ProcessEvents and
     * vlWorker_PollEvents to invoke callbacks and registered listeners.
     *
     * \param worker Pointer to a Worker object.
     * \returns \c true, on success; \c false, otherwise.
     */
    VL_SDK_API bool VL_CALLINGCONVENTION vlWorker_RunOnceSync(
        vlWorker_t* worker);

    /*!
     * \ingroup worker
     * \brief Adds a given URI pointing to a camera calibration database JSON
     *        file to the VisionLib.
     *
     * The VisionLib loads the added camera calibration database file before
     * loading a new tracking configuration file.
     *
     * This allows to provide a custom camera calibrations for devices for
     * which the VisionLib doesn't provide a default calibration. If a default
     * calibration with the same name already exists, then the default
     * calibration will get overwritten with the custom calibration.
     *
     * Please also have a look at [this reference](\ref cameraCalibration) for
     * more information about the camera calibration database format.
     *
     * \param worker Pointer to an Worker object.
     * \param uri Zero terminated string with an URI pointing to the camera
     *        calibration database file.
     * \returns \c true, on success; \c false otherwise.
     */
    VL_SDK_API bool VL_CALLINGCONVENTION
        vlWorker_AddCameraCalibrationDB(vlWorker_t* worker, const char uri[]);

    /*!
     * \ingroup worker
     * \brief Removes all references to all manually set calibration data bases.
     *
     * The VisionLib loads the added camera calibration database file before
     * loading a new tracking configuration file. This command removes the
     * queue of file to be loaded.
     *
     * This allows to provide a custom camera calibrations for devices for
     * which the VisionLib doesn't provide a default calibration. If a default
     * calibration with the same name already exists, then the default
     * calibration will get overwritten with the custom calibration.
     *
     * Please also have a look at [this reference](\ref cameraCalibration) for
     * more information about the camera calibration database format.
     *
     * \param worker Pointer to an Worker object.
     * \returns \c true, on success; \c false otherwise.
     */
    VL_SDK_API bool VL_CALLINGCONVENTION vlWorker_ResetCameraCalibrationDB(vlWorker_t* worker);

    /*!
    * \ingroup worker
    * \brief Processes the passed command.
    *
    * This function only works, if the Worker was created as synchronous
    * instance (vlNew_SyncWorker).
    *
    * \param worker Pointer to a Worker object.
    * \param jsonString Command as zero terminated JSON string.
    * \param callback Callback function, which will be called
    *        after the command was processed.
    * \param clientData The callback function will be called with the given
    *        pointer value as parameter. This can be used to invoke a member
    *        function.
    * \returns \c true, if the command is supported; \c false, otherwise.
    */
    VL_SDK_API bool VL_CALLINGCONVENTION vlWorker_ProcessJsonCommandSync(
        vlWorker_t* worker, const char jsonString[],
        vlCallbackJsonString callback, void* clientData);

    /*!
     * \ingroup worker
     * \brief Processes the passed json command along with binary data for the visionLib.
     *
     *  This function only works, if the Worker was created as synchronous
     *  instance (vlNew_SyncWorker).
     *
     *  Currently this command is used internally and will be documented in future versions.
     *  Thus it is considered as BETA!
     *
     * \param worker Pointer to a Worker object.
     * \param jsonString Command as zero terminated JSON string.
     * \param data Binary data pointer. The lifetime of the memory should be
     *        maintaned until the answer has been received. (might be 0)
     * \param size The size of the content passed.
     * \param callback Callback function, which will be called synchronously
     * \param clientData The callback function will be called with the given
     *        pointer value as parameter. This can be used to invoke a member
     *        function.
     * \returns \c true, if the command is supported; \c false, otherwise.
     */
    VL_SDK_API bool VL_CALLINGCONVENTION vlWorker_ProcessJsonAndBinaryCommandSync(
        vlWorker_t* worker,
        const char jsonString[],
        const char data[],
        unsigned int size,
        vlCallbackJsonAndBinaryString callback,
        void* clientData);

    /*!
     * \ingroup worker
     * \brief Returns a JSON string with information about the current device.
     *
     * It also contains information about the cameras currently connected to
     * the system.
     *
     * A typical device info might look like this:
     * \code{.json}
     * {
     *     "availableCameras": [{
     *         "cameraName": "HD Webcam C525",
     *         "deviceID": "HDWebcamC525",
     *         "internalID": "0x400000046d0826",
     *         "position": "unknown",
     *         "supportsDepthData": false,
     *         "supportsSmoothedDepthData": false
     *     }],
     * }
     * \endcode
     *
     * \param worker Pointer to an vlWorker object.
     * \returns A char* containing the current device information which MUST be
     *          freed using vlReleaseBinaryBuffer().
     */
    VL_SDK_API char* VL_CALLINGCONVENTION vlWorker_GetDeviceInfo(vlWorker_t* worker);

    /*!
     * \ingroup worker
     * \brief Sets the path of the license file.
     *
     * This is mandatory for being able to run the tracking. Alternatively you
     * can inject the license file from memory using the 
     * vlWorker_SetLicenseFileData function.
     *
     * \param worker Pointer to a Worker object.
     * \param licenseFilePath Zero terminated string with a path to the license
     *        file.
     * \returns \c true, on success; \c false otherwise.
     */
    VL_SDK_API bool VL_CALLINGCONVENTION
        vlWorker_SetLicenseFilePath(vlWorker_t* worker, const char licenseFilePath[]);

    /*!
     * \ingroup worker
     * \brief Allows to inject the license data from memory.
     *
     * This is mandatory for being able to run the tracking. Alternatively you
     * can load the license data from a file using the
     * vlWorker_SetLicenseFilePath function. If the
     * vlWorker_SetLicenseFilePath was previously used and
     * now the license data should be read from a file, then please set the
     * license file data to an empty string first.
     *
     * \param worker Pointer to a Worker object.
     * \param licenseFileData Zero terminated string with the license data.
     * \returns \c true, on success; \c false otherwise.
     */
    VL_SDK_API bool VL_CALLINGCONVENTION
        vlWorker_SetLicenseFileData(vlWorker_t* worker, const char licenseFileData[]);

    /*!
     * \ingroup worker
     * \brief Retrieves actual license information as a JSON encoded string.
     *
     * The string is JSON encoded and can be used for examining available
     * license features. It can only be valid after a license file has been set.
     *
     * \param worker Pointer to a Worker object.
     * \returns A char* containing the current license information which MUST 
     *          be freed using vlReleaseBinaryBuffer().
     */
    VL_SDK_API char* VL_CALLINGCONVENTION vlWorker_GetLicenseInformation(
        vlWorker_t* worker);

    /*!
     * \ingroup worker
     * \brief Loads the plugin with the given name
     *
     * \param worker Pointer to a Worker object.
     * \param pluginName Zero terminated string with the name of the plugin without prefix ("VP")
     *        and fileending (".dll", ".so") e.g. "VideoUEye".
     * \returns \c true, if plugin has been loaded successfully; \c false otherwise.
     */
    VL_SDK_API bool VL_CALLINGCONVENTION
        vlWorker_LoadPlugin(vlWorker_t* worker, const char pluginName[]);

    /*!
     * \ingroup worker
     * \brief Returns a pointer to the camera image.
     *
     * This function only works, if the Worker was created as synchronous
     * instance (vlNew_SyncWorker).
     *
     * The worker is the owner of the image. Therefore you should not call
     * ::vlDelete_ImageWrapper with the returned image as parameter.
     *
     * NOTICE: This functions is experimental and might get removed in future.
     *
     * \param worker Pointer to a Worker object.
     * \returns A pointer to an ImageWrapper object, on success;
     *          \c NULL otherwise.
     */
    VL_SDK_API vlImageWrapper_t* VL_CALLINGCONVENTION vlWorker_GetImageSync(
        vlWorker_t* worker);
    
    /*!
     * \ingroup worker
     * \brief Returns a pointer to the image with the given name from a given node.
     *
     * This function only works, if the Worker was created as synchronous
     * instance (vlNew_SyncWorker).
     *
     * The returned image has to be freed using ::vlDelete_ImageWrapper.
     *
     * NOTICE: This functions is experimental and might get removed in future.
     *
     * \param worker Pointer to a Worker object.
     * \param node name of the node
     * \param key Name of the exposed image
     * \returns A pointer to an ImageWrapper object, on success;
     *          \c NULL otherwise.
     */
    VL_SDK_API vlImageWrapper_t* VL_CALLINGCONVENTION
    vlWorker_GetNodeImageSync(vlWorker_t* worker, const char node[], const char key[]);
    
    /*!
     * \ingroup worker
     * \brief Returns a pointer to the image with the given name from a given node.
     *
     * This function only works, if the Worker was created as synchronous
     * instance (vlNew_SyncWorker).
     *
     * The returned image has to be freed using ::vlDelete_ImageWrapper.
     *
     * NOTICE: This functions is experimental and might get removed in future.
     *
     * \param worker Pointer to a Worker object.
     * \param node name of the node
     * \param key Name of the exposed image
     * \returns A pointer to an ImageWrapper object, on success;
     *          \c NULL otherwise.
     */
    VL_SDK_API vlImageWrapper_t* VL_CALLINGCONVENTION
    vlWorker_TryGetNodeImageSync(vlWorker_t* worker, const char node[], const char key[]);

    /*!
     * \ingroup worker
     * \brief Sets the given image in the given input of the given node.
     *
     * This function only works, if the Worker was created as synchronous
     * instance (vlNew_SyncWorker).
     * 
     * NOTICE: This function is experimental and might get removed in future.
     *
     * \param worker Pointer to a Worker object.
     * \param image Pointer to an Image object
     * \param node name of the node
     * \param key Name of the image to set
     * \returns \c true, on success; \c false, otherwise.
     */
    VL_SDK_API bool VL_CALLINGCONVENTION
    vlWorker_SetNodeImageSync(vlWorker_t* worker, vlImageWrapper_t* image, const char node[], const char key[]);
    
    /*!
     * \ingroup worker
     * \brief Returns a pointer to extrinsicdata with the given name from a given node.
     *
     * This function only works, if the Worker was created as synchronous
     * instance (vlNew_SyncWorker).
     *
     * The returned ExtrinsicData has to be freed using ::vlDelete_ExtrinsicDataWrapper.
     * 
     * NOTICE: This function is experimental and might get removed in future.
     *
     * \param worker Pointer to a Worker object.
     * \param node The name of the node.
     * \param key The name of the exposed ExtrinsicData.
     * \returns A pointer to an ExtrinsicDataWrapper object, on success;
     *          \c NULL otherwise.
     */
    VL_SDK_API vlExtrinsicDataWrapper_t* VL_CALLINGCONVENTION
        vlWorker_GetNodeExtrinsicDataSync(
        vlWorker_t* worker,
        const char node[],
        const char key[]);

    /*!
    * \ingroup worker
    * \brief Sets the given extrinsicData in the given input of the given node.
    *
    * This function only works, if the Worker was created as synchronous
    * instance (vlNew_SyncWorker).
    * 
    * NOTICE: This function is experimental and might get removed in future.
    *
    * \param worker Pointer to a Worker object.
    * \param extrinsicData Pointer to an ExtrinsicData object.
    * \param node The name of the node.
    * \param key The name of the ExtrinsicData to set.
    * \returns \c true, on success; \c false, otherwise.
    */
    VL_SDK_API bool VL_CALLINGCONVENTION vlWorker_SetNodeExtrinsicDataSync(
        vlWorker_t* worker,
        vlExtrinsicDataWrapper_t* extrinsicData,
        const char node[],
        const char key[]);

    /*!
     * \ingroup worker
     * \brief Returns a pointer to the SimilarityTransform with the given name from a given node.
     *
     * This function only works, if the Worker was created as synchronous
     * instance (vlNew_SyncWorker).
     *
     * The returned SimilarityTransform has to be freed using ::vlDelete_SimilarityTransformWrapper.
     * 
     * NOTICE: This function is experimental and might get removed in future.
     *
     * \param worker Pointer to a Worker object.
     * \param node The name of the node.
     * \param key The name of the exposed SimilarityTransform.
     * \returns A pointer to a SimilarityTransformWrapper object, on success;
     *          \c NULL otherwise.
     */
    VL_SDK_API vlSimilarityTransformWrapper_t* VL_CALLINGCONVENTION
        vlWorker_GetNodeSimilarityTransformSync(
        vlWorker_t* worker,
        const char node[],
        const char key[]);

    /*!
     * \ingroup worker
     * \brief Returns a pointer to the WorldFromAnchorTransform with the given anchor name.
     *
     * This function only works, if the Worker was created as synchronous
     * instance (vlNew_SyncWorker).
     *
     * The returned SimilarityTransform has to be freed using ::vlDelete_SimilarityTransformWrapper.
     *
     * \param worker Pointer to a Worker object.
     * \param anchorName Name of the anchor to be addressed.
     * \returns A pointer to a SimilarityTransformWrapper object, on success;
     *          \c NULL otherwise.
     */
    VL_SDK_API vlSimilarityTransformWrapper_t* VL_CALLINGCONVENTION
        vlWorker_GetWorldFromAnchorTransform(vlWorker_t* worker, const char anchorName[]);

    /*!
    * \ingroup worker
    * \brief Sets the given SimilarityTransform in the given input of the given node.
    *
    * This function only works, if the Worker was created as synchronous
    * instance (vlNew_SyncWorker).
    * 
    * NOTICE: This function is experimental and might get removed in future.
    *
    * \param worker Pointer to a Worker object.
    * \param similarityTransform Pointer to a SimilarityTransform object.
    * \param node The name of the node.
    * \param key The name of the SimilarityTransform to set.
    * \returns \c true, on success; \c false, otherwise.
    */
    VL_SDK_API bool VL_CALLINGCONVENTION vlWorker_SetNodeSimilarityTransformSync(
        vlWorker_t* worker,
        vlSimilarityTransformWrapper_t* similarityTransform,
        const char node[],
        const char key[]);

    /*!
     * \ingroup worker
     * \brief Returns a pointer to IntrinsicData with the given name from a given node.
     *
     * This function only works, if the Worker was created as synchronous
     * instance (vlNew_SyncWorker).
     *
     * The returned IntrinsicData has to be freed using ::vlDelete_IntrinsicDataWrapper
     *
     * NOTICE: This function is experimental and might get removed in future.
     *
     * \param worker Pointer to a Worker object.
    * \param node The name of the node.
     * \param key The name of the exposed IntrinsicData.
     * \returns A pointer to an IntrinsicData object, on success;
     *          \c NULL otherwise.
     */
    VL_SDK_API vlIntrinsicDataWrapper_t* VL_CALLINGCONVENTION
        vlWorker_GetNodeIntrinsicDataSync(
        vlWorker_t* worker,
        const char node[],
        const char key[]);
        
    /*!
    * \ingroup worker
    * \brief Sets the given IntrinsicData in the given input of the given node.
    *
    * This function only works, if the Worker was created as synchronous
    * instance (vlNew_SyncWorker).
    * 
    * NOTICE: This function is experimental and might get removed in future.
    *
    * \param worker Pointer to a Worker object.
    * \param intrinsicData Pointer to an IntrinsicData object
    * \param node The name of the node.
    * \param key The name of the IntrinsicData to set.
    * \returns \c true, on success; \c false, otherwise.
    */
    VL_SDK_API bool VL_CALLINGCONVENTION vlWorker_SetNodeIntrinsicDataSync(
        vlWorker_t* worker,
        vlIntrinsicDataWrapper_t* intrinsicData,
        const char node[],
        const char key[]);
    
    /*!
     * \ingroup worker
     * \brief Returns whether the thread is currently running or not.
     *
     * \param worker Pointer to a Worker object.
     * \returns \c true, if the thread is running; \c false otherwise.
     */
    VL_SDK_API bool VL_CALLINGCONVENTION vlWorker_IsRunning(
        vlWorker_t* worker);

    /*!
     * \ingroup worker
     * \brief Enqueues a command for the tracking thread as zero terminated
     *        JSON string.
     *
     *  The command gets processed asynchronously by the tracking thread and a
     *  callback will called once after the processing has finished.
     *
     *  A list of the available commands can be found under
     *  https://visionlib.com/documentation/#/Detailed_Reference/Command_Reference/README
     *
     * \param worker Pointer to a Worker object.
     * \param jsonString Command as zero terminated JSON string.
     * \param callback Callback function, which will be called inside
     *        vlWorker_ProcessCallbacks after the command was processed.
     * \param clientData The callback function will be called with the given
     *        pointer value as parameter. This can be used to invoke a member
     *        function.
     * \returns \c true, if the command was enqueue successfully;
     *          \c false otherwise.
     */
    VL_SDK_API bool VL_CALLINGCONVENTION vlWorker_PushJsonCommand(
        vlWorker_t* worker, const char jsonString[],
        vlCallbackJsonString callback, void* clientData);

    /*!
     * \ingroup worker
     * \brief Enqueues a json command along with binary data for the visionLib.
     *
     *  The command gets processed asynchronously by the tracking thread and a
     *  callback will called once after the processing has finished.
     *
     *  Currently this command is used internally and will be documented in future versions.
     *  Thus it is considered as BETA!
     *
     *  A list of the available commands can be found under
     *  https://visionlib.com/documentation/#/Detailed_Reference/Command_Reference/README
     *
     * \param worker Pointer to a Worker object.
     * \param jsonString Command as zero terminated JSON string.
     * \param data Binary data pointer. The lifetime of the memory should be maintaned until the answer has been received. (might be 0)
     * \param size The size of the content passed.
     * \param callback Callback function, which will be called inside
     *        vlWorker_ProcessCallbacks after the command was processed.
     * \param clientData The callback function will be called with the given
     *        pointer value as parameter. This can be used to invoke a member
     *        function.
     * \returns \c true, if the command was enqueue successfully;
     *          \c false otherwise.
     */
    VL_SDK_API bool VL_CALLINGCONVENTION vlWorker_PushJsonAndBinaryCommand(
        vlWorker_t* worker, const char jsonString[], const char data[],
        unsigned int size, vlCallbackJsonAndBinaryString callback,
        void* clientData);

   /*!
    * \ingroup worker
    * \brief Helper function for releasing a binary memory block.
    *
    * When an vlCallbackJsonAndBinaryString is issued, a memory block passed should be removed after using.
    *
    *  Currently this command is used internally and will be documented in future versions.
    *  Thus it is considered as BETA!
    *
    * \param data Pointer to a binary object.
    */
    VL_SDK_API void VL_CALLINGCONVENTION vlReleaseBinaryBuffer(const char data[]);

    /*!
     * \ingroup worker
     * \brief Executes all enqueued callbacks.
     *
     * Callbacks aren't called immediately from the tracking thread in order to
     * avoid synchronisation problems. Instead this method should be called
     * regularly from the main thread.
     *
     * \param worker Pointer to a Worker object.
     * \returns \c true, on success; \c false otherwise.
     */
    VL_SDK_API bool VL_CALLINGCONVENTION vlWorker_ProcessCallbacks(
        vlWorker_t* worker);

    /*!
     * \ingroup worker
     * \brief Registers a listener for image events.
     *
     * \param worker Pointer to a Worker object.
     * \param listener Listener function which will be notified during the
     *        event processing, if an image event occurred.
     * \param clientData The listener function will be called with the given
     *        pointer value as parameter. This can be used to invoke a member
     *        function.
     * \returns \c true, on success; \c false otherwise.
     */
    VL_SDK_API bool VL_CALLINGCONVENTION vlWorker_AddImageListener(
        vlWorker_t* worker, vlCallbackImageWrapper listener, void* clientData);

    /*!
     * \ingroup worker
     * \brief Unregisters a listener from image events.
     *
     * \param worker Pointer to a Worker object.
     * \param listener Listener function which should be unregistered.
     * \param clientData Pointer value used as parameter during the
     *        registration of the listener.
     * \returns \c true, on success; \c false otherwise.
     */
    VL_SDK_API bool VL_CALLINGCONVENTION vlWorker_RemoveImageListener(
        vlWorker_t* worker, vlCallbackImageWrapper listener, void* clientData);

    /*!
     * \ingroup worker
     * \brief Register a listener for debug image events.
     *
     * \param worker Pointer to a Worker object.
     * \param listener Listener function which will be notified during the
     *        event processing, if a debug image event occurred.
     * \param clientData The listener function will be called with the given
     *        pointer value as parameter. This can be used to invoke a member
     *        function.
     * \returns \c true, on success; \c false otherwise.
     */
    VL_SDK_API bool VL_CALLINGCONVENTION vlWorker_AddDebugImageListener(
        vlWorker_t* worker,
        vlCallbackImageWrapper listener,
        void* clientData);

    /*!
     * \ingroup worker
     * \brief Unregisters a listener from debug image events.
     *
     * \param worker Pointer to a Worker object.
     * \param listener Listener function which should be unregistered.
     * \param clientData Pointer value used as parameter during the
     *        registration of the listener.
     * \returns \c true, on success; \c false otherwise.
     */
    VL_SDK_API bool VL_CALLINGCONVENTION vlWorker_RemoveDebugImageListener(
        vlWorker_t* worker,
        vlCallbackImageWrapper listener,
        void* clientData);

    /*!
     * \ingroup worker
     * \brief Registers a listener for ExtrinsicData events.
     *
     * \param worker Pointer to a Worker object.
     * \param listener Listener function which will be notified during the
     *        event processing, if an ExtrinsicData event occurred.
     * \param clientData The listener function will be called with the given
     *        pointer value as parameter. This can be used to invoke a member
     *        function.
     * \returns \c true, on success; \c false otherwise.
     */
    VL_SDK_API bool VL_CALLINGCONVENTION vlWorker_AddExtrinsicDataListener(
        vlWorker_t* worker, vlCallbackExtrinsicDataWrapper listener,
        void* clientData);

    /*!
     * \ingroup worker
     * \brief Unregisters a listener from ExtrinsicData events.
     *
     * \param worker Pointer to a Worker object.
     * \param listener Listener function which should be unregistered.
     * \param clientData Pointer value used as parameter during the
     *        registration of the listener.
     * \returns \c true, on success; \c false otherwise.
     */
    VL_SDK_API bool VL_CALLINGCONVENTION vlWorker_RemoveExtrinsicDataListener(
        vlWorker_t* worker, vlCallbackExtrinsicDataWrapper listener,
        void* clientData);

    /*!
     * \ingroup worker
     * \brief Registers a listener for IntrinsicData events.
     *
     * \param worker Pointer to a Worker object.
     * \param listener Listener function which will be notified during the
     *        event processing, if an IntrinsicData event occurred.
     * \param clientData The listener function will be called with the given
     *        pointer value as parameter. This can be used to invoke a member
     *        function.
     * \returns \c true, on success; \c false otherwise.
     */
    VL_SDK_API bool VL_CALLINGCONVENTION vlWorker_AddIntrinsicDataListener(
        vlWorker_t* worker, vlCallbackIntrinsicDataWrapper listener,
        void* clientData);

    /*!
     * \ingroup worker
     * \brief Unregisters a listener from IntrinsicData events.
     *
     * \param worker Pointer to a Worker object.
     * \param listener Listener function which should be unregistered.
     * \param clientData Pointer value used as parameter during the
     *        registration of the listener.
     * \returns \c true, on success; \c false otherwise.
     */
    VL_SDK_API bool VL_CALLINGCONVENTION vlWorker_RemoveIntrinsicDataListener(
        vlWorker_t* worker, vlCallbackIntrinsicDataWrapper listener,
        void* clientData);

    /*!
     * \ingroup worker
     * \brief Registers a listener for CalibratedImage events.
     *
     * \param worker Pointer to a Worker object.
     * \param listener Listener function which will be notified during the
     *        event processing, if an CalibratedImage event occurred.
     * \param clientData The listener function will be called with the given
     *        pointer value as parameter. This can be used to invoke a member
     *        function.
     * \param format The image format for which the listener function should
     *        receive events.
     * \returns \c true, on success; \c false otherwise.
     */
    VL_SDK_API bool VL_CALLINGCONVENTION vlWorker_AddCalibratedImageListener(
        vlWorker_t* worker,
        vlCallbackCalibratedImageWrapper listener,
        void* clientData,
        vlImageFormat format);

    /*!
     * \ingroup worker
     * \brief Unregisters a listener from CalibratedImage events.
     *
     * \param worker Pointer to a Worker object.
     * \param listener Listener function which should be unregistered.
     * \param clientData Pointer value used as parameter during the
     *        registration of the listener.
     * \param format The image format for which the listener function has
     *        been registered.
     * \returns \c true, on success; \c false otherwise.
     */
    VL_SDK_API bool VL_CALLINGCONVENTION vlWorker_RemoveCalibratedImageListener(
        vlWorker_t* worker,
        vlCallbackCalibratedImageWrapper listener,
        void* clientData,
        vlImageFormat format);

    /*!
     * \ingroup worker
     * \brief Registers a listener for tracking state events.
     *
     * \param worker Pointer to a Worker object.
     * \param listener Listener function which will be notified during the
     *        event processing, if a tracking state event occurred.
     * \param clientData The listener function will be called with the given
     *        pointer value as parameter. This can be used to invoke a member
     *        function.
     * \returns \c true, on success; \c false otherwise.
     */
    VL_SDK_API bool VL_CALLINGCONVENTION vlWorker_AddTrackingStateListener(
        vlWorker_t* worker, vlCallbackZString listener, void* clientData);

    /*!
     * \ingroup worker
     * \brief Unregisters a listener from tracking state events.
     *
     * \param worker Pointer to a Worker object.
     * \param listener Listener function which should be unregistered.
     * \param clientData Pointer value used as parameter during the
     *        registration of the listener.
     * \returns \c true, on success; \c false otherwise.
     */
    VL_SDK_API bool VL_CALLINGCONVENTION vlWorker_RemoveTrackingStateListener(
        vlWorker_t* worker, vlCallbackZString listener, void* clientData);

    /*!
     * \ingroup worker
     * \brief Registers a listener for performance information events.
     *
     * \param worker Pointer to a Worker object.
     * \param listener Listener function which will be notified during the
     *        event processing, if a performance information event occurred.
     * \param clientData The listener function will be called with the given
     *        pointer value as parameter. This can be used to invoke a member
     *        function.
     * \returns \c true, on success; \c false otherwise.
     */
    VL_SDK_API bool VL_CALLINGCONVENTION vlWorker_AddPerformanceInfoListener(
        vlWorker_t* worker, vlCallbackZString listener, void* clientData);

    /*!
     * \ingroup worker
     * \brief Unregisters a listener from performance info events.
     *
     * \param worker Pointer to a Worker object.
     * \param listener Listener function which should be unregistered.
     * \param clientData Pointer value used as parameter during the
     *        registration of the listener.
     * \returns \c true, on success; \c false otherwise.
     */
    VL_SDK_API bool VL_CALLINGCONVENTION vlWorker_RemovePerformanceInfoListener(
        vlWorker_t* worker, vlCallbackZString listener, void* clientData);

    /*!
     * \ingroup worker
     * \brief Register a listener for world from anchor transform events 
     *        (SimilarityTransform events) produced by a certain anchor.
     *
     * @param worker Pointer to a Worker object.
     * @param anchorName Name of the anchor to be addressed.
     * @param listener Listener function which will be notified during the
     *        event processing, if an event occurred.
     * \param clientData The listener function will be called with the given
     *        pointer value as parameter. This can be used to invoke a member
     *        function.
     * \returns \c true, on success; \c false otherwise.
     */
    VL_SDK_API bool VL_CALLINGCONVENTION 
        vlWorker_AddWorldFromAnchorTransformListener(
        vlWorker_t* worker,
        const char anchorName[],
        vlCallbackSimilarityTransformWrapper listener,
        void* clientData);

    /*!
     * \ingroup worker
     * \brief Unregister a listener from  world from anchor transform events
     *        (SimilarityTransform events) produced by a certain anchor.
     *
     * @param worker Pointer to a Worker object.
     * @param anchorName Name of the anchor to be addressed.
     * @param listener Listener function which should be unregistered.
     * \param clientData The listener function will be called with the given
     *        pointer value as parameter. This can be used to invoke a member
     *        function.
     * \returns \c true, on success; \c false otherwise.
     */
    VL_SDK_API bool VL_CALLINGCONVENTION 
        vlWorker_RemoveWorldFromAnchorTransformListener(
        vlWorker_t* worker,
        const char anchorName[],
        vlCallbackSimilarityTransformWrapper listener,
        void* clientData);

    /*!
     * \ingroup worker
     * \brief Registers a listener for world from camera transform events.
     *
     * \param worker Pointer to a Worker object.
     * \param listener Listener function which will be notified during the
     *        event processing, if an world from camera transform event occurred.
     * \param clientData The listener function will be called with the given
     *        pointer value as parameter. This can be used to invoke a member
     *        function.
     * \returns \c true, on success; \c false otherwise.
     */
    VL_SDK_API bool VL_CALLINGCONVENTION vlWorker_AddWorldFromCameraTransformListener(
        vlWorker_t* worker,
        vlCallbackExtrinsicDataWrapper listener,
        void* clientData);

    /*!
     * \ingroup worker
     * \brief Unregisters a listener from world from camera transform events.
     *
     * \param worker Pointer to a Worker object.
     * \param listener Listener function which should be unregistered.
     * \param clientData Pointer value used as parameter during the
     *        registration of the listener.
     * \returns \c true, on success; \c false otherwise.
     */
    VL_SDK_API bool VL_CALLINGCONVENTION vlWorker_RemoveWorldFromCameraTransformListener(
        vlWorker_t* worker,
        vlCallbackExtrinsicDataWrapper listener,
        void* clientData);

    /*!
     * \internal
     * \ingroup worker
     * \brief Register a listener for named image events produced by a certain
     *        node.
     *
     * @param worker Pointer to a Worker object.
     * @param node Name of the node to be addressed.
     * @param key Name of the image to be addressed within the node.
     * @param listener Listener function which will be notified during the
     *        event processing, if an image event occurred.
     * \param clientData The listener function will be called with the given
     *        pointer value as parameter. This can be used to invoke a member
     *        function.
     * \returns \c true, on success; \c false otherwise.
     */
    VL_SDK_API bool VL_CALLINGCONVENTION vlWorker_AddNodeDataImageListener(
        vlWorker_t* worker, const char node[], const char key[],
        vlCallbackImageWrapper listener, void* clientData);

    /*!
     * \internal
     * \ingroup worker
     * \brief Unregister a listener from named image events produced by a
     *        certain node.
     *
     * @param worker Pointer to a Worker object.
     * @param node Name of the node to be addressed.
     * @param key Name of the image to be addressed within the node.
     * @param listener Listener function which should be unregistered.
     * \param clientData The listener function will be called with the given
     *        pointer value as parameter. This can be used to invoke a member
     *        function.
     * \returns \c true, on success; \c false otherwise.
     */
    VL_SDK_API bool VL_CALLINGCONVENTION vlWorker_RemoveNodeDataImageListener(
        vlWorker_t* worker, const char node[], const char key[],
        vlCallbackImageWrapper listener, void* clientData);

    /*!
     * \internal
     * \ingroup worker
     * \brief Register a listener for named ExtrinsicData events produced by a
     *        certain node.
     *
     * @param worker Pointer to a Worker object.
     * @param node Name of the node to be addressed.
     * @param key Name of the ExtrinsicData to be addressed within the node.
     * @param listener Listener function which will be notified during the
     *        event processing, if an ExtrinsicData event occurred.
     * \param clientData The listener function will be called with the given
     *        pointer value as parameter. This can be used to invoke a member
     *        function.
     * \returns \c true, on success; \c false otherwise.
     */
    VL_SDK_API bool VL_CALLINGCONVENTION
        vlWorker_AddNodeDataExtrinsicDataListener(
            vlWorker_t* worker, const char node[], const char key[],
            vlCallbackExtrinsicDataWrapper listener, void* clientData);

    /*!
     * \internal
     * \ingroup worker
     * \brief Unregister a listener from named ExtrinsicData events produced
     *        by a certain node.
     *
     * @param worker Pointer to a Worker object.
     * @param node Name of the node to be addressed.
     * @param key Name of the ExtrinsicData to be addressed within the node.
     * @param listener Listener function which should be unregistered.
     * \param clientData The listener function will be called with the given
     *        pointer value as parameter. This can be used to invoke a member
     *        function.
     * \returns \c true, on success; \c false otherwise.
     */
    VL_SDK_API bool VL_CALLINGCONVENTION
        vlWorker_RemoveNodeDataExtrinsicDataListener(
            vlWorker_t* worker, const char node[], const char key[],
            vlCallbackExtrinsicDataWrapper listener, void* clientData);

    /*!
     * \internal
     * \ingroup worker
     * \brief Register a listener for named SimilarityTransform events produced by a
     *        certain node.
     *
     * @param worker Pointer to a Worker object.
     * @param node Name of the node to be addressed.
     * @param key Name of the SimilarityTransform to be addressed within the node.
     * @param listener Listener function which will be notified during the
     *        event processing, if a SimilarityTransform event occurred.
     * \param clientData The listener function will be called with the given
     *        pointer value as parameter. This can be used to invoke a member
     *        function.
     * \returns \c true, on success; \c false otherwise.
     */
    VL_SDK_API bool VL_CALLINGCONVENTION
        vlWorker_AddNodeDataSimilarityTransformListener(
            vlWorker_t* worker, const char node[], const char key[],
            vlCallbackSimilarityTransformWrapper listener, void* clientData);

    /*!
     * \internal
     * \ingroup worker
     * \brief Unregister a listener from named SimilarityTransform events produced
     *        by a certain node.
     *
     * @param worker Pointer to a Worker object.
     * @param node Name of the node to be addressed.
     * @param key Name of the SimilarityTransform to be addressed within the node.
     * @param listener Listener function which should be unregistered.
     * \param clientData The listener function will be called with the given
     *        pointer value as parameter. This can be used to invoke a member
     *        function.
     * \returns \c true, on success; \c false otherwise.
     */
    VL_SDK_API bool VL_CALLINGCONVENTION
        vlWorker_RemoveNodeDataSimilarityTransformListener(
            vlWorker_t* worker, const char node[], const char key[],
            vlCallbackSimilarityTransformWrapper listener, void* clientData);

    /*!
     * \internal
     * \ingroup worker
     * \brief Register a listener for named IntrinsicData events produced by a
     *        certain node.
     *
     * @param worker Pointer to a Worker object.
     * @param node Name of the node to be addressed.
     * @param key Name of the IntrinsicData to be addressed within the node.
     * @param listener Listener function which will be notified during the
     *        event processing, if an IntrinsicData event occurred.
     * \param clientData The listener function will be called with the given
     *        pointer value as parameter. This can be used to invoke a member
     *        function.
     * \returns \c true, on success; \c false otherwise.
     */
    VL_SDK_API bool VL_CALLINGCONVENTION
        vlWorker_AddNodeDataIntrinsicDataListener(
            vlWorker_t* worker, const char node[], const char key[],
            vlCallbackIntrinsicDataWrapper listener, void* clientData);

    /*!
     * \internal
     * \ingroup worker
     * \brief Unregister a listener from named IntrinsicData events produced
     *        by a certain node.
     *
     * @param worker Pointer to a Worker object.
     * @param node Name of the node to be addressed.
     * @param key Name of the IntrinsicData to be addressed within the node.
     * @param listener Listener function which should be unregistered.
     * \param clientData The listener function will be called with the given
     *        pointer value as parameter. This can be used to invoke a member
     *        function.
     * \returns \c true, on success; \c false otherwise.
     */
    VL_SDK_API bool VL_CALLINGCONVENTION
        vlWorker_RemoveNodeDataIntrinsicDataListener(
            vlWorker_t* worker, const char node[], const char key[],
            vlCallbackIntrinsicDataWrapper listener, void* clientData);

    /*!
     * \internal
     * \ingroup worker
     * \brief Register a listener for tracking state events produced by a
     *        certain node.
     *
     * NOTICE: This function is experimental and might get removed in future.
     *
     * @param worker Pointer to a Worker object.
     * @param node Name of the node to be addressed.
     * @param listener Listener function which will be notified during the
     *        event processing, if an tracking state event occurred.
     * \param clientData The listener function will be called with the given
     *        pointer value as parameter. This can be used to invoke a member
     *        function.
     * \returns \c true, on success; \c false otherwise.
     */
    VL_SDK_API bool VL_CALLINGCONVENTION
        vlWorker_AddNodeTrackingStateListener(vlWorker_t* worker,
            const char node[], vlCallbackZString listener, void* clientData);

    /*!
     * \internal
     * \ingroup worker
     * \brief Returns the current tracking state of the given node.
     *
     * This function only works, if the Worker was created as synchronous
     * instance (vlNew_SyncWorker).
     * 
     * NOTICE: This function is experimental and might get removed in future.
     *
     * @param worker Pointer to a Worker object.
     * @param node Name of the node to be addressed.
     * \returns A char* containing the current tracking state of the given node
     *          which MUST be freed using vlReleaseBinaryBuffer(),.
     */
    VL_SDK_API char* VL_CALLINGCONVENTION
        vlWorker_GetNodeTrackingStateJsonSync(vlWorker_t* worker, const char node[]);

    /*!
     * \internal
     * \ingroup worker
     * \brief Unregister a listener from tracking state events produced by a
     *        certain node.
     *
     * @param worker Pointer to a Worker object.
     * @param node Name of the node to be addressed.
     * @param listener Listener function which should be unregistered.
     * \param clientData The listener function will be called with the given
     *        pointer value as parameter. This can be used to invoke a member
     *        function.
     * \returns \c true, on success; \c false otherwise.
     */
    VL_SDK_API bool VL_CALLINGCONVENTION
        vlWorker_RemoveNodeTrackingStateListener(vlWorker_t* worker,
            const char node[], vlCallbackZString listener, void* clientData);

    /*!
     * \ingroup worker
     * \brief Removes all listeners.
     *
     * @param worker Pointer to a Worker object.
     * \returns \c true, on success; \c false otherwise.
     */
    VL_SDK_API bool VL_CALLINGCONVENTION vlWorker_ClearListeners(
        vlWorker_t* worker);

    /*!
     * \ingroup worker
     * \brief Calls the registered listeners for the enqueued events.
     *
     * Listeners aren't called immediately from the tracking thread in order to
     * avoid synchronisation problems. Instead this method should be called
     * regularly from the main thread.
     *
     * \param worker Pointer to a Worker object.
     * \returns \c true, on success; \c false otherwise.
     */
    VL_SDK_API bool VL_CALLINGCONVENTION vlWorker_PollEvents(
        vlWorker_t* worker);

    /*!
     * \ingroup worker
     * \brief Waits for enqueued events and calls the registered listeners.
     *
     * Listeners aren't called immediately from the tracking thread in order to
     * avoid synchronisation problems. Instead this method should be called
     * regularly from the main thread.
     *
     * \param worker Pointer to a Worker object.
     * \param timeout Number of milliseconds before stopping to wait. Under
              normal circumstances this shouldn't happen, but in case
              something went wrong, we don't want to wait indefinitely.
     * \returns \c true, on success or \c false, if there was an error while
     *          waiting for events. \c false is also returned, if the tracking
     *          is enabled, but timeout elapsed without an event arriving.
     */
    VL_SDK_API bool VL_CALLINGCONVENTION vlWorker_WaitEvents(
        vlWorker_t* worker, unsigned int timeout);

    /*!
     * \ingroup worker
     * \brief For testing purposed. Don't use!
     */
    VL_SDK_API bool VL_CALLINGCONVENTION vlWorker_Lock(vlWorker_t* worker);

    /*!
     * \ingroup worker
     * \brief For testing purposed. Don't use!
     */
    VL_SDK_API bool VL_CALLINGCONVENTION vlWorker_Unlock(vlWorker_t* worker);

    /*!
     * \defgroup Utils
     *
     * \brief Functions for general purpose and supporting your app.
     *
     * Functions allowing to support your implementation with the visionlibSDK.
     */

    /*!
     * \ingroup Utils
     * \brief Retrieves a file and its data from a given URI.
     *
     * This function tries to retrieve a file synchronously from a given source
     * URI. The URI can point to a file on disk (file:///path) or
     * even to a file on a web server (http://192.0.0.1/file). VisionLib
     * schemes (e.g. project-dir:filename) and custom schemes are also
     * supported.
     *
     * \param uri A zero terminated string containing the URI.
     * \param size A pointer to an unsigned long integer, which will be used
     *        to store the size of the retrieved file.
     * \param options An optional pointer (can be NULL), which describes some
     *        options for download (not used yet).
     * \returns A valid pointer to the memory which MUST be freed using vlReleaseBinaryBuffer(),
     *          on success. Returns \c NULL if an error occurred.
     */
     VL_SDK_API char* VL_CALLINGCONVENTION vlSDKUtil_get(
        const char *uri, unsigned long *size, const char *options=0);

    /*!
     * \ingroup Utils
     * \brief Checks if a file exists at a given URI
     *
     * This function checks if a given file exists synchronously at a given
     * URI. The URI can point to a file on disk (file:///path) or
     * even to a file on a web server (http://192.0.0.1/file). VisionLib
     * schemes (e.g. project-dir:filename) and custom schemes are also
     * supported.
     *
     * \param uri A zero terminated string containing the URI.
     * \returns True if and only if a file exists at the given location.
     */
     VL_SDK_API bool VL_CALLINGCONVENTION vlSDKUtil_fileExists(const char *uri);

     /*!
      * \ingroup Utils
      * \brief Posts or writes data to given URI.
      *
      * This function tries to write a file synchronously to a given URI. An
      * AbstractApplication instance has to be created before calling this
      * function. The URI can point to a file on disk (file:///path) or
      * even to a file on a web server (http://192.0.0.1/file). VisionLib
      * schemes (e.g. data_dir:filename) and custom schemes are also
      * supported.
      *
      * \param uri A zero terminated string containing the URI.
      * \param data A pointer to the data to be written.
      * \param size Number of bytes to be written.
      * \param options An optional pointer (can be NULL), which describes some
      *        options for download (not used yet)
      * \returns \c true, on success; \c false, otherwise.
      */
     VL_SDK_API bool VL_CALLINGCONVENTION vlSDKUtil_set(
        const char* uri, const void* data, unsigned long size,
        const char* options=0);

     /*!
      * \ingroup Utils
      * \brief Creates an internal file scheme relative to the given uri.
      *
      * This function tries to create a new scheme relative to a given URI with
      * the given name. The URI can be an absolute path or contain another
      * scheme (e.g. http://192.0.0.1/some/sub/directory). If a scheme with this name already
      * exists, this command will overwrite the old scheme. 
      *
      * \param name A zero terminated string containing the scheme name.
      * \param uri A zero terminated string containing the scheme URI.
      * \returns \c true, on success; \c false, otherwise.
      */
     VL_SDK_API bool VL_CALLINGCONVENTION
         vlSDKUtil_registerScheme(const char* name, const char* uri);

    /*!
     * \ingroup Utils
     * \brief Generate a temporary file URI.
     *
     * This function creates a filename that can be written to on the local
     * file system of the device. A hint can be given, which will
     * get incorporated into the filename.
     *
     * \param prefName A zero terminated string containing the preferred name,
     *        which can also be NULL.
     * \param newName Buffer for storing the filename as zero terminated string.
     * \param maxSize Size of the buffer.
     * \returns \c true, on success; \c false, otherwise.
     */
     VL_SDK_API bool VL_CALLINGCONVENTION vlSDKUtil_getTempFilename(
        const char* prefName, char *newName, unsigned int maxSize);

     /*!
      * \ingroup Utils
      * \brief Transforms a workspace geometry into a list of points
      *
      * This function parses a workspace defined in json and transforms it into
      * an array of positions (3 float values).
      *
      * \param geometryJson A zero terminated string containing the geometry
      *       definition as json.
      * \param size A pointer to an unsigned long integer, which will be used
      *        to store the number of retrieved positions.
      * \returns A valid pointer to the memory which MUST be freed using vlReleaseBinaryBuffer(),
      *       on success. Returns \c NULL if an error occurred.
      */
     VL_SDK_API char* VL_CALLINGCONVENTION
         vlSDKUtil_getCameraPositionsFromGeometry(const char* geometryJson, unsigned long* size);

     /*!
      * \ingroup Utils
      * \brief Transforms a workspace definition into a list of points
      *
      * This function parses a workspace defined in json and transforms it into
      * an array of positions (3 float values).
      *
      * \param workspaceJson A zero terminated string containing the workspace
      *       definition as json.
      * \param size A pointer to an unsigned long integer, which will be used
      *        to store the number of retrieved positions.
      * \returns A valid pointer to the memory which MUST be freed using vlReleaseBinaryBuffer(),
      *       on success. Returns \c NULL if an error occurred.
      */
     VL_SDK_API char* VL_CALLINGCONVENTION vlSDKUtil_getCameraPositionsFromWorkspaceDefinition(
         const char* workspaceJson,
         unsigned long* size);

     /*!
      * \ingroup Utils
      * \brief Computes the origin transform of a given simple workspace definition.
      *
      * This function parses a workspace defined in json and computes the transform that will
      * be applied to the origin of the simple workspace.
      *
      * \param workspaceJson A zero terminated string containing the simple workspace
      *       definition as json.
      * \param size A pointer to an unsigned long integer, which will contain 1 on success
      *       and zero otherwise.
      * \returns A valid pointer to the memory containing 7 float values tx, ty, tz, rx, ry, rz, rw on success.
      *       It MUST be freed using vlReleaseBinaryBuffer().
      *       Returns \c NULL if an error occurred.
      */
     VL_SDK_API char* VL_CALLINGCONVENTION
         vlSDKUtil_getOriginTransformFromSimpleWorkspaceDefinition(
             const char* workspaceJson,
             unsigned long* size);

     /*!
      * \ingroup Utils
      * \brief Transforms a workspace definition into a list of poses
      *
      * This function parses a workspace defined in json and transforms it into
      * an array of camera transformations (7 float values tx, ty, tz, rx, ry, rz, rw).
      *
      * \param geometryJson A zero terminated string containing the geometry
      *       definition as json.
      * \param size A pointer to an unsigned long integer, which will be used
      *        to store the size of the retrieved file.
      * \returns A valid pointer to the memory which MUST be freed using vlReleaseBinaryBuffer(),
      *       on success. Returns \c NULL if an error occurred.
      */
     VL_SDK_API char* VL_CALLINGCONVENTION
         vlSDKUtil_getCameraTransformsFromWorkspaceDefinition(
             const char* workspaceJson,
             unsigned long* size);

     /*!
      * \ingroup Utils
      * \brief Estimates the quality of a given image as a PosterTracker reference.
      *
      * \param imageWrapper The image in any image format.
      * \returns The quality between 0 (bad) and 1 (good).
      */
     VL_SDK_API double VL_CALLINGCONVENTION
         vlSDKUtil_getPosterQuality(
        vlImageWrapper_t* imageWrapper);

     /*!
      * \ingroup Utils
      * \brief Returns wether or not the system supports external SLAM (ARKit, ARCore, HoloLens).
      *
      * \returns True if and only if external SLAM is supported.
      */
     VL_SDK_API bool VL_CALLINGCONVENTION vlSDKUtil_systemHasExternalSLAM();

    /*!
      * \ingroup Utils
      * \brief Resolves the given URI.
      *
      * This function tries to resolve the given URI. The URI can point to a file
      * on disk (file:///path) or even to a file on a web server
      * (http://192.0.0.1/file). VisionLib schemes and custom schemes are also
      * supported. The result is the physical path of the given URI and will be
      * written into the provided buffer, if it is large enough.
      *
      * \param uri A zero terminated string containing the URI.
      * \param physicalPath Buffer for storing the physical path.
      * \param maxSize The size of the buffer.
      * \returns \c true, on success; \c false otherwise.
      */
     VL_SDK_API bool VL_CALLINGCONVENTION vlSDKUtil_retrievePhysicalPath(
         const char uri[],
         char physicalPath[],
         unsigned int maxSize);

    /*!
    * \ingroup Utils
    * \brief Creates the model hash of the given model into a buffer.
    *
    * \param modelURI A zero terminated string containing the URI of the model.
    * \param modelHash Buffer for storing the hash code of the model.
    * \param maxSize The size of the buffer.
    * \returns \c true, on success; \c false otherwise.
    */
    VL_SDK_API bool VL_CALLINGCONVENTION vlSDKUtil_getModelHash(
        const char modelURI[],
        char modelHash[],
        unsigned int maxSize);

    /*!
    * \ingroup Utils
    * \brief Loads the model for the given uri and returns it in a serialized form.
    * 
    * Both the binary part and the json part have to be released by the client using vlReleaseBinaryBuffer.
    * 
    * \param uri A zero terminated string containing the URI of the model.
    * \param json The json part of the loaded model will be written to the address provided.
    * \param size The size of the binary part of the loaded model will be written to the address provided.
    * \returns A pointer to the binary part of the loaded model.
    */
    VL_SDK_API char* VL_CALLINGCONVENTION
        vlSDKUtil_loadModel(const char* uri, char** json, unsigned long* size);

    /*!
     * \ingroup Utils
     * \brief Writes the model represented by the given json and binary data to the given uri.
     *
     * \param uri A zero terminated string containing the location where the model should be
     * written. \param json A zero terminated string containing the json part of the model. \param
     * data A pointer to the binary part of the model. \param size The size of the binary part of
     * the model. \returns \c true, on success; \c false otherwise.
     */

    VL_SDK_API bool VL_CALLINGCONVENTION vlSDKUtil_writeModel(
        const char* uri,
        const char* json,
        const char* data,
        unsigned long size);

    /*!
     * \ingroup Utils
     * \brief Creates the model hashes of the given model(s) into a buffer.
     *
     * \param modelDescriptionJson The description of the provided modelData as zero terminated JSON
     *        string.
     * \param modelData  Binary data pointer. The lifetime of the memory should be
     *        maintained until the answer has been received.
     * \param modelDataSize The size of the content passed.
     * \param modelHash Buffer for storing the hash code of the model.
     * \param maxSize The size of the buffer.
     * \returns \c true, on success; \c false otherwise.
     */
    VL_SDK_API bool VL_CALLINGCONVENTION vlSDKUtil_getModelHashFromSerializedModel(
        const char modelDescriptionJson[],
        const char modelData[],
        unsigned int modelDataSize,
        char modelHash[],
        unsigned int maxSize);

    /**
     * \ingroup Utils
     * @brief Retrieve the bounding box of a 3D model specified by its URI.
     *
     * This function queries the bounding box of a 3D model defined by the provided URI.
     * The bounding box represents the spatial dimensions of the model, allowing you to
     * determine its extent in three-dimensional space.
     *
     * @param uri The Uniform Resource Identifier (URI) of the 3D model.
     * @param modelBoundingBoxOut Pointer to an array that will hold the bounding box information.
     *                            The bounding box consists of six floating-point values,
     *                            representing (minX, minY, minZ, maxX, maxY, maxZ).
     *                            The function will fill this array with the bounding box data.
     * \param maxSize The size of the buffer.
     *
     * @return Returns true if the bounding box retrieval was successful, and false otherwise.
     *         Possible reasons for failure include invalid URI or inability to access the model.
     *
     * @note The caller is responsible for allocating memory for the `modelBoundingBoxOut` array.
     *       The array must have space for at least six floating-point values.
     *       If the function fails, the content of `modelBoundingBoxOut` is undefined.
     *       It is recommended to check the return value before using the bounding box data.
     *
     * @warning The `uri` parameter should point to a valid and accessible 3D model resource.
     *          Incorrect URIs or inaccessible models may lead to unexpected behavior or errors.
     *          Ensure that the `uri` is properly formatted and the model file is available.
     */
    VL_SDK_API bool VL_CALLINGCONVENTION vlSDKUtil_getModelBoundingBox(
        const char* uri,
        float* modelBoundingBoxOut,
        unsigned int elementCount);

    /*!
     * \ingroup Utils
     * \brief Retrieve the ID of a 3D model specified by its name.
     *
     * \param name The name of a 3D model as zero terminated string.
     * \params modelID Pointer value where the model ID will be stored.
     * \returns \c true, on success; \c false otherwise.
     */
    VL_SDK_API bool VL_CALLINGCONVENTION vlSDKUtil_getModelIDFromName(const char name[], unsigned int* modelID);

#ifdef __cplusplus
}
#endif // __cplusplus

/**@}*/

#endif // VL_SDK_H
