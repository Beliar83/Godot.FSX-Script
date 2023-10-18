%{
/* Includes the header in the wrapper code */
#include "godot_cpp/classes/node.hpp"
#include "godot_cpp/core/type_info.hpp"
%}

%import "godot_string.i"
%import "string_name.i"
%import "object.i"

namespace godot{
    class BitField;
    class Window;
    class SceneTree;
    class Tween;
    class Ref;
    class Viewport;
    class MultiplayerAPI;
    class InputEvent;

    class Node : public Object {
            public:
            enum ProcessMode {
                PROCESS_MODE_INHERIT = 0,
                PROCESS_MODE_PAUSABLE = 1,
                PROCESS_MODE_WHEN_PAUSED = 2,
                PROCESS_MODE_ALWAYS = 3,
                PROCESS_MODE_DISABLED = 4,
            };

            enum ProcessThreadGroup {
                PROCESS_THREAD_GROUP_INHERIT = 0,
                PROCESS_THREAD_GROUP_MAIN_THREAD = 1,
                PROCESS_THREAD_GROUP_SUB_THREAD = 2,
            };

            enum ProcessThreadMessages {
                FLAG_PROCESS_THREAD_MESSAGES = 1,
                FLAG_PROCESS_THREAD_MESSAGES_PHYSICS = 2,
                FLAG_PROCESS_THREAD_MESSAGES_ALL = 3,
            };

            enum DuplicateFlags {
                DUPLICATE_SIGNALS = 1,
                DUPLICATE_GROUPS = 2,
                DUPLICATE_SCRIPTS = 4,
                DUPLICATE_USE_INSTANTIATION = 8,
            };

            enum InternalMode {
                INTERNAL_MODE_DISABLED = 0,
                INTERNAL_MODE_FRONT = 1,
                INTERNAL_MODE_BACK = 2,
            };

            static const int NOTIFICATION_ENTER_TREE = 10;
            static const int NOTIFICATION_EXIT_TREE = 11;
            static const int NOTIFICATION_MOVED_IN_PARENT = 12;
            static const int NOTIFICATION_READY = 13;
            static const int NOTIFICATION_PAUSED = 14;
            static const int NOTIFICATION_UNPAUSED = 15;
            static const int NOTIFICATION_PHYSICS_PROCESS = 16;
            static const int NOTIFICATION_PROCESS = 17;
            static const int NOTIFICATION_PARENTED = 18;
            static const int NOTIFICATION_UNPARENTED = 19;
            static const int NOTIFICATION_SCENE_INSTANTIATED = 20;
            static const int NOTIFICATION_DRAG_BEGIN = 21;
            static const int NOTIFICATION_DRAG_END = 22;
            static const int NOTIFICATION_PATH_RENAMED = 23;
            static const int NOTIFICATION_CHILD_ORDER_CHANGED = 24;
            static const int NOTIFICATION_INTERNAL_PROCESS = 25;
            static const int NOTIFICATION_INTERNAL_PHYSICS_PROCESS = 26;
            static const int NOTIFICATION_POST_ENTER_TREE = 27;
            static const int NOTIFICATION_DISABLED = 28;
            static const int NOTIFICATION_ENABLED = 29;
            static const int NOTIFICATION_NODE_RECACHE_REQUESTED = 30;
            static const int NOTIFICATION_EDITOR_PRE_SAVE = 9001;
            static const int NOTIFICATION_EDITOR_POST_SAVE = 9002;
            static const int NOTIFICATION_WM_MOUSE_ENTER = 1002;
            static const int NOTIFICATION_WM_MOUSE_EXIT = 1003;
            static const int NOTIFICATION_WM_WINDOW_FOCUS_IN = 1004;
            static const int NOTIFICATION_WM_WINDOW_FOCUS_OUT = 1005;
            static const int NOTIFICATION_WM_CLOSE_REQUEST = 1006;
            static const int NOTIFICATION_WM_GO_BACK_REQUEST = 1007;
            static const int NOTIFICATION_WM_SIZE_CHANGED = 1008;
            static const int NOTIFICATION_WM_DPI_CHANGE = 1009;
            static const int NOTIFICATION_VP_MOUSE_ENTER = 1010;
            static const int NOTIFICATION_VP_MOUSE_EXIT = 1011;
            static const int NOTIFICATION_OS_MEMORY_WARNING = 2009;
            static const int NOTIFICATION_TRANSLATION_CHANGED = 2010;
            static const int NOTIFICATION_WM_ABOUT = 2011;
            static const int NOTIFICATION_CRASH = 2012;
            static const int NOTIFICATION_OS_IME_UPDATE = 2013;
            static const int NOTIFICATION_APPLICATION_RESUMED = 2014;
            static const int NOTIFICATION_APPLICATION_PAUSED = 2015;
            static const int NOTIFICATION_APPLICATION_FOCUS_IN = 2016;
            static const int NOTIFICATION_APPLICATION_FOCUS_OUT = 2017;
            static const int NOTIFICATION_TEXT_SERVER_CHANGED = 2018;

            static void print_orphan_nodes();
            void add_sibling(Node *sibling, bool force_readable_name = false);
            void set_name(const String &name);
            StringName get_name() const;
            void add_child(Node *node, bool force_readable_name = false, Node::InternalMode internal = (Node::InternalMode)0);
            void remove_child(Node *node);
            void reparent(Node *new_parent, bool keep_global_transform = true);
            int32_t get_child_count(bool include_internal = false) const;
            TypedArray<Node> get_children(bool include_internal = false) const;
            Node *get_child(int32_t idx, bool include_internal = false) const;
            bool has_node(const NodePath &path) const;
            Node *get_node_internal(const NodePath &path) const;
            Node *get_node_or_null(const NodePath &path) const;
            Node *get_parent() const;
            Node *find_child(const String &pattern, bool recursive = true, bool owned = true) const;
            TypedArray<Node> find_children(const String &pattern, const String &type = String(), bool recursive = true, bool owned = true) const;
            Node *find_parent(const String &pattern) const;
            bool has_node_and_resource(const NodePath &path) const;
            Array get_node_and_resource(const NodePath &path);
            bool is_inside_tree() const;
            bool is_ancestor_of(Node *node) const;
            bool is_greater_than(Node *node) const;
            NodePath get_path() const;
            NodePath get_path_to(Node *node, bool use_unique_path = false) const;
            void add_to_group(const StringName &group, bool persistent = false);
            void remove_from_group(const StringName &group);
            bool is_in_group(const StringName &group) const;
            void move_child(Node *child_node, int32_t to_index);
            TypedArray<StringName> get_groups() const;
            void set_owner(Node *owner);
            Node *get_owner() const;
            int32_t get_index(bool include_internal = false) const;
            void print_tree();
            void print_tree_pretty();
            void set_scene_file_path(const String &scene_file_path);
            String get_scene_file_path() const;
            void propagate_notification(int32_t what);
            void propagate_call(const StringName &method, const Array &args = Array(), bool parent_first = false);
            void set_physics_process(bool enable);
            double get_physics_process_delta_time() const;
            bool is_physics_processing() const;
            double get_process_delta_time() const;
            void set_process(bool enable);
            void set_process_priority(int32_t priority);
            int32_t get_process_priority() const;
            void set_physics_process_priority(int32_t priority);
            int32_t get_physics_process_priority() const;
            bool is_processing() const;
            void set_process_input(bool enable);
            bool is_processing_input() const;
            void set_process_shortcut_input(bool enable);
            bool is_processing_shortcut_input() const;
            void set_process_unhandled_input(bool enable);
            bool is_processing_unhandled_input() const;
            void set_process_unhandled_key_input(bool enable);
            bool is_processing_unhandled_key_input() const;
            void set_process_mode(Node::ProcessMode mode);
            Node::ProcessMode get_process_mode() const;
            bool can_process() const;
            void set_process_thread_group(Node::ProcessThreadGroup mode);
            Node::ProcessThreadGroup get_process_thread_group() const;
            void set_process_thread_messages(BitField<Node::ProcessThreadMessages> flags);
            BitField<Node::ProcessThreadMessages> get_process_thread_messages() const;
            void set_process_thread_group_order(int32_t order);
            int32_t get_process_thread_group_order() const;
            void set_display_folded(bool fold);
            bool is_displayed_folded() const;
            void set_process_internal(bool enable);
            bool is_processing_internal() const;
            void set_physics_process_internal(bool enable);
            bool is_physics_processing_internal() const;
            Window *get_window() const;
            Window *get_last_exclusive_window() const;
            SceneTree *get_tree() const;
//            Ref<Tween> create_tween();
            Node *duplicate(int32_t flags = 15) const;
            void replace_by(Node *node, bool keep_groups = false);
            void set_scene_instance_load_placeholder(bool load_placeholder);
            bool get_scene_instance_load_placeholder() const;
            void set_editable_instance(Node *node, bool is_editable);
            bool is_editable_instance(Node *node) const;
            Viewport *get_viewport() const;
            void queue_free();
            void request_ready();
            bool is_node_ready() const;
            void set_multiplayer_authority(int32_t id, bool recursive = true);
            int32_t get_multiplayer_authority() const;
            bool is_multiplayer_authority() const;
//            Ref<MultiplayerAPI> get_multiplayer() const;
            void rpc_config(const StringName &method, const Variant &config);
            void set_editor_description(const String &editor_description);
            String get_editor_description() const;
            void set_unique_name_in_owner(bool enable);
            bool is_unique_name_in_owner() const;
//            private: private: Error rpc_internal(const Variant **args, GDExtensionInt arg_count);
//            public: template<class... Args> Error rpc(const StringName &method, const Args&... args) {
//                std::array<Variant, 1 + sizeof...(Args)> variant_args { Variant(method), Variant(args)... };
//                std::array<const Variant *, 1 + sizeof...(Args)> call_args;
//                for(size_t i = 0; i < variant_args.size(); i++) {
//                    call_args[i] = &variant_args[i];
//                }
//                return rpc_internal(call_args.data(), variant_args.size());
//            }
//            private: private: Error rpc_id_internal(const Variant **args, GDExtensionInt arg_count);
//            public: template<class... Args> Error rpc_id(int64_t peer_id, const StringName &method, const Args&... args) {
//                std::array<Variant, 2 + sizeof...(Args)> variant_args { Variant(peer_id), Variant(method), Variant(args)... };
//                std::array<const Variant *, 2 + sizeof...(Args)> call_args;
//                for(size_t i = 0; i < variant_args.size(); i++) {
//                    call_args[i] = &variant_args[i];
//                }
//                return rpc_id_internal(call_args.data(), variant_args.size());
//            }
            void update_configuration_warnings();
//            private: private: Variant call_deferred_thread_group_internal(const Variant **args, GDExtensionInt arg_count);
//            public: template<class... Args> Variant call_deferred_thread_group(const StringName &method, const Args&... args) {
//                std::array<Variant, 1 + sizeof...(Args)> variant_args { Variant(method), Variant(args)... };
//                std::array<const Variant *, 1 + sizeof...(Args)> call_args;
//                for(size_t i = 0; i < variant_args.size(); i++) {
//                    call_args[i] = &variant_args[i];
//                }
//                return call_deferred_thread_group_internal(call_args.data(), variant_args.size());
//            }
            void set_deferred_thread_group(const StringName &property, const Variant &value);
            void notify_deferred_thread_group(int32_t what);
//            private: private: Variant call_thread_safe_internal(const Variant **args, GDExtensionInt arg_count);
//            public: template<class... Args> Variant call_thread_safe(const StringName &method, const Args&... args) {
//                std::array<Variant, 1 + sizeof...(Args)> variant_args { Variant(method), Variant(args)... };
//                std::array<const Variant *, 1 + sizeof...(Args)> call_args;
//                for(size_t i = 0; i < variant_args.size(); i++) {
//                    call_args[i] = &variant_args[i];
//                }
//                return call_thread_safe_internal(call_args.data(), variant_args.size());
//            }
            void set_thread_safe(const StringName &property, const Variant &value);
            void notify_thread_safe(int32_t what);
            virtual void _process(double delta);
            virtual void _physics_process(double delta);
            virtual void _enter_tree();
            virtual void _exit_tree();
            virtual void _ready();
            virtual PackedStringArray _get_configuration_warnings() const;
            virtual void _input(const Ref<InputEvent> &event);
            virtual void _shortcut_input(const Ref<InputEvent> &event);
            virtual void _unhandled_input(const Ref<InputEvent> &event);
            virtual void _unhandled_key_input(const Ref<InputEvent> &event);
        };

};