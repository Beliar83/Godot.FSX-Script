#pragma once

#include <godot_string.h>
#include <godot_cpp/godot.hpp>

typedef struct {
    GDExtensionTypePtr pointer;
} GodotType;

GDExtensionBool convert_bool_from_dotnet(bool value);

bool convert_bool_to_dotnet(GDExtensionBool value);


