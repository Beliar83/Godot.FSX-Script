use crate::fsx_script::FsxScript;
use godot::classes::file_access::ModeFlags;
use godot::classes::{FileAccess, IResourceFormatLoader, ResourceFormatLoader};
use godot::prelude::*;

#[derive(GodotClass)]
#[class(base=ResourceFormatLoader)]
pub(crate) struct FsxScriptResourceFormatLoader {
    base: Base<ResourceFormatLoader>,
}
#[godot_api]
impl IResourceFormatLoader for FsxScriptResourceFormatLoader {
    fn init(base: Base<Self::Base>) -> Self {
        Self { base }
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
        let file = FileAccess::open(path, ModeFlags::READ);
        match file {
            None => Variant::from(FileAccess::get_open_error()),
            Some(file) => {
                let text = file.get_as_text();
                let mut script = FsxScript::new_gd();
                script.set_source_code(text);
                Variant::from(script)
            }
        }
    }
}
