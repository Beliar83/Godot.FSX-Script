using System.Runtime.CompilerServices;

namespace GodotSharpGDExtension;

public unsafe partial class Callable
{
    protected Callable(Delegate @delegate,Object @object)
    {
        if (@delegate is null)
        {
            throw new NullReferenceException();
        }
        this.@delegate = @delegate;
        this.@object = @object;
        gCHandle = GCHandle.Alloc(this);
        internalPointer = GodotSharpGDExtensionCustomCallable.Libgodot_create_callable((IntPtr)gCHandle);
    }
    internal readonly GCHandle gCHandle;
    internal Delegate @delegate;
    internal Object @object;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Callable From<T>(T target, Object @object = null) where T : Delegate
    {
        if(target is null)
        {
            throw new NullReferenceException();
        }
        if(target.Target is Object delobject)
        {
            @object = delobject;
        }
        if(@object is null)
        {
            throw new NullReferenceException();
        }
        return new Callable(target, @object);
    }

    internal void Free()
    {
        gCHandle.Free();
        @delegate = null;
        @object = null;
    }
}