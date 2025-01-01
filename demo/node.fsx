[<Godot.Tool>]
module TestNode
open Godot


//This sets the godot class to inherit from
type Base = Sprite2D 

//Define fields in this type. Use [Export] to mark exported fields.
type State = {
	value: int
	float: float
	float32: float32
	object: GodotObject
} with static member Default() = { value = 0; object = null; float = 0; float32 = 0f }

let _process(self : Base, delta, state : State) =
	self.RotationDegrees <- self.RotationDegrees + 100f * delta
	{ state with value = state.value + 1 }

let methodWithNodeParam(self : Base, node: Node, state : State) =
	state

let methodThatChangesState(self : Base, newValue: int, state : State) =
	{ state with value = newValue }

let methodWithReturnParameter(self: Base) (state: State) =
	(state, 5)
