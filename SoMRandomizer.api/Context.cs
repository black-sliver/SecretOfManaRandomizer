using System.Runtime.InteropServices;
using SoMRandomizer.processing.common;

namespace SoMRandomizer.api;

public static class Context
{
    public static IntPtr Ref(RandoContext context)
    {
        return GCHandle.ToIntPtr(GCHandle.Alloc(context));
    }

    [UnmanagedCallersOnly(EntryPoint = "SoMR_Context_Unref")]
    public static void Unref(IntPtr context)
    {
        GCHandle.FromIntPtr(context).Free();
    }

    [UnmanagedCallersOnly(EntryPoint = "SoMR_Context_GetError")]
    public static IntPtr GetError(IntPtr contextPtr)
    {
        var contextHandle = GCHandle.FromIntPtr(contextPtr);
        var context = (RandoContext)contextHandle.Target;
        if (context == null)
        {
            return Str.RefUni("Invalid context");
        }
        return context.error == null ? IntPtr.Zero : Str.RefUni(context.error);
    }

    [UnmanagedCallersOnly(EntryPoint = "SoMR_Context_GetWorkingData")]
    public static IntPtr GetWorkingData(IntPtr contextPtr)
    {
        var contextHandle = GCHandle.FromIntPtr(contextPtr);
        var context = (RandoContext)contextHandle.Target;
        return context == null ? IntPtr.Zero : WorkingData.Ref(context.workingData);
    }
}
