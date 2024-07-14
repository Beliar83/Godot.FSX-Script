use crate::fsx_script::FsxScript;
use godot::classes::script_language::ScriptNameCasing;
use godot::classes::{Engine, IScriptLanguageExtension, Script, ScriptLanguageExtension};
use godot::global::Error;
use godot::prelude::*;

#[derive(GodotClass)]
#[class(base=ScriptLanguageExtension)]
pub(crate) struct FsxScriptLanguage {
    base: Base<ScriptLanguageExtension>,
}

impl FsxScriptLanguage {
    pub fn singleton() -> Option<Gd<Self>> {
        Engine::singleton()
            .get_singleton(FsxScriptLanguage::class_name().to_string_name())
            .map(|gd| gd.cast())
    }
}

#[godot_api]
impl IScriptLanguageExtension for FsxScriptLanguage {
    fn init(base: Base<Self::Base>) -> Self {
        Self { base }
    }

    fn get_name(&self) -> GString {
        godot_print!("FsxScriptLanguage - get_name");
        GString::from("FsxScriptLanguage")
    }

    fn init_ext(&mut self) {
        godot_print!("FsxScriptLanguage - init_ext");
    }

    fn get_type(&self) -> GString {
        godot_print!("FsxScriptLanguage - get_type");
        GString::from("FsxScript")
    }

    fn get_extension(&self) -> GString {
        godot_print!("FsxScriptLanguage - get_extension");
        GString::from("fsx")
    }

    fn finish(&mut self) {
        godot_print!("FsxScriptLanguage - finish")
    }

    fn get_reserved_words(&self) -> PackedStringArray {
        godot_print!("FsxScriptLanguage - get_reserved_words");
        PackedStringArray::new()
    }

    fn is_control_flow_keyword(&self, _keyword: GString) -> bool {
        godot_print!("FsxScriptLanguage - is_control_flow_keyword");
        false
    }

    fn get_comment_delimiters(&self) -> PackedStringArray {
        godot_print!("FsxScriptLanguage - get_comment_delimiters");
        PackedStringArray::new()
    }

    fn make_template(
        &self,
        template: GString,
        class_name: GString,
        base_class_name: GString,
    ) -> Option<Gd<Script>> {
        godot_print!("FsxScriptLanguage - make_template {template}");
        let code = format!(
            "module {class_name}

//This sets the godot class to inherit from
type Base = {base_class_name}

//Define fields in this type. Use [Export] to mark exported fields.
type State = struct end

let _process(self : Base) (delta: float) =
    ()"
        );
        let mut script = FsxScript::new_gd();
        script.set_source_code(GString::from(code));
        Some(script.upcast())
    }

    fn get_built_in_templates(&self, object: StringName) -> Array<Dictionary> {
        godot_print!("FsxScriptLanguage - get_built_in_templates - {object}");
        Array::<Dictionary>::new()
    }

    fn is_using_templates(&mut self) -> bool {
        godot_print!("FsxScriptLanguage - is_using_templates");
        false
    }

    fn validate(
        &self,
        _script: GString,
        _path: GString,
        _validate_functions: bool,
        _validate_errors: bool,
        _validate_warnings: bool,
        _validate_safe_lines: bool,
    ) -> Dictionary {
        godot_print!("FsxScriptLanguage - validate");
        Dictionary::new()
    }

    fn validate_path(&self, _path: GString) -> GString {
        godot_print!("FsxScriptLanguage - validate_path");
        GString::new()
    }

    fn create_script(&self) -> Option<Gd<Object>> {
        godot_print!("FsxScriptLanguage - create_script");
        let script = FsxScript::new_gd();
        Some(script.upcast())
    }

    fn has_named_classes(&self) -> bool {
        godot_print!("FsxScriptLanguage - has_named_classes");
        false
    }

    fn supports_builtin_mode(&self) -> bool {
        godot_print!("FsxScriptLanguage - supports_builtin_mode");
        false
    }

    fn supports_documentation(&self) -> bool {
        godot_print!("FsxScriptLanguage - supports_documentation");
        false
    }

    fn can_inherit_from_file(&self) -> bool {
        godot_print!("FsxScriptLanguage - can_inherit_from_file");
        false
    }

    fn find_function(&self, _class_name: GString, _function_name: GString) -> i32 {
        godot_print!("FsxScriptLanguage - find_function");
        todo!()
    }

    fn make_function(
        &self,
        _class_name: GString,
        _function_name: GString,
        _function_args: PackedStringArray,
    ) -> GString {
        godot_print!("FsxScriptLanguage - make_function");
        todo!()
    }

    fn open_in_external_editor(&mut self, _script: Gd<Script>, _line: i32, _column: i32) -> Error {
        godot_print!("FsxScriptLanguage - open_in_external_editor");
        todo!()
    }

    fn overrides_external_editor(&mut self) -> bool {
        godot_print!("FsxScriptLanguage - overrides_external_editor");
        false
    }

    fn preferred_file_name_casing(&self) -> ScriptNameCasing {
        ScriptNameCasing::SNAKE_CASE
    }

    fn complete_code(&self, _code: GString, _path: GString, _owner: Gd<Object>) -> Dictionary {
        godot_print!("FsxScriptLanguage - complete_code");
        todo!()
    }

    fn lookup_code(
        &self,
        _code: GString,
        _symbol: GString,
        _path: GString,
        _owner: Gd<Object>,
    ) -> Dictionary {
        godot_print!("FsxScriptLanguage - lookup_code");
        todo!()
    }

    fn auto_indent_code(&self, _code: GString, _from_line: i32, _to_line: i32) -> GString {
        godot_print!("FsxScriptLanguage - auto_indent_code");
        todo!()
    }

    fn add_global_constant(&mut self, _name: StringName, _value: Variant) {
        godot_print!("FsxScriptLanguage - add_global_constant");
        todo!()
    }

    fn add_named_global_constant(&mut self, _name: StringName, _value: Variant) {
        godot_print!("FsxScriptLanguage - add_named_global_constant");
        todo!()
    }

    fn remove_named_global_constant(&mut self, _name: StringName) {
        godot_print!("FsxScriptLanguage - remove_named_global_constant");
        todo!()
    }

    fn thread_enter(&mut self) {
        godot_print!("FsxScriptLanguage - thread_enter");
    }

    fn thread_exit(&mut self) {
        godot_print!("FsxScriptLanguage - thread_exit");
    }

    fn get_recognized_extensions(&self) -> PackedStringArray {
        godot_print!("FsxScriptLanguage - get_recognized_extensions");
        PackedStringArray::from(&[GString::from("fsx")])
    }

    fn get_public_functions(&self) -> Array<Dictionary> {
        godot_print!("FsxScriptLanguage - get_public_functions");
        Array::<Dictionary>::new()
    }

    fn get_public_constants(&self) -> Dictionary {
        godot_print!("FsxScriptLanguage - get_public_constants");
        Dictionary::new()
    }

    fn get_public_annotations(&self) -> Array<Dictionary> {
        godot_print!("FsxScriptLanguage - get_public_annotations");
        Array::<Dictionary>::new()
    }

    fn frame(&mut self) {}

    fn handles_global_class_type(&self, class_type: GString) -> bool {
        godot_print!("FsxScriptLanguage - handles_global_class_type {class_type}");
        false
    }
    fn get_global_class_name(&self, _path: GString) -> Dictionary {
        godot_print!("FsxScriptLanguage - get_global_class_name");
        let mut dict = Dictionary::new();
        dict
    }
}
