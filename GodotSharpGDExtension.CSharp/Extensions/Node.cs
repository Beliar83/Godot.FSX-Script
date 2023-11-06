namespace GodotSharpGDExtension;

public unsafe partial class Node : GodotObject
{

    public T GetNode<T>(NodePath path) where T : Node => (T)GetNode(path);

    public T AddChild<T>(bool forceReadableName = (bool)false, Node.InternalMode @internal = (Node.InternalMode)0) where T : Node ,new()
    {
        var node = new T();
        AddChild(node, forceReadableName, @internal);
        return node;
    }



}
