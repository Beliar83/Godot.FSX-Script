using SharpGen.Runtime;

namespace GodotSharpGDExtension;

public sealed unsafe partial class Variant
{

    struct Constructor
    {
        // GodotSharpGDExtension.CSharp.
        public FunctionCallback fromType;
        public FunctionCallback toType;
    }

    static readonly Constructor[] Constructors = new Constructor[(int)Type.Max];

    public static void Register()
    {
        for (var i = 1; i < (int)Type.Max; i++)
        {
            Constructors[i] = new Constructor()
            {
                fromType = GDExtensionInterface.GetVariantFromTypeConstructor((GDExtensionVariantType)i),
                toType = GDExtensionInterface.GetVariantToTypeConstructor((GDExtensionVariantType)i),
            };
        }
    }

    public static void SaveIntoPointer(object value, IntPtr ptr)
    {
        var valCast = ObjectToVariant(value);
        if (valCast is null)
        {
            return;
        }
        SaveIntoPointer(valCast, ptr);
    }

    public static void SaveIntoPointer(Variant value, IntPtr ptr)
    {
        var srcSpan = new Span<byte>((void*)value.internalPointer, 24);
        var dstSpan = new Span<byte>((void*)ptr, 24);
        srcSpan.CopyTo(dstSpan);
    }

    public static void SaveIntoPointer(Object value, IntPtr ptr)
    {
        IntPtr objectPtr = value?.internalPointer ?? IntPtr.Zero;
        GDExtensionInterface.CallGDExtensionVariantFromTypeConstructorFunc(Constructors[(int)Type.Object].fromType, ptr, objectPtr);
    }
    public static Object GetObjectFromVariant(Variant @object)
    {
        IntPtr res = IntPtr.Zero;
        GDExtensionInterface.CallGDExtensionTypeFromVariantConstructorFunc(Constructors[(int)Type.Object].toType, res, @object.internalPointer);
        return Object.ConstructUnknown(res);
    }

    public static Object GetObjectFromPointer(IntPtr ptr)
    {
        IntPtr res = IntPtr.Zero;
        GDExtensionInterface.CallGDExtensionTypeFromVariantConstructorFunc(Constructors[(int)Type.Object].toType, res, ptr);
        
        return Object.ConstructUnknown(res);
    }

    internal IntPtr internalPointer;

    public Type NativeType => (Type)GDExtensionInterface.VariantGetType(internalPointer);

    internal Variant()
    {
        internalPointer = GDExtensionInterface.MemAlloc(24);
        byte* dataPointer = (byte*)internalPointer;
        for (int i = 0; i < 24; i++)
        {
            dataPointer[i] = 0;
        }
    }

    internal Variant(IntPtr data)
    {
        internalPointer = data;
        GC.SuppressFinalize(this);
    }

    public static Variant Nil => new(GDExtensionInterface.VariantNewNil());

    public Variant(int value) : this((long)value) { }
    public Variant(float value) : this((double)value) { }

    ~Variant()
    {
        GDExtensionInterface.VariantDestroy(internalPointer);
        GDExtensionInterface.MemFree(internalPointer);
    }
}
