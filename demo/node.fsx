module TestNode
open Godot
open Godot.Collections

//This sets the godot class to inherit from
type Base = Node2D

//Define fields in this type. Use [Export] to mark exported fields.
type State =
    struct
        val _boolean : bool
        val _integer : int
        val _decimal : float
        val _text : string
        val _Vector2: Vector2
        val _Vector2I: Vector2I
        val _Rect2: Rect2
        val _Rect2I: Rect2I
        val _Vector3: Vector3
        val _Vector3I: Vector3I
        val _Transform2D: Transform2D
        val _Vector4: Vector4
        val _Vector4I: Vector4I
        val _Plane: Plane
        val _Quaternion: Quaternion
        val _Aabb: Aabb
        val _Basis: Basis
        val _Transform3D: Transform3D
        val _Projection: Projection
        val _Color: Color
        val _StringName: StringName
        val _NodePath: NodePath
        val _Rid: Rid
        val _Object: GodotObject 
        val _Callable: Callable
        val _Signal: Signal
        val _Dictionary: Dictionary
        val _TypedDictionary: Dictionary<string, int>
        val _Array: Array
        val _PackedByteArray: byte[]
        val _PackedInt32Array: int32[]
        val _PackedInt64Array: int64[]
        val _PackedFloat32Array: float32[]
        val _PackedFloat64Array: double[]
        val _PackedStringArray: string[]
        val _PackedVector2Array: Vector2[]
        val _PackedVector3Array: Vector3[]
        val _PackedVector4Array: Vector4[]
        val _PackedColorArray: Color[]
    end

let _process(self : Base, delta: float) =
    () 
