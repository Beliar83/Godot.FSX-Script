module node

//This sets the godot class to inherit from
type Base = Node2D

//Define fields in this type. Use [Export] to mark exported fields.
type State = struct end

let _process(self : Base, delta: float) =
	()
