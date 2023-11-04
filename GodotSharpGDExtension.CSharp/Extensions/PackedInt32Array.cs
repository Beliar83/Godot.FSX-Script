using System.Runtime.CompilerServices;

namespace GodotSharpGDExtension;

public unsafe partial class PackedInt32Array
{

    public int this[int index]
    {
        get => this[(long)index];
        set => this[(long)index] = value;
    }

    public int this[long index]
    {
        get => Unsafe.Read<int>((void*)GDExtensionInterface.PackedInt32ArrayOperatorIndex(internalPointer, index));
        set => Set(index, value);
    }

    public static implicit operator PackedInt32Array(List<int> self)
    {
        var data = new PackedInt32Array();
        data.Resize(self.Count);
        for (int i = 0; i < self.Count; i++)
        {
            data[i] = self[i];
        }
        return data;
    }

    public static implicit operator PackedInt32Array(int[] self)
    {
        var data = new PackedInt32Array();
        data.Resize(self.Length);
        for (int i = 0; i < self.Length; i++)
        {
            data[i] = self[i];
        }
        return data;
    }

    public static implicit operator PackedInt32Array(Span<int> self)
    {
        var data = new PackedInt32Array();
        data.Resize(self.Length);
        for (int i = 0; i < self.Length; i++)
        {
            data[i] = self[i];
        }
        return data;
    }
}
