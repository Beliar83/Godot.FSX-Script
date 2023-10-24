%{
#include "script_interop/fsharp_method_info.h"
%}

%import "../godot/godot_string.i"
%import "../godot/property_info.i"
%include <std_vector.i>

%template(VariantVector) std::vector<godot::Variant>;


%template(PropertyInfoVector) std::vector<godot::PropertyInfo>;

struct FSharpMethodInfo {
    godot::StringName name;
    godot::PropertyInfo return_val;
    uint32_t flags;
    int id;
    std::vector<godot::PropertyInfo> arguments;
    std::vector<godot::Variant> default_arguments;
};