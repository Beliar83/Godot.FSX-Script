%{
/* Includes the header in the wrapper code */
#include "godot_cpp/variant/typed_array.hpp"
%}

%import "array.i"

namespace godot {

    template <class T>
    class TypedArray : public Array {
            public:
            void operator =(const Array
            &p_array);
            TypedArray(
            const Variant
            &p_variant) :
            Array(p_variant.operator
            Array(), Variant::OBJECT, T::get_class_static(), Variant());
            TypedArray(
            const Array
            &p_array) :
            Array(p_array, Variant::OBJECT, T::get_class_static(), Variant());
            TypedArray()
            {
                set_typed(Variant::OBJECT, T::get_class_static(), Variant());
            }
    };
} // namespace godot