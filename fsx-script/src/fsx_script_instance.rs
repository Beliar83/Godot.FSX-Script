use godot::builtin::{GString, StringName, Variant, VariantType};
use godot::classes::{Object, Script, ScriptLanguage};
use godot::meta::{MethodInfo, PropertyInfo};
use godot::obj::Gd;
use godot::obj::script::{ScriptInstance, SiMut};
use godot::prelude::godot_print;
use godot::sys::{GDEXTENSION_CALL_ERROR_INVALID_METHOD, GDExtensionBool, GDExtensionPropertyInfo, GDExtensionScriptInstanceDataPtr, GDExtensionScriptInstanceInfo3, GDExtensionScriptInstancePtr, get_interface};

use crate::fsx_script::FsxScript;
use crate::fsx_script_language::FsxScriptLanguage;

static INFO: GDExtensionScriptInstanceInfo3 = GDExtensionScriptInstanceInfo3 {
    set_func: None,
    get_func: None,
    get_property_list_func: None,
    free_property_list_func: None,
    get_class_category_func: None,
    property_can_revert_func: None,
    property_get_revert_func: None,
    get_owner_func: None,
    get_property_state_func: None,
    get_method_list_func: None,
    free_method_list_func: None,
    get_property_type_func: None,
    validate_property_func: None,
    has_method_func: None,
    get_method_argument_count_func: None,
    call_func: None,
    notification_func: None,
    to_string_func: None,
    refcount_incremented_func: None,
    refcount_decremented_func: None,
    get_script_func: None,
    is_placeholder_func: None,
    set_fallback_func: None,
    get_fallback_func: None,
    get_language_func: None,
    free_func: None,
};

static PLACEHOLDER_INFO: GDExtensionScriptInstanceInfo3 = GDExtensionScriptInstanceInfo3 {
    set_func: None,
    get_func: None,
    get_property_list_func: Some(FsxScriptPlaceholder::get_property_list),
    free_property_list_func: Some(FsxScriptPlaceholder::free_property_list),
    get_class_category_func: None,
    property_can_revert_func: None,
    property_get_revert_func: None,
    get_owner_func: None,
    get_property_state_func: None,
    get_method_list_func: None,
    free_method_list_func: None,
    get_property_type_func: None,
    validate_property_func: None,
    has_method_func: None,
    get_method_argument_count_func: None,
    call_func: None,
    notification_func: None,
    to_string_func: None,
    refcount_incremented_func: None,
    refcount_decremented_func: None,
    get_script_func: None,
    is_placeholder_func: Some(FsxScriptPlaceholder::is_placeholder),
    set_fallback_func: None,
    get_fallback_func: None,
    get_language_func: None,
    free_func: None,
};

fn script_class_name(script: &Gd<FsxScript>) -> GString {
    script.bind().get_class_name()
}

pub(crate) struct FsxScriptInstance {
    script: Gd<FsxScript>,
    generic_script: Gd<Script>,
    property_list: Box<[PropertyInfo]>,
    method_list: Box<[MethodInfo]>,
}

impl FsxScriptInstance {
    pub(crate) fn new(script: Gd<FsxScript>) -> Self {
        Self {
            generic_script: script.clone().upcast(),
            property_list: Box::default(),
            method_list: Box::default(),
            script,
        }
    }
}

impl From<FsxScriptInstance> for GDExtensionScriptInstancePtr {
    fn from(value: FsxScriptInstance) -> Self {
        unsafe { get_interface().script_instance_create3.unwrap()(&INFO, std::ptr::addr_of!(value) as GDExtensionScriptInstanceDataPtr) }
    }
}

impl ScriptInstance for FsxScriptInstance {
    type Base = Object;

    fn class_name(&self) -> GString {
        script_class_name(&self.script)
    }

    fn set_property(_this: SiMut<Self>, _name: StringName, _value: &Variant) -> bool {
        false
    }

    fn get_property(&self, _name: StringName) -> Option<Variant> {
        None
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
        godot_print!("{_method}");
        Err(GDEXTENSION_CALL_ERROR_INVALID_METHOD)
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
}

impl FsxScriptPlaceholder {
    pub fn new(script: Gd<FsxScript>) -> Self {
        Self {
            script,
        }
    }

    pub extern fn is_placeholder(_p_instance: GDExtensionScriptInstanceDataPtr) -> GDExtensionBool {
        GDExtensionBool::from(true)
    }

    unsafe extern fn get_property_list(p_instance: GDExtensionScriptInstanceDataPtr, r_count: *mut u32) -> *const GDExtensionPropertyInfo {
        let script: &FsxScript = p_instance.into();
        script.get_property_list(r_count)
    }

    unsafe extern fn free_property_list(_p_instance: GDExtensionScriptInstanceDataPtr, p_list: *const GDExtensionPropertyInfo, p_count: u32) {
        Vec::<GDExtensionPropertyInfo>::from_raw_parts(p_list.cast_mut(), p_count as usize, p_count as usize);
    }
}

impl From<FsxScriptPlaceholder> for GDExtensionScriptInstancePtr {
    fn from(value: FsxScriptPlaceholder) -> Self {
        unsafe { get_interface().script_instance_create3.unwrap()(&PLACEHOLDER_INFO, std::ptr::addr_of!(*value.script.bind()) as GDExtensionScriptInstanceDataPtr) }
    }
}

impl From<GDExtensionScriptInstanceDataPtr> for &FsxScript {
    fn from(value: GDExtensionScriptInstanceDataPtr) -> Self {
        assert!(!value.is_null(), "Instance pointer is null");
        unsafe { value.cast::<FsxScript>().as_ref().unwrap() }
    }
}
