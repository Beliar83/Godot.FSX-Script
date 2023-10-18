#include <filesystem>
#include <string>

#include "register_types.h"
#include <gdextension_interface.h>
#include <godot_cpp/core/defs.hpp>
#include <godot_cpp/core/class_db.hpp>
#include <godot_cpp/godot.hpp>
#include <sstream>
#include "nativehost.h"
#include "dotnet_api.h"
#include "godot_cpp/classes/gd_extension.hpp"
#include "godot_cpp/classes/engine.hpp"

using namespace godot;

//LocalVector<extension_data> extension_datas;
bool initialized = false;
GDExtension* extension;

void initialize_fsharp_extension_module(ModuleInitializationLevel p_level) {
    switch (p_level) {
        case godot::MODULE_INITIALIZATION_LEVEL_SCENE:
        {
            if (!initialized) {
                bind();
                initialized = true;
            }
            break;
        }
        case godot::MODULE_INITIALIZATION_LEVEL_CORE:
        {
            extension = memnew(GDExtension);
#ifdef _WIN32
            extension->open_library(std::filesystem::current_path().append("bin/fsharp.dll").c_str(), "script_interop_init");
#else
            extension->open_library(std::filesystem::current_path().append("bin/libfsharp.so").c_str(), "script_interop_init");
#endif
            extension->initialize_library(godot::GDExtension::INITIALIZATION_LEVEL_CORE);
            break;
        }
        case MODULE_INITIALIZATION_LEVEL_SERVERS:
        case MODULE_INITIALIZATION_LEVEL_EDITOR:
            break;
    }
//    for (auto extension_data: extension_datas) {
//        extension_data.initialize(p_level);
//    }
}

void uninitialize_fsharp_extension_module(ModuleInitializationLevel p_level) {
    switch (p_level) {

        case MODULE_INITIALIZATION_LEVEL_CORE:
            extension->close_library();
            memdelete(extension);
            extension = nullptr;
            break;
        case MODULE_INITIALIZATION_LEVEL_SERVERS:
        case MODULE_INITIALIZATION_LEVEL_SCENE:
        case MODULE_INITIALIZATION_LEVEL_EDITOR:
            break;
    }

//    for (auto extension_data: extension_datas) {
//        extension_data.uninitialize(p_level);
//    }
}

extern "C" {
    GDExtensionBool GDE_EXPORT fsharp_extension_init(GDExtensionInterfaceGetProcAddress p_get_proc_address, GDExtensionClassLibraryPtr p_library, GDExtensionInitialization *r_initialization) {
        godot::GDExtensionBinding::InitObject main_init_obj(p_get_proc_address, p_library, r_initialization);
        main_init_obj.register_initializer(initialize_fsharp_extension_module);
        main_init_obj.register_terminator(uninitialize_fsharp_extension_module);
        main_init_obj.set_minimum_library_initialization_level(MODULE_INITIALIZATION_LEVEL_CORE);
        return main_init_obj.init();
//        GDExtensionBool init = main_init_obj.init();
//        WARN_PRINT(godot::Variant(init));
//        return init;

//        auto ret = bind(p_get_proc_address, p_library, r_initialization);
//
//
//        if (ret != 1) {
//            godot::GDExtensionBinding::InitObject main_init_obj(p_get_proc_address, p_library, r_initialization);
//            main_init_obj.register_initializer(initialize_fsharp_extension_module);
//            main_init_obj.register_terminator(uninitialize_fsharp_extension_module);
//            main_init_obj.set_minimum_library_initialization_level(MODULE_INITIALIZATION_LEVEL_CORE);
//            return main_init_obj.init();
//        }
//        return ret;
    }
}
