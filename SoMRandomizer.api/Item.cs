using System.Runtime.InteropServices;
using SoMRandomizer.processing.hacks.openworld;
using SoMRandomizer.processing.openworld.randomization;

namespace SoMRandomizer.api;

public static class Item
{
    public static IntPtr Ref(PrizeItem item)
    {
        // NOTE: we could also pin and have a dict item -> handle
        return GCHandle.ToIntPtr(GCHandle.Alloc(item));
    }

    [UnmanagedCallersOnly(EntryPoint = "SoMR_Item_Unref")]
    public static void Unref(IntPtr item)
    {
        GCHandle.FromIntPtr(item).Free();;
    }

    [UnmanagedCallersOnly(EntryPoint = "SoMR_Item_GetName")]
    public static IntPtr GetName(IntPtr item)
    {
        var handle = GCHandle.FromIntPtr(item);
        if (handle.Target == null) return 0;
        var o = (PrizeItem)handle.Target;
        return Str.RefUni(o.prizeName);
    }

    [UnmanagedCallersOnly(EntryPoint = "SoMR_Item_GetType")]
    public static IntPtr GetType(IntPtr item)
    {
        var handle = GCHandle.FromIntPtr(item);
        if (handle.Target == null) return 0;
        var o = (PrizeItem)handle.Target;
        return Str.RefUni(o.prizeType);
    }

    [UnmanagedCallersOnly(EntryPoint = "SoMR_Item_GetEventFlag")]
    public static byte GetEventFlag(IntPtr item)
    {
        var handle = GCHandle.FromIntPtr(item);
        if (handle.Target == null) return 0; // nothing // TODO: or invalid?
        var o = (PrizeItem)handle.Target;
        return o.gotItemEventFlag;
    }

    [UnmanagedCallersOnly(EntryPoint = "SoMR_Item_GetItemId")]
    public static byte GetItemId(IntPtr item)
    {
        var handle = GCHandle.FromIntPtr(item);
        if (handle.Target == null) return (byte)ItemId.Nothing; // TODO: or invalid?
        var o = (PrizeItem)handle.Target;
        return (byte)o.itemId;
    }
}
