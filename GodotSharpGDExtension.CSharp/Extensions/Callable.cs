using System.Runtime.CompilerServices;

namespace GodotSharpGDExtension;

public unsafe partial class Callable
{
    protected Callable(Delegate @delegate,GodotObject godotObject)
    {
        if (@delegate is null)
        {
            throw new NullReferenceException();
        }
        this.@delegate = @delegate;
        this.godotObject = godotObject;
        gCHandle = GCHandle.Alloc(this);
        internalPointer = GodotSharpGDExtensionCustomCallable.Libgodot_create_callable((IntPtr)gCHandle);
    }
    internal readonly GCHandle gCHandle;
    internal Delegate @delegate;
    internal GodotObject godotObject;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Callable From<T>(T target, GodotObject godotObject = null) where T : Delegate
    {
        if(target is null)
        {
            throw new NullReferenceException();
        }
        if(target.Target is GodotObject delobject)
        {
            godotObject = delobject;
        }
        if(godotObject is null)
        {
            throw new NullReferenceException();
        }
        return new Callable(target, godotObject);
    }

    internal void Free()
    {
        gCHandle.Free();
        @delegate = null;
        godotObject = null;
    }
}