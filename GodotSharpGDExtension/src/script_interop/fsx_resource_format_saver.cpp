//
// Created by Karsten on 29.06.2023.
//

#include "fsx_resource_format_saver.h"
#include "fsx_script.h"
#include "godot_cpp/classes/file_access.hpp"
#include "godot_cpp/classes/global_constants.hpp"
#include "godot_cpp/classes/resource_format_saver.hpp"
#include "godot_cpp/core/class_db.hpp"
#include "godot_cpp/variant/utility_functions.hpp"

void godot::FSXResourceFormatSaver::_bind_methods() {
}

godot::PackedStringArray
godot::FSXResourceFormatSaver::_get_recognized_extensions(const godot::Ref<godot::Resource> &resource) const {
    godot::UtilityFunctions::print("_get_recognized_extensions");
    auto array = PackedStringArray();
    array.push_back("fsx");
    return array;
}

bool godot::FSXResourceFormatSaver::_recognize(const godot::Ref<godot::Resource> &resource) const {
    godot::UtilityFunctions::print("_recognize");
    bool b = Object::cast_to<FSXScript>(*resource) != nullptr;
    godot::UtilityFunctions::print(godot::Variant(b));
    return b;
}

bool godot::FSXResourceFormatSaver::_recognize_path(const godot::Ref<godot::Resource> &resource,
                                                    const godot::String &path) const {
    godot::UtilityFunctions::print("_recognize_path");
    return _recognize(resource) && path.get_extension() == "fsx";
}

godot::Error
godot::FSXResourceFormatSaver::_save(const godot::Ref<godot::Resource> &resource, const godot::String &path,
                                     uint32_t flags) {
    godot::UtilityFunctions::print("_save");
    auto file = godot::FileAccess::open(path, FileAccess::WRITE);
    godot::UtilityFunctions::print("_save 1", resource->get_path());
    resource->set_path(path);
    godot::UtilityFunctions::print("_save 2", resource->get_path());
    auto script = Object::cast_to<FSXScript>(*resource);
    file->store_string(script->get_source_code());
    file->close();

    return Error::OK;
}

godot::Error godot::FSXResourceFormatSaver::_set_uid(const godot::String &path, int64_t uid) {
    // TODO
    return Error::ERR_UNCONFIGURED;
}
