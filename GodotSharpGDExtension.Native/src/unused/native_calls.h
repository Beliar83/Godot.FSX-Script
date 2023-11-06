#pragma once
#include "godot_cpp/godot.hpp"

struct NativeCalls
{
public extern "C" mem_alloc;
public extern "C" mem_realloc;
public extern "C" mem_free;
public extern "C" print_error;
public extern "C" print_error_with_message;
public extern "C" print_warning;
public extern "C" print_warning_with_message;
public extern "C" print_script_error;
public extern "C" print_script_error_with_message;
public extern "C" get_native_struct_size;
public extern "C" variant_new_copy;
public extern "C" variant_new_nil;
public extern "C" variant_destroy;

public extern "C"
    variant_call;

public delegate* unmanaged<GDExtensionVariantType, IntPtr, IntPtr, long, IntPtr, Native.GDExtensionCallError
                                                                                 *, void> variant_call_static;

public extern "C"
    variant_evaluate;

public extern "C" variant_set;
public extern "C" variant_set_named;
public extern "C" variant_set_keyed;
public extern "C" variant_set_indexed;
public extern "C" variant_get;
public extern "C" variant_get_named;
public extern "C" variant_get_keyed;
public extern "C" variant_get_indexed;
public extern "C" variant_iter_init;
public extern "C" variant_iter_next;
public extern "C" variant_iter_get;
public extern "C" variant_hash;
public extern "C" variant_recursive_hash;
public extern "C" variant_hash_compare;
public extern "C" variant_booleanize;
public extern "C" variant_duplicate;
public extern "C" variant_stringify;
public extern "C" variant_get_type;
public extern "C" variant_has_method;
public extern "C" variant_has_member;
public extern "C" variant_has_key;
public extern "C" variant_get_type_name;
public extern "C" variant_can_convert;

public extern "C"
    variant_can_convert_strict;

public extern "C"
    get_variant_from_type_constructor;

public extern "C"
    get_variant_to_type_constructor;

public delegate* unmanaged<Native.GDExtensionVariantOperator, Native.GDExtensionVariantType,
            Native.GDExtensionVariantType, Native.GDExtensionPtrOperatorEvaluator>
    variant_get_ptr_operator_evaluator;

public extern "C"
    variant_get_ptr_builtin_method;

public extern "C"
    variant_get_ptr_constructor;

public extern "C"
    variant_get_ptr_destructor;

public extern "C"
    variant_construct;

public extern "C"
    variant_get_ptr_setter;

public extern "C"
    variant_get_ptr_getter;

public extern "C"
    variant_get_ptr_indexed_setter;

public extern "C"
    variant_get_ptr_indexed_getter;

public extern "C"
    variant_get_ptr_keyed_setter;

public extern "C"
    variant_get_ptr_keyed_getter;

public extern "C"
    variant_get_ptr_keyed_checker;

public extern "C" variant_get_constant_value;

public extern "C"
    variant_get_ptr_utility_function;

public extern "C" string_new_with_latin1_chars;
public extern "C" string_new_with_utf8_chars;
public extern "C" string_new_with_utf16_chars;
public extern "C" string_new_with_utf32_chars;
public extern "C" string_new_with_wide_chars;
public extern "C" string_new_with_latin1_chars_and_len;
public extern "C" string_new_with_utf8_chars_and_len;
public extern "C" string_new_with_utf16_chars_and_len;
public extern "C" string_new_with_utf32_chars_and_len;
public extern "C" string_new_with_wide_chars_and_len;
public extern "C" string_to_latin1_chars;
public extern "C" string_to_utf8_chars;
public extern "C" string_to_utf16_chars;
public extern "C" string_to_utf32_chars;
public extern "C" string_to_wide_chars;
public extern "C" string_operator_index;
public extern "C" string_operator_index_const;
public extern "C" string_operator_plus_eq_string;
public extern "C" string_operator_plus_eq_char;
public extern "C" string_operator_plus_eq_cstr;
public extern "C" string_operator_plus_eq_wcstr;
public extern "C" string_operator_plus_eq_c32str;
public extern "C" xml_parser_open_buffer;
public extern "C" file_access_store_buffer;
public extern "C" file_access_get_buffer;

public extern "C"
    worker_thread_pool_add_native_group_task;

public extern "C"
    worker_thread_pool_add_native_task;

public extern "C" packed_byte_array_operator_index;
public extern "C" packed_byte_array_operator_index_const;
public extern "C" packed_color_array_operator_index;
public extern "C" packed_color_array_operator_index_const;
public extern "C" packed_float32_array_operator_index;
public extern "C" packed_float32_array_operator_index_const;
public extern "C" packed_float64_array_operator_index;
public extern "C" packed_float64_array_operator_index_const;
public extern "C" packed_int32_array_operator_index;
public extern "C" packed_int32_array_operator_index_const;
public extern "C" packed_int64_array_operator_index;
public extern "C" packed_int64_array_operator_index_const;
public extern "C" packed_string_array_operator_index;
public extern "C" packed_string_array_operator_index_const;
public extern "C" packed_vector2_array_operator_index;
public extern "C" packed_vector2_array_operator_index_const;
public extern "C" packed_vector3_array_operator_index;
public extern "C" packed_vector3_array_operator_index_const;
public extern "C" array_operator_index;
public extern "C" array_operator_index_const;
public extern "C" array_ref;
public extern "C" array_set_typed;
public extern "C" dictionary_operator_index;
public extern "C" dictionary_operator_index_const;

public extern "C"
    object_method_bind_call;

public extern "C" object_method_bind_ptrcall;
public extern "C" object_destroy;
public extern "C" global_get_singleton;

public extern "C"
    object_get_instance_binding;

public extern "C"
    object_set_instance_binding;

public extern "C" object_set_instance;
public extern "C" object_cast_to;
public extern "C" object_get_instance_from_id;
public extern "C" object_get_instance_id;
public extern "C" ref_get_object;
public extern "C" ref_set_object;
public extern "C" script_instance_create;
public extern "C" classdb_construct_object;
public extern "C" classdb_get_method_bind;
public extern "C" classdb_get_class_tag;

public extern "C"
    classdb_register_extension_class;

public extern "C"
    classdb_register_extension_class_method;

public extern "C"
    classdb_register_extension_class_integer_constant;

public extern "C"
    classdb_register_extension_class_property;

public extern "C"
    classdb_register_extension_class_property_group;

public extern "C"
    classdb_register_extension_class_property_subgroup;

public extern "C"
    classdb_register_extension_class_signal;

public extern "C" classdb_unregister_extension_class;
public extern "C" get_library_path;
}