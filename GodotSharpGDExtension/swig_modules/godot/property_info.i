%{
#include "godot_cpp/core/property_info.hpp"
%}

%import "variant.i"
%import "godot_string.i"
%import "string_name.i"
%import "global_constants.i"

namespace godot {

    struct PropertyInfo {
        Variant::Type type = Variant::NIL;
        StringName name;
        StringName class_name;
        uint32_t hint = 0;
        String hint_string;
        uint32_t usage = 7;

        PropertyInfo() = default;

        PropertyInfo(Variant::Type p_type, const StringName &p_name, PropertyHint p_hint = PROPERTY_HINT_NONE, const String &p_hint_string = "", uint32_t p_usage = PROPERTY_USAGE_DEFAULT, const StringName &p_class_name = "");
    };

} // namespace godot