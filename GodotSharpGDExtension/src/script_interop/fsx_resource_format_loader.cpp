//
// Created by Karsten on 30.06.2023.
//

#include "fsx_resource_format_loader.h"
#include "fsx_script.h"
#include "fsx_script_language.h"
#include "godot_cpp/variant/variant.hpp"
#include "godot_cpp/classes/gd_script.hpp"
#include "godot_cpp/classes/file_access.hpp"
#include "godot_cpp/variant/utility_functions.hpp"

void godot::FSXResourceFormatLoader::_bind_methods() {
}

godot::PackedStringArray godot::FSXResourceFormatLoader::_get_recognized_extensions() const {
    auto array = PackedStringArray();
    array.push_back("fsx");
    return array;
}

bool godot::FSXResourceFormatLoader::_handles_type(const godot::StringName &type) const {
    return type == godot::StringName("FSXScript");
}

godot::String godot::FSXResourceFormatLoader::_get_resource_type(const godot::String &path) const {
    godot::UtilityFunctions::print("_get_resource_type");
    godot::UtilityFunctions::print(path);
    godot::UtilityFunctions::print(path.get_extension());
    const char *string = path.get_extension().to_lower() == "fsx" ? "Script" : "";
    godot::UtilityFunctions::print(string);
    return string;
}

godot::Variant godot::FSXResourceFormatLoader::_load(const godot::String &path, const godot::String &original_path,
                                                     bool use_sub_threads, int32_t cache_mode) const {
    godot::UtilityFunctions::print("load");
    auto script = Ref<FSXScript>();
    script.instantiate();
    auto script_file = FileAccess::open(path, FileAccess::READ);
    auto source_code = script_file->get_as_text();
    script_file->close();
    godot::UtilityFunctions::print("Path: ", path);
    godot::UtilityFunctions::print("script Path 1: ", script->get_path());
    script->set_path(path);
    godot::UtilityFunctions::print("script Path 2: ", script->get_path());
    script->_load_source_code(source_code);
    godot::UtilityFunctions::print("script Path 3: ", script->get_path());
    return script;
}

bool godot::FSXResourceFormatLoader::_recognize_path(const godot::String &path, const godot::StringName &type) const {
    return (type.is_empty() || type == String("Script") || type == String("FSXScript")) &&  path.get_extension() == "fsx";
}
