use crate::fsx_script_resource_format_loader::FsxScriptResourceFormatLoader;
use godot::classes::{Engine, ResourceFormatLoader, ResourceLoader};
use godot::meta::AsArg;
use godot::prelude::*;

#[cfg(feature = "editor")]
use crate::fsx_script_language::FsxScriptLanguage;
#[cfg(feature = "editor")]
use crate::fsx_script_resource_format_saver::FsxScriptResourceFormatSaver;
#[cfg(feature = "editor")]
use godot::classes::{ResourceFormatSaver, ResourceSaver, ScriptLanguage};
#[cfg(feature = "editor")]
mod fsx_script;
#[cfg(feature = "editor")]
mod fsx_script_instance;
#[cfg(feature = "editor")]
mod fsx_script_language;
#[cfg(feature = "editor")]
mod fsx_script_resource_format_saver;

mod fsx_script_resource_format_loader;

struct FsxScriptExtension;

#[gdextension]
unsafe impl ExtensionLibrary for FsxScriptExtension {
    fn on_level_init(level: InitLevel) {
        match level {
            InitLevel::Core => {
            }
            InitLevel::Servers => {}
            InitLevel::Scene => {
                #[cfg(feature = "editor")] {
                    let language = FsxScriptLanguage::new_alloc();
                    Engine::singleton().register_script_language(&language);
                    Engine::singleton().register_singleton(FsxScriptLanguage::singleton_name().into_arg(), &language);
                    let fsx_scrip_resource_format_saver = FsxScriptResourceFormatSaver::new_gd();
                    ResourceSaver::singleton()
                        .add_resource_format_saver(&fsx_scrip_resource_format_saver);
                    Engine::singleton().register_singleton(
                        "FsxScriptResourceFormatSaver",
                        &fsx_scrip_resource_format_saver,
                    );
                }
                let fsx_script_resource_format_loader = FsxScriptResourceFormatLoader::new_gd();
                ResourceLoader::singleton()
                    .add_resource_format_loader(&fsx_script_resource_format_loader);
                Engine::singleton().register_singleton(
                    "FsxScriptResourceFormatLoader",
                    &fsx_script_resource_format_loader,
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
                #[cfg(feature = "editor")] {
                    let language_name = FsxScriptLanguage::singleton_name();
                    let language = Engine::singleton()
                        .get_singleton(language_name.into_arg())
                        .and_then(|l| Some(l.cast::<ScriptLanguage>()));
                    match language {
                        None => {}
                        Some(language) => {
                            Engine::singleton().unregister_script_language(&language);
                            Engine::singleton().unregister_singleton(language_name.into_arg());
                            language.free();
                        }
                    }
                    let saver_name = StringName::from("FsxScriptResourceFormatSaver");
                    let fsx_script_resource_format_saver = Engine::singleton()
                        .get_singleton(saver_name.into_arg())
                        .and_then(|l| Some(l.cast::<ResourceFormatSaver>()));
                    match fsx_script_resource_format_saver {
                        None => {}
                        Some(saver) => {
                            ResourceSaver::singleton().remove_resource_format_saver(&saver);
                            Engine::singleton().unregister_singleton(saver_name.into_arg());
                        }
                    }
                }
                let loader_name = StringName::from("FsxScriptResourceFormatLoader");
                let fsx_script_resource_format_loader = Engine::singleton()
                    .get_singleton(loader_name.into_arg())
                    .and_then(|l| Some(l.cast::<ResourceFormatLoader>()));
                match fsx_script_resource_format_loader {
                    None => {}
                    Some(loader) => {
                        ResourceLoader::singleton().remove_resource_format_loader(&loader);
                        Engine::singleton().unregister_singleton(loader_name.into_arg());
                    }
                }
            }
            InitLevel::Editor => {}
        }
    }
}


