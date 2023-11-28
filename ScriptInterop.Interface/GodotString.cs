namespace GodotSharpGDExtension;

public partial struct GodotString : IDisposable
{
    /// <inheritdoc />
    public void Dispose()
    {
        GDExtensionInterface.DeleteString(this);
    }
}
