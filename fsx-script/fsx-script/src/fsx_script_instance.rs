use godot::obj::Gd;
use godot::sys::{GDExtensionBool, GDExtensionConstStringNamePtr, GDExtensionConstVariantPtr, GDExtensionPropertyInfo, GDExtensionScriptInstanceDataPtr, GDExtensionScriptInstanceInfo3, GDExtensionScriptInstancePtr, GDExtensionVariantPtr, get_interface};

use crate::fsx_script::FsxScript;

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
    free_func: None,
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
    free_func: None,
};

pub(crate) struct FsxScriptInstance {
    script: Gd<FsxScript>,
}

impl FsxScriptInstance {
    pub(crate) fn new(script: Gd<FsxScript>) -> Self {
        Self {
            script,
        }
    }

    pub extern fn is_placeholder(_p_instance: GDExtensionScriptInstanceDataPtr) -> GDExtensionBool {
        GDExtensionBool::from(false)
    }

    unsafe extern fn get_property_list(p_instance: GDExtensionScriptInstanceDataPtr, r_count: *mut u32) -> *const GDExtensionPropertyInfo {
        let script: &FsxScript = p_instance.into();
        script.get_property_list(r_count)
    }

    unsafe extern fn free_property_list(_p_instance: GDExtensionScriptInstanceDataPtr, p_list: *const GDExtensionPropertyInfo, p_count: u32) {
        Vec::<GDExtensionPropertyInfo>::from_raw_parts(p_list.cast_mut(), p_count as usize, p_count as usize);
    }

    unsafe extern fn get_property(p_instance: GDExtensionScriptInstanceDataPtr, p_name: GDExtensionConstStringNamePtr, r_ret: GDExtensionVariantPtr,
    ) -> GDExtensionBool {
        let script: &FsxScript = p_instance.into();
        script.get_property(p_name, r_ret)
    }

    unsafe extern fn set_property(p_instance: GDExtensionScriptInstanceDataPtr, p_name: GDExtensionConstStringNamePtr, p_value: GDExtensionConstVariantPtr,
    ) -> GDExtensionBool {
        let script: &mut FsxScript = p_instance.into();
        script.set_property(p_name, p_value)
    }
}

impl From<FsxScriptInstance> for GDExtensionScriptInstancePtr {
    fn from(value: FsxScriptInstance) -> Self {
        unsafe { get_interface().script_instance_create3.unwrap()(&INFO, std::ptr::addr_of!(*value.script.bind()) as GDExtensionScriptInstanceDataPtr) }
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

    unsafe extern fn get_property(p_instance: GDExtensionScriptInstanceDataPtr, p_name: GDExtensionConstStringNamePtr, r_ret: GDExtensionVariantPtr,
    ) -> GDExtensionBool {
        let script: &FsxScript = p_instance.into();
        script.get_property(p_name, r_ret)
    }

    unsafe extern fn set_property(p_instance: GDExtensionScriptInstanceDataPtr, p_name: GDExtensionConstStringNamePtr, p_value: GDExtensionConstVariantPtr,
    ) -> GDExtensionBool {
        let script: &mut FsxScript = p_instance.into();
        script.set_property(p_name, p_value)
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

impl From<GDExtensionScriptInstanceDataPtr> for &mut FsxScript {
    fn from(value: GDExtensionScriptInstanceDataPtr) -> Self {
        assert!(!value.is_null(), "Instance pointer is null");
        unsafe { value.cast::<FsxScript>().as_mut().unwrap() }
    }
}