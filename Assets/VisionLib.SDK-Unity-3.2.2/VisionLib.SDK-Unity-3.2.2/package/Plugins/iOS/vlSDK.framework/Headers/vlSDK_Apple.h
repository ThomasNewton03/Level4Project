//
//  vlSDK_Apple.h
//  VisionLib Objective-C Interface
//
//  Created by Timo Engelke on 02.11.16.
//  Copyright © 2016-2018 Visometry GmbH. All rights reserved.
//
// Version 0.4
//

/**
 * \file vlSDK_Apple.h
 * \brief This file contains all abstracted functions of the VisionLib SDK.
 *
 *
 *
 * Include this file for using the VisionLib in your Apple based software.
 *
 * \see http://www.visionlib.com
 */

/**
 * \defgroup VisionLibSDKObjectiveC VisionLib.SDK.Objective-C
 * \brief Objective-C Interface for the vlSDK for Apple devices
 * The Objective-C API allows a simple acces to the functions of the visionLib SDK.
 * The interface consists of a vlFrameListener delegate protocol and a vlSDK object.
 * You can easily initialize the framework by passing a URI to the vlSDK init object.
 * Please have a look at the example and the tutorials on how to use the SDK efficiently.
 *
 */

/**@{*/

#ifndef vlSDK_APPLE_h
#define vlSDK_APPLE_h

#include "vlSDKDef.h"

#import <Foundation/Foundation.h>

#include <TargetConditionals.h>

#if TARGET_OS_IOS
#import <UIKit/UIKit.h>
#import <ARKit/ARKit.h>
#endif

#import <Metal/Metal.h>
#import <CoreImage/CoreImage.h>

#if TARGET_OS_IOS
typedef void(*ARSessionCompletionHandler)(ARSession * _Nullable session);
#endif

/*
@interface vlCameraDevice:NSObject {
@public
    NSString *deviceID;
    NSString *internalID;
    NSString *cameraName;
    NSString *position;
}
@end

@interface vlDeviceInfo:NSObject {
@public
    NSArray<vlCameraDevice *> *availableCameras;
}
 @end
*/

FOUNDATION_EXPORT VL_SDK_API @interface vlTrackingObjectState:NSObject {
@public
    NSString* name;
    NSString* state;
    float quality;
    float _InitInlierRatio;
    int _InitNumOfCorresp;
    float _TrackingInlierRatio;
    int _TrackingNumOfCorresp;
    float _SFHFrameDist;
    int _NumberOfPatternRecognitions;
    int _NumberOfTemplates;
    int _NumberOfTemplatesDynamic;
    int _NumberOfTemplatesStatic;
    int _NumberOfLineModels;
    float _AutoInitSetupProgress;
    int _TrackingImageWidth;
    int _TrackingImageHeight;
    double timeStamp;
}
@end

FOUNDATION_EXPORT VL_SDK_API @interface vlTrackingDeviceState:NSObject {
@public
    NSString* name;
    NSString* _WorldMappingStatus;
}
@end

FOUNDATION_EXPORT VL_SDK_API @interface vlTrackingState:NSObject {
@public
    NSArray<vlTrackingObjectState*>* objects;
    NSArray<vlTrackingDeviceState*>* inputs;
}
@end

/**
 * \brief Properties of models managed by the visionlib.
 * The model properties can be queried by calling requestModelProperties.
 * Each model in the memory, has a corresponding entry of vlModelProperties.#
 * The properties depict stats about how hypothesises are generated in the current state
 * of tracking.
 */
FOUNDATION_EXPORT VL_SDK_API @interface vlModelProperties:NSObject {
@public
    
    /// \brief If the model is expected to be shown.
    bool enabled;

    /// \brief The model hash code for licensing of the model.
    NSString *modelHash;

    /// \brief The name which has either been automatically assigned, or is coming from the model definitions, when loading
    NSString *name;

    /// \brief The URI of the object that has been referenced
    NSString *uri;

    /// \brief If the model will occlude other parts as an invisible part
    bool occluder;
    
    /// \brief Stats of the model regarding the number of mehes
    int subMeshCount;
    
    /// \brief Stats of the model regarding the number of triangles used.
    int triangleCount;
}
/*@property (atomic, assign) bool enabled;
@property (nonatomic, retain) NSString * _Nonnull modelHash;
@property (nonatomic, retain) NSString *_Nonnull name;
@property (nonatomic, assign) bool occluder;
@property (nonatomic, assign) int subMeshCount;
@property (nonatomic, assign) int triangleCount;
@property (nonatomic, retain) NSString *_Nonnull uri;
*/
- (NSString * _Nullable)toString;
@end

FOUNDATION_EXPORT VL_SDK_API @interface VLIssue:NSObject {
@public
    NSString *info;
    int code;
    NSString *message;
}
@end

FOUNDATION_EXPORT VL_SDK_API @interface VLIssues:NSObject {
@public
    NSArray<VLIssue *> *issues;
}
/// returns true if the passed code can be found in the issue list.
-(BOOL)hasCode:(int)code;

@end

/**
 * vlFrameListenerInterface
 * \brief Listener for all callbacks of the visionLib.
 *
 * The vlFrameListenerInterface delegate protocol allows you gaining on frame information
 * about the current tracking state.
 */
FOUNDATION_EXPORT VL_SDK_API @protocol vlFrameListenerInterface <NSObject>

@optional

//

/** @brief Retreives the configured Debug Image
 * @discussion Use this for setting your own memory location to be the image copied in
 * @param width the width of the image
 * @param height the height of the image
 * @param bytesPerPixel the number of bytes occupied per pixel (e.g. 4 for RGBA images)
 */
-(NSMutableData * _Nonnull )onGetDebugImageBufferWithWidth:(int)width andHeight:(int)height andBytesPerPixel:(int)bytesPerPixel;

/** @brief Retreives the configured Debug Image
 * @discussion Overloading this function onGetDebugImageBufferWithWidth should not have been overloaded too.
 * will provide you on every frame with fresh image data
 * @param data The number of bytes occupied per pixel (e.g. 4 for RGBA images)
 * @param width The width of the image
 * @param height The height of the image
 * @param bytesPerPixel the number of bytes occupied per pixel (e.g. 4 for RGBA images)
 */
-(void)onRawDebugImageBuffer:(NSData * _Nonnull)data withWidth:(int)width andHeight:(int)height andBytesPerPixel:(int)bytesPerPixel;

/** @brief Retreives the configured Debug Image as a metal texture
 * @discussion When overloading this function, a metal texture will be passed, when needed to be set or rendered as background texture
 * functions like onGetDebugImageBufferWithWidth and/or onRawImageBuffer will not be called then, even if defined
 * @param texture MTLTexture object
 * @param m A pointer to 16 item matrix holding a rotation matrix of the content to be rotated with
 */
-(void)onMetalDebugImageTexture:(_Nonnull id<MTLTexture>)texture withRotationMatrix:(float * _Nonnull)m;

/** @brief Retreives the configured Debug Image as a CGImageRef
 * @discussion When overloading this function, a CGImageRef will be passed, when needed to be set or rendered as background texture
 * functions like onGetDebugImageBufferWithWidth and/or onRawImageBuffer will not be called then, even if defined
 * @param texture MTLTexture object
 * @param m A pointer to 16 item matrix holding a rotation matrix of the content to be rotated with
 */
-(void)onCGDebugImageRef:(CGImageRef _Nonnull)texture withRotationMatrix:(float * _Nonnull)m;



/** @brief Set your own image buffer to copy the image into
 * @discussion Use either this for setting your own memory location to be the image copied in
 * @param width the width of the image
 * @param height the height of the image
 * @param bytesPerPixel the number of bytes occupied per pixel (e.g. 4 for RGBA images)
 * @returns NSMutableData object that will be filled upon every arriving image
*/
-(NSMutableData * _Nonnull )onGetImageBufferWithWidth:(int)width andHeight:(int)height andBytesPerPixel:(int)bytesPerPixel;

/** @brief Receive a raw image buffer
 * @discussion Overloading this function onGetImageBufferWithWidth should not have been overloaded too.
 * will provide you on every frame with fresh image data
 * @param data The number of bytes occupied per pixel (e.g. 4 for RGBA images)
 * @param width The width of the image
 * @param height The height of the image
 * @param bytesPerPixel the number of bytes occupied per pixel (e.g. 4 for RGBA images)
*/
-(void)onRawImageBuffer:(NSData * _Nonnull)data withWidth:(int)width andHeight:(int)height andBytesPerPixel:(int)bytesPerPixel;

/** @brief Receive a metal image buffer (experimental)
 * @discussion When overloading this function, a metal texture will be passed, when needed to be set or rendered as background texture
 * functions like onGetImageBufferWithWidth and/or onRawImageBuffer will not be called then, even if defined
 * @param texture The number of bytes occupied per pixel (e.g. 4 for RGBA images)
 * @param m A pointer to 16 item matrix holding a rotation matrix of the content to be rotated with
 */
-(void)onMetalImageTexture:(_Nonnull id<MTLTexture>)texture withRotationMatrix:(float * _Nonnull)m;

/** @brief Receive a CGImageRef image buffer
 * @discussion When overloading this function, a CGImageRef will be passed, when needed to be set or rendered as background
 * functions like onGetImageBufferWithWidth and/or onRawImageBuffer will not be called then, even if defined
 * @param texture The number of bytes occupied per pixel (e.g. 4 for RGBA images)
 * @param m A pointer to 16 item matrix holding a rotation matrix of the content to be rotated with
 */
-(void)onCGImageRef:(CGImageRef _Nonnull)texture withRotationMatrix:(float * _Nonnull)m;


///
/** @brief When the tracker has been initialized
 * @discussion Will be called when the tracker has been done loading and is running now or not.
 * It is recommended also using the onIssuesTriggered:andErrors: function for more precise informations about the state of the tracking pipe.
 * @param worked true or false
 */
-(void)onTrackerInitialized:(bool)worked;

/** @brief Will be called when the tracker has been done loading and is running now.
 * @discussion Tracking issues are passed separated by errors and warnings. You should take these very serious. You can review
 * a complete list of possible issues in the documentation.
 *
 * When an error has occured, the tracking will NOT start working in anyway.
 * @param warnings Optional warnings may occured during initialization.
 * @param errors Optional errors may occured during initialization.
 */
-(void)onIssuesTriggered:(VLIssues* _Nullable)warnings andErrors:(VLIssues*  _Nullable)errors;

/** @brief Receiving the camera pose
 * @discussion Overloading this function will pass a model view matrix for the rendering system.
 * @param data A 16 element matrix holding the complete opengl capable model view matrix.
 * @param valid A boolean flag indicating if the pose is valid.
 */
-(void)onExtrinsicData:(float * _Nonnull)data isValid:(bool)valid;

/** @brief Receive the projection matrix
 * @discussion Overloading this function will pass a projection matrix regarding the set near and far, width and height value of your screen.
 * @param data A 16 element matrix holding the complete opengl capable projection matrix.
 */
-(void)onIntrinsicData:(float * _Nonnull)data;

/** @brief Receive the intrinsic data
 * @discussion Overloading this function will pass a projection matrix regarding the set near and far, width and height value of your screen.
 * @param width Width in pixels of the original intrinsics
 * @param height Height in pixels of the original intrinsics
 * @param cx Normed horizontal principal point of the intrinsics [0..1] (usually around 0.5)
 * @param cy Normed vertical principal point of the intrinsics [0..1] (usually around 0.5)
 * @param fx The normalized focal length of the intrinsic camera calibration in x direction.
 * @param fy The normalized focal length of the intrinsic camera calibration in y direction.
 */
-(void)onIntrinsicDataWithWidth:(float)width height:(float)height cx:(float)cx cy:(float)cy fx:(float)fx fy:(float)fy;

/** @brief Receive the internal visionLib Log.
 * @discussion Depending on the configured log level you will receive the internal log messages from the visionlib.
 * You might use these messages during development. Anyway it is NOT recommended using them for the end user nor parsing them.
 * Defined issues should be gathered by using onIssuesTriggered:andErrors:.
 * @param logString A NSString holding the message
 */
-(void)onLog:(NSString * _Nonnull)logString;

/** @brief Receive the pause state.
 * @discussion Will be called, when the tracking pause was issued
 * @param worked A boolean variable stating if the pause command has worked.
 */
-(void)onTrackingPaused:(BOOL)worked;

/** @brief Receive the stepped state.
 * @discussion Will be called, when the SDK has stepped one frame forward. This can be used in conjunction with stepFrame.
 */
-(void)onSteppedFrame;

/** @brief Receive runtime tracking information on each frame
 * @discussion Will pass a dictionary with information about the tracking quality etc into the application only if is tracking.
 * @param state A Dictionary holding informations about the tracking
 *
 * "quality": 0.39441,   // maps to tracking quality for now
 *
 * "state": "tracked", // Can be "tracked", "critical" or "lost"
 *
 * "_InitInlierRatio": 0.731507, // The quality that has been present at the moment of initialization
 *
 * "_InitNumOfCorresp": 365, // The number of correspondences used when initializing
 *
 * "_TrackingInlierRatio": 0.39441, // The current quality
 *
 * "_TrackingNumOfCorresp": 392, // The number of correspondences used actually
 *
 * "_SFHFrameDist": 466.549,  // Actual distance to last key frame
 *
 * "_NumberOfTemplates": 83 // Number of reinitialization key frames learned
 *
 */
-(void)onTrackingInformation:(vlTrackingState* _Nonnull)state;

/** @brief Receive the initial pose as quaternion and translation.
 * @discussion Will be called when getInitPose has been issued, it will return the currently set init pose (in openGL compatible system)
 */
-(void)onInitPose:(float * _Nonnull)t andQ:(float* _Nonnull)q;

/** @brief Receive the initial pose as matrix.
 * @discussion Will be called when getInitPose has been issued, it will return the currently set init pose (in openGL compatible system)
 */
-(void)onInitPoseMatrix:(float * _Nonnull)m;

/** @brief Receive the reset state
 * @discussion Will be called when a reset has occured (hard or soft)
 * @param hard A bool indicating if a hard or soft reset has occured.
 */
-(void)onResetTracking:(BOOL)hard;


///
/** @brief Receive the calibration data
 * @discussion Will be called with a valid calibration, when getResults has been called.
 * This function will only be called, when using the camera calibration configuration.
 * @param json A NSString holding a json structure with the calibration data.
 */
-(void) onCalibrationResults:(NSString * _Nullable)json;


/** @brief Receive the requested attribute
 * @discussion Will be called when an attribute of the pipeline has been requested using getAttributeRequest.
 * @param name A NSString holding the name of the set attribute
 * @param value A NSString holding the value of the set attribute
 */
-(void) onGetAttribute:(NSString *_Nonnull)name withValue:(NSString *_Nonnull)value;


-(void)onModelProperties:(NSArray<vlModelProperties*>* _Nonnull)info;
-(void)onModelRemoved:(NSString * _Nullable)json withError:(NSString * _Nullable)errorJson;
-(void)onRawModelAdded:(NSString * _Nullable)json withError:(NSString * _Nullable)errorJson;
@end


/** @brief Convenient function for using the vlSDK in a simple manner.
 * @discussion The vlSDK wrapper uses the C-Interface and prepares the interface in that way,
 * that it is easy accessable by a macOS or iOS application. Please have a look into the tutorials and examples.
 */
FOUNDATION_EXPORT VL_SDK_API @interface vlSDK : NSObject {
}

#if TARGET_OS_IOS
- (void)getARSession:(_Nonnull ARSessionCompletionHandler)callback;
#endif

/** @brief Initialize a new tracker
 * @discussion Initialize a new tracker with a given URI and the given delegate.
 * @param uri A valid URI pointing to .vl configuration file.
 * @param delegate Pass a vlFrameListenerInterface
 * @returns a vlSDK object id
 */
-(id _Nonnull) initTrackerWithURI:(NSString* _Nonnull)uri andDelegate:(id _Nullable)delegate __attribute((deprecated("Use functions that also set licensePath or licenseData")));

/** @brief Initialize a new tracker
 * @discussion Initialize a new tracker with a given URI and the given delegate.
 * @param uri A valid URI pointing to .vl configuration file.
 * @param licensePath A valid URI pointing to license file.
 * @param delegate Pass a vlFrameListenerInterface
 * @returns a vlSDK object id
 */
-(id _Nonnull) initTrackerWithURI:(NSString* _Nonnull)uri andLicensePath:(NSString* _Nonnull)licensePath andDelegate:(id _Nullable)delegate;

/** @brief Initialize a new tracker
 * @discussion Initialize a new tracker with a given URI and the given delegate.
 * @param uri A valid URI pointing to .vl configuration file.
 * @param licenseData The content of a license file.
 * @param delegate Pass a vlFrameListenerInterface
 * @returns a vlSDK object id
 */
-(id _Nonnull) initTrackerWithURI:(NSString* _Nonnull)uri andLicenseData:(NSString* _Nonnull)licenseData andDelegate:(id _Nullable)delegate;

/** @brief Initialize a new tracker
 * @discussion Initialize a new tracker with a given URI and the given delegate.
 * @param uri A valid URI pointing to .vl configuration file.
 * @param delegate Pass a vlFrameListenerInterface
 * @param options A dictionary with the following potential options:
 * cameraDatabaseURI: An uri holding your custom cameraCalibration database.
 * targetFPS: An unsigned integer holding the frames per seconds to be used for updates.
 * @returns a vlSDK object id
 */
-(id _Nonnull) initTrackerWithURI:(NSString* _Nonnull)uri andDelegate:(_Nullable id)delegate withOptions:(NSDictionary * _Nullable)options __attribute((deprecated("Use functions that also set licensePath or licenseData")));

/** @brief Initialize a new tracker
 * @discussion Initialize a new tracker with a given URI and the given delegate.
 * @param uri A valid URI pointing to .vl configuration file.
 * @param licensePath A valid URI pointing to license file.
 * @param delegate Pass a vlFrameListenerInterface
 * @param options A dictionary with the following potential options:
 * cameraDatabaseURI: An uri holding your custom cameraCalibration database.
 * targetFPS: An unsigned integer holding the frames per seconds to be used for updates.
 * @returns a vlSDK object id
 */
-(id _Nonnull) initTrackerWithURI:(NSString* _Nonnull)uri andLicensePath:(NSString* _Nonnull)licensePath andDelegate:(_Nullable id)delegate withOptions:(NSDictionary * _Nullable)options;

/** @brief Initialize a new tracker
 * @discussion Initialize a new tracker with a given URI and the given delegate.
 * @param uri A valid URI pointing to .vl configuration file.
 * @param licenseData The content of a license file.
 * @param delegate Pass a vlFrameListenerInterface
 * @param options A dictionary with the following potential options:
 * cameraDatabaseURI: An uri holding your custom cameraCalibration database.
 * targetFPS: An unsigned integer holding the frames per seconds to be used for updates.
 * @returns a vlSDK object id
 */
-(id _Nonnull) initTrackerWithURI:(NSString* _Nonnull)uri andLicenseData:(NSString* _Nonnull)licenseData andDelegate:(_Nullable id)delegate withOptions:(NSDictionary * _Nullable)options;


/** @brief Shut down everything
 * @discussion Shut down everything... Please be sure to not call anything in the sdk while calling dealloc
 */
- (void) dealloc;

/** @brief Process one frame
 * @discussion This method initiates the processing of the command queue and
 initiates the resolution of the delegates methode in the same thread as called.
 
 A common practice is calling this function during the renderloop allowing your delegate methods to be called.
 */
-(void) process;

/** @brief Step one frame
 * @discussion Usually this methode is not needed when running real time applications.
 Anyway you might step single frames using this function.
 */
-(void)step;

/** @brief Query pause state
 * @discussion Ask the SDK if it is paused right now. If you have been calling \ref pause:
 * @returns TRUE if paused
 */
-(BOOL) isPaused;


/** @brief Set pause state
 * @discussion Set the actual state to pause
 * @param enable pause or not
 */
-(void) pause:(BOOL)enable;

/** @brief Run the tracking pipeline
 * @discussion Run the tracking pipeline when it has been loaded
 */
-(void) run;

/** @brief Stop the tracking pipeline
 * @discussion Stop the tracking pipeline
 */
-(void) stop;
-(void) start;

#if !TARGET_OS_IOS

/** @brief (macOS only) Set the screen extends of the rendering area to be used.
 * @discussion This function should usually be called by a macOS application when a window resize happens.
 */
-(void) windowResized:(CGSize)size;

#endif

/** @brief Setting the log level for debugging
 * @discussion Set the log level for logging of the visionLib. It is strongly recommended only using the log if you experience rare problems.
 * @param level can be a number from 0-5, while 0 means LOG and 5 is mostly debug.
 */
-(void) setLogLevel:(int)level;

/** @brief Set the frames per second to be processed internally.
 * @discussion Set frames per second to process. A value of 0 will make the vlSDK run as fast as it can (not recommended).
 * @param fps integer holding the frames per second to run with.
 */
-(void) setFPS:(unsigned int)fps;

/** @brief Set the near and far plane.
 * @discussion Setting the near and far plane will influence the generated intrinsic data (projection matrix) passed to the delegate.
 * You should set this to a useful value, since it defines the frustum. If you cannot see your rendering, please start here!
 * @param near Near value as float
 * @param far Far value as float
 */
-(void) setNearPlane:(float)near andFarPlane:(float)far;

/** @brief Set initial pose with quaternion and translation
 * @discussion Set initial pose in OpenCV coordinate system from translation and quaternions
 * @param t Translation vector
 * @param q Quaternion holding the rotation
 */
-(void) setInitPose:(float * _Nonnull)t andQ:(float* _Nonnull)q;

/** @brief Set initial pose with a matrix
 * @discussion Set initial pose in openGL/Metal coordinate system from a model view matrix
 * @param m A matrix holding 16 elements
 */
-(void) setInitPoseFromMatrix:(float * _Nonnull)m;

///
/** @brief Treating camera poses
 * @discussion Define how the callbacks extrinsic data should be treated. Enabling this option is recommended when working with OpenGL or Metal rendering systems.
 * Like this you can easily directly pass the matrix of onExtrinsic: and onIntrinsic: to the rendering engine. Call this function right after calling the init.
 * @param invert Set this to true in order to invert the camera pose automatically for you.
 */
-(void) configureExtrinsicCameraInverted:(BOOL)invert;

/** @brief Do a soft reset
 * @discussion A soft reset enables the actual poster or model tracker pipe to get back to its initial pose.
 */
-(void) resetSoft;

/** @brief Do a hard reset
 * @discussion A hard reset enables the actual poster or model tracker pipe to get back to its initial pose.
 * It also releases all recorded features helping for reinitialization and stabilization. Use this for
 * starting over with the tracking as nothing happend before.
 */
-(void) resetHard;

/** @brief Trigger to receive an initial pose
 * @discussion Trigger to receive an initial pose-
 */
-(void) getInitPose;

/** @brief Receive last extrinsic data
 * @discussion Receives a matrix with the last valid extrinsic data
 * @param m Fills an array with expected 16 float values
 * @returns true if the pose has been valid
 */
-(BOOL) getLastExtrinsic:(float * _Nonnull)m;

/** @brief Enable debug images
 * @discussion You might enable receiving of ONE debug image along with the actual view image.
 * In more complex applications this can help debugging.
 * Enable the debug image which will call onGetDebugImageBufferWithWidth and companion methods if implemented in the delegate.
  * @returns True if the image could be enabled.
 */
-(BOOL) enableDebugImage;

/** @brief Disable debug images
  * @returns True if the image could be disabled.
 */
-(BOOL) disableDebugImage;

#if TARGET_OS_IOS

/** @brief (iOS) Get last image as UIImage
 * @discussion Get the most actual camera image. It is not recommended polling this function for receiving a realtime stream of the image.
 */
-(UIImage * _Nullable)getLastImage;
#else

/** @brief (macOS) Get last image as NSImage
 * @discussion Get the most actual camera image. It is not recommended polling this function for receiving a realtime stream of the image.
 */
-(NSImage * _Nullable) getLastImage;

#endif

/** @brief Set Attribute
 * @discussion Set a certain attribute in the pipeline
 * This only works, when the actual pipe has the attribute.
 * @param attribute A NSString holding the attributename.
 * @param value A float value.
 */
-(void)setAttributeCommand:(NSString * _Nonnull)attribute withFloatValue:(float)value;

/** @brief Set Attribute
 * @discussion Set a certain attribute in the pipeline
 * This only works, when the actual pipe has the attribute.
 * @param attribute An NSString holding the attributename.
 * @param value A string value. Note: Also float values can be set and will be converted when needed.
 */
-(void)setAttributeCommand:(NSString * _Nonnull)attribute withStringValue:(NSString * _Nonnull)value;

/** @brief Request an Attribute
 * @discussion Request an attribute from the actual running pipeline
 * This only works, when the actual pipe has the attribute.
 * @param attribute An NSString holding the attributename.
 */
-(void)getAttributeRequest:(NSString * _Nonnull)attribute;


/** @brief Send a calibration command
 * @discussion Send a calibration command as stated in the documentation.
 * This only works, when the actual pipe is a camera calibration.
 * @param command An NSString holding the command.
 */
-(void)setCalibrationCommand:(NSString * _Nonnull)command;

/** @brief Write the actual camera calibration to some URI
 * @discussion Writes the actual camera calibration to a given URI.
 * This only works, when the actual pipe is a camera calibration.
 * @param uri An NSString holding uri to write to.
 */
-(void)writeCalibrationDB:(NSString * _Nonnull)uri;

/** @brief Get type of tracker
 * @discussion The vlSDK uses different pipes for different purposes. When loading a .vl file
 * a certain pipe will be initialized. (e.g. modelTracker, posterTracker etc...)
 * This function will allow you to receive the actual configured pipe name.
 * @returns An NSString holding the actual tracker type loaded. Anyway it will pass NULL if no tracker has been loaded yet or it has failed during loading.
 */
-(NSString * _Nullable)getTrackerType;

/** @brief Get type of device
 * @discussion The vlSDK can use different image sources. When loading a .vl file
 * a certain image source will be initialized. (e.g. camera, image sequence etc...)
 * This function will allow you to receive the actual configured image source name (device).
 * @returns An NSString holding the actual device type loaded. Anyway it will pass NULL if no tracker has been loaded yet or it has failed during loading.
 */
-(NSString * _Nullable)getDeviceType;

/** @brief Write recorded init templates to uri
 * @discussion While tracking using the modelTracker pipeline init templates are
 * recorded for allowing to reinitialize the scene. It can be useful saving these templates
 * using this function to an uri.
 *
 * If an empty string is passed, the uri will be local-storage-dir:/VisionLib/InitData_XXXXX.binz, while XXXX depicts a timestamp.
 * You can load the init data as well using either the initDataURI parameter in the tracking configuration file or
 * passing the filename using url parameters, when loading the .vl file.
 *
 * If you pass a full filename, with .binz at the end. You can explicitly save the init data under this filename. This is the recommended way. You can load the init data again using the loadInitData function
 */
-(void)writeInitData:(NSString *_Nonnull)uri;




/** @brief Load recorded init templates from uri
 * @discussion While tracking using the modelTracker pipeline init templates are
 * recorded for allowing to reinitialize the scene. It can be useful loading these templates
 * during runtime from an uri.
 *
 * Currently the format must be a filename with an ending .binz.
 */
-(void)readInitData:(NSString *_Nonnull)uri;

/** @brief Reset all init data
 * @discussion You might remove statically loaded initialization data. Data which has currently already be recorded will not be deleted. You may call a resetHard in order to remove these as well.
 */
-(void)resetInitData;


/** @brief Sets a boolean property of a model (BETA)
 * @discussion You might enable/disable, occlude an objects using this function. The general calling mechanism is providing a nameURI which looks like this `name:YOUROBJECTNAME`. Possible properties are `enabled`, `occluder`
 */
-(void)setModel:(NSString *_Nonnull)name property:(NSString *_Nonnull)property state:(bool)enable;



/** @brief Request actual scene model properties (BETA)
 * @discussion Will call onModelProperties on the delegate.
 */
-(void)requestModelProperties;

/** @brief Request model data of the object (BETA)
 * @discussion THIS IS STILL BETA AND IS MATTER OF CHANGE
 */
-(void)requestModelData:(NSString*_Nonnull)name withOptions:(NSString *_Nonnull)options;


/** @brief Remove a certain model with a name uri (BETA)
 * @discussion Use `name:YOUROBJECTNAME` or `id:YOUROBJECTID`. This will in return call onModelRemoved
 */
-(void)removeModel:(NSString*_Nonnull)nameURI;

/** @brief Add a certain model with raw data (BETA)
 * @discussion You can inject model(s) with raw triangles and normals into the visionlib and use
 them for tracking.
 *
 * The string to be passed might look like this:
 * ```
 * [
 {
 "name" : "myModel",
 "shape" :
    {
        "binaryOffset" : 0,
        "name" : "dummy",
        "normalCount" : 4,
        "triangleIndexCount" : 6,
        "vertexCount" : 4
    },
    "transform" :
        {
            "s" :
            [
            0.10000000000000001,
            0.10000000000000001,
            0.10000000000000001
            ]
        }
    }
 ]
 * ```
 *
 * The binary structure might have this format: [vertex Nx3xfloat][triangleIndices:
 MxUint32][normals: Ox3xfloat]
 *
 * The function will call onRawModelAdded:withError:
 * *NOTE* This structure might still change.
 */
-(BOOL)addRawModelWithStruct:(NSString *_Nonnull)struc andData:(NSData*_Nonnull)data;


-(void)pushJsonAndBinaryCommand:(NSString *_Nonnull)struc andData:(NSData*_Nonnull)data;
-(void)pushJsonCommand:(NSString *_Nonnull)json;
-(void)writeLineModelData:(NSString *_Nonnull)uri;
-(void)readLineModelData:(NSString *_Nonnull)uri;

@end


#endif /* vlSDK_APPLE_h */

/**@}*/
