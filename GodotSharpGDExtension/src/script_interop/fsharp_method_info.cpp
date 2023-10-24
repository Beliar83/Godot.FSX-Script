//
// Created by Karsten on 17.10.2023.
//

#include "fsharp_method_info.h"

godot::Dictionary FSharpMethodInfo::to_dictionary() const {
    auto dict = godot::Dictionary();
    dict["name"] = name;
    auto return_val_dict = godot::Dictionary();
    return_val_dict["type"] = return_val.type;
    return_val_dict["name"] = return_val.name;
    return_val_dict["class_name"] = return_val.class_name;
    return_val_dict["hint"] = return_val.hint;
    return_val_dict["hint_string"] = return_val.hint_string;
    return_val_dict["usage"] = return_val.usage;
    dict["return_val"] = return_val_dict;
    dict["flags"] = flags;
    dict["id"] = id;
    auto arguments_array = godot::Array();
    for (const auto& argument : arguments) {
        auto argument_dict = godot::Dictionary();
        argument_dict["type"] = argument.type;
        argument_dict["name"] = argument.name;
        argument_dict["class_name"] = argument.class_name;
        argument_dict["hint"] = argument.hint;
        argument_dict["hint_string"] = argument.hint_string;
        argument_dict["usage"] = argument.usage;
        arguments_array.append(argument_dict);
    }
    dict["arguments"] = arguments_array;
    auto default_arguments_array = godot::Array();
    for (const auto& argument : default_arguments) {
        default_arguments_array.push_back(argument);
    }
    dict["default_arguments"] = default_arguments_array;
    return dict;
}
