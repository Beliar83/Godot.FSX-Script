//
// Created by Karsten on 27.06.2023.
//

#include "fsx_script_language.h"
#include "fsx_script.h"
#include "godot_cpp/classes/gd_script.hpp"
#include "godot_cpp/core/class_db.hpp"
#include "godot_cpp/variant/packed_string_array.hpp"
#include "godot_cpp/variant/utility_functions.hpp"

using namespace godot;

auto script_template = "module {ClassName}\n"
                       "#r GodotSharpGDExtension.dll\n"
                       "\n"
                       "//This sets the godot class to inherit from\n"
                       "type Base = {BaseClassName}\n"
                       "\n"
                       "//Define fields in this type. Use [Export] to mark exported fields.\n"
                       "type State = struct end\n"
                       "\n"
                       "let _process(self : Base) (delta: float) =\n"
                       "    ()\n"
                       ;

PackedStringArray FSXScriptLanguage::_get_recognized_extensions() const {
    auto array = PackedStringArray();
    array.push_back("fsx");
    return array;
}

TypedArray<Dictionary> FSXScriptLanguage::_get_public_functions() const {
    // TODO
    return {};
}

Dictionary FSXScriptLanguage::_get_public_constants() const {
    // TODO
    return {};
}

TypedArray<Dictionary> FSXScriptLanguage::_get_public_annotations() const {
    // TODO
    return {};
}

bool FSXScriptLanguage::_handles_global_class_type(const String &type) const {
    // TODO
    return {};
}

void FSXScriptLanguage::_init() {
    // TODO
}

void FSXScriptLanguage::_frame() {
    // TODO
}

String FSXScriptLanguage::_get_name() const {
    return "FSXSCript";
}

String FSXScriptLanguage::_get_type() const {
    return "FSXSCript";
}

String FSXScriptLanguage::_get_extension() const {
    return "fsx";
}

void FSXScriptLanguage::_finish() {
    // TODO
}

PackedStringArray FSXScriptLanguage::_get_reserved_words() const {
    // TODO
    return {};
}

bool FSXScriptLanguage::_is_control_flow_keyword(const String &keyword) const {
    // TODO
    return false;
}

PackedStringArray FSXScriptLanguage::_get_comment_delimiters() const {
    auto delimiters = PackedStringArray();
    delimiters.append("//");
    delimiters.append("(* *)");
    return delimiters;
}

PackedStringArray FSXScriptLanguage::_get_string_delimiters() const {
    auto delimiters = PackedStringArray();
    delimiters.append("\" \"");
    delimiters.append("' '");
    delimiters.append("@\" \"");
    return delimiters;
}

void FSXScriptLanguage::_thread_enter() {
    // TODO
}

void FSXScriptLanguage::_thread_exit() {
    // TODO
}

TypedArray<Dictionary> FSXScriptLanguage::_get_built_in_templates(const StringName &object) const {
    // TODO
    return {};
}

bool FSXScriptLanguage::_is_using_templates() {
    // TODO
    return {};
}

Dictionary
FSXScriptLanguage::_validate(const String &script, const String &path, bool validate_functions, bool validate_errors,
                             bool validate_warnings, bool validate_safe_lines) const {
    // TODO: Actually validate
    auto result = Dictionary();

    result["valid"] = true;
    return result;
}

String FSXScriptLanguage::_validate_path(const String &path) const {
    // TODO
    return {};
}

Object *FSXScriptLanguage::_create_script() const {
    auto script = memnew(FSXScript);
    return script;
}

bool FSXScriptLanguage::_has_named_classes() const {
    // TODO
    return {};
}

bool FSXScriptLanguage::_supports_builtin_mode() const {
    // TODO
    return {};
}

bool FSXScriptLanguage::_supports_documentation() const {
    // TODO
    return {};
}


bool FSXScriptLanguage::_can_inherit_from_file() const {
    // TODO
    return {};
}

int32_t FSXScriptLanguage::_find_function(const String &class_name, const String &function_name) const {
    // TODO
    return -1;
}

bool FSXScriptLanguage::_overrides_external_editor() {
    // TODO
    return {};
}

String FSXScriptLanguage::_make_function(const String &class_name, const String &function_name,
                                         const PackedStringArray &function_args) const {
    // TODO
    return {};
}

Dictionary FSXScriptLanguage::_complete_code(const String &code, const String &path, Object *owner) const {
    // TODO
    return {};
}

Dictionary
FSXScriptLanguage::_lookup_code(const String &code, const String &symbol, const String &path, Object *owner) const {
    // TODO
    return {};
}

String FSXScriptLanguage::_auto_indent_code(const String &code, int32_t from_line, int32_t to_line) const {
    // TODO
    return code;
}

void FSXScriptLanguage::_add_global_constant(const StringName &name, const Variant &value) {
    // TODO
}

void FSXScriptLanguage::_add_named_global_constant(const StringName &name, const Variant &value) {
    // TODO
}

void FSXScriptLanguage::_remove_named_global_constant(const StringName &name) {
    // TODO
}

Dictionary FSXScriptLanguage::_debug_get_globals(int32_t max_subitems, int32_t max_depth) {
    // TODO
    return {};
}

void FSXScriptLanguage::_reload_all_scripts() {
    // TODO
}

void FSXScriptLanguage::_reload_tool_script(const Ref<godot::Script> &script, bool soft_reload) {
    // TODO
}

void FSXScriptLanguage::_profiling_start() {
    // TODO
}

void FSXScriptLanguage::_profiling_stop() {
    // TODO
}

Dictionary FSXScriptLanguage::_get_global_class_name(const String &path) const {
    // TODO
    return {};
}

godot::Ref<godot::Script> FSXScriptLanguage::_make_template(const String &_template, const String &class_name,
                                                            const String &base_class_name) const {
    auto script = (FSXScript*)_create_script();
    auto code = String(script_template);
    auto variables = Dictionary();
    variables["ClassName"] = class_name;
    variables["BaseClassName"] = base_class_name;
    script->set_path("temp.fsx");
    script->_load_source_code(code.format(variables));

    return script;
}

void FSXScriptLanguage::_bind_methods() {
}

TypedArray<Dictionary> FSXScriptLanguage::_debug_get_current_stack_info() {
    return {};
}
