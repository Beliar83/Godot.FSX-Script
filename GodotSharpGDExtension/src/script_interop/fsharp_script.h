#pragma once
#include "godot_cpp/classes/script_extension.hpp"
#include "fsharp_method_info.h"
#include <vector>

class FSharpScript {
public:
    virtual ~FSharpScript() = default;
    virtual std::vector<FSharpMethodInfo> _getMethods() {
        return {};
    }
};

