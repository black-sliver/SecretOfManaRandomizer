using System.Runtime.InteropServices;

namespace SoMRandomizer.api;

public static class Str
{
    [UnmanagedCallersOnly(EntryPoint = "SoMR_Str_Free")]
    public static void Free(IntPtr name)
    {
        Marshal.FreeHGlobal(name);
    }

    /// <summary>
    /// Returns a native pointer to (a copy of) the Unicode data of s.
    /// Use SoMR_Str_Free to free the memory.
    /// </summary>
    /// <param name="s">The string to get a native data pointer for.</param>
    /// <returns>A pointer to UCS2 data representing the string s</returns>
    public static IntPtr RefUni(string s)
    {
        return Marshal.StringToHGlobalUni(s);
    }

    /// <summary>
    /// Returns a managed string from Unicode data.
    /// The data is copied, the caller can free the original data immediately.
    /// </summary>
    /// <param name="data">The UCS2 bytes that represent the string.</param>
    /// <returns>A new string representing data.</returns>
    public static string FromUni(IntPtr data)
    {
        return Marshal.PtrToStringUni(data);
    }
}
