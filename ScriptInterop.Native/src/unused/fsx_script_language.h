#pragma once

#include "godot_cpp/classes/script_language.hpp"
#include "fsx_script.h"
#include <godot_cpp/classes/script_language_extension.hpp>
#include <godot_cpp/classes/script.hpp>


namespace godot {

    class FSXScriptLanguage : public ScriptLanguageExtension {
        GDCLASS(FSXScriptLanguage, ScriptLanguageExtension)

        using String = godot::String;
        using PackedStringArray = godot::PackedStringArray;
        using Array = godot::Array;
        using StringName = godot::StringName;
        using Dictionary = godot::Dictionary;
        using Variant = godot::Variant;

    protected:
        static void _bind_methods();

    public:
        // API implementation
        void _thread_enter() override;
        void _thread_exit() override;
        void _frame() override;
        [[nodiscard]] String _get_name() const override;
        void _init() override;
        [[nodiscard]] String _get_type() const override;
        [[nodiscard]] String _get_extension() const override;
        void _finish() override;
        [[nodiscard]] PackedStringArray _get_reserved_words() const override;
        [[nodiscard]] bool _is_control_flow_keyword(const String& keyword) const override;
        [[nodiscard]] PackedStringArray _get_comment_delimiters() const override;
        [[nodiscard]] PackedStringArray _get_string_delimiters() const override;
        [[nodiscard]] TypedArray<Dictionary> _get_built_in_templates(const StringName& object) const override;
        bool _is_using_templates() override;
        [[nodiscard]] Dictionary _validate(const String& script, const String& path, bool validate_functions, bool validate_errors,
                             bool validate_warnings, bool validate_safe_lines) const override;
        [[nodiscard]] String _validate_path(const String& path) const override;
        [[nodiscard]] Object* _create_script() const override;
        [[nodiscard]] bool _has_named_classes() const override;
        [[nodiscard]] bool _supports_builtin_mode() const override;
        [[nodiscard]] bool _supports_documentation() const override;
        [[nodiscard]] bool _can_inherit_from_file() const override;
        [[nodiscard]] int32_t _find_function(const String& class_name, const String& function_name) const override;
        [[nodiscard]] String _make_function(const String& class_name, const String& function_name,
                              const PackedStringArray& function_args) const override;
        bool _overrides_external_editor() override;
        Dictionary _complete_code(const String& code, const String& path, Object* owner) const override;
        Dictionary _lookup_code(const String& code, const String& symbol, const String& path, Object* owner) const override;
        [[nodiscard]] String _auto_indent_code(const String& code, int32_t from_line, int32_t to_line) const override;
        void _add_global_constant(const StringName& name, const Variant& value) override;
        void _add_named_global_constant(const StringName& name, const Variant& value) override;
        void _remove_named_global_constant(const StringName& name) override;
        Dictionary _debug_get_globals(int32_t max_subitems, int32_t max_depth) override;
        void _reload_all_scripts() override;
        void _reload_tool_script(const godot::Ref<godot::Script>& script, bool soft_reload) override;
        [[nodiscard]] PackedStringArray _get_recognized_extensions() const override;
        [[nodiscard]] TypedArray<Dictionary> _get_public_functions() const override;
        [[nodiscard]] Dictionary _get_public_constants() const override;
        [[nodiscard]] TypedArray<Dictionary> _get_public_annotations() const override;
        void _profiling_start() override;
        void _profiling_stop() override;
        [[nodiscard]] bool _handles_global_class_type(const String& type) const override;
        [[nodiscard]] Dictionary _get_global_class_name(const String& path) const override;
        [[nodiscard]] godot::Ref<godot::Script> _make_template(const String& _template, const String& class_name, const String& base_class_name) const override;
        TypedArray<Dictionary> _debug_get_current_stack_info() override;
    };
}

