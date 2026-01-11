using System.Runtime.InteropServices;
using SoMRandomizer.config.settings;
using SoMRandomizer.processing.common;
using SoMRandomizer.processing.openworld;
using SoMRandomizer.processing.openworld.events;
using SoMRandomizer.processing.openworld.randomization;
using SoMRandomizer.util.rng;

namespace SoMRandomizer.api;

internal class NameComparer(string name)
{
    public bool Equals(PrizeItem other) => other.prizeName.Equals(name);
    public bool Equals(PrizeLocation other) => other.locationName.Equals(name);
    public bool StartsWith(PrizeItem other) => other.prizeName.StartsWith(name);
    public bool StartsWith(PrizeLocation other) => other.locationName.StartsWith(name);
}

public static class OpenWorld
{
    private static void RemoveDuplicates(ref List<PrizeItem> items)
    {
        var lastIndex = items.Count - 1;
        for (var i = lastIndex; i > 0; i--)
        {
            if (items.FindIndex(new NameComparer(items[i].prizeName).Equals) != i)
            {
                items.RemoveAt(i);
            }
        }
    }

    private static void RemoveDuplicates(ref List<PrizeLocation> locations)
    {
        var lastIndex = locations.Count - 1;
        for (var i = lastIndex; i > 0; i--)
        {
            if (locations.FindIndex(new NameComparer(locations[i].locationName).Equals) != i)
            {
                locations.RemoveAt(i);
            }
        }
    }

    private static Mutex mut = new();
    private static readonly List<PrizeItem> AllPossibleItems = [];
    private static readonly List<PrizeLocation> AllPossibleLocations = [];

    private static void GenerateAllItemsAndLocations()
    {
        lock (AllPossibleLocations)
        {
            if (AllPossibleLocations.Count != 0)
            {
                // already generated
                return;
            }

            List<PrizeItem> allItems = [];
            List<PrizeLocation> allLocations = [];
            var commonSettings = new CommonSettings();

            // Start With Sprite + Javelin + VLong + Start with Flammie Drum
            var settings = new OpenWorldSettings(commonSettings);
            settings.setBool(OpenWorldCharacterSelection.START_SOLO, true);
            settings.setBool(OpenWorldSettings.PROPERTYNAME_FLAMMIE_DRUM_IN_LOGIC, false);
            var context = new RandoContext
            {
                randomFunctional = new DotNet110Random()
            };
            context.workingData.set(OpenWorldGoalProcessor.GOAL_SHORT_NAME, OpenWorldGoalProcessor.GOAL_MANABEAST);
            context.workingData.setBool(OpenWorldCharacterSelection.BOY_EXISTS, true);
            context.workingData.setBool(OpenWorldCharacterSelection.GIRL_EXISTS, true);
            context.workingData.setBool(OpenWorldCharacterSelection.SPRITE_EXISTS, true);
            context.workingData.setBool(OpenWorldCharacterSelection.BOY_IN_LOGIC, true);
            context.workingData.setBool(OpenWorldCharacterSelection.GIRL_IN_LOGIC, true);
            context.workingData.setBool(OpenWorldCharacterSelection.SPRITE_IN_LOGIC, false);
            context.workingData.setBool(OpenWorldClassSelection.GIRL_MAGIC_EXISTS, true);
            context.workingData.setBool(OpenWorldClassSelection.SPRITE_MAGIC_EXISTS, true);
            context.workingData.setInt(StartingWeaponRandomizer.BOY_START_WEAPON_INDEX, 7);
            context.workingData.setInt(StartingWeaponRandomizer.GIRL_START_WEAPON_INDEX, 7);
            context.workingData.setInt(StartingWeaponRandomizer.SPRITE_START_WEAPON_INDEX, 7);
            var locations = OpenWorldLocations.getForSelectedOptions(settings, context);
            allLocations.AddRange(locations);
            allItems.AddRange(OpenWorldPrizes.getForSelectedOptions(settings, context, locations));

            // Boy-Only; this adds extra gold drops
            context.workingData.setBool(OpenWorldCharacterSelection.BOY_IN_LOGIC, false);
            context.workingData.setBool(OpenWorldCharacterSelection.SPRITE_IN_LOGIC, true);
            context.workingData.setBool(OpenWorldCharacterSelection.GIRL_EXISTS, false);
            context.workingData.setBool(OpenWorldCharacterSelection.SPRITE_EXISTS, false);
            context.workingData.setBool(OpenWorldClassSelection.GIRL_MAGIC_EXISTS, false);
            context.workingData.setBool(OpenWorldClassSelection.SPRITE_MAGIC_EXISTS, false);
            locations = OpenWorldLocations.getForSelectedOptions(settings, context);
            allLocations.AddRange(locations);
            allItems.AddRange(OpenWorldPrizes.getForSelectedOptions(settings, context, locations));

            // Start With Boy + Sword + MTR + Flammie Drum Logic
            settings.setBool(OpenWorldSettings.PROPERTYNAME_FLAMMIE_DRUM_IN_LOGIC, true);
            context.workingData.set(OpenWorldGoalProcessor.GOAL_SHORT_NAME, OpenWorldGoalProcessor.GOAL_MTR);
            context.workingData.setBool(OpenWorldCharacterSelection.GIRL_EXISTS, true);
            context.workingData.setBool(OpenWorldCharacterSelection.SPRITE_EXISTS, true);
            context.workingData.setBool(OpenWorldCharacterSelection.BOY_IN_LOGIC, false);
            context.workingData.setBool(OpenWorldCharacterSelection.SPRITE_IN_LOGIC, true);
            context.workingData.setBool(OpenWorldClassSelection.GIRL_MAGIC_EXISTS, true);
            context.workingData.setBool(OpenWorldClassSelection.SPRITE_MAGIC_EXISTS, true);
            context.workingData.setInt(StartingWeaponRandomizer.BOY_START_WEAPON_INDEX, 0);
            context.workingData.setInt(StartingWeaponRandomizer.GIRL_START_WEAPON_INDEX, 0);
            context.workingData.setInt(StartingWeaponRandomizer.SPRITE_START_WEAPON_INDEX, 0);
            locations = OpenWorldLocations.getForSelectedOptions(settings, context);
            allLocations.AddRange(locations);
            allItems.AddRange(OpenWorldPrizes.getForSelectedOptions(settings, context, locations));

            // TODO: XMAS gift hunt; this does not use regular "allLocations"

            // For money, we don't want the actual amount (for now), so just strip after ':'
            // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator // we don't want LINQ
            foreach (var item in allItems)
            {
                if (item.prizeName.Contains(':'))
                {
                    item.prizeName = item.prizeName.Split(':')[0];
                }
            }

            RemoveDuplicates(ref allItems);
            RemoveDuplicates(ref allLocations);

            // "starter weapon" are special items and locations that will be handled internally
            allItems.RemoveAll(new NameComparer("starter weapon").StartsWith);
            allLocations.RemoveAll(new NameComparer("starter weapon").StartsWith);

            // sort by item/location ID
            allItems.Sort((a, b) => a.itemId.CompareTo(b.itemId));
            allLocations.Sort((a, b) => a.locationId.CompareTo(b.locationId));

            AllPossibleItems.AddRange(allItems);
            AllPossibleLocations.AddRange(allLocations);
        }
    }

    public static List<PrizeItem> GetAllItemsManaged()
    {
        GenerateAllItemsAndLocations();
        return AllPossibleItems;
    }

    [UnmanagedCallersOnly(EntryPoint = "SoMR_OW_GetAllItems")]
    public static IntPtr GetAllItems()
    {
        return ItemList.Ref(GetAllItemsManaged());
    }

    public static List<PrizeLocation> GetAllLocationsManaged()
    {
        GenerateAllItemsAndLocations();
        return AllPossibleLocations;
    }

    [UnmanagedCallersOnly(EntryPoint = "SoMR_OW_GetAllLocations")]
    public static IntPtr GetAllLocations()
    {
        return LocationList.Ref(GetAllLocationsManaged());
    }

    [UnmanagedCallersOnly(EntryPoint = "SoMR_OW_NewSettings")]
    public static IntPtr NewSettings()
    {
        var commonSettings = new CommonSettings();
        commonSettings.set(CommonSettings.PROPERTYNAME_MODE, OpenWorldSettings.MODE_KEY);
        commonSettings.set(CommonSettings.PROPERTYNAME_VERSION, RomGenerator.VERSION_NUMBER);
        commonSettings.setBool(CommonSettings.PROPERTYNAME_DEBUG_LOG, true);
        return OWSettings.Ref(new OpenWorldSettings(commonSettings));
    }

    [UnmanagedCallersOnly(EntryPoint = "SoMR_OW_NewGenerator")]
    public static IntPtr NewGenerator()
    {
        return OWGenerator.Ref(new OpenWorldGenerator());
    }

    [UnmanagedCallersOnly(EntryPoint = "SoMR_OW_Init")]
    public static IntPtr Init(IntPtr srcPtr, IntPtr seedPtr, IntPtr generatorPtr, IntPtr settingsPtr)
    {
        try
        {
            var src = Str.FromUni(srcPtr);
            var seed = Str.FromUni(seedPtr);
            var generatorHandle = GCHandle.FromIntPtr(generatorPtr);
            var settingsHandle = GCHandle.FromIntPtr(settingsPtr);
            var generator = (OpenWorldGenerator)generatorHandle.Target;
            var settings = (OpenWorldSettings)settingsHandle.Target;
            if (src == null || seed == null || settings == null || generator == null)
            {
                throw new Exception("Invalid arguments to SoMR_OW_Init");
            }
            var commonSettings = settings.CommonSettings;
            var allGenerators = new Dictionary<string, RomGenerator> { { OpenWorldSettings.MODE_KEY, generator } };
            var allSettings = new Dictionary<string, RandoSettings> { { OpenWorldSettings.MODE_KEY, settings } };
                return Context.Ref(RomGenerator.Init(src, seed, allGenerators, commonSettings, allSettings));
        }
        catch (Exception e)
        {
            try
            {
                return Context.Ref(new RandoContext
                {
                    error = e.ToString(),
                });
            }
            catch
            {
                return IntPtr.Zero;
            }
        }
    }

    [UnmanagedCallersOnly(EntryPoint = "SoMR_OW_Run")]
    public static void Run(IntPtr dstPtr, IntPtr seedPtr, IntPtr generatorPtr, IntPtr settingsPtr, IntPtr contextPtr)
    {
        // TODO: move all the arguments into context?
        var contextHandle = GCHandle.FromIntPtr(contextPtr);
        var context = (RandoContext)contextHandle.Target;
        if (context?.error != null)
        {
            return; // already failed, keep error
        }
        try
        {
            var dst = Str.FromUni(dstPtr);
            var seed = Str.FromUni(seedPtr);
            var generatorHandle = GCHandle.FromIntPtr(generatorPtr);
            var settingsHandle = GCHandle.FromIntPtr(settingsPtr);
            var generator = (OpenWorldGenerator)generatorHandle.Target;
            var settings = (OpenWorldSettings)settingsHandle.Target;
            if (dst == null || seed == null || settings == null || generator == null || context == null)
            {
                throw new Exception("Invalid arguments to SoMR_OW_Run");
            }
            var commonSettings = settings.CommonSettings;
            var allGenerators = new Dictionary<string, RomGenerator> { { OpenWorldSettings.MODE_KEY, generator } };
            var allSettings = new Dictionary<string, RandoSettings> { { OpenWorldSettings.MODE_KEY, settings } };
            RomGenerator.Run(dst, seed, allGenerators, commonSettings, allSettings, context);
        }
        catch (Exception e)
        {
            context?.error = e.ToString();
        }
    }
}
