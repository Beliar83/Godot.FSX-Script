use std::collections::HashMap;

use godot::builtin::{GString, StringName, Variant, VariantType};
use godot::classes::{Object, Script, ScriptLanguage};
use godot::obj::script::{SiMut, ScriptInstance};
use godot::meta::{MethodInfo, PropertyInfo};
use godot::obj::Gd;

use crate::fsx_script::FsxScript;
use crate::fsx_script_language::FsxScriptLanguage;

fn script_class_name(script: &Gd<FsxScript>) -> GString {
    script.bind().get_class_name()
}

pub(crate) struct FsxScriptInstance {
    data: Gd<Object>,
    script: Gd<FsxScript>,
    generic_script: Gd<Script>,
    property_list: Box<[PropertyInfo]>,
    method_list: Box<[MethodInfo]>,
}

impl FsxScriptInstance {
    pub(crate) fn new(data: Gd<Object>, script: Gd<FsxScript>) -> Self {
        Self {
            data,
            generic_script: script.clone().upcast(),
            property_list: Box::default(),
            method_list: Box::default(),
            script,
        }
    }
}

impl ScriptInstance for FsxScriptInstance {
    type Base = Object;

    fn class_name(&self) -> GString {
        script_class_name(&self.script)
    }

    fn set_property(_this: SiMut<Self>, _name: StringName, _value: &Variant) -> bool {
        todo!()
    }

    fn get_property(&self, _name: StringName) -> Option<Variant> {
        todo!()
    }

    fn get_property_list(&self) -> Vec<PropertyInfo> {
        self.property_list.to_vec()
    }

    fn get_method_list(&self) -> Vec<MethodInfo> {
        self.method_list.to_vec()
    }

    fn call(
        _this: SiMut<Self>,
        _method: StringName,
        _args: &[&Variant],
    ) -> Result<Variant, godot::sys::GDExtensionCallErrorType> {
        todo!()
    }

    fn is_placeholder(&self) -> bool {
        false
    }

    fn has_method(&self, method_name: StringName) -> bool {
        self.method_list
            .iter()
            .any(|method| method.method_name == method_name)
    }

    fn get_script(&self) -> &Gd<Script> {
        &self.generic_script
    }

    fn get_property_type(&self, name: StringName) -> VariantType {
        self.get_property_list()
            .iter()
            .find(|prop| prop.property_name == name)
            .map(|prop| prop.variant_type)
            .unwrap_or(VariantType::NIL)
    }

    fn to_string(&self) -> GString {
        GString::new()
    }

    fn get_property_state(&self) -> Vec<(StringName, Variant)> {
        self.get_property_list()
            .iter()
            .map(|prop| &prop.property_name)
            .filter_map(|name| {
                self.get_property(name.to_owned())
                    .map(|value| (name.to_owned(), value))
            })
            .collect()
    }

    fn get_language(&self) -> Gd<ScriptLanguage> {
        FsxScriptLanguage::singleton()
            .map(Gd::upcast)
            .expect("FsxScriptLanguage singleton is not initialized")
    }

    fn on_refcount_decremented(&self) -> bool {
        true
    }

    fn on_refcount_incremented(&self) {}

    fn property_get_fallback(&self, _name: StringName) -> Option<Variant> {
        None
    }

    fn property_set_fallback(_this: SiMut<Self>, _name: StringName, _value: &Variant) -> bool {
        false
    }
}

pub(super) struct FsxScriptPlaceholder {
    script: Gd<FsxScript>,
    generic_script: Gd<Script>,
    properties: HashMap<StringName, Variant>,
    property_list: Box<[PropertyInfo]>,
    method_list: Box<[MethodInfo]>,
}

impl FsxScriptPlaceholder {
    pub fn new(script: Gd<FsxScript>) -> Self {
        Self {
            generic_script: script.clone().upcast(),
            properties: Default::default(),
            property_list: Box::default(),
            method_list: Box::default(),
            script,
        }
    }
}

impl ScriptInstance for FsxScriptPlaceholder {
    type Base = Object;

    fn class_name(&self) -> GString {
        script_class_name(&self.script)
    }

    fn set_property(mut this: SiMut<Self>, name: StringName, value: &Variant) -> bool {
        let exists = this
            .get_property_list()
            .iter()
            .any(|prop| prop.property_name == name);

        if !exists {
            return false;
        }

        this.properties.insert(name, value.to_owned());
        true
    }

    fn get_property(&self, name: StringName) -> Option<Variant> {
        self.properties.get(&name).cloned()
    }

    fn get_property_list(&self) -> Vec<PropertyInfo> {
        self.property_list.to_vec()
    }

    fn get_method_list(&self) -> Vec<MethodInfo> {
        self.method_list.to_vec()
    }

    fn call(
        _this: SiMut<Self>,
        _method: StringName,
        _args: &[&Variant],
    ) -> Result<Variant, godot::sys::GDExtensionCallErrorType> {
        Err(godot::sys::GDEXTENSION_CALL_ERROR_INVALID_METHOD)
    }

    fn get_script(&self) -> &Gd<Script> {
        &self.generic_script
    }

    fn has_method(&self, method_name: StringName) -> bool {
        self.get_method_list()
            .iter()
            .any(|method| method.method_name == method_name)
    }

    fn is_placeholder(&self) -> bool {
        true
    }

    fn get_property_type(&self, name: StringName) -> VariantType {
        self.get_property_list()
            .iter()
            .find(|prop| prop.property_name == name)
            .map(|prop| prop.variant_type)
            .unwrap_or(VariantType::NIL)
    }

    fn to_string(&self) -> GString {
        GString::new()
    }

    fn get_property_state(&self) -> Vec<(StringName, Variant)> {
        self.properties
            .iter()
            .map(|(name, value)| (name.to_owned(), value.to_owned()))
            .collect()
    }

    fn get_language(&self) -> Gd<ScriptLanguage> {
        FsxScriptLanguage::singleton()
            .map(Gd::upcast)
            .expect("FsxScriptLanguage singleton is not initialized")
    }

    fn on_refcount_decremented(&self) -> bool {
        true
    }

    fn on_refcount_incremented(&self) {}

    fn property_get_fallback(&self, _name: StringName) -> Option<Variant> {
        None
    }

    fn property_set_fallback(_this: SiMut<Self>, _name: StringName, _value: &Variant) -> bool {
        false
    }
}
