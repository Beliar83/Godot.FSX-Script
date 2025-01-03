﻿%{
/* Includes the header in the wrapper code */
#include "godot_cpp/variant/string.hpp"
#include "godot_cpp/variant/char_string.hpp"
#include "godot_cpp/variant/char_utils.hpp"
#include "godot_cpp/variant/packed_byte_array.hpp"
#include "godot_cpp/variant/packed_string_array.hpp"
#include "godot_cpp/variant/packed_float64_array.hpp"

%}
namespace godot {

struct AABB;
class Array;
struct Basis;
class Callable;
struct Color;
class Dictionary;
class NodePath;
class Object;
class PackedByteArray;
class PackedColorArray;
class PackedFloat32Array;
class PackedFloat64Array;
class PackedInt32Array;
class PackedInt64Array;
class PackedStringArray;
class PackedVector2Array;
class PackedVector3Array;
struct Plane;
struct Projection;
struct Quaternion;
struct Rect2;
struct Rect2i;
class Signal;
class StringName;
struct Transform2D;
struct Transform3D;
class Variant;
struct Vector2;
struct Vector2i;
struct Vector3;
struct Vector3i;
struct Vector4;
struct Vector4i;


%extend String {
	const char *AsString() {
		return $self->ascii().get_data();
	}
}

%rename(GodotString) String;
class String {
public:
	String();
	String(const String &from);
	String(const StringName &from);
	String(const NodePath &from);
	String(String &&other);
	String(const char *from);
	~String();	
int64_t casecmp_to(const String &to) const;
	int64_t nocasecmp_to(const String &to) const;
	int64_t naturalcasecmp_to(const String &to) const;
	int64_t naturalnocasecmp_to(const String &to) const;
	int64_t length() const;
	String substr(int64_t from, int64_t len = -1) const;
	String get_slice(const String &delimiter, int64_t slice) const;
	String get_slicec(int64_t delimiter, int64_t slice) const;
	int64_t get_slice_count(const String &delimiter) const;
	int64_t find(const String &what, int64_t from = 0) const;
	int64_t count(const String &what, int64_t from = 0, int64_t to = 0) const;
	int64_t countn(const String &what, int64_t from = 0, int64_t to = 0) const;
	int64_t findn(const String &what, int64_t from = 0) const;
	int64_t rfind(const String &what, int64_t from = -1) const;
	int64_t rfindn(const String &what, int64_t from = -1) const;
	bool match(const String &expr) const;
	bool matchn(const String &expr) const;
	bool begins_with(const String &text) const;
	bool ends_with(const String &text) const;
	bool is_subsequence_of(const String &text) const;
	bool is_subsequence_ofn(const String &text) const;
	PackedStringArray bigrams() const;
	double similarity(const String &text) const;
	String format(const Variant &values, const String &placeholder = "{_}") const;
	String replace(const String &what, const String &forwhat) const;
	String replacen(const String &what, const String &forwhat) const;
	String repeat(int64_t count) const;
	String insert(int64_t position, const String &what) const;
	String erase(int64_t position, int64_t chars = 1) const;
	String capitalize() const;
	String to_camel_case() const;
	String to_pascal_case() const;
	String to_snake_case() const;
	PackedStringArray split(const String &delimiter = String(), bool allow_empty = true, int64_t maxsplit = 0) const;
	PackedStringArray rsplit(const String &delimiter = String(), bool allow_empty = true, int64_t maxsplit = 0) const;
	PackedFloat64Array split_floats(const String &delimiter, bool allow_empty = true) const;
	String join(const PackedStringArray &parts) const;
	String to_upper() const;
	String to_lower() const;
	String left(int64_t length) const;
	String right(int64_t length) const;
	String strip_edges(bool left = true, bool right = true) const;
	String strip_escapes() const;
	String lstrip(const String &chars) const;
	String rstrip(const String &chars) const;
	String get_extension() const;
	String get_basename() const;
	String path_join(const String &file) const;
	int64_t unicode_at(int64_t at) const;
	String indent(const String &prefix) const;
	String dedent() const;
	int64_t hash() const;
	String md5_text() const;
	String sha1_text() const;
	String sha256_text() const;
	PackedByteArray md5_buffer() const;
	PackedByteArray sha1_buffer() const;
	PackedByteArray sha256_buffer() const;
	bool is_empty() const;
	bool contains(const String &what) const;
	bool is_absolute_path() const;
	bool is_relative_path() const;
	String simplify_path() const;
	String get_base_dir() const;
	String get_file() const;
	String xml_escape(bool escape_quotes = false) const;
	String xml_unescape() const;
	String uri_encode() const;
	String uri_decode() const;
	String c_escape() const;
	String c_unescape() const;
	String json_escape() const;
	String validate_node_name() const;
	String validate_filename() const;
	bool is_valid_identifier() const;
	bool is_valid_int() const;
	bool is_valid_float() const;
	bool is_valid_hex_number(bool with_prefix = false) const;
	bool is_valid_html_color() const;
	bool is_valid_ip_address() const;
	bool is_valid_filename() const;
	int64_t to_int() const;
	double to_float() const;
	int64_t hex_to_int() const;
	int64_t bin_to_int() const;
	String lpad(int64_t min_length, const String &character = " ") const;
	String rpad(int64_t min_length, const String &character = " ") const;
	String pad_decimals(int64_t digits) const;
	String pad_zeros(int64_t digits) const;
	String trim_prefix(const String &prefix) const;
	String trim_suffix(const String &suffix) const;
	PackedByteArray to_ascii_buffer() const;
	PackedByteArray to_utf8_buffer() const;
	PackedByteArray to_utf16_buffer() const;
	PackedByteArray to_utf32_buffer() const;
	PackedByteArray hex_decode() const;
	PackedByteArray to_wchar_buffer() const;
	static String num_scientific(double number);
	static String num(double number, int64_t decimals = -1);
	static String num_int64(int64_t number, int64_t base = 10, bool capitalize_hex = false);
	static String num_uint64(int64_t number, int64_t base = 10, bool capitalize_hex = false);
	static String chr(int64_t _char);
	static String humanize_size(int64_t size);
	static String utf8(const char *from, int len = -1);
	void parse_utf8(const char *from, int len = -1);
	static String utf16(const char16_t *from, int len = -1);
	void parse_utf16(const char16_t *from, int len = -1);
	//CharString utf8() const;
	//CharString ascii() const;
	//Char16String utf16() const;
	//Char32String utf32() const;
	//CharWideString wide_string() const;
	static String num_real(double p_num, bool p_trailing = true);
	bool operator==(const Variant &other) const;
	bool operator!=(const Variant &other) const;
	String operator%(const Variant &other) const;
	bool operatornot() const;
	String operator%(bool other) const;
	String operator%(int64_t other) const;
	String operator%(double other) const;
	bool operator==(const String &other) const;
	bool operator!=(const String &other) const;
	bool operator<(const String &other) const;
	bool operator<=(const String &other) const;
	bool operator>(const String &other) const;
	bool operator>=(const String &other) const;
	String operator+(const String &other) const;
	String operator%(const String &other) const;
	String operator%(const Vector2 &other) const;
	String operator%(const Vector2i &other) const;
	String operator%(const Rect2 &other) const;
	String operator%(const Rect2i &other) const;
	String operator%(const Vector3 &other) const;
	String operator%(const Vector3i &other) const;
	String operator%(const Transform2D &other) const;
	String operator%(const Vector4 &other) const;
	String operator%(const Vector4i &other) const;
	String operator%(const Plane &other) const;
	String operator%(const Quaternion &other) const;
	String operator%(const AABB &other) const;
	String operator%(const Basis &other) const;
	String operator%(const Transform3D &other) const;
	String operator%(const Projection &other) const;
	String operator%(const Color &other) const;
	bool operator==(const StringName &other) const;
	bool operator!=(const StringName &other) const;
	String operator+(const StringName &other) const;
	String operator%(const StringName &other) const;
	String operator%(const NodePath &other) const;
	String operator%(Object *other) const;
	String operator%(const Callable &other) const;
	String operator%(const Signal &other) const;
	String operator%(const Dictionary &other) const;
	String operator%(const Array &other) const;
	String operator%(const PackedByteArray &other) const;
	String operator%(const PackedInt32Array &other) const;
	String operator%(const PackedInt64Array &other) const;
	String operator%(const PackedFloat32Array &other) const;
	String operator%(const PackedFloat64Array &other) const;
	String operator%(const PackedStringArray &other) const;
	String operator%(const PackedVector2Array &other) const;
	String operator%(const PackedVector3Array &other) const;
	String operator%(const PackedColorArray &other) const;
	String &operator=(const String &other);
	String &operator=(String &&other);
	String &operator=(const char *p_str);
	String &operator=(const wchar_t *p_str);
	String &operator=(const char16_t *p_str);
	String &operator=(const char32_t *p_str);
	bool operator==(const char *p_str) const;
	bool operator==(const wchar_t *p_str) const;
	bool operator==(const char16_t *p_str) const;
	bool operator==(const char32_t *p_str) const;
	bool operator!=(const char *p_str) const;
	bool operator!=(const wchar_t *p_str) const;
	bool operator!=(const char16_t *p_str) const;
	bool operator!=(const char32_t *p_str) const;
	String operator+(const char *p_str);
	String operator+(const wchar_t *p_str);
	String operator+(const char16_t *p_str);
	String operator+(const char32_t *p_str);
	String operator+(char32_t p_char);
	String &operator+=(const String &p_str);
	String &operator+=(char32_t p_char);
	String &operator+=(const char *p_str);
	String &operator+=(const wchar_t *p_str);
	String &operator+=(const char32_t *p_str);
	const char32_t &operator[](int p_index) const;
	char32_t &operator[](int p_index);
	const char32_t *ptr() const;
	char32_t *ptrw();
};
}