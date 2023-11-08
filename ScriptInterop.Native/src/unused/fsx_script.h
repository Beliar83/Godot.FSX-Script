#pragma once

#include "godot_cpp/classes/script_extension.hpp"
#include "godot_cpp/classes/script_language.hpp"
#include "godot_cpp/templates/hash_map.hpp"
#include "fsharp_method_info.h"

namespace godot {
    class FSXScriptInstance;

    class FSXScript : public ScriptExtension {
        GDCLASS(FSXScript, ScriptExtension)

        friend class FSXScriptInstance;

    private:
        Ref<FSXScript> base_script;

        List<PropertyInfo> exported_members_cache; // members_cache
        HashMap<StringName, Variant> exported_members_defval_cache; // member_default_values_cache

        void _update_exports_values(HashMap<StringName, Variant> &values, List<PropertyInfo> &propnames);
        String code = "Empty";

    protected:
        static void _bind_methods();
    public:
        virtual ~FSXScript() = default;
        ScriptLanguage * _get_language() const override;
        bool _can_instantiate() const override;
        String _get_source_code() const override;
        void _set_source_code(const godot::String &code) override;
        Error _reload(bool keep_state) override;
        TypedArray<Dictionary> _get_script_method_list() const override;
        TypedArray<StringName> _get_members() const override;
        bool _is_tool() const override;
        void _update_exports() override;
        TypedArray<Dictionary> _get_documentation() const override;
        bool _has_source_code() const override;
        void * _instance_create(godot::Object *for_object) const override;
        void * _placeholder_instance_create(godot::Object *for_object) const override;
        bool _has_property_default_value(const godot::StringName &property) const override;
        Ref<Script> _get_base_script() const override;
        StringName _get_instance_base_type() const override;
        TypedArray<Dictionary> _get_script_property_list() const override;
//        TypedArray<Dictionary> get_script_property_list() const;
        Variant _get_rpc_config() const override;
        Variant _get_property_default_value(const godot::StringName &property) const override;
        TypedArray<Dictionary> _get_script_signal_list() const override;
        bool _is_valid() const override;
        virtual std::vector<FSharpMethodInfo> _get_methods() const {
            return {};
        }
        virtual Variant _call_method(StringName name, std::vector<Variant> args, Object* instance) const {
            return {};
        }

        void _load_source_code(const String &p_code);
    };

//    ERROR: Required virtual method FSXScript::_instance_create must be overridden before calling.
//    at: _gdvirtual__instance_create_call (./core/object/script_language_extension.h:60)
//    ERROR: Required virtual method FSXScript::_has_property_default_value must be overridden before calling.
//    at: _gdvirtual__has_property_default_value_call (./core/object/script_language_extension.h:119)
//    ERROR: Required virtual method FSXScript::_get_base_script must be overridden before calling.
//    at: _gdvirtual__get_base_script_call (./core/object/script_language_extension.h:55)
//    ERROR: Required virtual method FSXScript::_get_instance_base_type must be overridden before calling.


} // godot


