%module(directors="1") DotnetScriptInterop

%feature("director") FSharpScript;

%include "script_interop/fsharp_method_info.i"
%include "script_interop/fsharp_script.i"

%inline %{
#include <iostream>

     void test(const char* text) {
         std::cout << text << std::endl;
         WARN_PRINT(text);
     }

#include "fsharp_script.h"

void print_script_info(FSharpScript* script);

%}