use godot::classes::{EditorPlugin, IEditorPlugin, ResourceLoader, Script};
use godot::prelude::*;

use crate::fsx_script_language::FsxScriptLanguage;

#[derive(GodotClass)]
#[class(tool, init, editor_plugin, base = EditorPlugin)]
struct FsxScriptEditorPlugin {
    base: Base<EditorPlugin>,
}

#[godot_api]
impl IEditorPlugin for FsxScriptEditorPlugin {
    fn enter_tree(&mut self) {
        let mut language = FsxScriptLanguage::singleton().unwrap();
        let mut language = language.bind_mut();
        let script = ResourceLoader::singleton().load(GString::from("res://addons/fsx-script/FsxScriptLanguageExt.gd")).and_then(|r| Some(r.cast::<Script>())).unwrap();
        language.base_mut().set_script(script.to_variant());
    }
}