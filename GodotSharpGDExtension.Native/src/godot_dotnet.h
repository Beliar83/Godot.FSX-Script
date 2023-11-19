#pragma once

#include <godot_cpp/godot.hpp>
#include <vector>

wchar_t* convert_string_to_dotnet(GDExtensionTypePtr string);

GDExtensionTypePtr convert_string_from_dotnet(const wchar_t* string);

GDExtensionBool convert_bool_from_dotnet(bool value);

bool convert_bool_to_dotnet(GDExtensionBool value);


