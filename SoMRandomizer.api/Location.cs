using System.Runtime.InteropServices;
using SoMRandomizer.processing.openworld.randomization;

namespace SoMRandomizer.api;

public static class Location
{
    public static IntPtr Ref(PrizeLocation location)
    {
        // NOTE: we could also pin and have a dict item -> handle
        return GCHandle.ToIntPtr(GCHandle.Alloc(location));
    }

    [UnmanagedCallersOnly(EntryPoint = "SoMR_Location_Unref")]
    public static void Unref(IntPtr location)
    {
        GCHandle.FromIntPtr(location).Free();
    }

    [UnmanagedCallersOnly(EntryPoint = "SoMR_Location_GetName")]
    public static IntPtr GetName(IntPtr location)
    {
        var handle = GCHandle.FromIntPtr(location);
        if (handle.Target == null) return 0;
        var o = (PrizeLocation)handle.Target;
        return Str.RefUni(o.locationName);
    }

    [UnmanagedCallersOnly(EntryPoint = "SoMR_Location_GetMapNum")]
    public static int GetMapNum(IntPtr location)
    {
        var handle = GCHandle.FromIntPtr(location);
        if (handle.Target == null) return -1; // TODO: constant?
        var o = (PrizeLocation)handle.Target;
        return o.mapNum;
    }

    [UnmanagedCallersOnly(EntryPoint = "SoMR_Location_GetObjNum")]
    public static int GetObjNum(IntPtr location)
    {
        var handle = GCHandle.FromIntPtr(location);
        if (handle.Target == null) return -1; // TODO: constant?
        var o = (PrizeLocation)handle.Target;
        return o.objNum;
    }

    [UnmanagedCallersOnly(EntryPoint = "SoMR_Location_GetEventNum")]
    public static int GetEventNum(IntPtr location)
    {
        var handle = GCHandle.FromIntPtr(location);
        if (handle.Target == null) return -1; // TODO: constant?
        var o = (PrizeLocation)handle.Target;
        return o.eventNum;
    }

    [UnmanagedCallersOnly(EntryPoint = "SoMR_Location_GetEventIndex")]
    public static int GetEventIndex(IntPtr location)
    {
        var handle = GCHandle.FromIntPtr(location);
        if (handle.Target == null) return -1; // TODO: constant?
        var o = (PrizeLocation)handle.Target;
        return o.eventReplacementIndex;
    }

    [UnmanagedCallersOnly(EntryPoint = "SoMR_Location_GetLocationId")]
    public static byte GetLocationId(IntPtr location)
    {
        var handle = GCHandle.FromIntPtr(location);
        if (handle.Target == null) return 0; // TODO: constant?
        var o = (PrizeLocation)handle.Target;
        return (byte)o.locationId;
    }

    [UnmanagedCallersOnly(EntryPoint = "SoMR_Location_GetRequirements")]
    public static IntPtr GetRequirements(IntPtr location)
    {
        var handle = GCHandle.FromIntPtr(location);
        if (handle.Target == null) return IntPtr.Zero;
        var o = (PrizeLocation)handle.Target;
        return StrList.Ref(o.lockedByPrizes);
    }
}
