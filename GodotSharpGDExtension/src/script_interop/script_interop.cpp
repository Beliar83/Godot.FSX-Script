#include <gdextension_interface.h>
#include <godot_cpp/core/defs.hpp>
//#include <godot_cpp/core/class_db.hpp>
#include <godot_cpp/godot.hpp>
#include "godot_cpp/core/error_macros.hpp"
#include "script_interop/fsx_script_language.h"
#include "script_interop/fsx_script.h"
#include "script_interop/fsx_resource_format_loader.h"
#include "script_interop/fsx_resource_format_saver.h"
#include "godot_cpp/classes/resource_saver.hpp"
#include "godot_cpp/classes/resource_loader.hpp"
#include "godot_cpp/classes/engine.hpp"
#include "godot_cpp/variant/utility_functions.hpp"

using namespace godot;

FSXScriptLanguage *language = nullptr;
FSXResourceFormatSaver *fsxResourceFormatSaver = nullptr;
FSXResourceFormatLoader *fsxResourceFormatLoader = nullptr;

void initialize_fsharp_extension_module(godot::ModuleInitializationLevel p_level) {
    switch (p_level) {
        case MODULE_INITIALIZATION_LEVEL_SCENE:
            godot::UtilityFunctions::print("FSXScriptLanguage");
            ClassDB::register_class<FSXScriptLanguage>();
            godot::UtilityFunctions::print("FSXScript");
            ClassDB::register_class<FSXScript>();
            godot::UtilityFunctions::print("FSXResourceFormatSaver");
            ClassDB::register_class<FSXResourceFormatSaver>();
            godot::UtilityFunctions::print("FSXResourceFormatLoader");
            ClassDB::register_class<FSXResourceFormatLoader>();
            godot::UtilityFunctions::print("create language");
            language = memnew(FSXScriptLanguage);
            Engine::get_singleton()->register_singleton("FSXScriptLanguage", language);
            Engine::get_singleton()->register_script_language(language);

            godot::UtilityFunctions::print("create saver");
            fsxResourceFormatSaver = memnew(FSXResourceFormatSaver);
            Engine::get_singleton()->register_singleton("FSXResourceFormatSaver", fsxResourceFormatSaver);
            ResourceSaver::get_singleton()->add_resource_format_saver(fsxResourceFormatSaver);

            godot::UtilityFunctions::print("create loader");
            fsxResourceFormatLoader = memnew(FSXResourceFormatLoader);
            Engine::get_singleton()->register_singleton("FSXResourceFormatLoader", fsxResourceFormatLoader);
            ResourceLoader::get_singleton()->add_resource_format_loader(fsxResourceFormatLoader);
            break;
        case MODULE_INITIALIZATION_LEVEL_CORE:
        case MODULE_INITIALIZATION_LEVEL_SERVERS:
        case MODULE_INITIALIZATION_LEVEL_EDITOR:
            break;
    }



}

void unitialize_fsharp_extension_module(godot::ModuleInitializationLevel p_level) {
    switch (p_level) {
        case MODULE_INITIALIZATION_LEVEL_SCENE:
            Engine::get_singleton()->unregister_singleton("FSXScriptLanguage");
            Engine::get_singleton()->unregister_script_language(language);
            memdelete(language);

            Engine().unregister_singleton("FSXResourceFormatSaver");
            memdelete(fsxResourceFormatSaver);

            Engine().unregister_singleton("FSXResourceFormatLoader");
            memdelete(fsxResourceFormatLoader);
            break;
        case MODULE_INITIALIZATION_LEVEL_CORE:
        case MODULE_INITIALIZATION_LEVEL_SERVERS:
        case MODULE_INITIALIZATION_LEVEL_EDITOR:
            break;
    }
}

extern "C" {
GDExtensionBool GDE_EXPORT script_interop_init(GDExtensionInterfaceGetProcAddress p_get_proc_address, GDExtensionClassLibraryPtr p_library, GDExtensionInitialization *r_initialization) {
    godot::GDExtensionBinding::InitObject main_init_obj(p_get_proc_address, p_library, r_initialization);
    main_init_obj.register_initializer(initialize_fsharp_extension_module);
    main_init_obj.register_terminator(unitialize_fsharp_extension_module);
    main_init_obj.set_minimum_library_initialization_level(godot::MODULE_INITIALIZATION_LEVEL_CORE);
    return main_init_obj.init();
}
}

void print_script_info(godot::FSXScript* script) {
    godot::UtilityFunctions::print("Printing script info");
    for (auto method_info : script->_get_methods()) {
        godot::UtilityFunctions::print(method_info.name);
    }
}

