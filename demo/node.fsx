module TestNode
open Godot

//This sets the godot class to inherit from
type Base = Node2D 

//Define fields in this type. Use [Export] to mark exported fields.
type State =
	struct
		val test : int
		val string : string
	end

let _process(self : Base, delta: float) =
	()
