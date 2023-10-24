%{
/* Includes the header in the wrapper code */
#include "godot_cpp/variant/string_name.hpp"
%}

%import "interface.i"
%import "godot_string.i"

namespace godot{
        %extend StringName {
            const char *AsString() {
                return godot::String(*$self).ascii().get_data();
            }
        }

    class StringName {
        public:
            StringName();
            StringName(const StringName &from);
            StringName(const String &from);
            StringName(StringName &&other);
            StringName(const char *from);
            ~StringName();
    };
}