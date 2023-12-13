use std::ptr::{null, null_mut};
use godot::log::godot_error;
use godot::sys;
use godot::sys::{char16_t, GDExtensionConstStringPtr, GDExtensionInt, GDExtensionUninitializedStringPtr, GDExtensionVariantPtr, get_interface, wchar_t};

#[repr(u8)]
pub enum Ownership {
    Native = 1,
    Managed = 2,
}

#[repr(C)]
pub struct GodotString {
    internal_pointer : GDExtensionConstStringPtr,
}

#[repr(C)]
pub struct GodotWideString {
    data: *mut wchar_t,
    length: usize,
    capacity: usize,
    ownership: Ownership,
}

#[repr(C)]
pub struct GodotUTF16String {
    data: *const char16_t,
    length: usize,
}

#[no_mangle]
pub unsafe extern "C" fn convert_godot_string_to_wide_string(string: GodotString) -> GodotWideString {
    let string_to_wide_chars = match get_interface().string_to_wide_chars {
        Some(func) => func,
        None => {
            godot_error!("Godot is not initialized");
            return GodotWideString { data: null_mut(), length: 0, capacity: 0, ownership: Ownership::Native };
        }
    };

    let length = ((string_to_wide_chars(string.internal_pointer, null_mut(), 0)) + 1) as usize;

    if length == 0 {
        return GodotWideString { data: null_mut(), length: 0, capacity: 0, ownership: Ownership::Native };
    }

    let mut data = Vec::<wchar_t>::with_capacity(length);
    string_to_wide_chars(string.internal_pointer, data.as_mut_ptr(), length as GDExtensionInt);

    data.set_len(length);
    data[length - 1] = 0;

    let capacity = data.capacity();

    GodotWideString {
        data: std::mem::ManuallyDrop::new(data).as_mut_ptr(),
        length,
        capacity,
        ownership: Ownership::Native,
    }
}

#[no_mangle]
pub unsafe extern "C" fn convert_godot_string_to_utf16_string(string: GodotString) -> GodotUTF16String {
    let string_to_wide_chars = match get_interface().string_to_utf16_chars {
        Some(func) => func,
        None => {
            godot_error!("Godot is not initialized");
            return GodotUTF16String {data: null(), length : 0 }
        }
    };

    let length = ((string_to_wide_chars(string.internal_pointer, null_mut(), 0)) + 1) as usize;

    if length == 0 {
        return GodotUTF16String {data: null(), length : 0 }
    }

    let mut data = Vec::<char16_t>::with_capacity(length);
    string_to_wide_chars(string.internal_pointer, data.as_mut_ptr(), length as GDExtensionInt);

    data.set_len(length);
    data[length - 1] = 0;

    GodotUTF16String { data: std::mem::ManuallyDrop::new(data).as_ptr(), length }
}

#[no_mangle]
pub unsafe extern "C" fn convert_string_from_wide_string(mut string : GodotWideString) -> GodotString {
    let string_new_with_wide_chars = sys::interface_fn!(string_new_with_wide_chars);

    let mut new_string = std::mem::ManuallyDrop::new(Vec::<u8>::with_capacity(string.length));
    string.capacity = new_string.capacity();
    string_new_with_wide_chars(new_string.as_mut_ptr() as GDExtensionUninitializedStringPtr, string.data);
    new_string.set_len(string.length);
    new_string.set_len(string.length);

    return GodotString { internal_pointer : new_string.as_ptr() as GDExtensionConstStringPtr};
}

#[no_mangle]
pub unsafe extern "C" fn convert_string_from_utf16_string(string : *mut char16_t, length: GDExtensionInt) -> GodotString {
    let string_new_with_utf16_chars = sys::interface_fn!(string_new_with_utf16_chars_and_len);

    let mut new_string = std::mem::ManuallyDrop::new(Vec::<u8>::with_capacity((length + 1) as usize));
    string_new_with_utf16_chars(new_string.as_mut_ptr() as GDExtensionUninitializedStringPtr, string, length);
    new_string.set_len((length + 1) as usize);
    new_string[length as usize] = 0u8;

    return GodotString { internal_pointer : new_string.as_ptr() as GDExtensionConstStringPtr};
}

#[no_mangle]
pub unsafe extern "C" fn delete_godot_string(string : GodotString) {
    get_interface().variant_destroy.unwrap()(string.internal_pointer as GDExtensionVariantPtr)
}

