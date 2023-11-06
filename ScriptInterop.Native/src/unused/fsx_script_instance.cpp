//
// Created by Karsten on 03.08.2023.
//

#include <utility>
#include <vector>

#include "fsx_script_instance.h"
#include "fsx_script.h"
#include "../godot-cpp/gdextension_interface.h"
#include "godot_cpp/classes/engine.hpp"
#include "godot_cpp/classes/global_constants.hpp"
#include "godot_cpp/core/property_info.hpp"
#include "godot_cpp/variant/dictionary.hpp"
#include "godot_cpp/variant/typed_array.hpp"
#include "godot_cpp/variant/utility_functions.hpp"

CreateDotnetInstance godot::FSXScriptInstance::create_dotnet_instance_func = nullptr;
CallMethod godot::FSXScriptInstance::call_method_func = nullptr;

GDExtensionBool
godot::fsx_script_instance_set(GDExtensionScriptInstanceDataPtr p_instance, GDExtensionConstStringNamePtr p_name,
                               GDExtensionConstVariantPtr p_value) {
//    godot::UtilityFunctions::print("fsx_script_instance_set");
    return 0;
}

GDExtensionBool
godot::fsx_script_instance_get(GDExtensionScriptInstanceDataPtr p_instance, GDExtensionConstStringNamePtr p_name,
                               GDExtensionVariantPtr r_ret) {
//    godot::UtilityFunctions::print("fsx_script_instance_get: start");
    auto instance = reinterpret_cast<FSXScriptInstance*>(p_instance);
    auto name = reinterpret_cast<const StringName*>(p_name);
    auto ret = reinterpret_cast<Variant*>(r_ret);
//    godot::UtilityFunctions::print("fsx_script_instance_get: instance->get");
    bool valid = instance->get(*name, *ret);
//    godot::UtilityFunctions::print("fsx_script_instance_get: return");
    return valid;
}

GDExtensionBool godot::fsx_script_instance_set_fallback(GDExtensionScriptInstanceDataPtr p_instance,
                                                        GDExtensionConstStringNamePtr p_name,
                                                        GDExtensionConstVariantPtr p_value) {
//    godot::UtilityFunctions::print("fsx_script_instance_set_fallback");
    return 0;
}

GDExtensionBool godot::fsx_script_instance_get_fallback(GDExtensionScriptInstanceDataPtr p_instance,
                                                        GDExtensionConstStringNamePtr p_name,
                                                        GDExtensionVariantPtr r_ret) {
//    godot::UtilityFunctions::print("fsx_script_instance_get_fallback");
    return 0;
}


const GDExtensionPropertyInfo *
godot::fsx_script_instance_get_property_list(GDExtensionScriptInstanceDataPtr p_instance, uint32_t *r_count) {
    auto instance = reinterpret_cast<FSXScriptInstance*>(p_instance);
    godot::List<godot::PropertyInfo> properties;
    instance->get_property_list(&properties);
    *r_count = properties.size();
    auto array = reinterpret_cast<GDExtensionPropertyInfo *>(memalloc(sizeof(GDExtensionPropertyInfo) * properties.size()));
    int index = 0;
    for (const ::godot::PropertyInfo &E : properties) {
					array[index].type = static_cast<GDExtensionVariantType>(E.type);
					array[index].name = E.name._native_ptr();
					array[index].hint = E.hint;
					array[index].hint_string = E.hint_string._native_ptr();
					array[index].class_name = E.class_name._native_ptr();                                                                                               
					array[index].usage = E.usage;
					index++;
				}
    return array;
}

void godot::fsx_script_instance_free_property_list(GDExtensionScriptInstanceDataPtr p_instance,
                                                   const GDExtensionPropertyInfo *p_list) {

}

GDExtensionVariantType godot::fsx_script_instance_get_property_type(GDExtensionScriptInstanceDataPtr p_instance,
                                                                    GDExtensionConstStringNamePtr p_name,
                                                                    GDExtensionBool *r_is_valid) {
    return GDEXTENSION_VARIANT_TYPE_NIL;
}

GDExtensionBool godot::fsx_script_instance_property_can_revert(GDExtensionScriptInstanceDataPtr p_instance,
                                                               GDExtensionConstStringNamePtr p_name) {
    return 0;
}

GDExtensionBool godot::fsx_script_instance_property_get_revert(GDExtensionScriptInstanceDataPtr p_instance,
                                                               GDExtensionConstStringNamePtr p_name,
                                                               GDExtensionVariantPtr r_ret) {
    return 0;
}

GDExtensionObjectPtr godot::fsx_script_instance_get_owner(GDExtensionScriptInstanceDataPtr p_instance) {
//    godot::UtilityFunctions::print("fsx_script_instance_get_owner");
    return nullptr;
}

void
godot::fsx_script_instance_property_state_add(GDExtensionConstStringNamePtr p_name, GDExtensionConstVariantPtr p_value,
                                              void *p_userdata) {

}

void godot::fsx_script_instance_get_property_state(GDExtensionScriptInstanceDataPtr p_instance,
                                                   GDExtensionScriptInstancePropertyStateAdd p_add_func,
                                                   void *p_userdata) {

}

const GDExtensionMethodInfo* godot::fsx_script_instance_get_method_list(GDExtensionScriptInstanceDataPtr p_instance, uint32_t *r_count) {
    godot::UtilityFunctions::print("fsx_script_instance_get_method_list");
    auto fsx_instance = (FSXScriptInstance*)p_instance;
    auto infos = new std::vector<GDExtensionMethodInfo>();
    std::vector<FSharpMethodInfo> methodList = fsx_instance->get_method_list();
    for (auto &methodInfo : methodList) {
        auto info = GDExtensionMethodInfo();
        info.name = methodInfo.name._native_ptr();
        info.arguments = reinterpret_cast<GDExtensionPropertyInfo *>(methodInfo.arguments.data());
        info.flags = methodInfo.flags;
        info.id = methodInfo.id;
        info.argument_count = methodInfo.arguments.size();
        info.arguments = reinterpret_cast<GDExtensionPropertyInfo *>(methodInfo.arguments.data());
        info.default_argument_count = methodInfo.default_arguments.size();
        info.default_arguments = reinterpret_cast<GDExtensionVariantPtr *>(methodInfo.default_arguments.data());
        info.return_value.name = methodInfo.name._native_ptr();
        info.return_value.class_name = methodInfo.return_val.class_name._native_ptr();
        info.return_value.type = static_cast<GDExtensionVariantType>(methodInfo.return_val.type);
        info.return_value.hint = methodInfo.return_val.hint;
        info.return_value.hint_string = methodInfo.return_val.hint_string._native_ptr();
        info.return_value.usage = methodInfo.return_val.usage;
        infos->push_back(info);
    }
    
    return infos->data();
}

void godot::fsx_script_instance_free_method_list(GDExtensionScriptInstanceDataPtr p_instance,
                                                 const GDExtensionMethodInfo *p_list) {

}

GDExtensionBool godot::fsx_script_instance_has_method(GDExtensionScriptInstanceDataPtr p_instance,
                                                      GDExtensionConstStringNamePtr p_name) {
    return 0;
}

void godot::fsx_script_instance_call(GDExtensionScriptInstanceDataPtr p_self, GDExtensionConstStringNamePtr p_method,
                                     const GDExtensionConstVariantPtr *p_args, GDExtensionInt p_argument_count,
                                     GDExtensionVariantPtr r_return, GDExtensionCallError *r_error) {
    auto method_name = (StringName*)p_method;
    auto fsx_instance = (FSXScriptInstance*)p_self;
    auto args = std::vector<Variant>();
    godot::UtilityFunctions::print("fsx_script_instance_call: ", *method_name);
    fsx_instance->call_method(*method_name, args);

    auto ret_val = fsx_instance->get_script()->_call_method(*method_name, args, fsx_instance->get_owner());
    r_return = &ret_val;

}

void godot::fsx_script_instance_notification(GDExtensionScriptInstanceDataPtr p_instance, int32_t p_what) {

}

void godot::fsx_script_instance_to_string(GDExtensionScriptInstanceDataPtr p_instance, GDExtensionBool *r_is_valid,
                                          GDExtensionStringPtr r_out) {

}

void godot::fsx_script_instance_ref_count_incremented(GDExtensionScriptInstanceDataPtr p_instance) {

}

GDExtensionBool godot::fsx_script_instance_ref_count_decremented(GDExtensionScriptInstanceDataPtr p_instance) {
    return 0;
}

GDExtensionObjectPtr godot::fsx_script_instance_get_script(GDExtensionScriptInstanceDataPtr p_instance) {
    return reinterpret_cast<FSXScriptInstance *>(p_instance)->get_script().ptr();
}

GDExtensionBool godot::fsx_script_instance_is_placeholder(GDExtensionScriptInstanceDataPtr p_instance) {
    return 0;
}

GDExtensionScriptLanguagePtr godot::fsx_script_instance_get_language(GDExtensionScriptInstanceDataPtr p_instance) {
//    godot::UtilityFunctions::print("fsx_script_instance_get_language");
    return nullptr;
}

void godot::fsx_script_instance_free(GDExtensionScriptInstanceDataPtr p_instance) {

}

void godot::FSXScriptInstance::get_property_list(godot::List<godot::PropertyInfo>* p_properties) const {
    auto props = script->get_script_property_list();

    for (int i = 0, size = props.size(); i < size; i++) {
        auto property = props[i].operator Dictionary();
        for (int j = 0; j < property.keys().size(); ++j) {
            godot::UtilityFunctions::print(property.keys()[j]);
            godot::UtilityFunctions::print(property.values()[j].stringify());
        }
    }


    // Call _get_property_list

    ERR_FAIL_COND(!script.is_valid());

    StringName method = String("_get_property_list").to_pascal_case();

    GDExtensionCallError call_error;
    if (owner->has_method("method"))
    {
        auto ret = owner->call(method);
        if (ret.get_type() == Variant::NIL)
        {
            ERR_PRINT("Unexpected error calling '_get_property_list'");
        } else {
            get_properties_from_array(p_properties, ret);
        }
    }

    get_properties_from_array(p_properties, props);
}

void godot::FSXScriptInstance::get_properties_from_array(godot::List<godot::PropertyInfo> *p_properties,
                                                      const godot::TypedArray<Dictionary> &array) {
    for (int i = 0, size = array.size(); i < size; i++) {
        auto property = array[i].operator Dictionary();
//        for (int j = 0; j < property.keys().size(); ++j) {
//            godot::UtilityFunctions::print(property.keys()[j]);
//            godot::UtilityFunctions::print(property.values()[j].stringify());
//        }
        godot::UtilityFunctions::print(property["type"]);
        godot::UtilityFunctions::print(property["name"]);
        godot::UtilityFunctions::print(property["hint"]);
        godot::UtilityFunctions::print(property["hint_string"]);
        godot::UtilityFunctions::print(property["usage"]);
        godot::UtilityFunctions::print(property["class_name"]);
        auto info = PropertyInfo(
                Variant::Type(property["type"].operator uint32_t()),
                property["name"].operator StringName(),
                PropertyHint(property["hint"].operator uint32_t()),
                property["hint_string"],
                property["usage"].operator uint32_t(),
                property["class_name"]
                );
        p_properties->push_back(info);
    }
}

godot::FSXScriptInstance::FSXScriptInstance(const Ref<godot::FSXScript> &p_script, Object* p_fsx_instance) : script(p_script), owner(p_fsx_instance) {
    ERR_FAIL_NULL_MSG(create_dotnet_instance_func, "create_dotnet_instance_func is null");
    dotnet_instance = create_dotnet_instance_func(script->get_path(), script->code);
    godot::UtilityFunctions::print("FSXScriptInstance: dotnet_instance = ", *dotnet_instance);
}

godot::Ref<godot::FSXScript> godot::FSXScriptInstance::get_script() {
    return script;
}

bool godot::FSXScriptInstance::get(godot::StringName name, godot::Variant &ret) {
    return false;
    //    godot::UtilityFunctions::print("FSXScriptInstance::get: call");
    auto result = owner->call("TryGetValue", name).operator Dictionary();

//    godot::UtilityFunctions::print("FSXScriptInstance::get: verify");
    if (result["valid"].booleanize()) {
        ret = result["value"];
//        godot::UtilityFunctions::print("FSXScriptInstance::get: return true");
        return true;
    }

//    godot::UtilityFunctions::print("FSXScriptInstance::get: return false");
    return false;
}

std::vector<FSharpMethodInfo> godot::FSXScriptInstance::get_method_list() {
    godot::UtilityFunctions::print("get_method_list");
    return script->_get_methods();
}

void
godot::FSXScriptInstance::SetDotnetFunctions(CreateDotnetInstance p_create_dotnet_instance, CallMethod p_call_method) {
    godot::UtilityFunctions::print("SetDotnetFunctions");
    create_dotnet_instance_func = p_create_dotnet_instance;
    call_method_func = p_call_method;
}

godot::Variant
godot::FSXScriptInstance::call_method(godot::StringName name, std::vector<godot::Variant> args) {
    Variant ret_val = {};
    godot::UtilityFunctions::print("call_method ", dotnet_instance, ".", name);
    call_method_func(dotnet_instance, std::move(name), std::move(args), *owner, &ret_val);
    return ret_val;
}
