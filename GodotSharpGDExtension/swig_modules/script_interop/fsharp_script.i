%{
#include "fsharp_script.h"
#include "fsharp_method_info.h"
%}

%import "../godot/object.i"
%import "fsharp_method_info.i"

%include <std_vector.i>
%template(MethodInfoVector) std::vector<FSharpMethodInfo>;


class FSharpScript {
    public:
        virtual ~FSharpScript() = default;
        virtual std::vector<FSharpMethodInfo> _getMethods();

};