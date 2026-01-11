using System.Runtime.InteropServices;
using SoMRandomizer.config.settings;

namespace SoMRandomizer.api;

public static class WorkingData
{
    public static IntPtr Ref(StringValueSettings data)
    {
        return GCHandle.ToIntPtr(GCHandle.Alloc(data));
    }

    [UnmanagedCallersOnly(EntryPoint = "SoMR_WorkingData_Unref")]
    public static void Unref(IntPtr dataPtr)
    {
        GCHandle.FromIntPtr(dataPtr).Free();
    }

    [UnmanagedCallersOnly(EntryPoint = "SoMR_WorkingData_GetStr")]
    public static IntPtr GetStr(IntPtr dataPtr, IntPtr keyPtr)
    {
        var key = Str.FromUni(keyPtr);
        var handle = GCHandle.FromIntPtr(dataPtr);
        var o = (StringValueSettings)handle.Target;
        return (o == null || key == null) ? 0 : Str.RefUni(o.get(key));
    }

    [UnmanagedCallersOnly(EntryPoint = "SoMR_WorkingData_SetStr")]
    public static void SetStr(IntPtr dataPtr, IntPtr keyPtr, IntPtr valuePtr)
    {
        var key = Str.FromUni(keyPtr);
        var value = Str.FromUni(valuePtr);
        var handle = GCHandle.FromIntPtr(dataPtr);
        var o = (StringValueSettings)handle.Target;
        o?.set(key, value);
    }

    [UnmanagedCallersOnly(EntryPoint = "SoMR_WorkingData_Dump")]
    public static IntPtr Dump(IntPtr dataPtr)
    {
        var handle = GCHandle.FromIntPtr(dataPtr);
        var o = (StringValueSettings)handle.Target;
        return o == null ? 0 : Str.RefUni(o.ToString());
    }
}
