#include "gdextension_interface.h"
#include <godot_cpp/godot.hpp>
#include <godot_cpp/core/defs.hpp>
#include <filesystem>
#include "nativehost.h"
#include "godot_cpp/classes/gd_extension.hpp"

using namespace godot;

//FSXScriptLanguage *language = nullptr;
//FSXResourceFormatSaver *fsxResourceFormatSaver = nullptr;
//FSXResourceFormatLoader *fsxResourceFormatLoader = nullptr;

bool initialized = false;
godot::GDExtension* extension;
DotnetInitialization* dotnetInitialization;

void initialize_godot_sharp_gdextension_extension_module(godot::ModuleInitializationLevel p_level) {
    switch (p_level) {
        case MODULE_INITIALIZATION_LEVEL_SCENE:
            extension->initialize_library(godot::GDExtension::INITIALIZATION_LEVEL_SCENE);
            dotnetInitialization->initialize(GDEXTENSION_INITIALIZATION_SCENE);

//            godot::UtilityFunctions::print("FSXScriptLanguage");
//            ClassDB::register_class<FSXScriptLanguage>();
//            godot::UtilityFunctions::print("FSXScript");
//            ClassDB::register_class<FSXScript>();
//            godot::UtilityFunctions::print("FSXResourceFormatSaver");
//            ClassDB::register_class<FSXResourceFormatSaver>();
//            godot::UtilityFunctions::print("FSXResourceFormatLoader");
//            ClassDB::register_class<FSXResourceFormatLoader>();
//            godot::UtilityFunctions::print("create language");
//            language = memnew(FSXScriptLanguage);
//            Engine::get_singleton()->register_singleton("FSXScriptLanguage", language);
//            Engine::get_singleton()->register_script_language(language);
//
//            godot::UtilityFunctions::print("create saver");
//            fsxResourceFormatSaver = memnew(FSXResourceFormatSaver);
//            Engine::get_singleton()->register_singleton("FSXResourceFormatSaver", fsxResourceFormatSaver);
//            ResourceSaver::get_singleton()->add_resource_format_saver(fsxResourceFormatSaver);
//
//            godot::UtilityFunctions::print("create loader");
//            fsxResourceFormatLoader = memnew(FSXResourceFormatLoader);
//            Engine::get_singleton()->register_singleton("FSXResourceFormatLoader", fsxResourceFormatLoader);
//            ResourceLoader::get_singleton()->add_resource_format_loader(fsxResourceFormatLoader);
            break;
        case MODULE_INITIALIZATION_LEVEL_CORE:
            extension = memnew(GDExtension);
#ifdef _WIN32
            extension->open_library(std::filesystem::current_path().append("bin/godot_sharp_gdextension.dll").c_str(), "godot_sharp_gdextension_init");
#else
            extension->open_library(std::filesystem::current_path().append("bin/libgodot_sharp_gdextension.so").c_str(), "godot_sharp_gdextension_init");
#endif
            if (!initialized) {
                dotnetInitialization = bind();
                initialized = true;
            }
            extension->initialize_library(godot::GDExtension::INITIALIZATION_LEVEL_CORE);
            dotnetInitialization->initialize(GDEXTENSION_INITIALIZATION_CORE);
            break;
        case MODULE_INITIALIZATION_LEVEL_SERVERS:
            extension->initialize_library(godot::GDExtension::INITIALIZATION_LEVEL_SERVERS);
            dotnetInitialization->initialize(GDEXTENSION_INITIALIZATION_SERVERS);
            break;
        case MODULE_INITIALIZATION_LEVEL_EDITOR:
            extension->initialize_library(godot::GDExtension::INITIALIZATION_LEVEL_EDITOR);
            dotnetInitialization->initialize(GDEXTENSION_INITIALIZATION_EDITOR);
            break;
    }
}

void unitialize_fsharp_extension_module(godot::ModuleInitializationLevel p_level) {

    switch (p_level) {
        case MODULE_INITIALIZATION_LEVEL_SCENE:
            dotnetInitialization->uninitialize(GDEXTENSION_INITIALIZATION_SCENE);
            break;
        case MODULE_INITIALIZATION_LEVEL_CORE:
            dotnetInitialization->uninitialize(GDEXTENSION_INITIALIZATION_CORE);
            extension->close_library();
            memdelete(extension);
            extension = nullptr;
            delete dotnetInitialization;
            dotnetInitialization = nullptr;
            break;
        case MODULE_INITIALIZATION_LEVEL_SERVERS:
            dotnetInitialization->uninitialize(GDEXTENSION_INITIALIZATION_SERVERS);
            break;
        case MODULE_INITIALIZATION_LEVEL_EDITOR:
            dotnetInitialization->uninitialize(GDEXTENSION_INITIALIZATION_EDITOR);
            break;
    }
}

extern "C" {
GDExtensionBool GDE_EXPORT script_interop_init(GDExtensionInterfaceGetProcAddress p_get_proc_address, GDExtensionClassLibraryPtr p_library, GDExtensionInitialization *r_initialization) {
    godot::GDExtensionBinding::InitObject main_init_obj(p_get_proc_address, p_library, r_initialization);
    main_init_obj.register_initializer(initialize_godot_sharp_gdextension_extension_module);
    main_init_obj.register_terminator(unitialize_fsharp_extension_module);
    main_init_obj.set_minimum_library_initialization_level(godot::MODULE_INITIALIZATION_LEVEL_CORE);
    return main_init_obj.init();
}
}

