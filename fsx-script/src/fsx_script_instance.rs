use crate::fsx_script::FsxScript;
use godot::builtin::{StringName, Variant};
use godot::obj::Gd;
use godot::prelude::{Object, Var};
use godot::sys::{get_interface, GDExtensionBool, GDExtensionConstStringNamePtr, GDExtensionConstTypePtr, GDExtensionConstVariantPtr, GDExtensionPropertyInfo, GDExtensionScriptInstanceDataPtr, GDExtensionScriptInstanceInfo3, GDExtensionScriptInstancePtr, GDExtensionVariantPtr, GodotFfi};
use std::collections::HashMap;

// TODO: If possible, combine Instance and PlaceholderInstance 

static INFO: GDExtensionScriptInstanceInfo3 = GDExtensionScriptInstanceInfo3 {
    set_func: Some(FsxScriptInstance::set_property),
    get_func: Some(FsxScriptInstance::get_property),
    get_property_list_func: Some(FsxScriptInstance::get_property_list),
    free_property_list_func: Some(FsxScriptInstance::free_property_list),
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
    is_placeholder_func: Some(FsxScriptInstance::is_placeholder),
    set_fallback_func: None,
    get_fallback_func: None,
    get_language_func: None,
    free_func: Some(FsxScriptInstance::free),
};

static PLACEHOLDER_INFO: GDExtensionScriptInstanceInfo3 = GDExtensionScriptInstanceInfo3 {
    set_func: Some(FsxScriptPlaceholder::set_property),
    get_func: Some(FsxScriptPlaceholder::get_property),
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
    free_func: Some(FsxScriptPlaceholder::free),
};

pub(crate) struct FsxScriptInstance {
    script: Gd<FsxScript>,
    object: Gd<Object>,
}

impl FsxScriptInstance {
    pub(crate) fn new(script: Gd<FsxScript>, object: Gd<Object>) -> Self {
        Self {
            script,
            object
        }
    }

    pub extern fn is_placeholder(_p_instance: GDExtensionScriptInstanceDataPtr) -> GDExtensionBool {
        GDExtensionBool::from(false)
    }

    unsafe extern fn get_property_list(p_instance: GDExtensionScriptInstanceDataPtr, r_count: *mut u32) -> *const GDExtensionPropertyInfo {
        let instance: &FsxScriptPlaceholder = p_instance.into();
        let properties = instance.script.bind().get_property_list(r_count);
        properties
    }

    unsafe extern fn free_property_list(_p_instance: GDExtensionScriptInstanceDataPtr, p_list: *const GDExtensionPropertyInfo, p_count: u32) {
        Vec::<GDExtensionPropertyInfo>::from_raw_parts(p_list.cast_mut(), p_count as usize, p_count as usize);
    }

    unsafe extern fn get_property(p_instance: GDExtensionScriptInstanceDataPtr, p_name: GDExtensionConstStringNamePtr, r_ret: GDExtensionVariantPtr,
    ) -> GDExtensionBool {
        let instance: &FsxScriptPlaceholder = p_instance.into();
        let name = StringName::new_from_sys(p_name as GDExtensionConstTypePtr);
        if instance.script.bind().has_property(&name) {
            if instance.properties.contains_key(&name) {
                get_interface().variant_duplicate.unwrap()(instance.properties[&name].sys() as GDExtensionConstVariantPtr, r_ret, GDExtensionBool::from(false));
                GDExtensionBool::from(true)
            } else {
                GDExtensionBool::from(false)

            }
        } else {
            GDExtensionBool::from(false)
        }
    }

    unsafe extern fn set_property(p_instance: GDExtensionScriptInstanceDataPtr, p_name: GDExtensionConstStringNamePtr, p_value: GDExtensionConstVariantPtr,
    ) -> GDExtensionBool {
        let instance: &mut FsxScriptPlaceholder = p_instance.into();
        let name = StringName::new_from_sys(p_name as GDExtensionConstTypePtr);
        if instance.script.bind().has_property(&name) {
            let variant = Variant::new_from_sys(p_value as GDExtensionConstTypePtr);
            instance.properties.insert(name, variant);
            GDExtensionBool::from(true)
        } else {
            GDExtensionBool::from(false)
        }
    }

    unsafe extern fn free(p_instance: GDExtensionScriptInstanceDataPtr) {
        let _instance: Box<FsxScriptInstance> = Box::from_raw(p_instance.cast());
    }    
}

impl From<FsxScriptInstance> for GDExtensionScriptInstancePtr {
    fn from(value: FsxScriptInstance) -> Self {
        unsafe { get_interface().script_instance_create3.unwrap()(&INFO, std::ptr::addr_of!(*value.script.bind()) as GDExtensionScriptInstanceDataPtr) }
    }
}


pub(super) struct FsxScriptPlaceholder {
    script: Gd<FsxScript>,
    object: Gd<Object>,
    properties: HashMap<StringName, Variant>,
}

impl FsxScriptPlaceholder {
    pub fn new(script: Gd<FsxScript>, object: Gd<Object>) -> Self {
        Self {
            script,
            object,
            properties: HashMap::new(),
        }
    }

    pub extern fn is_placeholder(_p_instance: GDExtensionScriptInstanceDataPtr) -> GDExtensionBool {
        GDExtensionBool::from(true)
    }

    unsafe extern fn get_property_list(p_instance: GDExtensionScriptInstanceDataPtr, r_count: *mut u32) -> *const GDExtensionPropertyInfo {
        let instance: &FsxScriptPlaceholder = p_instance.into();
        let properties = instance.script.bind().get_property_list(r_count);
        properties
    }

    unsafe extern fn free_property_list(_p_instance: GDExtensionScriptInstanceDataPtr, p_list: *const GDExtensionPropertyInfo, p_count: u32) {
        Vec::<GDExtensionPropertyInfo>::from_raw_parts(p_list.cast_mut(), p_count as usize, p_count as usize);
    }

    unsafe extern fn get_property(p_instance: GDExtensionScriptInstanceDataPtr, p_name: GDExtensionConstStringNamePtr, r_ret: GDExtensionVariantPtr,
    ) -> GDExtensionBool {
        let instance: &FsxScriptPlaceholder = p_instance.into();
        let name = StringName::new_from_sys(p_name as GDExtensionConstTypePtr);
        if instance.script.bind().has_property(&name) {
            if instance.properties.contains_key(&name) {               
                get_interface().variant_duplicate.unwrap()(instance.properties[&name].sys() as GDExtensionConstVariantPtr, r_ret, GDExtensionBool::from(false));
                GDExtensionBool::from(true)
            } else {
                GDExtensionBool::from(false)
                
            }
        } else {
            GDExtensionBool::from(false)
        }    
    }

    unsafe extern fn set_property(p_instance: GDExtensionScriptInstanceDataPtr, p_name: GDExtensionConstStringNamePtr, p_value: GDExtensionConstVariantPtr,
    ) -> GDExtensionBool {
        let instance: &mut FsxScriptPlaceholder = p_instance.into();
        let name = StringName::new_from_sys(p_name as GDExtensionConstTypePtr);
        if instance.script.bind().has_property(&name) {
            let variant = Variant::new_from_sys(p_value as GDExtensionConstTypePtr);
            instance.properties.insert(name, variant);
            GDExtensionBool::from(true)
        } else {
            GDExtensionBool::from(false)
        }
    }

    unsafe extern fn free(p_instance: GDExtensionScriptInstanceDataPtr) {
        let _instance: Box<FsxScriptPlaceholder> = Box::from_raw(p_instance.cast());
    }

}

impl From<FsxScriptPlaceholder> for GDExtensionScriptInstancePtr {
    fn from(value: FsxScriptPlaceholder) -> Self {
        let value = Box::<FsxScriptPlaceholder>::from(value);
        unsafe { get_interface().script_instance_create3.unwrap()(&PLACEHOLDER_INFO, Box::into_raw(value) as GDExtensionScriptInstanceDataPtr) }
    }
}

impl From<GDExtensionScriptInstanceDataPtr> for &FsxScriptPlaceholder {
    fn from(value: GDExtensionScriptInstanceDataPtr) -> Self {
        assert!(!value.is_null(), "Instance pointer is null");
        unsafe { &*value.cast::<FsxScriptPlaceholder>() }
    }
}

impl From<GDExtensionScriptInstanceDataPtr> for &mut FsxScriptPlaceholder {
    fn from(value: GDExtensionScriptInstanceDataPtr) -> Self {
        assert!(!value.is_null(), "Instance pointer is null");
        unsafe { &mut *value.cast::<FsxScriptPlaceholder>() }
    }
}
