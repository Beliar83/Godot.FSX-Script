// namespace GodotSharpGDExtension;
//
// internal partial struct GodotWideString : IDisposable
// {
//     /// <inheritdoc />
//     public void Dispose()
//     {
//         if (ownership != Ownership.Managed)
//         {
//             NativeMethods.delete_wide_string(this);
//         }
//     }
//
// #pragma warning disable CS0465 // Introducing a 'Finalize' method can interfere with destructor invocation
//     public void Finalize()
//     {
//         Dispose();
//     }
// #pragma warning restore CS0465 // Introducing a 'Finalize' method can interfere with destructor invocation
// }
//
// internal partial struct GodotUTF16String : IDisposable
// {
//     /// <inheritdoc />
//     public void Dispose()
//     {
//         if (ownership != Ownership.Managed)
//         {
//             NativeMethods.delete_utf16_string(this);
//         }
//     }
//
// #pragma warning disable CS0465 // Introducing a 'Finalize' method can interfere with destructor invocation
//     public void Finalize()
//     {
//         Dispose();
//     }
// #pragma warning restore CS0465 // Introducing a 'Finalize' method can interfere with destructor invocation
// }
//
