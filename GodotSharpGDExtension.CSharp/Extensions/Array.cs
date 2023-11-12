using System.Collections;

namespace GodotSharpGDExtension;

public unsafe partial class GodotArray : Array, IList, ICollection
{

    public object this[int index]
    {
        get => this[(long)index];
        set
        {
            this[(long)index] = value;
        }
    }

    public object this[long index]
    {
        get
        {
            return Variant.VariantToObject(new Variant(GDExtensionMain.extensionInterface.GodotArray_operator_index.Data.Pointer(_internal_pointer, index)));
        }
        set
        {
            var parentData = Variant.ObjectToVariant(value);
            if (parentData is null) { return; }
            Variant.SaveIntoPointer(parentData, GDExtensionMain.extensionInterface.GodotArray_operator_index.Data.Pointer(_internal_pointer, index));
        }
    }

    public bool IsFixedSize => false;

    public bool IsSynchronized => false;

    public object SyncRoot => this;

    bool IList.IsReadOnly => false;

    int ICollection.Count => (int)Size();

    public long Length => Size();

    public int Add(object value)
    {
        Append(Variant.ObjectToVariant(value));
        return (int)Size() - 1;
    }

    public bool Contains(object value)
    {
        return Has(Variant.ObjectToVariant(value));
    }

    public void CopyTo(System.Array GodotArray, int index)
    {
        var amount = System.Math.Max(Length - index, GodotArray.Length);
        for (int i = 0; i < amount; i++)
        {
            GodotArray.SetValue(this[i + index], i);
        }
    }

    public IEnumerator GetEnumerator()
    {
        for (int i = 0; i < Length; i++)
        {
            yield return this[i];
        }
    }

    public int IndexOf(object value)
    {
        return (int)Find(Variant.ObjectToVariant(value));
    }

    public void Insert(int index, object value)
    {
        Insert((long)index, Variant.ObjectToVariant(value));
    }

    public void Remove(object value)
    {
        Erase(Variant.ObjectToVariant(value));
    }

    public void RemoveAt(int index)
    {
        RemoveAt((long)index);
    }
}
