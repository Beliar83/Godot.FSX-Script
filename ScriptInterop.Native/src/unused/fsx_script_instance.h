#pragma once

#include "fsx_script.h"
#include "godot_cpp/classes/script.hpp"
#include "godot_cpp/classes/script_language_extension.hpp"
#include "godot_cpp/templates/hash_map.hpp"
#include "godot_cpp/variant/string.hpp"
#include "godot_cpp/variant/string_name.hpp"
#include <vector>

typedef godot::StringName* (*CreateDotnetInstance)(godot::String path, godot::String code);
typedef void (*CallMethod)(godot::StringName *script, godot::StringName name, std::vector<godot::Variant> args, godot::Object instance, godot::Variant* return_val);

namespace godot {
    GDExtensionBool
    fsx_script_instance_set(GDExtensionScriptInstanceDataPtr p_instance, GDExtensionConstStringNamePtr p_name,
                            GDExtensionConstVariantPtr p_value);

    GDExtensionBool
    fsx_script_instance_get(GDExtensionScriptInstanceDataPtr p_instance, GDExtensionConstStringNamePtr p_name,
                            GDExtensionVariantPtr r_ret);

    GDExtensionBool
    fsx_script_instance_set_fallback(GDExtensionScriptInstanceDataPtr p_instance, GDExtensionConstStringNamePtr p_name,
                                     GDExtensionConstVariantPtr p_value);

    GDExtensionBool
    fsx_script_instance_get_fallback(GDExtensionScriptInstanceDataPtr p_instance, GDExtensionConstStringNamePtr p_name,
                            GDExtensionVariantPtr r_ret);

    const GDExtensionPropertyInfo *fsx_script_instance_get_property_list(GDExtensionScriptInstanceDataPtr p_instance, uint32_t *r_count);

    void fsx_script_instance_free_property_list(GDExtensionScriptInstanceDataPtr p_instance,
                                              const GDExtensionPropertyInfo *p_list);

    GDExtensionVariantType fsx_script_instance_get_property_type(GDExtensionScriptInstanceDataPtr p_instance,
                                                               GDExtensionConstStringNamePtr p_name,
                                                               GDExtensionBool *r_is_valid);

    GDExtensionBool fsx_script_instance_property_can_revert(GDExtensionScriptInstanceDataPtr p_instance,
                                                          GDExtensionConstStringNamePtr p_name);

    GDExtensionBool fsx_script_instance_property_get_revert(GDExtensionScriptInstanceDataPtr p_instance,
                                                          GDExtensionConstStringNamePtr p_name,
                                                          GDExtensionVariantPtr r_ret);

    GDExtensionObjectPtr fsx_script_instance_get_owner(GDExtensionScriptInstanceDataPtr p_instance);

    void fsx_script_instance_property_state_add(GDExtensionConstStringNamePtr p_name, GDExtensionConstVariantPtr p_value,
                                              void *p_userdata);

    void fsx_script_instance_get_property_state(GDExtensionScriptInstanceDataPtr p_instance,
                                              GDExtensionScriptInstancePropertyStateAdd p_add_func, void *p_userdata);

    const GDExtensionMethodInfo* fsx_script_instance_get_method_list(GDExtensionScriptInstanceDataPtr p_instance, uint32_t *r_count);

    void fsx_script_instance_free_method_list(GDExtensionScriptInstanceDataPtr p_instance,
                                            const GDExtensionMethodInfo *p_list);

    GDExtensionBool
    fsx_script_instance_has_method(GDExtensionScriptInstanceDataPtr p_instance, GDExtensionConstStringNamePtr p_name);

    void fsx_script_instance_call(GDExtensionScriptInstanceDataPtr p_self, GDExtensionConstStringNamePtr p_method,
                                  const GDExtensionConstVariantPtr *p_args, GDExtensionInt p_argument_count,
                                  GDExtensionVariantPtr r_return, GDExtensionCallError *r_error);

    void fsx_script_instance_notification(GDExtensionScriptInstanceDataPtr p_instance, int32_t p_what);

    void fsx_script_instance_to_string(GDExtensionScriptInstanceDataPtr p_instance, GDExtensionBool *r_is_valid,
                                      GDExtensionStringPtr r_out);

    void fsx_script_instance_ref_count_incremented(GDExtensionScriptInstanceDataPtr p_instance);

    GDExtensionBool fsx_script_instance_ref_count_decremented(GDExtensionScriptInstanceDataPtr p_instance);

    GDExtensionObjectPtr fsx_script_instance_get_script(GDExtensionScriptInstanceDataPtr p_instance);

    GDExtensionBool fsx_script_instance_is_placeholder(GDExtensionScriptInstanceDataPtr p_instance);

    GDExtensionScriptLanguagePtr fsx_script_instance_get_language(GDExtensionScriptInstanceDataPtr p_instance);

    void fsx_script_instance_free(GDExtensionScriptInstanceDataPtr p_instance);

    class FSXScriptInstance  {
        Ref<FSXScript> script;
        Object *owner;
        godot::StringName *dotnet_instance;
        static CreateDotnetInstance create_dotnet_instance_func;
        static CallMethod call_method_func;

    public:
        static void SetDotnetFunctions(CreateDotnetInstance p_create_dotnet_instance, CallMethod p_call_method);


        Object *get_owner() { return owner; }

        Ref<FSXScript> get_script();

        FSXScriptInstance(const Ref<FSXScript> &p_script, Object* p_fsx_instance);
//        ~FSXScriptInstance();
        static void get_properties_from_array(List <PropertyInfo> *p_properties, const godot::TypedArray<Dictionary> &array);

        void get_property_list(List <PropertyInfo> *p_properties) const;

        std::vector<FSharpMethodInfo> get_method_list();

        bool get(StringName name, Variant &ret);

        godot::Variant call_method(godot::StringName name, std::vector<godot::Variant> args);
    };


}

