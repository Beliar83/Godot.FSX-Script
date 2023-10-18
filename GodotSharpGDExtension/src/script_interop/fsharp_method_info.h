#pragma once
#include <vector>
#include "godot_cpp/core/property_info.hpp"

struct FSharpMethodInfo {
    godot::String Name;
    std::vector<godot::PropertyInfo> args;
};

