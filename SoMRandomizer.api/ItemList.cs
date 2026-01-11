using System.Runtime.InteropServices;
using SoMRandomizer.processing.openworld.randomization;

namespace SoMRandomizer.api;

public static class ItemList
{
    public static IntPtr Ref(List<PrizeItem> list)
    {
        // NOTE: we could also pin and have a dict list -> handle
        return GCHandle.ToIntPtr(GCHandle.Alloc(list));
    }

    [UnmanagedCallersOnly(EntryPoint = "SoMR_ItemList_Unref")]
    public static void Unref(IntPtr list)
    {
        GCHandle.FromIntPtr(list).Free();
    }

    [UnmanagedCallersOnly(EntryPoint = "SoMR_ItemList_Count")]
    public static int Count(IntPtr list)
    {
        var handle = GCHandle.FromIntPtr(list);
        if (handle.Target == null) return 0; // or -1 for error?
        var o = (List<PrizeItem>)handle.Target;
        return o.Count;
    }

    [UnmanagedCallersOnly(EntryPoint = "SoMR_ItemList_At")]
    public static IntPtr At(IntPtr list, int index)
    {
        var handle = GCHandle.FromIntPtr(list);
        if (handle.Target == null) return 0;
        var o = (List<PrizeItem>)handle.Target;
        return (index < 0 || index >= o.Count) ? IntPtr.Zero : Item.Ref(o[index]);
    }
}
