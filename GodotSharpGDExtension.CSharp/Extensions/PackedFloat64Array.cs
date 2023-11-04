using System.Runtime.CompilerServices;

namespace GodotSharpGDExtension;

public unsafe partial class PackedFloat64Array
{

    public double this[int index]
    {
        get => this[(long)index];
        set => this[(long)index] = value;
    }

    public double this[long index]
    {
        get => Unsafe.Read<float>((void*)GDExtensionInterface.PackedFloat64ArrayOperatorIndex(internalPointer, index));
        set => Set(index, value);
    }

    public static implicit operator PackedFloat64Array(List<double> self)
    {
        var data = new PackedFloat64Array();
        data.Resize(self.Count);
        for (int i = 0; i < self.Count; i++)
        {
            data[i] = self[i];
        }
        return data;
    }

    public static implicit operator PackedFloat64Array(double[] self)
    {
        var data = new PackedFloat64Array();
        data.Resize(self.Length);
        for (int i = 0; i < self.Length; i++)
        {
            data[i] = self[i];
        }
        return data;
    }

    public static implicit operator PackedFloat64Array(Span<double> self)
    {
        var data = new PackedFloat64Array();
        data.Resize(self.Length);
        for (int i = 0; i < self.Length; i++)
        {
            data[i] = self[i];
        }
        return data;
    }
}
