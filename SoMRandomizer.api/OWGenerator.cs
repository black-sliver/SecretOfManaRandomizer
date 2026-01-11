using System.Runtime.InteropServices;
using SoMRandomizer.processing.openworld;

namespace SoMRandomizer.api;

// ReSharper disable once InconsistentNaming
public static class OWGenerator
{
    public static IntPtr Ref(OpenWorldGenerator generator)
    {
        // NOTE: we could also pin and have a dict item -> handle
        return GCHandle.ToIntPtr(GCHandle.Alloc(generator));
    }

    [UnmanagedCallersOnly(EntryPoint = "SoMR_OWGenerator_GetItems")]
    public static IntPtr GetItems(IntPtr generatorPtr)
    {
        var generatorHandle = GCHandle.FromIntPtr(generatorPtr);
        var generator = (OpenWorldGenerator)generatorHandle.Target;
        return generator == null ? IntPtr.Zero : ItemList.Ref(generator.GetItems());
    }

    [UnmanagedCallersOnly(EntryPoint = "SoMR_OWGenerator_GetLocations")]
    public static IntPtr GetLocations(IntPtr generatorPtr)
    {
        var generatorHandle = GCHandle.FromIntPtr(generatorPtr);
        var generator = (OpenWorldGenerator)generatorHandle.Target;
        return generator == null ? IntPtr.Zero : LocationList.Ref(generator.GetLocations());
    }

    [UnmanagedCallersOnly(EntryPoint = "SoMR_OWGenerator_Unref")]
    public static void Unref(IntPtr generator)
    {
        GCHandle.FromIntPtr(generator).Free();
    }
}
