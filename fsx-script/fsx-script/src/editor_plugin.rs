use godot::classes::{CodeEdit, EditorInterface, EditorPlugin, IEditorPlugin, ResourceLoader, Script};
use godot::prelude::*;

use crate::fsx_script::FsxScript;
use crate::fsx_script_language::FsxScriptLanguage;

#[derive(GodotClass)]
#[class(tool, init, editor_plugin, base = EditorPlugin)]
struct FsxScriptEditorPlugin {
    base: Base<EditorPlugin>,
}

#[godot_api]
impl FsxScriptEditorPlugin {
    #[func]
    pub(crate) fn editor_script_changed(&self, script: Variant) {
        match script.try_to::<Gd<FsxScript>>() {
            Ok(_) => {
                match EditorInterface::singleton().get_script_editor().and_then(|x| x.get_current_editor()).and_then(|x| x.get_base_editor()).and_then(|x| x.try_cast::<CodeEdit>().ok()) {
                    None => {
                        godot_error!("Could not get CodeEdit for open Fsx script. Fsx Extensions are not available")
                    }
                    Some(mut edit) => {
                        edit.set_indent_using_spaces(true);
                    }
                }
            }
            Err(_) => {}
        }
    }
}

#[godot_api]
impl IEditorPlugin for FsxScriptEditorPlugin {
    fn enter_tree(&mut self) {
        let mut language = FsxScriptLanguage::singleton().unwrap();
        let mut language = language.bind_mut();
        let script = ResourceLoader::singleton().load(GString::from("res://addons/fsx-script/FsxScriptLanguageExt.gd")).and_then(|r| Some(r.cast::<Script>())).unwrap();
        language.base_mut().set_script(script.to_variant());
        EditorInterface::singleton().get_script_editor().unwrap().connect(StringName::from("editor_script_changed"), self.to_gd().callable("editor_script_changed"));
    }
}