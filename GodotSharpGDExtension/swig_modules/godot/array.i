%{
/* Includes the header in the wrapper code */
#include "godot_cpp/variant/array.hpp"
%}

namespace godot {

        class Callable;
        class Dictionary;
        class PackedByteArray;
        class PackedColorArray;
        class PackedFloat32Array;
        class PackedFloat64Array;
        class PackedInt32Array;
        class PackedInt64Array;
        class PackedStringArray;
        class PackedVector2Array;
        class PackedVector3Array;
        class StringName;
        class Variant;

        class Array {
            Array();
            Array(
            const Array
            &from);
            Array(
            const Array
            &base, int64_t
            type,
            const StringName
            &class_name,
            const Variant
            &script);
            Array(
            const PackedByteArray
            &from);
            Array(
            const PackedInt32Array
            &from);
            Array(
            const PackedInt64Array
            &from);
            Array(
            const PackedFloat32Array
            &from);
            Array(
            const PackedFloat64Array
            &from);
            Array(
            const PackedStringArray
            &from);
            Array(
            const PackedVector2Array
            &from);
            Array(
            const PackedVector3Array
            &from);
            Array(
            const PackedColorArray
            &from);
            Array(Array && other);
            ~Array();
            int64_t
            size() const;
            bool is_empty() const;
            void clear();
            int64_t
            hash() const;
            void assign(const Array
            &array);
            void push_back(const Variant
            &value);
            void push_front(const Variant
            &value);
            void append(const Variant
            &value);
            void append_array(const Array
            &array);
            int64_t resize(int64_t size);
            int64_t
            insert(int64_t
            position,
            const Variant
            &value);
            void remove_at(int64_t position);
            void fill(const Variant
            &value);
            void erase(const Variant
            &value);
            Variant
            front() const;
            Variant
            back() const;
            Variant
            pick_random() const;
            int64_t
            find(
            const Variant
            &what, int64_t
            from = 0) const;
            int64_t
            rfind(
            const Variant
            &what, int64_t
            from = -1) const;
            int64_t
            count(
            const Variant
            &value) const;
            bool has(const Variant
            &value) const;
            Variant pop_back();
            Variant pop_front();
            Variant pop_at(int64_t position);
            void sort();
            void sort_custom(const Callable
            &func);
            void shuffle();
            int64_t
            bsearch(
            const Variant
            &value, bool
            before = true) const;
            int64_t
            bsearch_custom(
            const Variant
            &value,
            const Callable
            &func, bool
            before = true) const;
            void reverse();
            Array
            duplicate(bool
            deep = false) const;
            Array
            slice(int64_t
            begin, int64_t
            end = 2147483647, int64_t
            step = 1, bool
            deep = false) const;
            Array
            filter(
            const Callable
            &method) const;
            Array
            map(
            const Callable
            &method) const;
            Variant
            reduce(
            const Callable
            &method,
            const Variant
            &accum) const;
            bool any(const Callable
            &method) const;
            bool all(const Callable
            &method) const;
            Variant
            max() const;
            Variant
            min() const;
            bool is_typed() const;
            bool is_same_typed(const Array
            &array) const;
            int64_t
            get_typed_builtin() const;
            StringName
            get_typed_class_name() const;
            Variant
            get_typed_script() const;
            void make_read_only();
            bool is_read_only() const;
            bool operator==(const Variant
            &other) const;
            bool operator!=(const Variant
            &other) const;
            bool operatornot() const;
            bool operator==(const Array
            &other) const;
            bool operator!=(const Array
            &other) const;
            bool operator<(const Array
            &other) const;
            bool operator<=(const Array
            &other) const;
            bool operator>(const Array
            &other) const;
            bool operator>=(const Array
            &other) const;
            Array
            operator +(const Array
            &other) const;
            Array & operator =(const Array
            &other);
            Array & operator = (Array && other);
            template < class... Args >
            static Array make(Args... args) {
                return helpers::append_all(Array(), args...);
            }
            const Variant
            &operator[](
            int p_index) const;
            Variant & operator[](
            int p_index);
            void set_typed(uint32_t p_type, const StringName
            &p_class_name,
            const Variant
            &p_script);
            void _ref(const Array
            &p_from) const;
        };
}