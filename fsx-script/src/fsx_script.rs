use std::cell::RefCell;
use std::collections::HashSet;
use std::ffi::c_void;

use godot::classes::{IScriptExtension, Script, ScriptExtension, ScriptLanguage, WeakRef};
use godot::global::weakref;
use godot::prelude::*;
use godot::sys::{GDExtensionPropertyInfo, GDExtensionScriptInstancePtr};

use crate::fsx_script_instance::{FsxScriptInstance, FsxScriptPlaceholder};
use crate::fsx_script_language::{DotnetMethods, FsxScriptLanguage};

#[derive(GodotClass)]
#[class(base = ScriptExtension)]
pub(crate) struct FsxScript {
    #[var(usage_flags = [STORAGE])]
    code: GString,

    #[var(get = owner_ids, set = set_owner_ids, usage_flags = [STORAGE])]
    #[allow(dead_code)]
    owner_ids: Array<i64>,

    owners: RefCell<Vec<Gd<WeakRef>>>,
    dotnet_methods: DotnetMethods,
    session_pointer: *const c_void,
    base: Base<ScriptExtension>,
}

#[godot_api]
impl FsxScript {
    #[func]
    pub fn get_class_name(&self) -> GString {
        (self.dotnet_methods.get_class_name)(self.session_pointer)
    }

    #[func]
    pub fn get_base_type(&self) -> GString {
        (self.dotnet_methods.get_base_type)(self.session_pointer)
    }

    pub(crate) unsafe fn get_property_list(&self, count: *mut u32) -> *const GDExtensionPropertyInfo {
        (self.dotnet_methods.get_property_list)(self.session_pointer, count)
    }

    #[func]
    fn owner_ids(&self) -> Array<i64> {
        let owners = self.owners.borrow();

        let set: HashSet<_> = owners
            .iter()
            .filter_map(|item| item.get_ref().to::<Option<Gd<Object>>>())
            .map(|obj| obj.instance_id().to_i64())
            .collect();

        set.into_iter().collect()
    }

    #[func]
    fn set_owner_ids(&mut self, ids: Array<i64>) {
        if ids.is_empty() {
            // ignore empty owners list from engine
            return;
        }

        if !self.owners.borrow().is_empty() {
            godot_warn!("over writing existing owners of fsx script");
        }

        *self.owners.borrow_mut() = ids
            .iter_shared()
            .map(InstanceId::from_i64)
            .filter_map(|id| {
                let result: Option<Gd<Object>> = Gd::try_from_instance_id(id).ok();
                result
            })
            .map(|gd_ref| weakref(gd_ref.to_variant()).to())
            .collect();
    }
}

#[godot_api]
impl IScriptExtension for FsxScript {
    fn init(base: Base<Self::Base>) -> Self {
        let dotnet_methods = FsxScriptLanguage::singleton().unwrap().cast::<FsxScriptLanguage>().bind().dotnet_methods.clone();
        let create_session = dotnet_methods.create_session;
        let session_pointer = create_session();

        Self {
            code: GString::new(),
            base,
            owners: Default::default(),
            owner_ids: Default::default(),
            dotnet_methods,
            session_pointer,
        }
    }

    fn can_instantiate(&self) -> bool {
        godot_print!("FSXScript - can_instantiate");
        // TODO: Check
        false
    }

    fn get_base_script(&self) -> Option<Gd<Script>> {
        godot_print!("FSXScript - get_base_script");
        // TODO: Return actual base
        None
    }

    fn get_global_name(&self) -> StringName {
        let global_name = (self.dotnet_methods.get_class_name)(self.session_pointer);
        StringName::from(global_name)
    }

    fn inherits_script(&self, _script: Gd<Script>) -> bool {
        godot_print!("FSXScript - inherits_script");
        todo!()
    }

    fn get_instance_base_type(&self) -> StringName {
        let base_type = (self.dotnet_methods.get_base_type)(self.session_pointer);
        StringName::from(base_type)
    }

    unsafe fn instance_create(&self, for_object: Gd<Object>) -> *mut c_void {
        self.owners
            .borrow_mut()
            .push(weakref(for_object.to_variant()).to());

        let instance = FsxScriptInstance::new(self.to_gd());

        let instance : GDExtensionScriptInstancePtr = instance.into();
        instance.cast::<c_void>()
    }

    unsafe fn placeholder_instance_create(&self, for_object: Gd<Object>) -> *mut c_void {
        self.owners
            .borrow_mut()
            .push(weakref(for_object.to_variant()).to());

        let placeholder = FsxScriptPlaceholder::new(self.to_gd());
        let instance : GDExtensionScriptInstancePtr = placeholder.into();
        instance.cast::<c_void>()
    }

    fn has_source_code(&self) -> bool {
        self.code.is_empty()
    }

    fn get_source_code(&self) -> GString {
        self.code.clone()
    }

    fn set_source_code(&mut self, code: GString) {
        self.code = code.clone();
        (self.dotnet_methods.parse_script)(self.session_pointer, code);
        let mut language = FsxScriptLanguage::singleton().unwrap();
        let mut language = language.bind_mut();
        let self_gd = self.to_gd();
        let path = self_gd.get_path();
        language.scripts.insert(path, self_gd);
    }

    fn has_method(&self, _method: StringName) -> bool {
        godot_print!("FSXScript - has_method");
        // TODO: Actually check
        false
    }

    fn has_static_method(&self, _method: StringName) -> bool {
        godot_print!("FSXScript - has_static_method");
        // TODO: Actually check
        false
    }

    fn get_method_info(&self, _method: StringName) -> Dictionary {
        godot_print!("FSXScript - get_method_info");
        // TODO: Actually generate
        Dictionary::new()
    }

    fn is_tool(&self) -> bool {
        godot_print!("FSXScript - is_tool");
        // TODO: Actually check
        true
    }

    fn is_valid(&self) -> bool {
        godot_print!("FSXScript - is_valid");
        // TODO: Actually check
        false
    }

    fn is_abstract(&self) -> bool {
        godot_print!("FSXScript - is_abstract");
        // TODO: Actually check
        false
    }

    fn get_language(&self) -> Option<Gd<ScriptLanguage>> {
        FsxScriptLanguage::singleton().map(Gd::upcast)
    }

    fn has_script_signal(&self, _signal: StringName) -> bool {
        false
    }

    fn get_script_signal_list(&self) -> Array<Dictionary> {
        Array::<Dictionary>::new()
    }

    fn has_property_default_value(&self, _property: StringName) -> bool {
        false
    }

    fn get_member_line(&self, _member: StringName) -> i32 {
        godot_print!("FSXScript - get_member_line");
        todo!()
    }

    fn get_constants(&self) -> Dictionary {
        godot_print!("FSXScript - get_constants");
        todo!()
    }

    fn get_members(&self) -> Array<StringName> {
        godot_print!("FSXScript - get_members");
        // TODO: Actually generate
        Array::<StringName>::new()
    }
}
