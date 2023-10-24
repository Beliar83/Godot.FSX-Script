%{
#include "godot_cpp/classes/object.hpp"
%}

%include <std_vector.i>
%import "../godot/array.i"
%import "../godot/property_info.i"

%template(PropertyInfoVector) std::vector<godot::PropertyInfo>;
%template(VariantVector) std::vector<godot::Variant>;


namespace godot {
            struct MethodInfo {
                %rename(Name) name;
                StringName name;
                %rename(ReturnVal) return_val;
                PropertyInfo return_val;
                %rename(Flags) flags;
                uint32_t flags;
                %rename(Id) id;
                int id = 0;
                %rename(Arguments) arguments;
                std::vector<PropertyInfo> arguments;
                %rename(DefaultArguments) default_arguments;
                std::vector<Variant> default_arguments;
            };
}