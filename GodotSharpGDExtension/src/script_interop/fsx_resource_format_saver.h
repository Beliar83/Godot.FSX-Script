#pragma once
#include "godot_cpp/classes/resource_format_saver.hpp"
#include "godot_cpp/classes/resource.hpp"
#include "godot_cpp/variant/string.hpp"

namespace godot {
    class FSXResourceFormatSaver : public ResourceFormatSaver {
    GDCLASS(FSXResourceFormatSaver, ResourceFormatSaver);
    private:

    protected:
        static void _bind_methods();
    public:
        void connect(Object *saver);

        PackedStringArray _get_recognized_extensions(const Ref<godot::Resource> &resource) const override;
        bool _recognize(const Ref<godot::Resource> &resource) const override;
        bool _recognize_path(const Ref<godot::Resource> &resource, const godot::String &path) const override;
        Error _save(const Ref<godot::Resource> &resource, const godot::String &path, uint32_t flags) override;
        Error _set_uid(const godot::String &path, int64_t uid) override;
    };
}

