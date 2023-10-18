cmake_minimum_required(VERSION 3.26)
project(fsharp_extension)

set(CMAKE_CXX_STANDARD 17)

#add_definitions(-DWINDOWS_ENABLED)
add_compile_definitions(HOSTFXR_PATH="Test")

include_directories(src)
include_directories(godot-cpp/gdextension)
include_directories(godot-cpp/gen/include)
include_directories(godot-cpp/include)
include_directories(third_party)

add_library(fsharp_extension
        src/register_types.cpp
        src/nativehost.cpp
        src/native_calls.h
        src/dotnet_api.cpp
        src/swig/godot_sharp_swig_wrap.cxx
        src/swig/script_interop_wrap.cxx
        src/script_interop/fsharp_script.cpp
        src/script_interop/fsharp_method_info.cpp
        src/script_interop/script_interop.cpp)
