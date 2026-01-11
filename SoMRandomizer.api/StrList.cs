using System.Runtime.InteropServices;

namespace SoMRandomizer.api;

public static class StrList
{
    public static IntPtr Ref(IEnumerable<string> list)
    {
        // NOTE: we could also pin and have a dict list -> handle
        return GCHandle.ToIntPtr(GCHandle.Alloc(list));
    }

    [UnmanagedCallersOnly(EntryPoint = "SoMR_StrList_Unref")]
    public static void Unref(IntPtr list)
    {
        GCHandle.FromIntPtr(list).Free();
    }

    [UnmanagedCallersOnly(EntryPoint = "SoMR_StrList_Count")]
    public static int Count(IntPtr list)
    {
        var handle = GCHandle.FromIntPtr(list);
        if (handle.Target == null) return 0; // or -1 for error?
        var o = (IEnumerable<string>)handle.Target;
        return o.Count();
    }

    [UnmanagedCallersOnly(EntryPoint = "SoMR_StrList_At")]
    public static IntPtr At(IntPtr list, int index)
    {
        var handle = GCHandle.FromIntPtr(list);
        if (handle.Target == null) return 0;
        var o = (IEnumerable<string>)handle.Target;
        if (o is string[] arr)
        {
            return (index < 0 || index >= arr.Length) ? IntPtr.Zero : Str.RefUni(arr[index]);
        }
        var lst = o as List<string> ?? o.ToList();
        return (index < 0 || index >= lst.Count) ? IntPtr.Zero : Str.RefUni(lst[index]);
    }
}
