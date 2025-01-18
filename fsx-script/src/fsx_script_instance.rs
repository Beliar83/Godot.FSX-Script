use crate::fsx_script::FsxScript;
use godot::builtin::{StringName, Variant};
use godot::classes::{Engine, ResourceLoader, Script};
use godot::global::godot_error;
use godot::obj::Gd;
use godot::prelude::*;
use godot::sys::{
    get_interface, GDExtensionBool, GDExtensionCallError, GDExtensionConstStringNamePtr,
    GDExtensionConstTypePtr, GDExtensionConstVariantPtr, GDExtensionInt, GDExtensionMethodInfo,
    GDExtensionPropertyInfo, GDExtensionScriptInstanceDataPtr, GDExtensionScriptInstanceInfo3,
    GDExtensionScriptInstancePtr, GDExtensionTypePtr, GDExtensionVariantPtr, GodotFfi, PtrcallType,
    GDEXTENSION_CALL_ERROR_INVALID_METHOD, GDEXTENSION_CALL_OK,
};
use std::collections::HashMap;
// TODO: If possible, combine FsxScriptInstance and FsxScriptPlaceholderInstance

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
    get_method_list_func: Some(FsxScriptInstance::get_method_list),
    free_method_list_func: Some(FsxScriptInstance::free_method_list),
    get_property_type_func: None,
    validate_property_func: None,
    has_method_func: Some(FsxScriptInstance::has_method),
    get_method_argument_count_func: None,
    call_func: Some(FsxScriptInstance::call),
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
    set_func: Some(FsxScriptPlaceholderInstance::set_property),
    get_func: Some(FsxScriptPlaceholderInstance::get_property),
    get_property_list_func: Some(FsxScriptPlaceholderInstance::get_property_list),
    free_property_list_func: Some(FsxScriptPlaceholderInstance::free_property_list),
    get_class_category_func: None,
    property_can_revert_func: None,
    property_get_revert_func: None,
    get_owner_func: None,
    get_property_state_func: None,
    get_method_list_func: Some(FsxScriptPlaceholderInstance::get_method_list),
    free_method_list_func: Some(FsxScriptPlaceholderInstance::free_method_list),
    get_property_type_func: None,
    validate_property_func: None,
    has_method_func: Some(FsxScriptPlaceholderInstance::has_method),
    get_method_argument_count_func: None,
    call_func: Some(FsxScriptPlaceholderInstance::call),
    notification_func: None,
    to_string_func: None,
    refcount_incremented_func: None,
    refcount_decremented_func: None,
    get_script_func: None,
    is_placeholder_func: Some(FsxScriptPlaceholderInstance::is_placeholder),
    set_fallback_func: None,
    get_fallback_func: None,
    get_language_func: None,
    free_func: Some(FsxScriptPlaceholderInstance::free),
};

pub(crate) struct FsxScriptInstance {
    script: Gd<FsxScript>,
    internal_object: Variant,
    object: Gd<Object>,
}

impl FsxScriptInstance {
    pub(crate) fn new(script: Gd<FsxScript>, object: Gd<Object>) -> Self {
        let mut interop_script = ResourceLoader::singleton()
            .load("res://addons/fsx_script/Interop.cs")
            .expect("C# Interop script could not be loaded")
            .cast::<Script>();
        let variant = if !interop_script.is_instance_valid() {
            godot_error!("Could not create C# instance for script");
            Variant::default()
        } else {
            let resource_path = script.clone().upcast::<Resource>().get_path().to_variant();
            interop_script.call(
                "CreateInstanceForObjectAndScript",
                &[object.to_variant(), resource_path],
            )
        };

        Self {
            script,
            internal_object: variant,
            object,
        }
    }

    pub extern "C" fn is_placeholder(
        _p_instance: GDExtensionScriptInstanceDataPtr,
    ) -> GDExtensionBool {
        GDExtensionBool::from(false)
    }

    unsafe extern "C" fn get_property_list(
        p_instance: GDExtensionScriptInstanceDataPtr,
        r_count: *mut u32,
    ) -> *const GDExtensionPropertyInfo {
        let instance: &FsxScriptInstance = p_instance.into();
        let properties = instance.script.bind().get_property_list(r_count);
        properties
    }

    unsafe extern "C" fn free_property_list(
        _p_instance: GDExtensionScriptInstanceDataPtr,
        p_list: *const GDExtensionPropertyInfo,
        p_count: u32,
    ) {
        Vec::<GDExtensionPropertyInfo>::from_raw_parts(
            p_list.cast_mut(),
            p_count as usize,
            p_count as usize,
        );
    }

    unsafe extern "C" fn get_property(
        p_instance: GDExtensionScriptInstanceDataPtr,
        p_name: GDExtensionConstStringNamePtr,
        r_ret: GDExtensionVariantPtr,
    ) -> GDExtensionBool {
        let instance: &FsxScriptInstance = p_instance.into();
        let name = StringName::new_from_sys(p_name as GDExtensionConstTypePtr);
        if instance.script.bind().has_property(&name) {
            let args = [name.to_variant()];
            let value = instance.internal_object.call("GetFsxValue", &args);
            get_interface().variant_duplicate.expect("gdext is not initialized")(
                value.sys() as GDExtensionConstVariantPtr,
                r_ret,
                GDExtensionBool::from(false),
            );
            GDExtensionBool::from(true)
        } else {
            GDExtensionBool::from(false)
        }
    }

    unsafe extern "C" fn set_property(
        p_instance: GDExtensionScriptInstanceDataPtr,
        p_name: GDExtensionConstStringNamePtr,
        p_value: GDExtensionConstVariantPtr,
    ) -> GDExtensionBool {
        let instance: &mut FsxScriptInstance = p_instance.into();
        let name = StringName::new_from_sys(p_name as GDExtensionConstTypePtr);
        if instance.script.bind().has_property(&name) {
            let value = Variant::new_from_sys(p_value as GDExtensionConstTypePtr);
            let args = [name.to_variant(), value];
            instance.internal_object.call("SetFsxValue", &args);
            GDExtensionBool::from(true)
        } else {
            GDExtensionBool::from(false)
        }
    }

    unsafe extern "C" fn free(p_instance: GDExtensionScriptInstanceDataPtr) {
        let _instance: Box<FsxScriptInstance> = Box::from_raw(p_instance.cast());
    }

    unsafe extern "C" fn has_method(
        p_instance: GDExtensionScriptInstanceDataPtr,
        p_name: GDExtensionConstStringNamePtr,
    ) -> GDExtensionBool {
        let instance: &mut FsxScriptInstance = p_instance.into();
        let name = StringName::new_from_sys(p_name as GDExtensionConstTypePtr);
        GDExtensionBool::from(instance.script.bind().has_method(name))
    }

    unsafe extern "C" fn get_method_list(
        p_instance: GDExtensionScriptInstanceDataPtr,
        r_count: *mut u32,
    ) -> *const GDExtensionMethodInfo {
        let instance: &mut FsxScriptInstance = p_instance.into();
        instance.script.bind().get_method_list(r_count)
    }

    unsafe extern "C" fn free_method_list(
        _p_instance: GDExtensionScriptInstanceDataPtr,
        p_list: *const GDExtensionMethodInfo,
        p_count: u32,
    ) {
        Vec::<GDExtensionMethodInfo>::from_raw_parts(
            p_list.cast_mut(),
            p_count as usize,
            p_count as usize,
        );
    }

    unsafe extern "C" fn call(
        p_self: GDExtensionScriptInstanceDataPtr,
        p_method: GDExtensionConstStringNamePtr,
        p_args: *const GDExtensionConstVariantPtr,
        p_argument_count: GDExtensionInt,
        r_return: GDExtensionVariantPtr,
        r_error: *mut GDExtensionCallError,
    ) {
        let instance: &mut FsxScriptInstance = p_self.into();
        let name = StringName::new_from_sys(p_method as GDExtensionConstTypePtr);
        if !instance.script.bind().has_method(name.clone()) {
            let error = GDExtensionCallError {
                error: GDEXTENSION_CALL_ERROR_INVALID_METHOD,
                argument: -1,
                expected: -1,
            };
            *r_error = error;
            return;
        }

        let call_args = std::slice::from_raw_parts(
            p_args as *const GDExtensionConstTypePtr,
            p_argument_count as usize,
        )
        .iter()
        .map(|arg| Variant::new_from_sys(*arg))
        .collect::<Vec<_>>();

        let args = [name.to_variant(), call_args.to_variant()];

        let ret = instance.internal_object.call("CallFsxMethod", &args);
        ret.move_return_ptr(r_return as GDExtensionTypePtr, PtrcallType::Standard);

        let error = GDExtensionCallError {
            error: GDEXTENSION_CALL_OK,
            argument: -1,
            expected: -1,
        };
        *r_error = error;
    }
}

impl From<FsxScriptInstance> for GDExtensionScriptInstancePtr {
    fn from(value: FsxScriptInstance) -> Self {
        let value = Box::<FsxScriptInstance>::from(value);
        unsafe {
            get_interface().script_instance_create3.expect("gdext is not initialized")(
                &INFO,
                Box::into_raw(value) as GDExtensionScriptInstanceDataPtr,
            )
        }
    }
}

impl From<GDExtensionScriptInstanceDataPtr> for &FsxScriptInstance {
    fn from(value: GDExtensionScriptInstanceDataPtr) -> Self {
        assert!(!value.is_null(), "Instance pointer is null");
        unsafe { &*value.cast::<FsxScriptInstance>() }
    }
}

impl From<GDExtensionScriptInstanceDataPtr> for &mut FsxScriptInstance {
    fn from(value: GDExtensionScriptInstanceDataPtr) -> Self {
        assert!(!value.is_null(), "Instance pointer is null");
        unsafe { &mut *value.cast::<FsxScriptInstance>() }
    }
}

pub(super) struct FsxScriptPlaceholderInstance {
    script: Gd<FsxScript>,
    properties: HashMap<StringName, Variant>,
}

impl FsxScriptPlaceholderInstance {
    pub fn new(script: Gd<FsxScript>, _: Gd<Object>) -> Self {
        Self {
            script,
            properties: HashMap::new(),
        }
    }

    pub extern "C" fn is_placeholder(
        _p_instance: GDExtensionScriptInstanceDataPtr,
    ) -> GDExtensionBool {
        GDExtensionBool::from(true)
    }

    unsafe extern "C" fn get_property_list(
        p_instance: GDExtensionScriptInstanceDataPtr,
        r_count: *mut u32,
    ) -> *const GDExtensionPropertyInfo {
        let instance: &FsxScriptPlaceholderInstance = p_instance.into();
        let properties = instance.script.bind().get_property_list(r_count);
        properties
    }

    unsafe extern "C" fn free_property_list(
        _p_instance: GDExtensionScriptInstanceDataPtr,
        p_list: *const GDExtensionPropertyInfo,
        p_count: u32,
    ) {
        Vec::<GDExtensionPropertyInfo>::from_raw_parts(
            p_list.cast_mut(),
            p_count as usize,
            p_count as usize,
        );
    }

    unsafe extern "C" fn get_property(
        p_instance: GDExtensionScriptInstanceDataPtr,
        p_name: GDExtensionConstStringNamePtr,
        r_ret: GDExtensionVariantPtr,
    ) -> GDExtensionBool {
        let instance: &FsxScriptPlaceholderInstance = p_instance.into();
        let name = StringName::new_from_sys(p_name as GDExtensionConstTypePtr);
        if instance.script.bind().has_property(&name) {
            if instance.properties.contains_key(&name) {
                get_interface().variant_duplicate.expect("gdext is not initialized")(
                    instance.properties[&name].sys() as GDExtensionConstVariantPtr,
                    r_ret,
                    GDExtensionBool::from(false),
                );
                GDExtensionBool::from(true)
            } else {
                GDExtensionBool::from(false)
            }
        } else {
            GDExtensionBool::from(false)
        }
    }

    unsafe extern "C" fn set_property(
        p_instance: GDExtensionScriptInstanceDataPtr,
        p_name: GDExtensionConstStringNamePtr,
        p_value: GDExtensionConstVariantPtr,
    ) -> GDExtensionBool {
        let instance: &mut FsxScriptPlaceholderInstance = p_instance.into();
        let name = StringName::new_from_sys(p_name as GDExtensionConstTypePtr);
        if instance.script.bind().has_property(&name) {
            let variant = Variant::new_from_sys(p_value as GDExtensionConstTypePtr);
            instance.properties.insert(name, variant);
            GDExtensionBool::from(true)
        } else {
            GDExtensionBool::from(false)
        }
    }

    unsafe extern "C" fn free(p_instance: GDExtensionScriptInstanceDataPtr) {
        let _instance: Box<FsxScriptPlaceholderInstance> = Box::from_raw(p_instance.cast());
    }

    unsafe extern "C" fn has_method(
        p_instance: GDExtensionScriptInstanceDataPtr,
        p_name: GDExtensionConstStringNamePtr,
    ) -> GDExtensionBool {
        let instance: &mut FsxScriptPlaceholderInstance = p_instance.into();
        let name = StringName::new_from_sys(p_name as GDExtensionConstTypePtr);
        GDExtensionBool::from(instance.script.bind().has_method(name))
    }

    unsafe extern "C" fn get_method_list(
        p_instance: GDExtensionScriptInstanceDataPtr,
        r_count: *mut u32,
    ) -> *const GDExtensionMethodInfo {
        let instance: &mut FsxScriptPlaceholderInstance = p_instance.into();
        instance.script.bind().get_method_list(r_count)
    }

    unsafe extern "C" fn free_method_list(
        _p_instance: GDExtensionScriptInstanceDataPtr,
        p_list: *const GDExtensionMethodInfo,
        p_count: u32,
    ) {
        Vec::<GDExtensionMethodInfo>::from_raw_parts(
            p_list.cast_mut(),
            p_count as usize,
            p_count as usize,
        );
    }

    unsafe extern "C" fn call(
        _p_self: GDExtensionScriptInstanceDataPtr,
        _p_method: GDExtensionConstStringNamePtr,
        _p_args: *const GDExtensionConstVariantPtr,
        _p_argument_count: GDExtensionInt,
        r_return: GDExtensionVariantPtr,
        r_error: *mut GDExtensionCallError,
    ) {
        *r_error = GDExtensionCallError {
            error: GDEXTENSION_CALL_ERROR_INVALID_METHOD,
            argument: -1,
            expected: -1,
        };
        let mut ret = if Engine::singleton().is_editor_hint() {
            "Attempt to call a method on a placeholder instance. Check if the script is in tool mode.".to_variant()
        } else {
            "Attempt to call a method on a placeholder instance. Probably a bug, please report."
                .to_variant()
        };
        *r_return = *(ret.sys_mut() as GDExtensionVariantPtr);
    }
}

impl From<FsxScriptPlaceholderInstance> for GDExtensionScriptInstancePtr {
    fn from(value: FsxScriptPlaceholderInstance) -> Self {
        let value = Box::<FsxScriptPlaceholderInstance>::from(value);
        unsafe {
            get_interface().script_instance_create3.expect("gdext is not initialized")(
                &PLACEHOLDER_INFO,
                Box::into_raw(value) as GDExtensionScriptInstanceDataPtr,
            )
        }
    }
}

impl From<GDExtensionScriptInstanceDataPtr> for &FsxScriptPlaceholderInstance {
    fn from(value: GDExtensionScriptInstanceDataPtr) -> Self {
        assert!(!value.is_null(), "Instance pointer is null");
        unsafe { &*value.cast::<FsxScriptPlaceholderInstance>() }
    }
}

impl From<GDExtensionScriptInstanceDataPtr> for &mut FsxScriptPlaceholderInstance {
    fn from(value: GDExtensionScriptInstanceDataPtr) -> Self {
        assert!(!value.is_null(), "Instance pointer is null");
        unsafe { &mut *value.cast::<FsxScriptPlaceholderInstance>() }
    }
}
