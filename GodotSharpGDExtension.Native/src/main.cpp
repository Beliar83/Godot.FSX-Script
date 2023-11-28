#include <filesystem>

#include <gdextension_interface.h>
#include <godot_cpp/core/defs.hpp>


#include "main.h"
//#include "dotnet_api.h"
#include "godot_cpp/variant/utility_functions.hpp"
#include "godot_cpp/godot.hpp"

//typedef struct init_info {
//    GDExtensionInterfaceGetProcAddress get_proc_address;
//    GDExtensionClassLibraryPtr library;
//    GDExtensionInitialization initialization;
//};

void initialize_godot_sharp_gdextension_extension_module(godot::ModuleInitializationLevel p_level) {
//    for (auto initialization : initializations) {
//        initialization.initialize(static_cast<GDExtensionInitializationLevel>(p_level));
//    }
}

void uninitialize_godot_sharp_gdextension_extension_module(godot::ModuleInitializationLevel p_level) {
//    for (auto initialization : initializations) {
//        initialization.uninitialize(static_cast<GDExtensionInitializationLevel>(p_level));
//    }
}

extern "C" {
    GDExtensionBool GDE_EXPORT godot_sharp_gdextension_init(GDExtensionInterfaceGetProcAddress p_get_proc_address, GDExtensionClassLibraryPtr p_library, GDExtensionInitialization *r_initialization) {
        godot::GDExtensionBinding::InitObject main_init_obj(p_get_proc_address, p_library, r_initialization);
        main_init_obj.register_initializer(initialize_godot_sharp_gdextension_extension_module);
        main_init_obj.register_terminator(uninitialize_godot_sharp_gdextension_extension_module);
        main_init_obj.set_minimum_library_initialization_level(godot::MODULE_INITIALIZATION_LEVEL_CORE);
        return main_init_obj.init();
    }
}
