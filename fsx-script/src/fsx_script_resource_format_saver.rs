use godot::classes::{FileAccess, IResourceFormatSaver, ResourceFormatSaver};
use godot::classes::file_access::ModeFlags;
use godot::global::Error;
use godot::meta::AsArg;
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

    fn save(&mut self, resource: Option<Gd<Resource>>, path: GString, _flags: u32) -> Error {
        let resource = match resource {
            None => {return Error::ERR_INVALID_PARAMETER}
            Some(resource) => { resource }
        };
        let script = resource.cast::<FsxScript>();
        let file = FileAccess::open(path.into_arg(), ModeFlags::WRITE);
        match file {
            None => {
                godot_error!("Could not open {path} for writing");
                FileAccess::get_open_error()
            }
            Some(mut file) => {
                file.store_string(script.get_source_code().into_arg());
                let error = file.get_error();
                file.close();
                error
            }
        }
    }

    fn recognize(&self, resource: Option<Gd<Resource>>) -> bool {
        match resource {
            None => false,
            Some(resource) => resource.is_class(GString::from("Script").into_arg()),
        }

    }

    fn get_recognized_extensions(&self, resource: Option<Gd<Resource>>) -> PackedStringArray {
        if self.recognize(resource) {
            PackedStringArray::from(&[GString::from("fsx")])
        } else {
            PackedStringArray::new()
        }
    }
}
