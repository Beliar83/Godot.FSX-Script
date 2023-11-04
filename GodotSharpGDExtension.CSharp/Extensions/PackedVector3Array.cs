using System.Runtime.CompilerServices;

namespace GodotSharpGDExtension;

public unsafe partial class PackedVector3Array
{

    public Vector3 this[int index]
    {
        get => this[(long)index];
        set => this[(long)index] = value;
    }

    public Vector3 this[long index]
    {
        get => Unsafe.Read<Vector3>((void*)GDExtensionInterface.PackedVector3ArrayOperatorIndex(internalPointer, index));
        set => Set(index, value);
    }

    public static implicit operator PackedVector3Array(List<Vector3> self)
    {
        var data = new PackedVector3Array();
        data.Resize(self.Count);
        for (int i = 0; i < self.Count; i++)
        {
            data[i] = self[i];
        }
        return data;
    }

    public static implicit operator PackedVector3Array(Vector3[] self)
    {
        var data = new PackedVector3Array();
        data.Resize(self.Length);
        for (int i = 0; i < self.Length; i++)
        {
            data[i] = self[i];
        }
        return data;
    }

    public static implicit operator PackedVector3Array(Span<Vector3> self)
    {
        var data = new PackedVector3Array();
        data.Resize(self.Length);
        for (int i = 0; i < self.Length; i++)
        {
            data[i] = self[i];
        }
        return data;
    }
}
