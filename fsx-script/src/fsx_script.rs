use godot::global::{weakref, Error};
use godot::prelude::*;
use godot::sys::types::{OpaqueString, OpaqueStringName};
use godot::sys::{
    c_str_from_str, get_interface, GDExtensionInt, GDExtensionPropertyInfo,
    GDExtensionScriptInstancePtr, GDExtensionStringNamePtr, GDExtensionStringPtr,
    GDExtensionUninitializedStringNamePtr, GDExtensionUninitializedStringPtr,
};
use std::cell::RefCell;
use std::collections::HashSet;
use std::convert::From;
use std::ffi::c_void;
use std::mem::ManuallyDrop;
use std::ops::Add;
use godot::classes::{IScriptExtension, Script, ScriptExtension, ScriptLanguage};
use crate::fsx_script_instance::{FsxScriptInstance, FsxScriptPlaceholder};
use crate::fsx_script_language::{get_or_create_session, FsxScriptLanguage};

#[derive(GodotClass)]
#[class(base = ScriptExtension, tool)]
pub(crate) struct FsxScript {
    code: String,

    #[var(get = owner_ids, set = set_owner_ids, usage_flags = [STORAGE])]
    #[allow(dead_code)]
    owner_ids: Array<i64>,
    owners: RefCell<Vec<Gd<Object>>>,
    base: Base<ScriptExtension>,
}

// This creates a vec that holds a single StringName. This is needed to get a unique address that can be passed to godot
fn create_string_name_from_string(content: String) -> Vec<OpaqueStringName> {
    let mut buf = Vec::<OpaqueStringName>::with_capacity(1);

    unsafe {
        let content = content.add("\0");
        get_interface()
            .string_name_new_with_utf8_chars_and_len
            .unwrap()(
            buf.as_mut_ptr() as GDExtensionUninitializedStringNamePtr,
            c_str_from_str(content.as_str()),
            content.len() as GDExtensionInt,
        );
        buf.set_len(1);
    }

    buf
}

// This creates a vec that holds a single String. This is needed to get a unique address that can be passed to godot
fn create_godot_string_from_string(content: String) -> Vec<OpaqueString> {
    let mut buf = Vec::<OpaqueString>::with_capacity(1);

    unsafe {
        let content = content.add("\0");
        get_interface().string_new_with_utf8_chars_and_len2.unwrap()(
            buf.as_mut_ptr() as GDExtensionUninitializedStringPtr,
            c_str_from_str(content.as_str()),
            content.len() as GDExtensionInt,
        );
        buf.set_len(1);
    }

    buf
}

#[godot_api]
impl FsxScript {
    #[func]
    pub fn get_class_name(&self) -> GString {
        match self.get_session() {
            None => GString::default(),
            Some(session) => {
                let class_name = session.call("GetClassName", &[]);
                let class_name = StringName::from_variant(&class_name);
                GString::from(class_name)
            }
        }
    }

    #[func]
    pub fn get_base_type(&self) -> GString {
        match self.get_session() {
            None => GString::default(),
            Some(session) => {
                let base_type = session.call("GetBaseType", &[]);
                let base_type = StringName::from_variant(&base_type);
                GString::from(base_type)
            }
        }
    }

    fn get_session(&self) -> Option<Variant> {
        get_or_create_session(self.base().get_path())
    }

    pub(crate) unsafe fn get_property_list(
        &self,
        count: *mut u32,
    ) -> *const GDExtensionPropertyInfo {
        let property_list = match self.get_session() {
            None => Array::<Dictionary>::new(),
            Some(session) => {
                let variant = session.call("GetProperties", &[]);
                Array::<Dictionary>::from_variant(&variant)
            }
        };

        let mut buf = Vec::<GDExtensionPropertyInfo>::with_capacity(property_list.len());
        for property in property_list.iter_shared() {
            let name = property
                .get(StringName::from("Name"))
                .expect("PropertyInfoDictionary was not in the correct format")
                .clone();
            let name = ManuallyDrop::new(create_string_name_from_string(name.to_string()));
            let name = name.as_ptr() as GDExtensionStringNamePtr;

            let class_name = property
                .get(StringName::from("ClassName"))
                .expect("PropertyInfoDictionary was not in the correct format")
                .clone();
            let class_name =
                ManuallyDrop::new(create_string_name_from_string(class_name.to_string()));
            let class_name = class_name.as_ptr() as GDExtensionStringNamePtr;

            let type_ = property
                .get(StringName::from("Type"))
                .expect("PropertyInfoDictionary was not in the correct format");
            let type_ = type_.to();
            let hint = property
                .get(StringName::from("Hint"))
                .expect("PropertyInfoDictionary was not in the correct format");
            let hint = hint.to();
            let hint_string = property
                .get(StringName::from("HintString"))
                .expect("PropertyInfoDictionary was not in the correct format")
                .clone();
            let hint_string =
                ManuallyDrop::new(create_godot_string_from_string(hint_string.to_string()));
            let hint_string = hint_string.as_ptr() as GDExtensionStringPtr;

            let usage = property
                .get(StringName::from("Usage"))
                .expect("PropertyInfoDictionary was not in the correct format");
            let usage = usage.to();
            let info = GDExtensionPropertyInfo {
                name,
                class_name,
                type_,
                hint,
                hint_string,
                usage,
            };
            buf.push(info);
        }

        *count = buf.len() as u32;
        let x = buf.as_ptr();
        std::mem::forget(buf);
        x
    }

    pub(crate) fn has_property(&self, name: &StringName) -> bool {
        match self.get_session() {
            None => false,
            Some(session) => session
                .call("HasProperty", &[name.to_variant()])
                .booleanize(),
        }
    }

    #[func]
    fn owner_ids(&self) -> Array<i64> {
        let owners = self.owners.borrow();

        let set: HashSet<_> = owners
            .iter()
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
            .map(|gd_ref| weakref(&gd_ref.to_variant()).to())
            .collect();
    }
}

#[godot_api]
impl IScriptExtension for FsxScript {
    fn init(base: Base<Self::Base>) -> Self {
        Self {
            code: String::new(),
            base,
            owners: Default::default(),
            owner_ids: Default::default(),
        }
    }

    fn editor_can_reload_from_file(&mut self) -> bool {
        todo!()
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
        match self.get_session() {
            None => StringName::default(),
            Some(session) => {
                let global_name = session.call("GetClassName", &[]);
                StringName::from_variant(&global_name)
            }
        }
    }

    fn inherits_script(&self, _script: Gd<Script>) -> bool {
        godot_print!("FSXScript - inherits_script");
        todo!()
    }

    fn get_instance_base_type(&self) -> StringName {
        match self.get_session() {
            None => StringName::default(),
            Some(session) => {
                let base_type = session.call("GetBaseType", &[]);
                StringName::from_variant(&base_type)
            }
        }
    }

    unsafe fn instance_create(&self, for_object: Gd<Object>) -> *mut c_void {
        self.owners
            .borrow_mut()
            .push(Gd::<Object>::from_instance_id(for_object.instance_id()));

        let self_gd = self.to_gd();
        let instance = FsxScriptInstance::new(self_gd, for_object);
        let instance: GDExtensionScriptInstancePtr = instance.into();
        instance.cast::<c_void>()
    }

    unsafe fn placeholder_instance_create(&self, for_object: Gd<Object>) -> *mut c_void {
        self.owners
            .borrow_mut()
            .push(Gd::<Object>::from_instance_id(for_object.instance_id()));

        let self_gd = self.to_gd();
        let placeholder = FsxScriptPlaceholder::new(self_gd, for_object);
        let instance: GDExtensionScriptInstancePtr = placeholder.into();
        instance.cast::<c_void>()
    }

    fn instance_has(&self, object: Gd<Object>) -> bool {
        todo!()
    }

    fn has_source_code(&self) -> bool {
        self.code.is_empty()
    }

    fn get_source_code(&self) -> GString {
        let code = self.code.clone();
        GString::from(code)
    }

    fn set_source_code(&mut self, code: GString) {
        self.code = code.to_string();
        match self.get_session() {
            None => {}
            Some(session) => {
                Gd::<Object>::from_variant(&session).call_deferred("UpdateScript", &[]);
            }
        }
        let mut language = FsxScriptLanguage::singleton().unwrap();
        let mut language = language.bind_mut();
        let self_gd = self.to_gd();
        let path = self_gd.get_path();
        language.scripts.insert(path, self_gd);
    }

    fn reload(&mut self, keep_state: bool) -> Error {
        todo!()
    }

    fn get_documentation(&self) -> Array<Dictionary> {
        todo!()
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

    fn get_property_default_value(&self, property: StringName) -> Variant {
        todo!()
    }

    fn update_exports(&mut self) {
        todo!()
    }

    fn get_script_method_list(&self) -> Array<Dictionary> {
        godot_print!("FSXScript - get_script_property_list");
        Array::new()
    }

    fn get_script_property_list(&self) -> Array<Dictionary> {
        godot_print!("FSXScript - get_script_property_list");
        Array::new()
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

    fn is_placeholder_fallback_enabled(&self) -> bool {
        todo!()
    }

    fn get_rpc_config(&self) -> Variant {
        todo!()
    }
}
