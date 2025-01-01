use godot::classes::{IResourceFormatLoader, ResourceFormatLoader};
use godot::meta::AsArg;
use godot::prelude::*;

#[cfg(feature = "editor")]
use crate::fsx_script::FsxScript;
#[cfg(feature = "editor")]
use godot::classes::file_access::ModeFlags;
#[cfg(feature = "editor")]
use godot::classes::FileAccess;
#[cfg(feature = "template")]
use godot::classes::{ConfigFile, ResourceLoader};

#[cfg(feature = "editor")]
#[derive(GodotClass)]
#[class(base = ResourceFormatLoader)]
pub(crate) struct FsxScriptResourceFormatLoader {
    base: Base<ResourceFormatLoader>,
}
#[cfg(feature = "template")]
#[derive(GodotClass)]
#[class(base = ResourceFormatLoader)]
pub(crate) struct FsxScriptResourceFormatLoader {
    base: Base<ResourceFormatLoader>,
    script_mapping: Dictionary,
}

#[godot_api]
impl IResourceFormatLoader for FsxScriptResourceFormatLoader {

    #[cfg(feature = "editor")]
    fn init(base: Base<Self::Base>) -> Self {
        Self { base }
    }

    #[cfg(feature = "template")]
    fn init(base: Base<Self::Base>) -> Self {
        let mut script_config = ConfigFile::new_gd();
        script_config.load("res://fsx_script.ini");
        
        let script_mapping = Dictionary::from_variant(&script_config.get_value("Scripts", "Mapping"));
        
        Self { base, script_mapping }
    }

    fn get_recognized_extensions(&self) -> PackedStringArray {
        PackedStringArray::from(&[GString::from("fsx")])
    }

    fn handles_type(&self, resource_type: StringName) -> bool {
        resource_type == StringName::from("Script")
    }

    fn get_resource_type(&self, path: GString) -> GString {
        if path.to_string().ends_with(".fsx") {
            GString::from("Script")
        } else {
            GString::default()
        }
    }

    fn load(
        &self,
        path: GString,
        _original_path: GString,
        _use_sub_threads: bool,
        _cache_mode: i32,
    ) -> Variant {
        #[cfg(feature = "template")] {
            let value = self.script_mapping.get(path.into_arg());
            match value {
                None => {
                    godot_error!("Could not get C# script for {}", path);
                    Variant::default()                    
                }
                Some(value) => {
                    ResourceLoader::singleton().load(value.stringify().into_arg()).unwrap().to_variant()
                }
            }
        }

        #[cfg(feature = "editor")] {
            let file = FileAccess::open(path.into_arg(), ModeFlags::READ);
            match file {
                None => Variant::from(FileAccess::get_open_error()),
                Some(file) => {
                    let text = file.get_as_text();
                    let mut script = FsxScript::new_gd();
                    script.set_path(path.into_arg());
                    script.set_source_code(text.into_arg());
                    Variant::from(script)
                }
            }
        }
    }
}
