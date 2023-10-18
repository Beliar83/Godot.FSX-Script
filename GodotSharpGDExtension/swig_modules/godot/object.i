%{
/* Includes the header in the wrapper code */
#include "godot_cpp/classes/object.hpp"
%}

%import "godot_string.i"
%import "string_name.i"
%import "typed_array.i"
%import "global_constants.i"

namespace godot {
    %rename(GodotObject) Object;
    class Object {
            public:

            enum ConnectFlags {
                CONNECT_DEFERRED = 1,
                CONNECT_PERSIST = 2,
                CONNECT_ONE_SHOT = 4,
                CONNECT_REFERENCE_COUNTED = 8,
            };

            static const int NOTIFICATION_POSTINITIALIZE = 0;
            static const int NOTIFICATION_PREDELETE = 1;

            String get_class() const;
            bool is_class(const String &_class) const;
            void set(const StringName &property, const Variant &value);
            Variant get(const StringName &property) const;
            void set_indexed(const NodePath &property_path, const Variant &value);
            Variant get_indexed(const NodePath &property_path) const;
            TypedArray<Dictionary> get_property_list() const;
            TypedArray<Dictionary> get_method_list() const;
            bool property_can_revert(const StringName &property) const;
            Variant property_get_revert(const StringName &property) const;
            void notification(int32_t what, bool reversed = false);
            String to_string();
            uint64_t get_instance_id() const;
            void set_script(const Variant &script);
            Variant get_script() const;
            void set_meta(const StringName &name, const Variant &value);
            void remove_meta(const StringName &name);
            Variant get_meta(const StringName &name, const Variant &_default = nullptr) const;
            bool has_meta(const StringName &name) const;
            TypedArray<StringName> get_meta_list() const;
            void add_user_signal(const String &signal, const Array &arguments = Array());
            bool has_user_signal(const StringName &signal) const;
//            public: template<class... Args> Error emit_signal(const StringName &signal, const Args&... args) {
//                std::array<Variant, 1 + sizeof...(Args)> variant_args { Variant(signal), Variant(args)... };
//                std::array<const Variant *, 1 + sizeof...(Args)> call_args;
//                for(size_t i = 0; i < variant_args.size(); i++) {
//                    call_args[i] = &variant_args[i];
//                }
//                return emit_signal_internal(call_args.data(), variant_args.size());
//            }
//            public: template<class... Args> Variant call(const StringName &method, const Args&... args) {
//                std::array<Variant, 1 + sizeof...(Args)> variant_args { Variant(method), Variant(args)... };
//                std::array<const Variant *, 1 + sizeof...(Args)> call_args;
//                for(size_t i = 0; i < variant_args.size(); i++) {
//                    call_args[i] = &variant_args[i];
//                }
//                return call_internal(call_args.data(), variant_args.size());
//            }
//            public: template<class... Args> Variant call_deferred(const StringName &method, const Args&... args) {
//                std::array<Variant, 1 + sizeof...(Args)> variant_args { Variant(method), Variant(args)... };
//                std::array<const Variant *, 1 + sizeof...(Args)> call_args;
//                for(size_t i = 0; i < variant_args.size(); i++) {
//                    call_args[i] = &variant_args[i];
//                }
//                return call_deferred_internal(call_args.data(), variant_args.size());
//            }
            void set_deferred(const StringName &property, const Variant &value);
            Variant callv(const StringName &method, const Array &arg_array);
            bool has_method(const StringName &method) const;
            bool has_signal(const StringName &signal) const;
            TypedArray<Dictionary> get_signal_list() const;
            TypedArray<Dictionary> get_signal_connection_list(const StringName &signal) const;
            TypedArray<Dictionary> get_incoming_connections() const;
            Error connect(const StringName &signal, const Callable &callable, uint32_t flags = 0);
            void disconnect(const StringName &signal, const Callable &callable);
            bool is_connected(const StringName &signal, const Callable &callable) const;
            void set_block_signals(bool enable);
            bool is_blocking_signals() const;
            void notify_property_list_changed();
            void set_message_translation(bool enable);
            bool can_translate_messages() const;
            String tr(const StringName &message, const StringName &context = String()) const;
            String tr_n(const StringName &message, const StringName &plural_message, int32_t n, const StringName &context = String()) const;
            bool is_queued_for_deletion() const;
            void cancel_free();
    };
};