using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using System.Text;

namespace FSXScript.Editor;

public partial class HostFxr
{
    private static IntPtr library;

    private enum HostFxrDelegateType
    {
        HdtLoadAssemblyAndGetFunctionPointer = 5,
    }

    private static unsafe delegate* unmanaged<ushort*, ushort*, ushort*, nint, nint, nint*, int>
        loadAssemblyAndGetFunctionPointerDelegate;

    private static string GetHostFxrPath()
    {
        if (OperatingSystem.IsWindows())
        {
            char[] buffer = new char[1024];
            nint bufferSize = buffer.Length;
            int rc = get_hostfxr_path_windows(buffer, ref bufferSize, IntPtr.Zero);
            if (rc == 0)
            {
                return new string(buffer.AsSpan().Slice(0, (int)bufferSize - 1));
            }

            throw new Exception($"get_hostfxr_path failed: {rc}");
        }
        else
        {
            byte[] buffer = new byte[1024];
            nint bufferSize = buffer.Length;
            int rc = get_hostfxr_path_non_windows(buffer, ref bufferSize, IntPtr.Zero);
            if (rc == 0)
            {
                return Encoding.UTF8.GetString(buffer.AsSpan().Slice(0, (int)bufferSize - 1));
            }

            throw new Exception($"get_hostfxr_path failed: {rc}");
        }
    }

    [LibraryImport("*", EntryPoint = "get_hostfxr_path")]
    private static partial int get_hostfxr_path_windows(char[] buffer, ref nint bufferSize, IntPtr parameters);

    [LibraryImport("*", EntryPoint = "get_hostfxr_path")]
    private static partial int get_hostfxr_path_non_windows(byte[] buffer, ref nint bufferSize, IntPtr parameters);

    [LibraryImport("hostfxr", EntryPoint = "hostfxr_initialize_for_runtime_config",
        StringMarshalling = StringMarshalling.Utf16)]
    private static partial int hostfxr_initialize_for_runtime_config(string configPath,
        IntPtr parameters, out IntPtr context);

    [LibraryImport("hostfxr", EntryPoint = "hostfxr_get_runtime_delegate")]
    private static partial int hostfxr_get_runtime_delegate(IntPtr hostContextHandle, HostFxrDelegateType type,
        out IntPtr functionDelegate);

    [LibraryImport("hostfxr", EntryPoint = "hostfxr_close")]
    private static partial int hostfxr_close(IntPtr hostContextHandle);


    public static unsafe void LoadAssembly(string configPath)
    {
        try
        {
            if (library == IntPtr.Zero)
            {
                string hostFxrPath = GetHostFxrPath();
                library = NativeLibrary.Load(hostFxrPath);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($@"Failed to get load hostfxr library: {ex.Message}");
            return;
        }

        int result = hostfxr_initialize_for_runtime_config(configPath, IntPtr.Zero, out IntPtr contextHandle);
        if (result != 0 || contextHandle == IntPtr.Zero)
        {
            Console.WriteLine($@"Init failed: {result:x8}");
#pragma warning disable CA1806 // This already is in an error condition and done for cleanup
            hostfxr_close(contextHandle);
#pragma warning restore CA1806
            return;
        }

        result = hostfxr_get_runtime_delegate(contextHandle, HostFxrDelegateType.HdtLoadAssemblyAndGetFunctionPointer,
            out IntPtr functionDelegate);
        if (result == 0 && functionDelegate != IntPtr.Zero)
        {
            result = hostfxr_close(contextHandle);
            if (result != 0)
            {
                Console.WriteLine($@"Could not close hostfxr: {result:x8}");
            }

            loadAssemblyAndGetFunctionPointerDelegate =
                (delegate* unmanaged<ushort*, ushort*, ushort*, IntPtr, IntPtr, IntPtr*, int>)functionDelegate;
            return;
        }

        Console.WriteLine($@"Get delegate failed: {result:x8}");
    }

    public static unsafe int LoadAssemblyAndGetFunctionPointer(string assemblyPath, string typeName,
        string methodName, string delegateTypeName, out IntPtr functionDelegate)
    {
        int retVal;
        fixed (nint* functionDelegateNative = &functionDelegate)
        fixed (void* delegateTypeNameNative = &Utf16StringMarshaller.GetPinnableReference(delegateTypeName))
        fixed (void* methodNameNative = &Utf16StringMarshaller.GetPinnableReference(methodName))
        fixed (void* typeNameNative = &Utf16StringMarshaller.GetPinnableReference(typeName))
        fixed (void* assemblyPathNative = &Utf16StringMarshaller.GetPinnableReference(assemblyPath))
        {
            retVal = loadAssemblyAndGetFunctionPointerDelegate((ushort*)assemblyPathNative, (ushort*)typeNameNative,
                (ushort*)methodNameNative, (IntPtr)delegateTypeNameNative, IntPtr.Zero, functionDelegateNative);
        }

        return retVal;
    }

    public static unsafe int LoadAssemblyAndGetFunctionPointerForUnmanagedCallersOnly(string assemblyPath,
        string typeName,
        string methodName, out IntPtr functionDelegate)
    {
        int retVal;
        fixed (nint* functionDelegateNative = &functionDelegate)
        fixed (void* methodNameNative = &Utf16StringMarshaller.GetPinnableReference(methodName))
        fixed (void* typeNameNative = &Utf16StringMarshaller.GetPinnableReference(typeName))
        fixed (void* assemblyPathNative = &Utf16StringMarshaller.GetPinnableReference(assemblyPath))
        {
            retVal = loadAssemblyAndGetFunctionPointerDelegate((ushort*)assemblyPathNative, (ushort*)typeNameNative,
                (ushort*)methodNameNative, -1, IntPtr.Zero, functionDelegateNative);
        }

        return retVal;
    }
}
