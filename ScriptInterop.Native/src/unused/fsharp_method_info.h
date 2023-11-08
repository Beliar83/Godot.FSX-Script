#pragma once
#include <vector>
#include "godot_cpp/core/property_info.hpp"

struct FSharpMethodInfo {
    godot::StringName name;
    godot::PropertyInfo return_val;
    uint32_t flags;
    int id = 0;
    std::vector<godot::PropertyInfo> arguments;
    std::vector<godot::Variant> default_arguments;

    godot::Dictionary to_dictionary() const;;
};

