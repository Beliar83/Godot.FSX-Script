%{
#include "script_interop/fsx_script.h"
#include "godot_cpp/classes/object.hpp"
%}

%include <std_vector.i>
%template(MethodInfoVector) std::vector<FSharpMethodInfo>;
%import "../godot/ref.i"
%template (FSXScriptRef) godot::Ref<godot::FSXScript>;

%import "../godot/object.i"

%rename(GetMethods) _get_methods;
%rename(CallMethod) _call_method;
%rename(LoadSourceCode) _load_source_code;
namespace godot {
        class FSXScript {
        };
}