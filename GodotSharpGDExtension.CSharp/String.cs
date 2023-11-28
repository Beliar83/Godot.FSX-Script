namespace GodotSharpGDExtension;

public partial class String : IDisposable
{
    private GodotString? data = null;
    
    public static implicit operator String(string value)
    {
        return new String(GDExtensionInterface.ConvertStringFromWideString(value));
    }

    public static implicit operator string(String value)
    {
        // Godot strings are immutable
        if (value.data.HasValue)
        {
            return value.data.Value.Data;
        }
        value.data = GDExtensionInterface.ConvertStringToDotnet(value.InternalPointer);
        return value.data.Value.Data;
    }
    
    /// <inheritdoc />
    public void Dispose()
    {
        data?.Dispose();
        GC.SuppressFinalize(this);
    }
}
