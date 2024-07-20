use std::ffi::c_void;

use godot::builtin::GString;
use godot::classes::{Engine, IScriptLanguageExtension, Script, ScriptLanguageExtension,Os, ProjectSettings};
use godot::classes::script_language::ScriptNameCasing;
use godot::global::Error;
use godot::prelude::*;
use std::path::{Path, PathBuf};

use godot::sys::{GDExtensionInterface, get_interface};
use netcorehost::{nethost, pdcstr};
use netcorehost::hostfxr::ManagedFunction;
use netcorehost::pdcstring::PdCString;
use crate::fsx_script::FsxScript;

#[repr(C)]
#[derive(Clone)]
pub(crate) struct DotnetMethods {
    pub(crate) init: extern "system" fn(*const GDExtensionInterface),
    pub(crate) set_base_path: extern "system" fn (GString),
    pub(crate) create_session: extern "system" fn () -> *const c_void,
    pub(crate) get_class_name: extern "system" fn (*const c_void) -> GString,
    pub(crate) parse_script: extern "system" fn (*const c_void, GString),
    pub(crate) string_test: extern "system" fn() -> GString,
    pub(crate) from_rust: extern "system" fn(GString),
}

#[derive(GodotClass)]
#[class(base=ScriptLanguageExtension)]
pub(crate) struct FsxScriptLanguage {
    base: Base<ScriptLanguageExtension>,
    pub(crate) dotnet_methods: DotnetMethods,
}

#[godot_api]
impl FsxScriptLanguage {
    
    #[func]
    pub fn singleton_name() -> StringName {
        StringName::from(format!("{}Instance", FsxScriptLanguage::class_name()))
    }
    
    pub fn singleton() -> Option<Gd<Self>> {
        Engine::singleton()
            .get_singleton(Self::singleton_name())
            .map(|gd| gd.cast())
    }

    #[func]
    fn complete_code(&self, _code: GString, _path: GString, _owner: Option<Gd<Object>>) -> Dictionary {
        godot_print!("FsxScriptLanguage - complete_code");
        Dictionary::new()
    }

    #[func]
    fn lookup_code(
        &self,
        _code: GString,
        _symbol: GString,
        _path: GString,
        _owner: Option<Gd<Object>>,
    ) -> Dictionary {
        godot_print!("FsxScriptLanguage - lookup_code");
        Dictionary::new()
    }    

}

#[godot_api]
impl IScriptLanguageExtension for FsxScriptLanguage {
    fn init(base: Base<Self::Base>) -> Self {
        let hostfxr = nethost::load_hostfxr().unwrap();
        let root_path = if Os::singleton().has_feature(GString::from("editor")) {
            ProjectSettings::singleton().globalize_path(GString::from("res://"))
        } else {
            GString::from(PathBuf::from(Os::singleton().get_executable_path().to_string()).parent().unwrap().to_string_lossy().to_string())
        };

        let fsx_script_path = PathBuf::from(root_path.to_string()).join(Path::new("addons/fsx-script"));

        let config_path = fsx_script_path.join(Path::new("bin/FSXScript.Editor.runtimeconfig.json"));
        let config_path = PdCString::from_os_str(config_path.as_os_str()).unwrap();
        let dll_path = fsx_script_path.join(Path::new("bin/FSXScript.Editor.dll"));
        let dll_path = PdCString::from_os_str(dll_path.as_os_str()).unwrap();

        let context = hostfxr
            .initialize_for_runtime_config(config_path)
            .unwrap();
        let fn_loader = context
            .get_delegate_loader_for_assembly(dll_path)
            .unwrap();


        let result: Result<ManagedFunction<extern "system" fn() -> DotnetMethods>, _> = fn_loader.get_function_with_unmanaged_callers_only::<fn() -> DotnetMethods>(
            pdcstr!("FSXScript.Editor.Main, FSXScript.Editor"),
            pdcstr!("GetMethods"),
        );

        let get_dotnet_methods = result.unwrap();
        let dotnet_methods = get_dotnet_methods();
        let init = dotnet_methods.init;
        let interface = unsafe { get_interface() };
        init(interface);
        let set_base_path = dotnet_methods.set_base_path;
        set_base_path(root_path);
        Self { base, dotnet_methods }
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
        PackedStringArray::from(vec![GString::from("//")])
    }

    fn get_string_delimiters(&self) -> PackedStringArray {
        PackedStringArray::from(vec![GString::from("\" \""), GString::from("' '"), GString::from("@\" \"")])
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
open Godot

//This sets the godot class to inherit from
type Base = {base_class_name}

//Define fields in this type. Use [Export] to mark exported fields.
type State = struct end

let _process(self : Base, delta: float) =
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
        Dictionary::new()
    }
}
