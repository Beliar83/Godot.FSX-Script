#include <gdextension_interface.h>
#include <godot_cpp/core/defs.hpp>
//#include <godot_cpp/core/class_db.hpp>
#include <godot_cpp/godot.hpp>
#include "godot_cpp/core/error_macros.hpp"



void initialize_fsharp_extension_module(godot::ModuleInitializationLevel p_level) {
    WARN_PRINT("Test");
}

extern "C" {
GDExtensionBool GDE_EXPORT script_interop_init(GDExtensionInterfaceGetProcAddress p_get_proc_address, GDExtensionClassLibraryPtr p_library, GDExtensionInitialization *r_initialization) {
    godot::GDExtensionBinding::InitObject main_init_obj(p_get_proc_address, p_library, r_initialization);
    main_init_obj.register_initializer(initialize_fsharp_extension_module);
    main_init_obj.register_terminator(initialize_fsharp_extension_module);
    main_init_obj.set_minimum_library_initialization_level(godot::MODULE_INITIALIZATION_LEVEL_CORE);
    return main_init_obj.init();
}
}

#include "fsharp_script.h"

void print_script_info(FSharpScript* script) {
    WARN_PRINT("Printing script info");
    for (auto method_info : script->_getMethods()) {
        WARN_PRINT(method_info.Name);
    }
}

