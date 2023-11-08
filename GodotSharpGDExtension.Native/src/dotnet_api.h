#pragma once

#include "gdextension_interface.h"

#ifndef __castxml__ // This avoids issues with clang version of CastXML not matching the one of VS. Also, it is not needed for the wrapper. 
#include "godot_cpp/core/defs.hpp"
#include <vector>
#else
#define GDE_EXPORT
#endif

typedef void (*de_init)(GDExtensionInitializationLevel p_level);

typedef struct {
    de_init initialize;
    de_init uninitialize;
} DotnetInitialization;

#ifndef __castxml__
extern std::vector<DotnetInitialization> initializations;
#endif

extern "C" {
GDE_EXPORT void register_class(const char* p_name, const char* p_parent_name, const GDExtensionClassCreationInfo* p_extension_funcs);
GDE_EXPORT GDExtensionConstStringNamePtr create_string_name(const char* name);
GDE_EXPORT void add_extension_library(de_init initialize, de_init uninitialize);
GDE_EXPORT GDExtensionClassLibraryPtr get_library();
GDE_EXPORT GDExtensionInterfaceFunctionPtr get_proc_address(const char* p_function_name);
GDE_EXPORT void get_godot_version(GDExtensionGodotVersion* r_godot_version);
GDE_EXPORT void* mem_alloc(size_t p_bytes);
GDE_EXPORT void* mem_realloc(void* p_ptr, size_t p_bytes);
GDE_EXPORT void mem_free(void* p_ptr);
GDE_EXPORT void print_error(const char* p_description, const char* p_function, const char* p_file, int32_t p_line, GDExtensionBool p_editor_notify);
GDE_EXPORT void print_error_with_message(const char* p_description, const char* p_message, const char* p_function, const char* p_file, int32_t p_line, GDExtensionBool p_editor_notify);
GDE_EXPORT void print_warning(const char* p_description, const char* p_function, const char* p_file, int32_t p_line, GDExtensionBool p_editor_notify);
GDE_EXPORT void print_warning_with_message(const char* p_description, const char* p_message, const char* p_function, const char* p_file, int32_t p_line, GDExtensionBool p_editor_notify);
GDE_EXPORT void print_script_error(const char* p_description, const char* p_function, const char* p_file, int32_t p_line, GDExtensionBool p_editor_notify);
GDE_EXPORT void print_script_error_with_message(const char* p_description, const char* p_message, const char* p_function, const char* p_file, int32_t p_line, GDExtensionBool p_editor_notify);
GDE_EXPORT uint64_t get_native_struct_size(GDExtensionConstStringNamePtr p_name);
GDE_EXPORT void variant_new_copy(GDExtensionUninitializedVariantPtr r_dest, GDExtensionConstVariantPtr p_src);
GDE_EXPORT void variant_new_nil(GDExtensionUninitializedVariantPtr r_dest);
GDE_EXPORT void variant_destroy(GDExtensionVariantPtr p_self);
GDE_EXPORT void variant_call(GDExtensionVariantPtr p_self, GDExtensionConstStringNamePtr p_method, const GDExtensionConstVariantPtr* p_args, GDExtensionInt p_argument_count, GDExtensionUninitializedVariantPtr r_return, GDExtensionCallError* r_error);
GDE_EXPORT void variant_call_static(GDExtensionVariantType p_type, GDExtensionConstStringNamePtr p_method, const GDExtensionConstVariantPtr* p_args, GDExtensionInt p_argument_count, GDExtensionUninitializedVariantPtr r_return, GDExtensionCallError* r_error);
GDE_EXPORT void variant_evaluate(GDExtensionVariantOperator p_op, GDExtensionConstVariantPtr p_a, GDExtensionConstVariantPtr p_b, GDExtensionUninitializedVariantPtr r_return, GDExtensionBool* r_valid);
GDE_EXPORT void variant_set(GDExtensionVariantPtr p_self, GDExtensionConstVariantPtr p_key, GDExtensionConstVariantPtr p_value, GDExtensionBool* r_valid);
GDE_EXPORT void variant_set_named(GDExtensionVariantPtr p_self, GDExtensionConstStringNamePtr p_key, GDExtensionConstVariantPtr p_value, GDExtensionBool* r_valid);
GDE_EXPORT void variant_set_keyed(GDExtensionVariantPtr p_self, GDExtensionConstVariantPtr p_key, GDExtensionConstVariantPtr p_value, GDExtensionBool* r_valid);
GDE_EXPORT void variant_set_indexed(GDExtensionVariantPtr p_self, GDExtensionInt p_index, GDExtensionConstVariantPtr p_value, GDExtensionBool* r_valid, GDExtensionBool* r_oob);
GDE_EXPORT void variant_get(GDExtensionConstVariantPtr p_self, GDExtensionConstVariantPtr p_key, GDExtensionUninitializedVariantPtr r_ret, GDExtensionBool* r_valid);
GDE_EXPORT void variant_get_named(GDExtensionConstVariantPtr p_self, GDExtensionConstStringNamePtr p_key, GDExtensionUninitializedVariantPtr r_ret, GDExtensionBool* r_valid);
GDE_EXPORT void variant_get_keyed(GDExtensionConstVariantPtr p_self, GDExtensionConstVariantPtr p_key, GDExtensionUninitializedVariantPtr r_ret, GDExtensionBool* r_valid);
GDE_EXPORT void variant_get_indexed(GDExtensionConstVariantPtr p_self, GDExtensionInt p_index, GDExtensionUninitializedVariantPtr r_ret, GDExtensionBool* r_valid, GDExtensionBool* r_oob);
GDE_EXPORT GDExtensionBool variant_iter_init(GDExtensionConstVariantPtr p_self, GDExtensionUninitializedVariantPtr r_iter, GDExtensionBool* r_valid);
GDE_EXPORT GDExtensionBool variant_iter_next(GDExtensionConstVariantPtr p_self, GDExtensionVariantPtr r_iter, GDExtensionBool* r_valid);
GDE_EXPORT void variant_iter_get(GDExtensionConstVariantPtr p_self, GDExtensionVariantPtr r_iter, GDExtensionUninitializedVariantPtr r_ret, GDExtensionBool* r_valid);
GDE_EXPORT GDExtensionInt variant_hash(GDExtensionConstVariantPtr p_self);
GDE_EXPORT GDExtensionInt variant_recursive_hash(GDExtensionConstVariantPtr p_self, GDExtensionInt p_recursion_count);
GDE_EXPORT GDExtensionBool variant_hash_compare(GDExtensionConstVariantPtr p_self, GDExtensionConstVariantPtr p_other);
GDE_EXPORT GDExtensionBool variant_booleanize(GDExtensionConstVariantPtr p_self);
GDE_EXPORT void variant_duplicate(GDExtensionConstVariantPtr p_self, GDExtensionVariantPtr r_ret, GDExtensionBool p_deep);
GDE_EXPORT void variant_stringify(GDExtensionConstVariantPtr p_self, GDExtensionStringPtr r_ret);
GDE_EXPORT GDExtensionVariantType variant_get_type(GDExtensionConstVariantPtr p_self);
GDE_EXPORT GDExtensionBool variant_has_method(GDExtensionConstVariantPtr p_self, GDExtensionConstStringNamePtr p_method);
GDE_EXPORT GDExtensionBool variant_has_member(GDExtensionVariantType p_type, GDExtensionConstStringNamePtr p_member);
GDE_EXPORT GDExtensionBool variant_has_key(GDExtensionConstVariantPtr p_self, GDExtensionConstVariantPtr p_key, GDExtensionBool* r_valid);
GDE_EXPORT void variant_get_type_name(GDExtensionVariantType p_type, GDExtensionUninitializedStringPtr r_name);
GDE_EXPORT GDExtensionBool variant_can_convert(GDExtensionVariantType p_from, GDExtensionVariantType p_to);
GDE_EXPORT GDExtensionBool variant_can_convert_strict(GDExtensionVariantType p_from, GDExtensionVariantType p_to);
GDE_EXPORT GDExtensionVariantFromTypeConstructorFunc get_variant_from_type_constructor(GDExtensionVariantType p_type);
GDE_EXPORT GDExtensionTypeFromVariantConstructorFunc get_variant_to_type_constructor(GDExtensionVariantType p_type);
GDE_EXPORT GDExtensionPtrOperatorEvaluator variant_get_ptr_operator_evaluator(GDExtensionVariantOperator p_operator, GDExtensionVariantType p_type_a, GDExtensionVariantType p_type_b);
GDE_EXPORT GDExtensionPtrBuiltInMethod variant_get_ptr_builtin_method(GDExtensionVariantType p_type, GDExtensionConstStringNamePtr p_method, GDExtensionInt p_hash);
GDE_EXPORT GDExtensionPtrConstructor variant_get_ptr_constructor(GDExtensionVariantType p_type, int32_t p_constructor);
GDE_EXPORT GDExtensionPtrDestructor variant_get_ptr_destructor(GDExtensionVariantType p_type);
GDE_EXPORT void variant_construct(GDExtensionVariantType p_type, GDExtensionUninitializedVariantPtr r_base, const GDExtensionConstVariantPtr* p_args, int32_t p_argument_count, GDExtensionCallError* r_error);
GDE_EXPORT GDExtensionPtrSetter variant_get_ptr_setter(GDExtensionVariantType p_type, GDExtensionConstStringNamePtr p_member);
GDE_EXPORT GDExtensionPtrGetter variant_get_ptr_getter(GDExtensionVariantType p_type, GDExtensionConstStringNamePtr p_member);
GDE_EXPORT GDExtensionPtrIndexedSetter variant_get_ptr_indexed_setter(GDExtensionVariantType p_type);
GDE_EXPORT GDExtensionPtrIndexedGetter variant_get_ptr_indexed_getter(GDExtensionVariantType p_type);
GDE_EXPORT GDExtensionPtrKeyedSetter variant_get_ptr_keyed_setter(GDExtensionVariantType p_type);
GDE_EXPORT GDExtensionPtrKeyedGetter variant_get_ptr_keyed_getter(GDExtensionVariantType p_type);
GDE_EXPORT GDExtensionPtrKeyedChecker variant_get_ptr_keyed_checker(GDExtensionVariantType p_type);
GDE_EXPORT void variant_get_constant_value(GDExtensionVariantType p_type, GDExtensionConstStringNamePtr p_constant, GDExtensionUninitializedVariantPtr r_ret);
GDE_EXPORT GDExtensionPtrUtilityFunction variant_get_ptr_utility_function(GDExtensionConstStringNamePtr p_function, GDExtensionInt p_hash);
GDE_EXPORT void string_new_with_latin1_chars(GDExtensionUninitializedStringPtr r_dest, const char* p_contents);
GDE_EXPORT void string_new_with_utf8_chars(GDExtensionUninitializedStringPtr r_dest, const char* p_contents);
GDE_EXPORT void string_new_with_wide_chars(GDExtensionUninitializedStringPtr r_dest, const wchar_t* p_contents);
GDE_EXPORT void string_new_with_latin1_chars_and_len(GDExtensionUninitializedStringPtr r_dest, const char* p_contents, GDExtensionInt p_size);
GDE_EXPORT void string_new_with_utf8_chars_and_len(GDExtensionUninitializedStringPtr r_dest, const char* p_contents, GDExtensionInt p_size);
GDE_EXPORT void string_new_with_wide_chars_and_len(GDExtensionUninitializedStringPtr r_dest, const wchar_t* p_contents, GDExtensionInt p_size);
GDE_EXPORT GDExtensionInt string_to_latin1_chars(GDExtensionConstStringPtr p_self, char* r_text, GDExtensionInt p_max_write_length);
GDE_EXPORT GDExtensionInt string_to_utf8_chars(GDExtensionConstStringPtr p_self, char* r_text, GDExtensionInt p_max_write_length);
GDE_EXPORT GDExtensionInt string_to_wide_chars(GDExtensionConstStringPtr p_self, wchar_t* r_text, GDExtensionInt p_max_write_length);
GDE_EXPORT void string_operator_plus_eq_string(GDExtensionStringPtr p_self, GDExtensionConstStringPtr p_b);
GDE_EXPORT void string_operator_plus_eq_cstr(GDExtensionStringPtr p_self, const char* p_b);
GDE_EXPORT void string_operator_plus_eq_wcstr(GDExtensionStringPtr p_self, const wchar_t* p_b);
GDE_EXPORT GDExtensionInt xml_parser_open_buffer(GDExtensionObjectPtr p_instance, const uint8_t* p_buffer, size_t p_size);
GDE_EXPORT void file_access_store_buffer(GDExtensionObjectPtr p_instance, const uint8_t* p_src, uint64_t p_length);
GDE_EXPORT uint64_t file_access_get_buffer(GDExtensionConstObjectPtr p_instance, uint8_t* p_dst, uint64_t p_length);
GDE_EXPORT int64_t worker_thread_pool_add_native_group_task(GDExtensionObjectPtr p_instance, void(*p_func)(void*, uint32_t), void* p_userdata, int p_elements, int p_tasks, GDExtensionBool p_high_priority, GDExtensionConstStringPtr p_description);
GDE_EXPORT int64_t worker_thread_pool_add_native_task(GDExtensionObjectPtr p_instance, void(*p_func)(void*), void* p_userdata, GDExtensionBool p_high_priority, GDExtensionConstStringPtr p_description);
GDE_EXPORT uint8_t* packed_byte_array_operator_index(GDExtensionTypePtr p_self, GDExtensionInt p_index);
GDE_EXPORT const uint8_t* packed_byte_array_operator_index_const(GDExtensionConstTypePtr p_self, GDExtensionInt p_index);
GDE_EXPORT GDExtensionTypePtr packed_color_array_operator_index(GDExtensionTypePtr p_self, GDExtensionInt p_index);
GDE_EXPORT GDExtensionTypePtr packed_color_array_operator_index_const(GDExtensionConstTypePtr p_self, GDExtensionInt p_index);
GDE_EXPORT float* packed_float32_array_operator_index(GDExtensionTypePtr p_self, GDExtensionInt p_index);
GDE_EXPORT const float* packed_float32_array_operator_index_const(GDExtensionConstTypePtr p_self, GDExtensionInt p_index);
GDE_EXPORT double* packed_float64_array_operator_index(GDExtensionTypePtr p_self, GDExtensionInt p_index);
GDE_EXPORT const double* packed_float64_array_operator_index_const(GDExtensionConstTypePtr p_self, GDExtensionInt p_index);
GDE_EXPORT int32_t* packed_int32_array_operator_index(GDExtensionTypePtr p_self, GDExtensionInt p_index);
GDE_EXPORT const int32_t* packed_int32_array_operator_index_const(GDExtensionConstTypePtr p_self, GDExtensionInt p_index);
GDE_EXPORT int64_t* packed_int64_array_operator_index(GDExtensionTypePtr p_self, GDExtensionInt p_index);
GDE_EXPORT const int64_t* packed_int64_array_operator_index_const(GDExtensionConstTypePtr p_self, GDExtensionInt p_index);
GDE_EXPORT GDExtensionStringPtr packed_string_array_operator_index(GDExtensionTypePtr p_self, GDExtensionInt p_index);
GDE_EXPORT GDExtensionStringPtr packed_string_array_operator_index_const(GDExtensionConstTypePtr p_self, GDExtensionInt p_index);
GDE_EXPORT GDExtensionTypePtr packed_vector2_array_operator_index(GDExtensionTypePtr p_self, GDExtensionInt p_index);
GDE_EXPORT GDExtensionTypePtr packed_vector2_array_operator_index_const(GDExtensionConstTypePtr p_self, GDExtensionInt p_index);
GDE_EXPORT GDExtensionTypePtr packed_vector3_array_operator_index(GDExtensionTypePtr p_self, GDExtensionInt p_index);
GDE_EXPORT GDExtensionTypePtr packed_vector3_array_operator_index_const(GDExtensionConstTypePtr p_self, GDExtensionInt p_index);
GDE_EXPORT GDExtensionVariantPtr array_operator_index(GDExtensionTypePtr p_self, GDExtensionInt p_index);
GDE_EXPORT GDExtensionVariantPtr array_operator_index_const(GDExtensionConstTypePtr p_self, GDExtensionInt p_index);
GDE_EXPORT void array_ref(GDExtensionTypePtr p_self, GDExtensionConstTypePtr p_from);
GDE_EXPORT void array_set_typed(GDExtensionTypePtr p_self, GDExtensionVariantType p_type, GDExtensionConstStringNamePtr p_class_name, GDExtensionConstVariantPtr p_script);
GDE_EXPORT GDExtensionVariantPtr dictionary_operator_index(GDExtensionTypePtr p_self, GDExtensionConstVariantPtr p_key);
GDE_EXPORT GDExtensionVariantPtr dictionary_operator_index_const(GDExtensionConstTypePtr p_self, GDExtensionConstVariantPtr p_key);
GDE_EXPORT void object_method_bind_call(GDExtensionMethodBindPtr p_method_bind, GDExtensionObjectPtr p_instance, const GDExtensionConstVariantPtr* p_args, GDExtensionInt p_arg_count, GDExtensionUninitializedVariantPtr r_ret, GDExtensionCallError* r_error);
GDE_EXPORT void object_method_bind_ptrcall(GDExtensionMethodBindPtr p_method_bind, GDExtensionObjectPtr p_instance, const GDExtensionConstTypePtr* p_args, GDExtensionTypePtr r_ret);
GDE_EXPORT void object_destroy(GDExtensionObjectPtr p_o);
GDE_EXPORT GDExtensionObjectPtr global_get_singleton(GDExtensionConstStringNamePtr p_name);
GDE_EXPORT void* object_get_instance_binding(GDExtensionObjectPtr p_o, void* p_token, const GDExtensionInstanceBindingCallbacks* p_callbacks);
GDE_EXPORT void object_set_instance_binding(GDExtensionObjectPtr p_o, void* p_token, void* p_binding, const GDExtensionInstanceBindingCallbacks* p_callbacks);
GDE_EXPORT void object_set_instance(GDExtensionObjectPtr p_o, GDExtensionConstStringNamePtr p_classname, GDExtensionClassInstancePtr p_instance);
GDE_EXPORT GDExtensionBool object_get_class_name(GDExtensionConstObjectPtr p_object, GDExtensionClassLibraryPtr p_library, GDExtensionUninitializedStringNamePtr r_class_name);
GDE_EXPORT GDExtensionObjectPtr object_cast_to(GDExtensionConstObjectPtr p_object, void* p_class_tag);
GDE_EXPORT GDExtensionObjectPtr object_get_instance_from_id(GDObjectInstanceID p_instance_id);
GDE_EXPORT GDObjectInstanceID object_get_instance_id(GDExtensionConstObjectPtr p_object);
GDE_EXPORT GDExtensionObjectPtr ref_get_object(GDExtensionConstRefPtr p_ref);
GDE_EXPORT void ref_set_object(GDExtensionRefPtr p_ref, GDExtensionObjectPtr p_object);
GDE_EXPORT GDExtensionScriptInstancePtr script_instance_create(const GDExtensionScriptInstanceInfo* p_info, GDExtensionScriptInstanceDataPtr p_instance_data);
GDE_EXPORT GDExtensionObjectPtr classdb_construct_object(GDExtensionConstStringNamePtr p_classname);
GDE_EXPORT GDExtensionMethodBindPtr classdb_get_method_bind(GDExtensionConstStringNamePtr p_classname, GDExtensionConstStringNamePtr p_methodname, GDExtensionInt p_hash);
GDE_EXPORT void* classdb_get_class_tag(GDExtensionConstStringNamePtr p_classname);
GDE_EXPORT void classdb_register_extension_class(GDExtensionClassLibraryPtr p_library, GDExtensionConstStringNamePtr p_class_name, GDExtensionConstStringNamePtr p_parent_class_name, const GDExtensionClassCreationInfo* p_extension_funcs);
GDE_EXPORT void classdb_register_extension_class_method(GDExtensionClassLibraryPtr p_library, GDExtensionConstStringNamePtr p_class_name, const GDExtensionClassMethodInfo* p_method_info);
GDE_EXPORT void classdb_register_extension_class_integer_constant(GDExtensionClassLibraryPtr p_library, GDExtensionConstStringNamePtr p_class_name, GDExtensionConstStringNamePtr p_enum_name, GDExtensionConstStringNamePtr p_constant_name, GDExtensionInt p_constant_value, GDExtensionBool p_is_bitfield);
GDE_EXPORT void classdb_register_extension_class_property(GDExtensionClassLibraryPtr p_library, GDExtensionConstStringNamePtr p_class_name, const GDExtensionPropertyInfo* p_info, GDExtensionConstStringNamePtr p_setter, GDExtensionConstStringNamePtr p_getter);
GDE_EXPORT void classdb_register_extension_class_property_group(GDExtensionClassLibraryPtr p_library, GDExtensionConstStringNamePtr p_class_name, GDExtensionConstStringPtr p_group_name, GDExtensionConstStringPtr p_prefix);
GDE_EXPORT void classdb_register_extension_class_property_subgroup(GDExtensionClassLibraryPtr p_library, GDExtensionConstStringNamePtr p_class_name, GDExtensionConstStringPtr p_subgroup_name, GDExtensionConstStringPtr p_prefix);
GDE_EXPORT void classdb_register_extension_class_signal(GDExtensionClassLibraryPtr p_library, GDExtensionConstStringNamePtr p_class_name, GDExtensionConstStringNamePtr p_signal_name, const GDExtensionPropertyInfo* p_argument_info, GDExtensionInt p_argument_count);
GDE_EXPORT void classdb_unregister_extension_class(GDExtensionClassLibraryPtr p_library, GDExtensionConstStringNamePtr p_class_name);
GDE_EXPORT void get_library_path(GDExtensionClassLibraryPtr p_library, GDExtensionUninitializedStringPtr r_path);
GDE_EXPORT void editor_add_plugin(GDExtensionConstStringNamePtr p_class_name);
GDE_EXPORT void editor_remove_plugin(GDExtensionConstStringNamePtr p_class_name);
GDE_EXPORT void call_GDExtensionInterfaceFunctionPtr(GDExtensionInterfaceFunctionPtr func);
GDE_EXPORT void call_GDExtensionVariantFromTypeConstructorFunc(GDExtensionVariantFromTypeConstructorFunc func, GDExtensionUninitializedVariantPtr arg0, GDExtensionTypePtr arg1);
GDE_EXPORT void call_GDExtensionTypeFromVariantConstructorFunc(GDExtensionTypeFromVariantConstructorFunc func, GDExtensionUninitializedTypePtr arg0, GDExtensionVariantPtr arg1);
GDE_EXPORT void call_GDExtensionPtrOperatorEvaluator(GDExtensionPtrOperatorEvaluator func, GDExtensionConstTypePtr p_left, GDExtensionConstTypePtr p_right, GDExtensionTypePtr r_result);
GDE_EXPORT void call_GDExtensionPtrBuiltInMethod(GDExtensionPtrBuiltInMethod func, GDExtensionTypePtr p_base, const GDExtensionConstTypePtr* p_args, GDExtensionTypePtr r_return, int p_argument_count);
GDE_EXPORT void call_GDExtensionPtrConstructor(GDExtensionPtrConstructor func, GDExtensionUninitializedTypePtr p_base, const GDExtensionConstTypePtr* p_args);
GDE_EXPORT void call_GDExtensionPtrDestructor(GDExtensionPtrDestructor func, GDExtensionTypePtr p_base);
GDE_EXPORT void call_GDExtensionPtrSetter(GDExtensionPtrSetter func, GDExtensionTypePtr p_base, GDExtensionConstTypePtr p_value);
GDE_EXPORT void call_GDExtensionPtrGetter(GDExtensionPtrGetter func, GDExtensionConstTypePtr p_base, GDExtensionTypePtr r_value);
GDE_EXPORT void call_GDExtensionPtrIndexedSetter(GDExtensionPtrIndexedSetter func, GDExtensionTypePtr p_base, GDExtensionInt p_index, GDExtensionConstTypePtr p_value);
GDE_EXPORT void call_GDExtensionPtrIndexedGetter(GDExtensionPtrIndexedGetter func, GDExtensionConstTypePtr p_base, GDExtensionInt p_index, GDExtensionTypePtr r_value);
GDE_EXPORT void call_GDExtensionPtrKeyedSetter(GDExtensionPtrKeyedSetter func, GDExtensionTypePtr p_base, GDExtensionConstTypePtr p_key, GDExtensionConstTypePtr p_value);
GDE_EXPORT void call_GDExtensionPtrKeyedGetter(GDExtensionPtrKeyedGetter func, GDExtensionConstTypePtr p_base, GDExtensionConstTypePtr p_key, GDExtensionTypePtr r_value);
GDE_EXPORT uint32_t call_GDExtensionPtrKeyedChecker(GDExtensionPtrKeyedChecker func, GDExtensionConstVariantPtr p_base, GDExtensionConstVariantPtr p_key);
GDE_EXPORT void call_GDExtensionPtrUtilityFunction(GDExtensionPtrUtilityFunction func, GDExtensionTypePtr r_return, const GDExtensionConstTypePtr* p_args, int p_argument_count);
typedef void (*NativeGroupTask)(void*, uint32_t);
GDE_EXPORT void call_NativeGroupTask(NativeGroupTask func, void* arg0, uint32_t arg1);
typedef void (*NativeTask)(void*);
GDE_EXPORT void call_NativeTask(NativeTask func, void* arg0);
}
