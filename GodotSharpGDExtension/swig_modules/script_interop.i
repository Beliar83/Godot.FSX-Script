%module(directors="1") DotnetScriptInterop

%feature("director") godot::FSXScript;

%include "script_interop/fsharp_method_info.i"
%include "script_interop/fsx_script.i"
%import "godot/string_name.i"
%import "godot/variant.i"

%inline %{
#include "fsx_script_instance.h"

typedef godot::StringName* (*CreateDotnetInstance)(godot::String path, godot::String code);
typedef void (*CallMethod)(godot::StringName* script, godot::StringName name, std::vector<godot::Variant> args, godot::Object instance, godot::Variant* return_val);


void SetDotnetFunctions(CreateDotnetInstance p_create_dotnet_instance, CallMethod p_call_method) {
    godot::FSXScriptInstance::SetDotnetFunctions(p_create_dotnet_instance, p_call_method);
}

typedef godot::StringName* (*CreateDotnetInstance)(godot::String path, godot::String code);
typedef void (*CallMethod)(godot::StringName* script, godot::StringName name, std::vector<godot::Variant> args, godot::Object instance, godot::Variant* return_val);


#include <iostream>

     void test(const char* text) {
         std::cout << text << std::endl;
         WARN_PRINT(text);
     }

void print_script_info(godot::FSXScript* script);

%}