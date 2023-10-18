%{
#include "fsharp_method_info.h"
%}

%import "../godot/godot_string.i"
%import "../godot/property_info.i"
%include <std_vector.i>

%template(PropertyInfoVector) std::vector<godot::PropertyInfo>;

struct FSharpMethodInfo {
    godot::String Name;
    std::vector<godot::PropertyInfo> args;
};