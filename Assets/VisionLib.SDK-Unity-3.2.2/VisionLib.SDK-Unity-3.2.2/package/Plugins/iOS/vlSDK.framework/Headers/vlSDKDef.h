// Copyright (c) Visometry GmbH.

#ifndef VL_SDK_DEF_H
#define VL_SDK_DEF_H

#if defined _WIN32 || defined __CYGWIN__
    #define VL_SDK_DLL_IMPORT __declspec(dllimport)
    #define VL_SDK_DLL_EXPORT __declspec(dllexport)
#else
    #if __GNUC__ >= 4
        #define VL_SDK_DLL_IMPORT __attribute__ ((visibility ("default")))
        #define VL_SDK_DLL_EXPORT __attribute__ ((visibility ("default")))
    #else
        #define VL_SDK_DLL_IMPORT
        #define VL_SDK_DLL_EXPORT
    #endif
#endif

#ifdef VIS_BUILD_SDK
    // Building shared library
    #define VL_SDK_API VL_SDK_DLL_EXPORT
#else
    // Using shared library
    #define VL_SDK_API VL_SDK_DLL_IMPORT
#endif

#ifdef _WIN32
    #define VL_CALLINGCONVENTION __stdcall
#else
    #define VL_CALLINGCONVENTION
#endif

#endif // VL_SDK_DEF_H
