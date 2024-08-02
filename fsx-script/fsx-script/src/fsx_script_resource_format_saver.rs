use godot::classes::{FileAccess, IResourceFormatSaver, ResourceFormatSaver};
use godot::classes::file_access::ModeFlags;
use godot::global::Error;
use godot::prelude::*;

use crate::fsx_script::FsxScript;

#[derive(GodotClass)]
#[class(base = ResourceFormatSaver)]
pub(crate) struct FsxScriptResourceFormatSaver {
    base: Base<ResourceFormatSaver>,
}

#[godot_api]
impl IResourceFormatSaver for FsxScriptResourceFormatSaver {
    fn init(base: Base<Self::Base>) -> Self {
        Self { base }
    }

    fn save(&mut self, resource: Gd<Resource>, path: GString, _flags: u32) -> Error {
        let script = resource.cast::<FsxScript>();
        let file = FileAccess::open(path.clone(), ModeFlags::WRITE);
        match file {
            None => {
                godot_error!("Could not open {path} for writing");
                FileAccess::get_open_error()
            }
            Some(mut file) => {
                file.store_string(script.get_source_code());
                let error = file.get_error();
                file.close();
                error
            }
        }
    }

    fn recognize(&self, resource: Gd<Resource>) -> bool {
        resource.is_class(GString::from("Script"))
    }

    fn get_recognized_extensions(&self, resource: Gd<Resource>) -> PackedStringArray {
        if self.recognize(resource) {
            PackedStringArray::from(&[GString::from("fsx")])
        } else {
            PackedStringArray::new()
        }
    }
}
