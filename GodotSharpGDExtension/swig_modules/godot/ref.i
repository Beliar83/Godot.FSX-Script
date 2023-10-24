%{
#include "godot_cpp/classes/ref.hpp"
%}

namespace godot {
        template <class T>
        class Ref {
            public:
            Ref(const Ref &p_from);
            template <class T_Other> Ref(const Ref<T_Other> &p_from);
            Ref(T *p_reference);
            Ref(const Variant &p_variant);
        };
};