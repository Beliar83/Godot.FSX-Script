namespace GodotSharpGDExtension;

public unsafe partial class ArrayMesh
{
    public T SurfaceGetMaterial<T>(long surfIdx = 0) where T: Material
    {
        return (T)SurfaceGetMaterial(surfIdx);
    }
}

