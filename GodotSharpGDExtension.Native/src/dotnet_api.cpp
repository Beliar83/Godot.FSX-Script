#include "dotnet_api.h"
#include "godot_cpp/godot.hpp"

GDExtensionClassLibraryPtr get_library() {
    return godot::internal::library;
}
GDExtensionInterfaceFunctionPtr get_proc_address(const char* p_function_name) {
    return godot::internal::gdextension_interface_get_proc_address(p_function_name);
}
void get_godot_version(GDExtensionGodotVersion* r_godot_version) {
    return godot::internal::gdextension_interface_get_godot_version(r_godot_version);
}
void* mem_alloc(size_t p_bytes) {
    return godot::internal::gdextension_interface_mem_alloc(p_bytes);
}
void* mem_realloc(void* p_ptr, size_t p_bytes) {
    return godot::internal::gdextension_interface_mem_realloc(p_ptr, p_bytes);
}
void mem_free(void* p_ptr) {
    return godot::internal::gdextension_interface_mem_free(p_ptr);
}
void print_error(const char* p_description, const char* p_function, const char* p_file, int32_t p_line, GDExtensionBool p_editor_notify) {
    return godot::internal::gdextension_interface_print_error(p_description, p_function, p_file, p_line, p_editor_notify);
}
void print_error_with_message(const char* p_description, const char* p_message, const char* p_function, const char* p_file, int32_t p_line, GDExtensionBool p_editor_notify) {
    return godot::internal::gdextension_interface_print_error_with_message(p_description, p_message, p_function, p_file, p_line, p_editor_notify);
}
void print_warning(const char* p_description, const char* p_function, const char* p_file, int32_t p_line, GDExtensionBool p_editor_notify) {
    return godot::internal::gdextension_interface_print_warning(p_description, p_function, p_file, p_line, p_editor_notify);
}
void print_warning_with_message(const char* p_description, const char* p_message, const char* p_function, const char* p_file, int32_t p_line, GDExtensionBool p_editor_notify) {
    return godot::internal::gdextension_interface_print_warning_with_message(p_description, p_message, p_function, p_file, p_line, p_editor_notify);
}
void print_script_error(const char* p_description, const char* p_function, const char* p_file, int32_t p_line, GDExtensionBool p_editor_notify) {
    return godot::internal::gdextension_interface_print_script_error(p_description, p_function, p_file, p_line, p_editor_notify);
}
void print_script_error_with_message(const char* p_description, const char* p_message, const char* p_function, const char* p_file, int32_t p_line, GDExtensionBool p_editor_notify) {
    return godot::internal::gdextension_interface_print_script_error_with_message(p_description, p_message, p_function, p_file, p_line, p_editor_notify);
}
uint64_t get_native_struct_size(GDExtensionConstStringNamePtr p_name) {
    return godot::internal::gdextension_interface_get_native_struct_size(p_name);
}
void variant_new_copy(GDExtensionUninitializedVariantPtr r_dest, GDExtensionConstVariantPtr p_src) {
    return godot::internal::gdextension_interface_variant_new_copy(r_dest, p_src);
}
void variant_new_nil(GDExtensionUninitializedVariantPtr r_dest) {
    return godot::internal::gdextension_interface_variant_new_nil(r_dest);
}
void variant_destroy(GDExtensionVariantPtr p_self) {
    return godot::internal::gdextension_interface_variant_destroy(p_self);
}
void variant_call(GDExtensionVariantPtr p_self, GDExtensionConstStringNamePtr p_method, const GDExtensionConstVariantPtr* p_args, GDExtensionInt p_argument_count, GDExtensionUninitializedVariantPtr r_return, GDExtensionCallError* r_error) {
    return godot::internal::gdextension_interface_variant_call(p_self, p_method, p_args, p_argument_count, r_return, r_error);
}
void variant_call_static(GDExtensionVariantType p_type, GDExtensionConstStringNamePtr p_method, const GDExtensionConstVariantPtr* p_args, GDExtensionInt p_argument_count, GDExtensionUninitializedVariantPtr r_return, GDExtensionCallError* r_error) {
    return godot::internal::gdextension_interface_variant_call_static(p_type, p_method, p_args, p_argument_count, r_return, r_error);
}
void variant_evaluate(GDExtensionVariantOperator p_op, GDExtensionConstVariantPtr p_a, GDExtensionConstVariantPtr p_b, GDExtensionUninitializedVariantPtr r_return, GDExtensionBool* r_valid) {
    return godot::internal::gdextension_interface_variant_evaluate(p_op, p_a, p_b, r_return, r_valid);
}
void variant_set(GDExtensionVariantPtr p_self, GDExtensionConstVariantPtr p_key, GDExtensionConstVariantPtr p_value, GDExtensionBool* r_valid) {
    return godot::internal::gdextension_interface_variant_set(p_self, p_key, p_value, r_valid);
}
void variant_set_named(GDExtensionVariantPtr p_self, GDExtensionConstStringNamePtr p_key, GDExtensionConstVariantPtr p_value, GDExtensionBool* r_valid) {
    return godot::internal::gdextension_interface_variant_set_named(p_self, p_key, p_value, r_valid);
}
void variant_set_keyed(GDExtensionVariantPtr p_self, GDExtensionConstVariantPtr p_key, GDExtensionConstVariantPtr p_value, GDExtensionBool* r_valid) {
    return godot::internal::gdextension_interface_variant_set_keyed(p_self, p_key, p_value, r_valid);
}
void variant_set_indexed(GDExtensionVariantPtr p_self, GDExtensionInt p_index, GDExtensionConstVariantPtr p_value, GDExtensionBool* r_valid, GDExtensionBool* r_oob) {
    return godot::internal::gdextension_interface_variant_set_indexed(p_self, p_index, p_value, r_valid, r_oob);
}
void variant_get(GDExtensionConstVariantPtr p_self, GDExtensionConstVariantPtr p_key, GDExtensionUninitializedVariantPtr r_ret, GDExtensionBool* r_valid) {
    return godot::internal::gdextension_interface_variant_get(p_self, p_key, r_ret, r_valid);
}
void variant_get_named(GDExtensionConstVariantPtr p_self, GDExtensionConstStringNamePtr p_key, GDExtensionUninitializedVariantPtr r_ret, GDExtensionBool* r_valid) {
    return godot::internal::gdextension_interface_variant_get_named(p_self, p_key, r_ret, r_valid);
}
void variant_get_keyed(GDExtensionConstVariantPtr p_self, GDExtensionConstVariantPtr p_key, GDExtensionUninitializedVariantPtr r_ret, GDExtensionBool* r_valid) {
    return godot::internal::gdextension_interface_variant_get_keyed(p_self, p_key, r_ret, r_valid);
}
void variant_get_indexed(GDExtensionConstVariantPtr p_self, GDExtensionInt p_index, GDExtensionUninitializedVariantPtr r_ret, GDExtensionBool* r_valid, GDExtensionBool* r_oob) {
    return godot::internal::gdextension_interface_variant_get_indexed(p_self, p_index, r_ret, r_valid, r_oob);
}
GDExtensionBool variant_iter_init(GDExtensionConstVariantPtr p_self, GDExtensionUninitializedVariantPtr r_iter, GDExtensionBool* r_valid) {
    return godot::internal::gdextension_interface_variant_iter_init(p_self, r_iter, r_valid);
}
GDExtensionBool variant_iter_next(GDExtensionConstVariantPtr p_self, GDExtensionVariantPtr r_iter, GDExtensionBool* r_valid) {
    return godot::internal::gdextension_interface_variant_iter_next(p_self, r_iter, r_valid);
}
void variant_iter_get(GDExtensionConstVariantPtr p_self, GDExtensionVariantPtr r_iter, GDExtensionUninitializedVariantPtr r_ret, GDExtensionBool* r_valid) {
    return godot::internal::gdextension_interface_variant_iter_get(p_self, r_iter, r_ret, r_valid);
}
GDExtensionInt variant_hash(GDExtensionConstVariantPtr p_self) {
    return godot::internal::gdextension_interface_variant_hash(p_self);
}
GDExtensionInt variant_recursive_hash(GDExtensionConstVariantPtr p_self, GDExtensionInt p_recursion_count) {
    return godot::internal::gdextension_interface_variant_recursive_hash(p_self, p_recursion_count);
}
GDExtensionBool variant_hash_compare(GDExtensionConstVariantPtr p_self, GDExtensionConstVariantPtr p_other) {
    return godot::internal::gdextension_interface_variant_hash_compare(p_self, p_other);
}
GDExtensionBool variant_booleanize(GDExtensionConstVariantPtr p_self) {
    return godot::internal::gdextension_interface_variant_booleanize(p_self);
}
void variant_duplicate(GDExtensionConstVariantPtr p_self, GDExtensionVariantPtr r_ret, GDExtensionBool p_deep) {
    return godot::internal::gdextension_interface_variant_duplicate(p_self, r_ret, p_deep);
}
void variant_stringify(GDExtensionConstVariantPtr p_self, GDExtensionStringPtr r_ret) {
    return godot::internal::gdextension_interface_variant_stringify(p_self, r_ret);
}
GDExtensionVariantType variant_get_type(GDExtensionConstVariantPtr p_self) {
    return godot::internal::gdextension_interface_variant_get_type(p_self);
}
GDExtensionBool variant_has_method(GDExtensionConstVariantPtr p_self, GDExtensionConstStringNamePtr p_method) {
    return godot::internal::gdextension_interface_variant_has_method(p_self, p_method);
}
GDExtensionBool variant_has_member(GDExtensionVariantType p_type, GDExtensionConstStringNamePtr p_member) {
    return godot::internal::gdextension_interface_variant_has_member(p_type, p_member);
}
GDExtensionBool variant_has_key(GDExtensionConstVariantPtr p_self, GDExtensionConstVariantPtr p_key, GDExtensionBool* r_valid) {
    return godot::internal::gdextension_interface_variant_has_key(p_self, p_key, r_valid);
}
void variant_get_type_name(GDExtensionVariantType p_type, GDExtensionUninitializedStringPtr r_name) {
    return godot::internal::gdextension_interface_variant_get_type_name(p_type, r_name);
}
GDExtensionBool variant_can_convert(GDExtensionVariantType p_from, GDExtensionVariantType p_to) {
    return godot::internal::gdextension_interface_variant_can_convert(p_from, p_to);
}
GDExtensionBool variant_can_convert_strict(GDExtensionVariantType p_from, GDExtensionVariantType p_to) {
    return godot::internal::gdextension_interface_variant_can_convert_strict(p_from, p_to);
}
GDExtensionVariantFromTypeConstructorFunc get_variant_from_type_constructor(GDExtensionVariantType p_type) {
    return godot::internal::gdextension_interface_get_variant_from_type_constructor(p_type);
}
GDExtensionTypeFromVariantConstructorFunc get_variant_to_type_constructor(GDExtensionVariantType p_type) {
    return godot::internal::gdextension_interface_get_variant_to_type_constructor(p_type);
}
GDExtensionPtrOperatorEvaluator variant_get_ptr_operator_evaluator(GDExtensionVariantOperator p_operator, GDExtensionVariantType p_type_a, GDExtensionVariantType p_type_b) {
    return godot::internal::gdextension_interface_variant_get_ptr_operator_evaluator(p_operator, p_type_a, p_type_b);
}
GDExtensionPtrBuiltInMethod variant_get_ptr_builtin_method(GDExtensionVariantType p_type, GDExtensionConstStringNamePtr p_method, GDExtensionInt p_hash) {
    return godot::internal::gdextension_interface_variant_get_ptr_builtin_method(p_type, p_method, p_hash);
}
GDExtensionPtrConstructor variant_get_ptr_constructor(GDExtensionVariantType p_type, int32_t p_constructor) {
    return godot::internal::gdextension_interface_variant_get_ptr_constructor(p_type, p_constructor);
}
GDExtensionPtrDestructor variant_get_ptr_destructor(GDExtensionVariantType p_type) {
    return godot::internal::gdextension_interface_variant_get_ptr_destructor(p_type);
}
void variant_construct(GDExtensionVariantType p_type, GDExtensionUninitializedVariantPtr r_base, const GDExtensionConstVariantPtr* p_args, int32_t p_argument_count, GDExtensionCallError* r_error) {
    return godot::internal::gdextension_interface_variant_construct(p_type, r_base, p_args, p_argument_count, r_error);
}
GDExtensionPtrSetter variant_get_ptr_setter(GDExtensionVariantType p_type, GDExtensionConstStringNamePtr p_member) {
    return godot::internal::gdextension_interface_variant_get_ptr_setter(p_type, p_member);
}
GDExtensionPtrGetter variant_get_ptr_getter(GDExtensionVariantType p_type, GDExtensionConstStringNamePtr p_member) {
    return godot::internal::gdextension_interface_variant_get_ptr_getter(p_type, p_member);
}
GDExtensionPtrIndexedSetter variant_get_ptr_indexed_setter(GDExtensionVariantType p_type) {
    return godot::internal::gdextension_interface_variant_get_ptr_indexed_setter(p_type);
}
GDExtensionPtrIndexedGetter variant_get_ptr_indexed_getter(GDExtensionVariantType p_type) {
    return godot::internal::gdextension_interface_variant_get_ptr_indexed_getter(p_type);
}
GDExtensionPtrKeyedSetter variant_get_ptr_keyed_setter(GDExtensionVariantType p_type) {
    return godot::internal::gdextension_interface_variant_get_ptr_keyed_setter(p_type);
}
GDExtensionPtrKeyedGetter variant_get_ptr_keyed_getter(GDExtensionVariantType p_type) {
    return godot::internal::gdextension_interface_variant_get_ptr_keyed_getter(p_type);
}
GDExtensionPtrKeyedChecker variant_get_ptr_keyed_checker(GDExtensionVariantType p_type) {
    return godot::internal::gdextension_interface_variant_get_ptr_keyed_checker(p_type);
}
void variant_get_constant_value(GDExtensionVariantType p_type, GDExtensionConstStringNamePtr p_constant, GDExtensionUninitializedVariantPtr r_ret) {
    return godot::internal::gdextension_interface_variant_get_constant_value(p_type, p_constant, r_ret);
}
GDExtensionPtrUtilityFunction variant_get_ptr_utility_function(GDExtensionConstStringNamePtr p_function, GDExtensionInt p_hash) {
    return godot::internal::gdextension_interface_variant_get_ptr_utility_function(p_function, p_hash);
}
void string_new_with_latin1_chars(GDExtensionUninitializedStringPtr r_dest, const char* p_contents) {
    return godot::internal::gdextension_interface_string_new_with_latin1_chars(r_dest, p_contents);
}
void string_new_with_utf8_chars(GDExtensionUninitializedStringPtr r_dest, const char* p_contents) {
    return godot::internal::gdextension_interface_string_new_with_utf8_chars(r_dest, p_contents);
}
void string_new_with_wide_chars(GDExtensionUninitializedStringPtr r_dest, const wchar_t* p_contents) {
    return godot::internal::gdextension_interface_string_new_with_wide_chars(r_dest, p_contents);
}
void string_new_with_latin1_chars_and_len(GDExtensionUninitializedStringPtr r_dest, const char* p_contents, GDExtensionInt p_size) {
    return godot::internal::gdextension_interface_string_new_with_latin1_chars_and_len(r_dest, p_contents, p_size);
}
void string_new_with_utf8_chars_and_len(GDExtensionUninitializedStringPtr r_dest, const char* p_contents, GDExtensionInt p_size) {
    return godot::internal::gdextension_interface_string_new_with_utf8_chars_and_len(r_dest, p_contents, p_size);
}
void string_new_with_wide_chars_and_len(GDExtensionUninitializedStringPtr r_dest, const wchar_t* p_contents, GDExtensionInt p_size) {
    return godot::internal::gdextension_interface_string_new_with_wide_chars_and_len(r_dest, p_contents, p_size);
}
GDExtensionInt string_to_latin1_chars(GDExtensionConstStringPtr p_self, char* r_text, GDExtensionInt p_max_write_length) {
    return godot::internal::gdextension_interface_string_to_latin1_chars(p_self, r_text, p_max_write_length);
}
GDExtensionInt string_to_utf8_chars(GDExtensionConstStringPtr p_self, char* r_text, GDExtensionInt p_max_write_length) {
    return godot::internal::gdextension_interface_string_to_utf8_chars(p_self, r_text, p_max_write_length);
}
GDExtensionInt string_to_wide_chars(GDExtensionConstStringPtr p_self, wchar_t* r_text, GDExtensionInt p_max_write_length) {
    return godot::internal::gdextension_interface_string_to_wide_chars(p_self, r_text, p_max_write_length);
}
void string_operator_plus_eq_string(GDExtensionStringPtr p_self, GDExtensionConstStringPtr p_b) {
    return godot::internal::gdextension_interface_string_operator_plus_eq_string(p_self, p_b);
}
void string_operator_plus_eq_cstr(GDExtensionStringPtr p_self, const char* p_b) {
    return godot::internal::gdextension_interface_string_operator_plus_eq_cstr(p_self, p_b);
}
void string_operator_plus_eq_wcstr(GDExtensionStringPtr p_self, const wchar_t* p_b) {
    return godot::internal::gdextension_interface_string_operator_plus_eq_wcstr(p_self, p_b);
}
GDExtensionInt xml_parser_open_buffer(GDExtensionObjectPtr p_instance, const uint8_t* p_buffer, size_t p_size) {
    return godot::internal::gdextension_interface_xml_parser_open_buffer(p_instance, p_buffer, p_size);
}
void file_access_store_buffer(GDExtensionObjectPtr p_instance, const uint8_t* p_src, uint64_t p_length) {
    return godot::internal::gdextension_interface_file_access_store_buffer(p_instance, p_src, p_length);
}
uint64_t file_access_get_buffer(GDExtensionConstObjectPtr p_instance, uint8_t* p_dst, uint64_t p_length) {
    return godot::internal::gdextension_interface_file_access_get_buffer(p_instance, p_dst, p_length);
}
int64_t worker_thread_pool_add_native_group_task(GDExtensionObjectPtr p_instance, void(*p_func)(void*, uint32_t), void* p_userdata, int p_elements, int p_tasks, GDExtensionBool p_high_priority, GDExtensionConstStringPtr p_description) {
    return godot::internal::gdextension_interface_worker_thread_pool_add_native_group_task(p_instance, p_func, p_userdata, p_elements, p_tasks, p_high_priority, p_description);
}
int64_t worker_thread_pool_add_native_task(GDExtensionObjectPtr p_instance, void(*p_func)(void*), void* p_userdata, GDExtensionBool p_high_priority, GDExtensionConstStringPtr p_description) {
    return godot::internal::gdextension_interface_worker_thread_pool_add_native_task(p_instance, p_func, p_userdata, p_high_priority, p_description);
}
uint8_t* packed_byte_array_operator_index(GDExtensionTypePtr p_self, GDExtensionInt p_index) {
    return godot::internal::gdextension_interface_packed_byte_array_operator_index(p_self, p_index);
}
const uint8_t* packed_byte_array_operator_index_const(GDExtensionConstTypePtr p_self, GDExtensionInt p_index) {
    return godot::internal::gdextension_interface_packed_byte_array_operator_index_const(p_self, p_index);
}
GDExtensionTypePtr packed_color_array_operator_index(GDExtensionTypePtr p_self, GDExtensionInt p_index) {
    return godot::internal::gdextension_interface_packed_color_array_operator_index(p_self, p_index);
}
GDExtensionTypePtr packed_color_array_operator_index_const(GDExtensionConstTypePtr p_self, GDExtensionInt p_index) {
    return godot::internal::gdextension_interface_packed_color_array_operator_index_const(p_self, p_index);
}
float* packed_float32_array_operator_index(GDExtensionTypePtr p_self, GDExtensionInt p_index) {
    return godot::internal::gdextension_interface_packed_float32_array_operator_index(p_self, p_index);
}
const float* packed_float32_array_operator_index_const(GDExtensionConstTypePtr p_self, GDExtensionInt p_index) {
    return godot::internal::gdextension_interface_packed_float32_array_operator_index_const(p_self, p_index);
}
double* packed_float64_array_operator_index(GDExtensionTypePtr p_self, GDExtensionInt p_index) {
    return godot::internal::gdextension_interface_packed_float64_array_operator_index(p_self, p_index);
}
const double* packed_float64_array_operator_index_const(GDExtensionConstTypePtr p_self, GDExtensionInt p_index) {
    return godot::internal::gdextension_interface_packed_float64_array_operator_index_const(p_self, p_index);
}
int32_t* packed_int32_array_operator_index(GDExtensionTypePtr p_self, GDExtensionInt p_index) {
    return godot::internal::gdextension_interface_packed_int32_array_operator_index(p_self, p_index);
}
const int32_t* packed_int32_array_operator_index_const(GDExtensionConstTypePtr p_self, GDExtensionInt p_index) {
    return godot::internal::gdextension_interface_packed_int32_array_operator_index_const(p_self, p_index);
}
int64_t* packed_int64_array_operator_index(GDExtensionTypePtr p_self, GDExtensionInt p_index) {
    return godot::internal::gdextension_interface_packed_int64_array_operator_index(p_self, p_index);
}
const int64_t* packed_int64_array_operator_index_const(GDExtensionConstTypePtr p_self, GDExtensionInt p_index) {
    return godot::internal::gdextension_interface_packed_int64_array_operator_index_const(p_self, p_index);
}
GDExtensionStringPtr packed_string_array_operator_index(GDExtensionTypePtr p_self, GDExtensionInt p_index) {
    return godot::internal::gdextension_interface_packed_string_array_operator_index(p_self, p_index);
}
GDExtensionStringPtr packed_string_array_operator_index_const(GDExtensionConstTypePtr p_self, GDExtensionInt p_index) {
    return godot::internal::gdextension_interface_packed_string_array_operator_index_const(p_self, p_index);
}
GDExtensionTypePtr packed_vector2_array_operator_index(GDExtensionTypePtr p_self, GDExtensionInt p_index) {
    return godot::internal::gdextension_interface_packed_vector2_array_operator_index(p_self, p_index);
}
GDExtensionTypePtr packed_vector2_array_operator_index_const(GDExtensionConstTypePtr p_self, GDExtensionInt p_index) {
    return godot::internal::gdextension_interface_packed_vector2_array_operator_index_const(p_self, p_index);
}
GDExtensionTypePtr packed_vector3_array_operator_index(GDExtensionTypePtr p_self, GDExtensionInt p_index) {
    return godot::internal::gdextension_interface_packed_vector3_array_operator_index(p_self, p_index);
}
GDExtensionTypePtr packed_vector3_array_operator_index_const(GDExtensionConstTypePtr p_self, GDExtensionInt p_index) {
    return godot::internal::gdextension_interface_packed_vector3_array_operator_index_const(p_self, p_index);
}
GDExtensionVariantPtr array_operator_index(GDExtensionTypePtr p_self, GDExtensionInt p_index) {
    return godot::internal::gdextension_interface_array_operator_index(p_self, p_index);
}
GDExtensionVariantPtr array_operator_index_const(GDExtensionConstTypePtr p_self, GDExtensionInt p_index) {
    return godot::internal::gdextension_interface_array_operator_index_const(p_self, p_index);
}
void array_ref(GDExtensionTypePtr p_self, GDExtensionConstTypePtr p_from) {
    return godot::internal::gdextension_interface_array_ref(p_self, p_from);
}
void array_set_typed(GDExtensionTypePtr p_self, GDExtensionVariantType p_type, GDExtensionConstStringNamePtr p_class_name, GDExtensionConstVariantPtr p_script) {
    return godot::internal::gdextension_interface_array_set_typed(p_self, p_type, p_class_name, p_script);
}
GDExtensionVariantPtr dictionary_operator_index(GDExtensionTypePtr p_self, GDExtensionConstVariantPtr p_key) {
    return godot::internal::gdextension_interface_dictionary_operator_index(p_self, p_key);
}
GDExtensionVariantPtr dictionary_operator_index_const(GDExtensionConstTypePtr p_self, GDExtensionConstVariantPtr p_key) {
    return godot::internal::gdextension_interface_dictionary_operator_index_const(p_self, p_key);
}
void object_method_bind_call(GDExtensionMethodBindPtr p_method_bind, GDExtensionObjectPtr p_instance, const GDExtensionConstVariantPtr* p_args, GDExtensionInt p_arg_count, GDExtensionUninitializedVariantPtr r_ret, GDExtensionCallError* r_error) {
    return godot::internal::gdextension_interface_object_method_bind_call(p_method_bind, p_instance, p_args, p_arg_count, r_ret, r_error);
}
void object_method_bind_ptrcall(GDExtensionMethodBindPtr p_method_bind, GDExtensionObjectPtr p_instance, const GDExtensionConstTypePtr* p_args, GDExtensionTypePtr r_ret) {
    return godot::internal::gdextension_interface_object_method_bind_ptrcall(p_method_bind, p_instance, p_args, r_ret);
}
void object_destroy(GDExtensionObjectPtr p_o) {
    return godot::internal::gdextension_interface_object_destroy(p_o);
}
GDExtensionObjectPtr global_get_singleton(GDExtensionConstStringNamePtr p_name) {
    return godot::internal::gdextension_interface_global_get_singleton(p_name);
}
void* object_get_instance_binding(GDExtensionObjectPtr p_o, void* p_token, const GDExtensionInstanceBindingCallbacks* p_callbacks) {
    return godot::internal::gdextension_interface_object_get_instance_binding(p_o, p_token, p_callbacks);
}
void object_set_instance_binding(GDExtensionObjectPtr p_o, void* p_token, void* p_binding, const GDExtensionInstanceBindingCallbacks* p_callbacks) {
    return godot::internal::gdextension_interface_object_set_instance_binding(p_o, p_token, p_binding, p_callbacks);
}
void object_set_instance(GDExtensionObjectPtr p_o, GDExtensionConstStringNamePtr p_classname, GDExtensionClassInstancePtr p_instance) {
    return godot::internal::gdextension_interface_object_set_instance(p_o, p_classname, p_instance);
}
GDExtensionBool object_get_class_name(GDExtensionConstObjectPtr p_object, GDExtensionClassLibraryPtr p_library, GDExtensionUninitializedStringNamePtr r_class_name) {
    return godot::internal::gdextension_interface_object_get_class_name(p_object, p_library, r_class_name);
}
GDExtensionObjectPtr object_cast_to(GDExtensionConstObjectPtr p_object, void* p_class_tag) {
    return godot::internal::gdextension_interface_object_cast_to(p_object, p_class_tag);
}
GDExtensionObjectPtr object_get_instance_from_id(GDObjectInstanceID p_instance_id) {
    return godot::internal::gdextension_interface_object_get_instance_from_id(p_instance_id);
}
GDObjectInstanceID object_get_instance_id(GDExtensionConstObjectPtr p_object) {
    return godot::internal::gdextension_interface_object_get_instance_id(p_object);
}
GDExtensionObjectPtr ref_get_object(GDExtensionConstRefPtr p_ref) {
    return godot::internal::gdextension_interface_ref_get_object(p_ref);
}
void ref_set_object(GDExtensionRefPtr p_ref, GDExtensionObjectPtr p_object) {
    return godot::internal::gdextension_interface_ref_set_object(p_ref, p_object);
}
GDExtensionScriptInstancePtr script_instance_create(const GDExtensionScriptInstanceInfo* p_info, GDExtensionScriptInstanceDataPtr p_instance_data) {
    return godot::internal::gdextension_interface_script_instance_create(p_info, p_instance_data);
}
GDExtensionObjectPtr classdb_construct_object(GDExtensionConstStringNamePtr p_classname) {
    return godot::internal::gdextension_interface_classdb_construct_object(p_classname);
}
GDExtensionMethodBindPtr classdb_get_method_bind(GDExtensionConstStringNamePtr p_classname, GDExtensionConstStringNamePtr p_methodname, GDExtensionInt p_hash) {
    return godot::internal::gdextension_interface_classdb_get_method_bind(p_classname, p_methodname, p_hash);
}
void* classdb_get_class_tag(GDExtensionConstStringNamePtr p_classname) {
    return godot::internal::gdextension_interface_classdb_get_class_tag(p_classname);
}
void classdb_register_extension_class(GDExtensionClassLibraryPtr p_library, GDExtensionConstStringNamePtr p_class_name, GDExtensionConstStringNamePtr p_parent_class_name, const GDExtensionClassCreationInfo* p_extension_funcs) {
    return godot::internal::gdextension_interface_classdb_register_extension_class(p_library, p_class_name, p_parent_class_name, p_extension_funcs);
}
void classdb_register_extension_class_method(GDExtensionClassLibraryPtr p_library, GDExtensionConstStringNamePtr p_class_name, const GDExtensionClassMethodInfo* p_method_info) {
    return godot::internal::gdextension_interface_classdb_register_extension_class_method(p_library, p_class_name, p_method_info);
}
void classdb_register_extension_class_integer_constant(GDExtensionClassLibraryPtr p_library, GDExtensionConstStringNamePtr p_class_name, GDExtensionConstStringNamePtr p_enum_name, GDExtensionConstStringNamePtr p_constant_name, GDExtensionInt p_constant_value, GDExtensionBool p_is_bitfield) {
    return godot::internal::gdextension_interface_classdb_register_extension_class_integer_constant(p_library, p_class_name, p_enum_name, p_constant_name, p_constant_value, p_is_bitfield);
}
void classdb_register_extension_class_property(GDExtensionClassLibraryPtr p_library, GDExtensionConstStringNamePtr p_class_name, const GDExtensionPropertyInfo* p_info, GDExtensionConstStringNamePtr p_setter, GDExtensionConstStringNamePtr p_getter) {
    return godot::internal::gdextension_interface_classdb_register_extension_class_property(p_library, p_class_name, p_info, p_setter, p_getter);
}
void classdb_register_extension_class_property_group(GDExtensionClassLibraryPtr p_library, GDExtensionConstStringNamePtr p_class_name, GDExtensionConstStringPtr p_group_name, GDExtensionConstStringPtr p_prefix) {
    return godot::internal::gdextension_interface_classdb_register_extension_class_property_group(p_library, p_class_name, p_group_name, p_prefix);
}
void classdb_register_extension_class_property_subgroup(GDExtensionClassLibraryPtr p_library, GDExtensionConstStringNamePtr p_class_name, GDExtensionConstStringPtr p_subgroup_name, GDExtensionConstStringPtr p_prefix) {
    return godot::internal::gdextension_interface_classdb_register_extension_class_property_subgroup(p_library, p_class_name, p_subgroup_name, p_prefix);
}
void classdb_register_extension_class_signal(GDExtensionClassLibraryPtr p_library, GDExtensionConstStringNamePtr p_class_name, GDExtensionConstStringNamePtr p_signal_name, const GDExtensionPropertyInfo* p_argument_info, GDExtensionInt p_argument_count) {
    return godot::internal::gdextension_interface_classdb_register_extension_class_signal(p_library, p_class_name, p_signal_name, p_argument_info, p_argument_count);
}
void classdb_unregister_extension_class(GDExtensionClassLibraryPtr p_library, GDExtensionConstStringNamePtr p_class_name) {
    return godot::internal::gdextension_interface_classdb_unregister_extension_class(p_library, p_class_name);
}
void get_library_path(GDExtensionClassLibraryPtr p_library, GDExtensionUninitializedStringPtr r_path) {
    return godot::internal::gdextension_interface_get_library_path(p_library, r_path);
}
void editor_add_plugin(GDExtensionConstStringNamePtr p_class_name) {
    return godot::internal::gdextension_interface_editor_add_plugin(p_class_name);
}
void editor_remove_plugin(GDExtensionConstStringNamePtr p_class_name) {
    return godot::internal::gdextension_interface_editor_remove_plugin(p_class_name);
}
void call_GDExtensionInterfaceFunctionPtr(GDExtensionInterfaceFunctionPtr func) {
    return func();
}
void call_GDExtensionVariantFromTypeConstructorFunc(GDExtensionVariantFromTypeConstructorFunc func, GDExtensionUninitializedVariantPtr arg0, GDExtensionTypePtr arg1) {
    return func(arg0, arg1);
}
void call_GDExtensionTypeFromVariantConstructorFunc(GDExtensionTypeFromVariantConstructorFunc func, GDExtensionUninitializedTypePtr arg0, GDExtensionVariantPtr arg1) {
    return func(arg0, arg1);
}
void call_GDExtensionPtrOperatorEvaluator(GDExtensionPtrOperatorEvaluator func, GDExtensionConstTypePtr p_left, GDExtensionConstTypePtr p_right, GDExtensionTypePtr r_result) {
    return func(p_left, p_right, r_result);
}
void call_GDExtensionPtrBuiltInMethod(GDExtensionPtrBuiltInMethod func, GDExtensionTypePtr p_base, const GDExtensionConstTypePtr* p_args, GDExtensionTypePtr r_return, int p_argument_count) {
    return func(p_base, p_args, r_return, p_argument_count);
}
void call_GDExtensionPtrConstructor(GDExtensionPtrConstructor func, GDExtensionUninitializedTypePtr p_base, const GDExtensionConstTypePtr* p_args) {
    return func(p_base, p_args);
}
void call_GDExtensionPtrDestructor(GDExtensionPtrDestructor func, GDExtensionTypePtr p_base) {
    return func(p_base);
}
void call_GDExtensionPtrSetter(GDExtensionPtrSetter func, GDExtensionTypePtr p_base, GDExtensionConstTypePtr p_value) {
    return func(p_base, p_value);
}
void call_GDExtensionPtrGetter(GDExtensionPtrGetter func, GDExtensionConstTypePtr p_base, GDExtensionTypePtr r_value) {
    return func(p_base, r_value);
}
void call_GDExtensionPtrIndexedSetter(GDExtensionPtrIndexedSetter func, GDExtensionTypePtr p_base, GDExtensionInt p_index, GDExtensionConstTypePtr p_value) {
    return func(p_base, p_index, p_value);
}
void call_GDExtensionPtrIndexedGetter(GDExtensionPtrIndexedGetter func, GDExtensionConstTypePtr p_base, GDExtensionInt p_index, GDExtensionTypePtr r_value) {
    return func(p_base, p_index, r_value);
}
void call_GDExtensionPtrKeyedSetter(GDExtensionPtrKeyedSetter func, GDExtensionTypePtr p_base, GDExtensionConstTypePtr p_key, GDExtensionConstTypePtr p_value) {
    return func(p_base, p_key, p_value);
}
void call_GDExtensionPtrKeyedGetter(GDExtensionPtrKeyedGetter func, GDExtensionConstTypePtr p_base, GDExtensionConstTypePtr p_key, GDExtensionTypePtr r_value) {
    return func(p_base, p_key, r_value);
}
uint32_t call_GDExtensionPtrKeyedChecker(GDExtensionPtrKeyedChecker func, GDExtensionConstVariantPtr p_base, GDExtensionConstVariantPtr p_key) {
    return func(p_base, p_key);
}
void call_GDExtensionPtrUtilityFunction(GDExtensionPtrUtilityFunction func, GDExtensionTypePtr r_return, const GDExtensionConstTypePtr* p_args, int p_argument_count) {
    return func(r_return, p_args, p_argument_count);
}
void call_NativeGroupTask(NativeGroupTask func, void* arg0, uint32_t arg1) {
    return func(arg0, arg1);
}
void call_NativeTask(NativeTask func, void* arg0) {
    return func(arg0);
}
