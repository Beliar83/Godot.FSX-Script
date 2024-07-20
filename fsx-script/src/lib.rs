use godot::classes::{Engine, ResourceFormatLoader, ResourceFormatSaver, ResourceLoader, ResourceSaver, ScriptLanguage};
use godot::prelude::*;

use crate::fsx_script_language::FsxScriptLanguage;
use crate::fsx_script_resource_format_loader::FsxScriptResourceFormatLoader;
use crate::fsx_script_resource_format_saver::FsxScriptResourceFormatSaver;

mod fsx_script;
mod fsx_script_instance;
mod fsx_script_language;
mod fsx_script_resource_format_loader;
mod fsx_script_resource_format_saver;
mod editor_plugin;

struct FsxScriptExtension;

#[gdextension]
unsafe impl ExtensionLibrary for FsxScriptExtension {
    fn on_level_init(level: InitLevel) {
        match level {
            InitLevel::Core => {}
            InitLevel::Servers => {}
            InitLevel::Scene => {
                let language = FsxScriptLanguage::new_alloc();
                Engine::singleton().register_script_language(language.clone().upcast());
                Engine::singleton().register_singleton(FsxScriptLanguage::singleton_name(), language.upcast());
                let fsx_scrip_resource_format_saver = FsxScriptResourceFormatSaver::new_gd();
                ResourceSaver::singleton()
                    .add_resource_format_saver(fsx_scrip_resource_format_saver.clone().upcast());
                Engine::singleton().register_singleton(
                    StringName::from("FsxScriptResourceFormatSaver"),
                    fsx_scrip_resource_format_saver.upcast(),
                );
                let fsx_script_resource_format_loader = FsxScriptResourceFormatLoader::new_gd();
                ResourceLoader::singleton()
                    .add_resource_format_loader(fsx_script_resource_format_loader.clone().upcast());
                Engine::singleton().register_singleton(
                    StringName::from("FsxScriptResourceFormatLoader"),
                    fsx_script_resource_format_loader.upcast(),
                );
            }
            InitLevel::Editor => {}
        }
    }

    fn on_level_deinit(level: InitLevel) {
        match level {
            InitLevel::Core => {}
            InitLevel::Servers => {}
            InitLevel::Scene => {
                let language_name = FsxScriptLanguage::singleton_name();
                let language = Engine::singleton()
                    .get_singleton(language_name.clone())
                    .and_then(|l| Some(l.cast::<ScriptLanguage>()));
                match language {
                    None => {}
                    Some(language) => {
                        Engine::singleton().unregister_script_language(language.clone());
                        Engine::singleton().unregister_singleton(language_name);
                        language.free();
                    }
                }
                let saver_name = StringName::from("FsxScriptResourceFormatSaver");
                let fsx_script_resource_format_saver = Engine::singleton()
                    .get_singleton(saver_name.clone())
                    .and_then(|l| Some(l.cast::<ResourceFormatSaver>()));
                match fsx_script_resource_format_saver {
                    None => {}
                    Some(saver) => {
                        ResourceSaver::singleton().remove_resource_format_saver(saver);
                        Engine::singleton().unregister_singleton(saver_name);
                    }
                }
                let loader_name = StringName::from("FsxScriptResourceFormatLoader");
                let fsx_script_resource_format_loader = Engine::singleton()
                    .get_singleton(loader_name.clone())
                    .and_then(|l| Some(l.cast::<ResourceFormatLoader>()));
                match fsx_script_resource_format_loader {
                    None => {}
                    Some(loader) => {
                        ResourceLoader::singleton().remove_resource_format_loader(loader);
                        Engine::singleton().unregister_singleton(loader_name);
                    }
                }
            }
            InitLevel::Editor => {}
        }
    }
}


