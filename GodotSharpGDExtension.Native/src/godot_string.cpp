#include "godot_string.h"
#include "godot_dotnet.h"
#include "godot_cpp/core/memory.hpp."

// Only for strings created by convert_string_to_dotnet
void delete_string(const GodotString string) {
    if (string.memory_own) {
        delete[] string.data;
    }
}

const wchar_t* to_string(const GodotString *string) {
    return string->data;
}

// GodotString from_string(const wchar_t* string) {
//     GodotString godot_string = {};
//     *godot_string.data = *string;
//     return godot_string;
// }