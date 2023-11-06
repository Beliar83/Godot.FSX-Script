// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#ifndef __NETHOST_H__
#define __NETHOST_H__

#include <stddef.h>

#ifdef _WIN32
    #ifdef NETHOST_EXPORT
        #define NETHOST_API __declspec(dllexport)
    #else
        // Consuming the nethost as a static library
        // Shouldn't export attempt to dllimport.
        #ifdef NETHOST_USE_AS_STATIC
            #define NETHOST_API
        #else
            #define NETHOST_API __declspec(dllimport)
        #endif
    #endif

    #define NETHOST_CALLTYPE __stdcall
    #ifdef _WCHAR_T_DEFINED
        typedef wchar_t char_t;
    #else
        typedef unsigned short char_t;
    #endif
#else
    #ifdef NETHOST_EXPORT
        #define NETHOST_API __attribute__((__visibility__("default")))
    #else
        #define NETHOST_API
    #endif

    #define NETHOST_CALLTYPE
    typedef char char_t;
#endif

#ifdef __cplusplus
extern "C" {
#endif

// Parameters for get_hostfxr_path
//
// Fields:
//   size
//     Size of the struct. This is used for versioning.
//
//   assembly_path
//     Path to the component's assembly.
//     If specified, hostfxr is located as if the assembly_path is the apphost
//
//   dotnet_root
//     Path to directory containing the dotnet executable.
//     If specified, hostfxr is located as if an application is started using
//     'dotnet app.dll', which means it will be searched for under the dotnet_root
//     path and the assembly_path is ignored.
//
struct get_hostfxr_parameters {
    size_t size;
    const char_t *assembly_path;
    const char_t *dotnet_root;
};

#ifdef __cplusplus
} // extern "C"
#endif

#endif // __NETHOST_H__
