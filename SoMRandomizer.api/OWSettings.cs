using System.Runtime.InteropServices;
using SoMRandomizer.config.settings;

namespace SoMRandomizer.api;

// ReSharper disable once InconsistentNaming
public static class OWSettings
{
    public static IntPtr Ref(OpenWorldSettings settings)
    {
        // NOTE: we could also pin and have a dict item -> handle
        return GCHandle.ToIntPtr(GCHandle.Alloc(settings));
    }

    [UnmanagedCallersOnly(EntryPoint = "SoMR_OWSettings_Unref")]
    public static void Unref(IntPtr settings)
    {
        GCHandle.FromIntPtr(settings).Free();
    }

    [UnmanagedCallersOnly(EntryPoint = "SoMR_OWSettings_SetStr")]
    public static void SetStr(IntPtr settingsPtr, IntPtr keyPtr, IntPtr valuePtr)
    {
        var key = Str.FromUni(keyPtr);
        var value = Str.FromUni(valuePtr);
        var handle = GCHandle.FromIntPtr(settingsPtr);
        var o = (StringValueSettings)handle.Target;
        o?.set(key, value);
    }

    [UnmanagedCallersOnly(EntryPoint = "SoMR_OWSettings_Dump")]
    public static IntPtr Dump(IntPtr settings)
    {
        var handle = GCHandle.FromIntPtr(settings);
        var o = (OpenWorldSettings)handle.Target;
        return o == null ? 0 : Str.RefUni(o.ToString());
    }
}
