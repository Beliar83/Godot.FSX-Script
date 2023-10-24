#pragma once
#include "godot_cpp/classes/object.hpp"
#include "godot_cpp/classes/resource_format_loader.hpp"

namespace godot {

    class FSXResourceFormatLoader : public ResourceFormatLoader {
        GDCLASS(FSXResourceFormatLoader, ResourceFormatLoader);
    private:

    protected:
        static void _bind_methods();
    public:
        Variant _load(const godot::String &path, const godot::String &original_path, bool use_sub_threads, int32_t cache_mode) const override;
        PackedStringArray _get_recognized_extensions() const override;
        bool _handles_type(const godot::StringName &type) const override;
        String _get_resource_type(const godot::String &path) const override;
        bool _recognize_path(const godot::String &path, const godot::StringName &type) const override;
    };
}

