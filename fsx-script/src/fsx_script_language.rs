use crate::fsx_script::FsxScript;
use godot::builtin::GString;
use godot::classes::native::ScriptLanguageExtensionProfilingInfo;
use godot::classes::script_language::ScriptNameCasing;
use godot::classes::{
    Engine, IScriptLanguageExtension, ProjectSettings, ResourceLoader, Script,
    ScriptLanguageExtension,
};
use godot::global::Error;
use godot::meta::AsArg;
use godot::prelude::*;
use std::collections::HashMap;
use std::ffi::c_void;

pub fn get_or_create_session(script_path: GString) -> Option<Variant> {
    let global_script_classes = ProjectSettings::singleton().get_global_class_list();
    let plugin_class = global_script_classes
        .iter_shared()
        .filter(|class| class.get_or_nil("class").to_string() == "FsxScriptPlugin")
        .map(|class| class.get_or_nil("path").stringify())
        .last();

    let session = match plugin_class {
        None => {
            godot_error!("FsxScriptPlugin is not registered");
            None
        }
        Some(path) => ResourceLoader::singleton()
            .load(path.into_arg())
            .map(|mut resource| {
                resource
                    .call("new", &[])
                    .call("GetOrCreateSession", &[Variant::from(script_path)])
            }),
    };
    session
}

#[derive(GodotClass)]
#[class(base = ScriptLanguageExtension, tool)]
pub(crate) struct FsxScriptLanguage {
    base: Base<ScriptLanguageExtension>,
    pub(crate) scripts: HashMap<GString, Gd<FsxScript>>,
}

#[godot_api]
impl FsxScriptLanguage {
    #[func]
    pub fn singleton_name() -> StringName {
        StringName::from(format!("{}Instance", FsxScriptLanguage::class_name()))
    }

    pub fn singleton() -> Option<Gd<Self>> {
        Engine::singleton()
            .get_singleton(Self::singleton_name().into_arg())
            .map(|gd| gd.cast())
    }
}

#[godot_api]
impl IScriptLanguageExtension for FsxScriptLanguage {
    fn init(base: Base<Self::Base>) -> Self {
        Self {
            base,
            scripts: HashMap::new(),
        }
    }

    fn get_name(&self) -> GString {
        GString::from("FsxScriptLanguage")
    }

    fn init_ext(&mut self) {}

    fn get_type(&self) -> GString {
        GString::from("FsxScript")
    }

    fn get_extension(&self) -> GString {
        GString::from("fsx")
    }

    fn finish(&mut self) {}

    fn get_reserved_words(&self) -> PackedStringArray {
        PackedStringArray::new()
    }

    fn is_control_flow_keyword(&self, _keyword: GString) -> bool {
        false
    }

    fn get_comment_delimiters(&self) -> PackedStringArray {
        PackedStringArray::from(vec![GString::from("//")])
    }

    fn get_string_delimiters(&self) -> PackedStringArray {
        PackedStringArray::from(vec![
            GString::from("\" \""),
            GString::from("' '"),
            GString::from("@\" \""),
        ])
    }

    fn make_template(
        &self,
        template: GString,
        class_name: GString,
        base_class_name: GString,
    ) -> Option<Gd<Script>> {
        let code = format!(
            "module {class_name}
open Godot

//This sets the godot class to inherit from
type Base = {base_class_name}

//Define fields in this type. Use [Export] to mark exported fields.
type State = struct end

let _process(self : Base, delta: float) =
    ()"
        );
        let mut script = FsxScript::new_gd();
        script.set_source_code(code.as_str());
        Some(script.upcast())
    }

    fn get_built_in_templates(&self, object: StringName) -> Array<Dictionary> {
        Array::<Dictionary>::new()
    }

    fn is_using_templates(&mut self) -> bool {
        false
    }

    fn validate(
        &self,
        script: GString,
        path: GString,
        validate_functions: bool,
        validate_errors: bool,
        validate_warnings: bool,
        validate_safe_lines: bool,
    ) -> Dictionary {
        let session = get_or_create_session(GString::from("GeneralFsxScriptSession"));

        match session {
            None => {
                godot_error!("Could not get global session");
                Dictionary::new()
            }
            Some(session) => {
                let result = session.call(
                    "Validate",
                    &[
                        script.to_variant(),
                        path.to_variant(),
                        validate_functions.to_variant(),
                        validate_errors.to_variant(),
                        validate_warnings.to_variant(),
                        validate_safe_lines.to_variant(),
                    ],
                );
                Dictionary::from_variant(&result)
            }
        }
    }

    fn validate_path(&self, _path: GString) -> GString {
        GString::new()
    }

    fn create_script(&self) -> Option<Gd<Object>> {
        let script = FsxScript::new_gd();
        Some(script.upcast::<Object>())
    }

    fn has_named_classes(&self) -> bool {
        false
    }

    fn supports_builtin_mode(&self) -> bool {
        false
    }

    fn supports_documentation(&self) -> bool {
        false
    }

    fn can_inherit_from_file(&self) -> bool {
        false
    }

    fn find_function(&self, _class_name: GString, _function_name: GString) -> i32 {
        todo!()
    }

    fn make_function(
        &self,
        _class_name: GString,
        _function_name: GString,
        _function_args: PackedStringArray,
    ) -> GString {
        todo!()
    }

    fn can_make_function(&self) -> bool {
        todo!()
    }

    fn open_in_external_editor(
        &mut self,
        script: Option<Gd<Script>>,
        line: i32,
        column: i32,
    ) -> Error {
        Error::ERR_UNAVAILABLE
    }

    fn overrides_external_editor(&mut self) -> bool {
        false
    }

    fn preferred_file_name_casing(&self) -> ScriptNameCasing {
        ScriptNameCasing::SNAKE_CASE
    }

    fn complete_code(&self, code: GString, path: GString, owner: Option<Gd<Object>>) -> Dictionary {
        let session = get_or_create_session(path.clone());
        match session {
            None => {
                godot_error!("Could not get session for {path}");
                Dictionary::new()
            }
            Some(session) => {
                let result = session.call("Complete", &[code.to_variant()]);
                Dictionary::from_variant(&result)
            }
        }
    }

    fn lookup_code(
        &self,
        code: GString,
        symbol: GString,
        path: GString,
        owner: Option<Gd<Object>>,
    ) -> Dictionary {
        let session = get_or_create_session(path.clone());
        match session {
            None => {
                godot_error!("Could not get session for {path}");
                Dictionary::new()
            }
            Some(session) => {
                let lines = code.split("\n");
                let cursor_character = "\u{ffff}";
                let (line, line_string) = lines
                    .as_slice()
                    .iter()
                    .enumerate()
                    .find(|(_, l)| l.contains(cursor_character))
                    .expect("Could not find line for cursor character");
                let line = line + 1;
                let column = line_string
                    .find(cursor_character)
                    .expect("Could not find cursor character in line");
                let line_string = line_string.replace(cursor_character, "");
                let column = match line_string
                    .find_ex(symbol.into_arg())
                    .from(column)
                    .r()
                    .done()
                {
                    None => line_string.len(),
                    Some(index) => index + symbol.len(),
                };

                let result = session.call(
                    "Lookup",
                    &[
                        Variant::from(line as i64),
                        Variant::from(column as i64),
                        line_string.to_variant(),
                        symbol.to_variant(),
                    ],
                );
                Dictionary::from_variant(&result)
            }
        }
    }

    fn auto_indent_code(&self, _code: GString, _from_line: i32, _to_line: i32) -> GString {
        todo!()
    }

    fn add_global_constant(&mut self, name: StringName, _value: Variant) {
        godot_print!("FsxScriptLanguage - add_global_constant {name}");
        todo!()
    }

    fn add_named_global_constant(&mut self, name: StringName, _value: Variant) {
        godot_print!("FsxScriptLanguage - add_named_global_constant {name}");
        todo!()
    }

    fn remove_named_global_constant(&mut self, name: StringName) {
        godot_print!("FsxScriptLanguage - remove_named_global_constant {name}");
        todo!()
    }

    fn thread_enter(&mut self) {}

    fn thread_exit(&mut self) {}

    fn debug_get_error(&self) -> GString {
        todo!()
    }

    fn debug_get_stack_level_count(&self) -> i32 {
        todo!()
    }

    fn debug_get_stack_level_line(&self, level: i32) -> i32 {
        todo!()
    }

    fn debug_get_stack_level_function(&self, level: i32) -> GString {
        todo!()
    }

    fn debug_get_stack_level_source(&self, level: i32) -> GString {
        todo!()
    }

    fn debug_get_stack_level_locals(
        &mut self,
        level: i32,
        max_subitems: i32,
        max_depth: i32,
    ) -> Dictionary {
        todo!()
    }

    fn debug_get_stack_level_members(
        &mut self,
        level: i32,
        max_subitems: i32,
        max_depth: i32,
    ) -> Dictionary {
        todo!()
    }

    unsafe fn debug_get_stack_level_instance(&mut self, level: i32) -> *mut c_void {
        todo!()
    }

    fn debug_get_globals(&mut self, max_subitems: i32, max_depth: i32) -> Dictionary {
        todo!()
    }

    fn debug_parse_stack_level_expression(
        &mut self,
        level: i32,
        expression: GString,
        max_subitems: i32,
        max_depth: i32,
    ) -> GString {
        todo!()
    }

    fn debug_get_current_stack_info(&mut self) -> Array<Dictionary> {
        todo!()
    }

    fn reload_all_scripts(&mut self) {
        todo!()
    }

    fn reload_scripts(&mut self, scripts: VariantArray, soft_reload: bool) {
        todo!()
    }

    fn reload_tool_script(&mut self, script: Option<Gd<Script>>, soft_reload: bool) {
        todo!()
    }

    fn get_recognized_extensions(&self) -> PackedStringArray {
        PackedStringArray::from(&[GString::from("fsx")])
    }

    fn get_public_functions(&self) -> Array<Dictionary> {
        Array::<Dictionary>::new()
    }

    fn get_public_constants(&self) -> Dictionary {
        Dictionary::new()
    }

    fn get_public_annotations(&self) -> Array<Dictionary> {
        Array::<Dictionary>::new()
    }

    fn profiling_start(&mut self) {
        todo!()
    }

    fn profiling_stop(&mut self) {
        todo!()
    }

    fn profiling_set_save_native_calls(&mut self, enable: bool) {
        todo!()
    }

    unsafe fn profiling_get_accumulated_data(
        &mut self,
        info_array: *mut ScriptLanguageExtensionProfilingInfo,
        info_max: i32,
    ) -> i32 {
        todo!()
    }

    unsafe fn profiling_get_frame_data(
        &mut self,
        info_array: *mut ScriptLanguageExtensionProfilingInfo,
        info_max: i32,
    ) -> i32 {
        todo!()
    }

    fn frame(&mut self) {}

    fn handles_global_class_type(&self, class_type: GString) -> bool {
        class_type == GString::from("Script")
    }

    fn get_global_class_name(&self, path: GString) -> Dictionary {
        match self.scripts.get(&path) {
            None => Dictionary::new(),
            Some(script) => {
                let mut dict = Dictionary::new();
                let script = script.bind();
                let name = script.get_class_name();
                let base_type = script.get_base_type();
                if !name.is_empty() {
                    dict.set("name", name);
                }
                if !base_type.is_empty() {
                    dict.set("base_type", base_type);
                }
                dict
            }
        }
    }
}
