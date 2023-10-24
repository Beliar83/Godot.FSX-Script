//
// Created by Karsten on 28.06.2023.
//

#include "fsx_script.h"
#include "gdextension_interface.h"
#include "godot_cpp/classes/engine.hpp"
#include "godot_cpp/classes/global_constants.hpp"
#include "godot_cpp/classes/object.hpp"
#include "godot_cpp/classes/wrapped.hpp"
#include "godot_cpp/core/class_db.hpp"
#include "godot_cpp/godot.hpp"
#include "godot_cpp/variant/dictionary.hpp"
#include "fsx_script_instance.h"
#include "godot_cpp/variant/variant.hpp"
#include "godot_cpp/variant/utility_functions.hpp"

namespace godot {
    void FSXScript::_bind_methods() {
    }

    ScriptLanguage *FSXScript::_get_language() const {
        godot::UtilityFunctions::print("_get_language");
        return reinterpret_cast<ScriptLanguage*>(Engine::get_singleton()->get_singleton("FSXScriptLanguage"));
    }

    bool FSXScript::_can_instantiate() const {
        //TODO
        return true;
    }

    String FSXScript::_get_source_code() const {
        return code;
    }

    void FSXScript::_set_source_code(const String &code) {
        this->code = code;
    }

    Error FSXScript::_reload(bool keep_state) {
        //TODO
        return Error::ERR_UNCONFIGURED;
    }

    TypedArray<Dictionary> FSXScript::_get_script_method_list() const {
        //TODO
        auto methods = TypedArray<Dictionary>();
        for (const auto& method_info : _get_methods()) {
            methods.append(method_info.to_dictionary());
        }
        return methods;
    }

    TypedArray<StringName> FSXScript::_get_members() const {
        //TODO
        return {};
    }

    bool FSXScript::_is_tool() const {
        //TODO
        return false;
    }

    void FSXScript::_update_exports() {
        //TODO
    }

    TypedArray<Dictionary> FSXScript::_get_documentation() const {
        //TODO
        return {};
    }

    bool FSXScript::_has_source_code() const {
        //TODO
        return false;
    }

    void *FSXScript::_instance_create(godot::Object *for_object) const {
        auto info = new GDExtensionScriptInstanceInfo;
        info->call_func = &fsx_script_instance_call;
        info->free_func = &fsx_script_instance_free;
        info->free_method_list_func = &fsx_script_instance_free_method_list;
        info->to_string_func = &fsx_script_instance_to_string;
        info->set_func = &fsx_script_instance_set;
        info->set_fallback_func = &fsx_script_instance_set_fallback;
        info->refcount_incremented_func = &fsx_script_instance_ref_count_incremented;
        info->refcount_decremented_func = &fsx_script_instance_ref_count_decremented;
        info->property_get_revert_func = &fsx_script_instance_property_get_revert;
        info->property_can_revert_func = &fsx_script_instance_property_can_revert;
        info->notification_func = &fsx_script_instance_notification;
        info->is_placeholder_func = &fsx_script_instance_is_placeholder;
        info->has_method_func = &fsx_script_instance_has_method;
        info->get_script_func = &fsx_script_instance_get_script;
        info->get_property_type_func = &fsx_script_instance_get_property_type;
        info->get_property_state_func = &fsx_script_instance_get_property_state;
        info->get_property_list_func = &fsx_script_instance_get_property_list;
        info->get_owner_func = &fsx_script_instance_get_owner;
        info->get_method_list_func = &fsx_script_instance_get_method_list;
        info->get_language_func = &fsx_script_instance_get_language;
        info->get_func = &fsx_script_instance_get;
        info->get_fallback_func = &fsx_script_instance_get_fallback;
        info->free_property_list_func = &fsx_script_instance_free_property_list;

        godot::UtilityFunctions::print("_instance_create:", get_path());

        auto instance = new FSXScriptInstance(Ref<FSXScript>(this), for_object);

//        instance->get_method_list();

        return internal::gdextension_interface_script_instance_create(info, instance);
    }

    void *FSXScript::_placeholder_instance_create(godot::Object *for_object) const {
        // TODO
        return nullptr;
    }

    bool FSXScript::_has_property_default_value(const StringName &property) const {
        // TODO
        return false;
    }

    Ref<Script> FSXScript::_get_base_script() const {
        godot::UtilityFunctions::print("_get_base_script");
        // TODO
        return {};
    }

    StringName FSXScript::_get_instance_base_type() const {
        godot::UtilityFunctions::print("_get_instance_base_type");
        // TODO
        return "Script";
    }

    void FSXScript::_update_exports_values(godot::HashMap<godot::StringName, godot::Variant> &values,
                                                          godot::List<godot::PropertyInfo> &propnames) {
        for (const KeyValue<StringName, Variant> &E : exported_members_defval_cache) {
            values[E.key] = E.value;
        }

        for (const PropertyInfo &prop_info : exported_members_cache) {
            propnames.push_back(prop_info);
        }

        if (base_script.is_valid()) {
            base_script->_update_exports_values(values, propnames);
        }
    }

    TypedArray<Dictionary> FSXScript::_get_script_property_list() const {
        godot::UtilityFunctions::print("_get_script_property_list");
        // TODO
        return {};
    }

    Variant FSXScript::_get_rpc_config() const {
        godot::UtilityFunctions::print("_get_rpc_config");
        return {};
    }

    Variant FSXScript::_get_property_default_value(const StringName &property) const {
        godot::UtilityFunctions::print("_get_property_default_value");
        return {};
    }

    TypedArray<Dictionary> FSXScript::_get_script_signal_list() const {
        godot::UtilityFunctions::print("_get_script_signal_list");
        return {};
    }

    bool FSXScript::_is_valid() const {
        //TODO
        return true;
    }

    void FSXScript::_load_source_code(const String &p_code) {
        code = p_code;

    }

} // godot