using System.Runtime.CompilerServices;

namespace GodotSharpGDExtension;

public unsafe partial class PackedColorArray
{

    public Color this[int index]
    {
        get => this[(long)index];
        set => this[(long)index] = value;
    }

    public Color this[long index]
    {
        get => Unsafe.Read<Color>((void*)GDExtensionInterface.PackedColorArrayOperatorIndex(internalPointer, index));
        set => Set(index, value);
    }

    public static implicit operator PackedColorArray(List<Color> self)
    {
        var data = new PackedColorArray();
        data.Resize(self.Count);
        for (int i = 0; i < self.Count; i++)
        {
            data[i] = self[i];
        }
        return data;
    }

    public static implicit operator PackedColorArray(Color[] self)
    {
        var data = new PackedColorArray();
        data.Resize(self.Length);
        for (int i = 0; i < self.Length; i++)
        {
            data[i] = self[i];
        }
        return data;
    }

    public static implicit operator PackedColorArray(Span<Color> self)
    {
        var data = new PackedColorArray();
        data.Resize(self.Length);
        for (int i = 0; i < self.Length; i++)
        {
            data[i] = self[i];
        }
        return data;
    }
}
