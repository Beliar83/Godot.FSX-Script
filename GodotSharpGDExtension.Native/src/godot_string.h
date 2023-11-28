#pragma once
#include "godot_cpp/core/defs.hpp"
#include "gdextension_interface.h"

extern "C" {

    typedef struct {
        wchar_t* data;
        bool memory_own;
    } GodotString;

    GDE_EXPORT void delete_string(GodotString string);
    GDE_EXPORT const wchar_t* to_string(GodotString *string);
    GDE_EXPORT GodotString convert_string_to_dotnet(GDExtensionTypePtr string);
    GDE_EXPORT GDExtensionTypePtr convert_string_from_godot_string(GodotString string);
    GDE_EXPORT GDExtensionTypePtr convert_string_from_wide_string(const wchar_t* string);
    // GDE_EXPORT GodotString from_string(const wchar_t* string);
}