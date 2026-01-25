using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SoMRandomizer.config.settings;
using SoMRandomizer.logging;
using SoMRandomizer.processing.common;
using SoMRandomizer.processing.openworld;
using SoMRandomizer.processing.openworld.events;
using SoMRandomizer.processing.openworld.randomization;
using SoMRandomizer.util;

namespace SoMRandomizer.processing.hacks.openworld
{
    /// <summary>
    /// Add stuff required to make multiworld work. 
    /// </summary>
    /// 
    /// <remarks>Author: black-sliver</remarks>
    public class MultiWorld : RandoProcessor
    {
        /// <summary>
        /// Context's workData can have {prefix}{locationId} to show a message when checking a location.
        /// </summary>
        private const string RewardMessageKeyPrefix = "mwRewardMessage"; // append location number

        /// <summary>
        /// Flags used for Locations that don't have their own flags in solo.
        /// </summary>
        private static readonly Dictionary<LocationId, byte> LocationsWithoutFlags = new Dictionary<LocationId, byte>
        {
            // TODO: move this to some data struct that more completely describes locations
            { LocationId.ShadePalaceGloveOrbChest, EventFlags.OPENWORLD_ORB_FLAGS[0] },
            { LocationId.SunkenContinentSwordOrbChest, EventFlags.OPENWORLD_ORB_FLAGS[1] },
            { LocationId.LuminaTowerAxeOrbChest, EventFlags.OPENWORLD_ORB_FLAGS[2] },
            { LocationId.FirePalaceAxeOrbChest, EventFlags.OPENWORLD_ORB_FLAGS[3] },
            { LocationId.LuminaTowerSpearOrbChest, EventFlags.OPENWORLD_ORB_FLAGS[4] },
            { LocationId.SunkenContinentBoomerangOrbChest, EventFlags.OPENWORLD_ORB_FLAGS[5] },
            { LocationId.FirePalaceChest1, EventFlags.OPENWORLD_ORB_FLAGS[6] },
            { LocationId.FirePalaceChest2, EventFlags.OPENWORLD_ORB_FLAGS[7] },
            { LocationId.ShadePalaceChest, EventFlags.OPENWORLD_ORB_FLAGS[8] },
            { LocationId.MoogleVillageGloveOrbChest, EventFlags.OPENWORLD_ORB_FLAGS[9] },
            { LocationId.IceCastleGloveOrbChest, EventFlags.OPENWORLD_ORB_FLAGS[10] },
            { LocationId.PandoraSwordOrbChest, EventFlags.OPENWORLD_ORB_FLAGS[11] },
            { LocationId.NtRuinsSwordOrbChest, EventFlags.OPENWORLD_ORB_FLAGS[12] },
            { LocationId.MoogleVillageAxeOrbChest, EventFlags.OPENWORLD_ORB_FLAGS[13] },
            { LocationId.NtCastleAxeOrbChest, EventFlags.OPENWORLD_ORB_FLAGS[14] },
            { LocationId.NtRuinsSpearOrbChest, EventFlags.OPENWORLD_ORB_FLAGS[15] },
            { LocationId.PandoraSpearOrbChest, EventFlags.OPENWORLD_ORB_FLAGS[16] },
            { LocationId.SantaSpearOrbChest, EventFlags.OPENWORLD_ORB_FLAGS[17] },
            { LocationId.KilroyWhipOrbChest, EventFlags.OPENWORLD_ORB_FLAGS[18] },
            { LocationId.NtCastleWhipOrbChest, EventFlags.OPENWORLD_ORB_FLAGS[19] },
            { LocationId.NtRuinsBowOrbChest, EventFlags.OPENWORLD_ORB_FLAGS[20] },
            { LocationId.PotosChest, EventFlags.OPENWORLD_ORB_FLAGS[21] },
            { LocationId.PandoraChest1, EventFlags.OPENWORLD_ORB_FLAGS[22] },
            { LocationId.PandoraChest2, EventFlags.OPENWORLD_ORB_FLAGS[23] },
            { LocationId.PandoraChest3, EventFlags.OPENWORLD_ORB_FLAGS[24] },
            { LocationId.PandoraChest4, EventFlags.OPENWORLD_ORB_FLAGS[25] },
            { LocationId.MagicRopeChest, EventFlags.OPENWORLD_ORB_FLAGS[26] },
            { LocationId.NtCastleChest, EventFlags.OPENWORLD_ORB_FLAGS[27] },
            { LocationId.MatangoInnJavelinOrbChest, EventFlags.OPENWORLD_ORB_FLAGS[28] },
            { LocationId.TurtleIsland, EventFlags.OPENWORLD_ORB_FLAGS[29] },
            { LocationId.DwarfElder, EventFlags.OPENWORLD_ORB_FLAGS[30] },
            { LocationId.Girl, EventFlags.OPENWORLD_ORB_FLAGS[31] },
            { LocationId.Lighthouse, EventFlags.OPENWORLD_ORB_FLAGS[32] },
            { LocationId.SwordPedestal, EventFlags.OPENWORLD_ORB_FLAGS[33] },
        };

        private readonly OpenWorldRandomizer world;

        public MultiWorld(OpenWorldRandomizer openWorldRandomizer)
        {
            world = openWorldRandomizer;
        }

        private enum RamAddr
        {
            ReceiveIndexLo = CustomRamOffsets.MULTIWORLD_RECEIVE_INDEX_LO, // 0x7ecf88
            ReceiveIndexHi = CustomRamOffsets.MULTIWORLD_RECEIVE_INDEX_HI, // 0x7ecf8a
            ReceiveItem = CustomRamOffsets.MULTIWORLD_RECEIVE_ITEM, // 0x7e0441..2
            ReceiveStart = CustomRamOffsets.MULTIWORLD_RECEIVE_START, // 0x7e0443..4
            MapState = 0x7e005c, // 0x80 bit set while in boss encounter
            EventState = 0x7e00d0,
            EventPtr = 0x7e00d1,
        }

        private enum RomAddr
        {
            ApSection = 0x3d0040, // $fd0040
            ApSeed = ApSection + 0, // $fd0040
            ApConnectName = ApSection + 32, // $fd0060
        }

        protected override string getName()
        {
            return "MultiWorld";
        }

        public override void prepare(byte[] origRom, string seed, RandoSettings settings, RandoContext context)
        {
            context.initialValues[CustomRamOffsets.MULTIWORLD_RECEIVE_ITEM] = 0;
            context.initialValues[CustomRamOffsets.MULTIWORLD_RECEIVE_ITEM + 1] = 0;
            context.initialValues[CustomRamOffsets.MULTIWORLD_RECEIVE_START] = 0;
            context.initialValues[CustomRamOffsets.MULTIWORLD_RECEIVE_START + 1] = 0; // this byte stays 0 forever
        }

        protected override bool process(byte[] origRom, byte[] outRom, string seed, RandoSettings settings,
            RandoContext context)
        {
            if (!settings.getBool(OpenWorldSettings.PROPERTYNAME_MULTIWORLD))
            {
                return false;
            }

            CreateRemoteItems(outRom, settings, context);
            var placements = FillFakeItems(context);
            var result = new OpenWorldSimulator.SimResult(); // TODO: get this from AP somehow
            world.WriteResult(outRom, placements, result,  settings, context);
            // remove empty textboxes
            foreach (var script in context.replacementEvents.Values)
            {
                // wait (0x28) 0 (0x00) - clear (0x52) - wait (0x28) 0 (0x00) -> wait (0x28) 0 (0x00)
                for (var i = script.Count - 5; i >= 0; i--)
                {
                    if (script[i + 0] == 0x28 && script[i + 1] == 0x00 && script[i + 2] == 0x52 &&
                        script[i + 3] == 0x28 && script[i + 4] == 0x00)
                    {
                        script.RemoveRange(i + 2, 3);
                        i -= 2;
                    }
                }

                // open (0x50) - wait (0x28) 0 (0x00) - close (0x51) -> nothing
                for (var i = script.Count - 4; i >= 0; i--)
                {
                    if (script[i + 0] == 0x50 && script[i + 1] == 0x28 && script[i + 2] == 0x00 &&
                        script[i + 3] == 0x51)
                    {
                        script.RemoveRange(i, 4);
                        i -= 3;
                    }
                }

                // same as above, but with 2 waits - this may not exist
                for (var i = script.Count - 6; i >= 0; i--)
                {
                    if (script[i + 0] == 0x50 && script[i + 1] == 0x28 && script[i + 2] == 0x00 &&
                        script[i + 3] == 0x28 && script[i + 4] == 0x00 && script[i + 5] == 0x51)
                    {
                        script.RemoveRange(i, 6);
                        i -= 5;
                    }
                }
            }
            // apply AP seed name and AP slot connect name from settings:
            // apSeed: (up to) 32B seed, from hex; apConnectName (up to) 32B connectName, from hex
            var apSeedHex = settings.get("apSeed");
            if (apSeedHex.Length < 2 || apSeedHex.Length > 64)
            {
                throw new Exception("Invalid apSeed");
            }
            for (var i = 0; i < apSeedHex.Length; i += 2)
            {
                outRom[(int)RomAddr.ApSeed + i / 2] = Convert.ToByte(apSeedHex.Substring(i, 2), 16);
            }

            var apConnectNameHex = settings.get("apConnectName");
            if (apConnectNameHex.Length < 2 || apConnectNameHex.Length > 64)
            {
                throw new Exception("Invalid apConnectName");
            }
            for (var i = 0; i < apConnectNameHex.Length; i += 2)
            {
                outRom[(int)RomAddr.ApConnectName + i / 2] = Convert.ToByte(apConnectNameHex.Substring(i, 2), 16);
            }

            return true;
        }

        /// <summary>
        /// Gets the first instance of an item by id, or null.
        /// </summary>
        /// <param name="itemId">The ItemID to look for.</param>
        /// <returns>The first item matching the itemId, or null if none was found.</returns>
        private PrizeItem GetItemById(ItemId itemId)
        {
            return world.allPrizes.FirstOrDefault(item => item.itemId == itemId);
        }

        /// <summary>
        /// Get the item that should be locked at a location id, or null if the location should not be locked.
        /// </summary>
        /// <param name="locationId">The location id of the location to check.</param>
        /// <returns>An item that should be locked at the location.</returns>
        private PrizeItem GetLockedItem(LocationId locationId)
        {
            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
            switch (locationId)
            {
                case LocationId.StarterWeaponMain:
                    return GetItemById(ItemId.StarterWeaponMain);
                case LocationId.StarterWeaponAlt:
                    return GetItemById(ItemId.StarterWeaponAlt);
                default:
                    return null;
            }
        }

        /// <summary>
        /// Replace open world prizes with fake items that have no effect.
        /// For locations that have no flag, the fake items provide a flag.
        /// And then fill locations.
        /// </summary>
        private Dictionary<PrizeLocation, PrizeItem> FillFakeItems(RandoContext context)
        {
            var placements = new Dictionary<PrizeLocation, PrizeItem>();
            foreach (var location in world.allLocations)
            {
                var item = GetLockedItem(location.locationId);
                if (item == null)
                {
                    // create an empty item that may check a flag depending on which location it goes into
                    if (!LocationsWithoutFlags.TryGetValue(location.locationId, out var flag))
                    {
                        flag = 0;
                    }
                    
                    var message = context.workingData.get(RewardMessageKeyPrefix + (int)location.locationId);

                    byte[] eventData;
                    if (flag == 0 && string.IsNullOrEmpty(message))
                    {
                        eventData = new byte[] { };
                    }
                    else if (flag == 0)
                    {
                        eventData = MakeBareMessage(message).ToArray();
                    }
                    else
                    {
                        eventData = new byte[]
                        {
                            EventCommandEnum.EVENT_LOGIC.Value,
                            flag,
                            0x1F, // 1..15
                            (byte)(EventCommandEnum.JUMP_BASE.Value + 0x03),
                            0x96, // event 396 is our "already got it" event
                            EventCommandEnum.INCREMENT_FLAG.Value,
                            flag,
                        };
                        if (string.IsNullOrEmpty(message))
                        {
                            // if it's a chest: replace event without textbox
                            // other events will be updated later
                            var ev = context.replacementEvents[location.eventNum];
                            ev.Clear();
                            PrizeEvents.injectReplacementPattern(ev, 0, false);
                            ev.Add(EventCommandEnum.REFRESH_OBJECTS.Value);
                            ev.Add(EventCommandEnum.END.Value);
                        }
                        else
                        {
                            eventData = eventData.Concat(MakeBareMessage(message)).ToArray();
                        }
                    }

                    item = new PrizeItem(ItemId.Nothing, "remote", "", eventData, "", flag, 0);
                }

                placements[location] = item;
                Logging.log(getName() + ": placed " + item.prizeName + " at " + location.locationName, "debug");
            }

            return placements;
        }

        /// <summary>
        /// Creates code to receive remote items from a remote server.
        /// </summary>
        /// <param name="outRom">The game data to patch.</param>
        /// <param name="settings">The current generation's settings dict.</param>
        /// <param name="context">The current generation's context.</param>
        private void CreateRemoteItems(byte[] outRom, RandoSettings settings, RandoContext context)
        {
            var workingOffset = context.workingOffset;

            // create event scripts for all receivable items.
            // NOTE: they don't need an event number, so we just put it wherever
            var eventAddresses = new Dictionary<ItemId, int>
            {
                {
                    ItemId.Nothing, AppendBlock(outRom, ref workingOffset,
                        MakeTextOnlyEventData("Nothing"))
                },
                {
                    ItemId.SeaHareTail, AppendBlock(outRom, ref workingOffset,
                        MakeFlagEventData(EventFlags.SEA_HARES_TAIL_FLAG, "the Sea Hare Tail"))
                },
                {
                    ItemId.GoldTowerKey, AppendBlock(outRom, ref workingOffset,
                        MakeFlagEventData(EventFlags.LUMINA_TOWER_PROGRESS_FLAG, "the Gold Key"))
                },
                {
                    ItemId.MidgeMallet, AppendBlock(outRom, ref workingOffset,
                        MakeInventoryEventData(EventFlags.MIDGE_MALLET_FLAG, 0x49, "the Midge Mallet"))
                },
                {
                    ItemId.MoogleBelt, AppendBlock(outRom, ref workingOffset,
                        MakeInventoryEventData(EventFlags.OPENWORLD_MOOGLE_BELT_FLAG, 0x48, "the Moogle Belt"))
                },
            };

            if (settings.getBool(OpenWorldSettings.PROPERTYNAME_FLAMMIE_DRUM_IN_LOGIC))
            {
                // NOTE: if flammie drum is an item, Gp0 does not exist in the game
                var flammieDrumFlag = EventFlags.OPENWORLD_GOLD_FLAGS[0];
                eventAddresses[ItemId.FlammieDrum] = AppendBlock(outRom, ref workingOffset,
                    MakeInventoryEventData(flammieDrumFlag, 0x47, "the Flammie Drum"));
            }

            for (byte i = 0; i < 8; i++)
            {
                eventAddresses[IdMap.WeaponOrbs[i]] = AppendBlock(outRom, ref workingOffset, MakeOrbEventData(i));
            }

            for (byte i = 0; i < 8; i++)
            {
                // don't add remote weapon items that characters start with
                if ((context.workingData.getBool(OpenWorldCharacterSelection.BOY_EXISTS) &&
                     i == context.workingData.getInt(StartingWeaponRandomizer.BOY_START_WEAPON_INDEX)) ||
                    (context.workingData.getBool(OpenWorldCharacterSelection.GIRL_EXISTS) &&
                     i == context.workingData.getInt(StartingWeaponRandomizer.GIRL_START_WEAPON_INDEX)) ||
                    (context.workingData.getBool(OpenWorldCharacterSelection.SPRITE_EXISTS) &&
                     i == context.workingData.getInt(StartingWeaponRandomizer.SPRITE_START_WEAPON_INDEX)))
                {
                    Logging.log("Multiworld: Skipping starter weapon " + i, "debug");
                    continue;
                }
                eventAddresses[IdMap.Weapons[i]] = AppendBlock(outRom, ref workingOffset, MakeWeaponEventData(i));
            }

            for (byte i = 0; i < 8; i++)
            {
                eventAddresses[IdMap.Seeds[i]] = AppendBlock(outRom, ref workingOffset, MakeSeedEventData(i));
            }

            if (context.workingData.getBool(OpenWorldCharacterSelection.BOY_EXISTS))
            {
                var weaponId = (byte)context.workingData.getInt(StartingWeaponRandomizer.BOY_START_WEAPON_INDEX);
                eventAddresses[ItemId.Boy] = AppendBlock(outRom, ref workingOffset, MakeCharEventData(0, weaponId));
            }

            if (context.workingData.getBool(OpenWorldCharacterSelection.GIRL_EXISTS))
            {
                var weaponId = (byte)context.workingData.getInt(StartingWeaponRandomizer.GIRL_START_WEAPON_INDEX);
                eventAddresses[ItemId.Girl] = AppendBlock(outRom, ref workingOffset, MakeCharEventData(1, weaponId));
            }

            if (context.workingData.getBool(OpenWorldCharacterSelection.SPRITE_EXISTS))
            {
                var weaponId = (byte)context.workingData.getInt(StartingWeaponRandomizer.SPRITE_START_WEAPON_INDEX);
                eventAddresses[ItemId.Sprite] = AppendBlock(outRom, ref workingOffset, MakeCharEventData(2, weaponId));
            }

            var roleSpellFlags = new Dictionary<string, byte>
            {
                { "OGboy", 0x00 },
                { "OGgirl", 0x38 },
                { "OGsprite", 0x07 },
            };
            var boySpells = roleSpellFlags[context.workingData.get(OpenWorldClassSelection.BOY_CLASS)];
            var girlSpells = roleSpellFlags[context.workingData.get(OpenWorldClassSelection.GIRL_CLASS)];
            var spriteSpells = roleSpellFlags[context.workingData.get(OpenWorldClassSelection.SPRITE_CLASS)];
            var girlSpellsExist = context.workingData.getBool(OpenWorldClassSelection.GIRL_MAGIC_EXISTS);
            var spriteSpellsExist = context.workingData.getBool(OpenWorldClassSelection.SPRITE_MAGIC_EXISTS);
            var giftMode = settings.get(OpenWorldGoalProcessor.GOAL_SHORT_NAME)
                           == OpenWorldGoalProcessor.GOAL_GIFTMODE;

            if (girlSpellsExist || spriteSpellsExist || giftMode)
            {
                eventAddresses[ItemId.UndineSpells] = AppendBlock(outRom, ref workingOffset, MakeSpellEventData(
                    EventFlags.ELEMENT_UNDINE_FLAG, 0xc9, "Undine",
                    boySpells, girlSpells, spriteSpells));
                eventAddresses[ItemId.GnomeSpells] = AppendBlock(outRom, ref workingOffset, MakeSpellEventData(
                    EventFlags.ELEMENT_GNOME_FLAG, 0xc8, "Gnome",
                    boySpells, girlSpells, spriteSpells));
                eventAddresses[ItemId.SylphidSpells] = AppendBlock(outRom, ref workingOffset, MakeSpellEventData(
                    EventFlags.ELEMENT_SYLPHID_FLAG, 0xcb, "Sylphid",
                    boySpells, girlSpells, spriteSpells));
                eventAddresses[ItemId.SalamandoSpells] = AppendBlock(outRom, ref workingOffset, MakeSpellEventData(
                    EventFlags.ELEMENT_SALAMANDO_FLAG, 0xca, "Salamando",
                    boySpells, girlSpells, spriteSpells));
                eventAddresses[ItemId.LunaSpells] = AppendBlock(outRom, ref workingOffset, MakeSpellEventData(
                    EventFlags.ELEMENT_LUNA_FLAG, 0xcc, "Luna",
                    boySpells, girlSpells, spriteSpells));
                eventAddresses[ItemId.DryadSpells] = AppendBlock(outRom, ref workingOffset, MakeSpellEventData(
                    EventFlags.ELEMENT_DRYAD_FLAG, 0xcd, "Dryad",
                    boySpells, girlSpells, spriteSpells));
            }

            if (spriteSpellsExist || giftMode)
            {
                eventAddresses[ItemId.ShadeSpells] = AppendBlock(outRom, ref workingOffset, MakeSpellEventData(
                    EventFlags.ELEMENT_SHADE_FLAG, 0xce, "Shade",
                    boySpells, girlSpells, spriteSpells));
            }

            if (girlSpellsExist || giftMode)
            {
                eventAddresses[ItemId.LuminaSpells] = AppendBlock(outRom, ref workingOffset, MakeSpellEventData(
                    EventFlags.ELEMENT_LUMINA_FLAG, 0xcf, "Lumina",
                    boySpells, girlSpells, spriteSpells));
            }

            foreach (var prize in world.allPrizes)
            {
                if (!prize.prizeName.StartsWith("GP ")) continue;
                var parts = prize.prizeName.Substring(3).Split(':');
                if (parts.Length < 2 || !int.TryParse(parts[0], out var i) || !int.TryParse(parts[1], out var amount))
                {
                    Logging.log("Multiworld: malformed GP prize: " + prize.prizeName);
                    continue;
                }

                var itemId = (ItemId)((int)ItemId.Gp0 + i);
                if (itemId < ItemId.Gp0 || itemId > ItemId.Gp16)
                {
                    Logging.log("Multiworld: invalid GP itemId for " + prize.prizeName);
                    continue;
                }

                eventAddresses[itemId] = AppendBlock(outRom, ref workingOffset, MakeGpEventData(amount));
            }

            // for each gold drop that did not make it in, create a 0 GP drop so we get a message
            var zeroGoldDrop = 0;
            for (var itemId = ItemId.Gp0; itemId <= ItemId.Gp16; itemId++)
            {
                if (eventAddresses.ContainsKey(itemId)) continue;
                if (zeroGoldDrop == 0)
                {
                    zeroGoldDrop = AppendBlock(outRom, ref workingOffset, MakeGpEventData(0));
                }
                eventAddresses[itemId] = zeroGoldDrop;
            }

            // special event that does nothing
            var noopAddress = AppendBlock(outRom, ref workingOffset, new byte[] { });

            // write table to rom
            const int eventCount = (int)ItemId.Last + 1;
            const int lenOfScript = 90;
            // NOTE: table and code needs to be in the same bank if we switch to short addressing in the future
            CodeGenerationUtils.ensureSpaceInBank(ref workingOffset, 3 * eventCount + lenOfScript);
            var itemTableStart = workingOffset;
            Logging.log("Multiworld: writing item event table at 0x" + itemTableStart.ToString("X6"));
            for (ItemId i = 0; i <= ItemId.Last; i++)
            {
                if (!eventAddresses.TryGetValue(i, out var romAddr))
                {
                    romAddr = noopAddress;
                }
                AppendRomAddrAsBusAddr(outRom, ref workingOffset, romAddr);
            }

            // create an NMI hook that can queue events created above
            // NOTE: SoMR does not differentiate between data and code "half-banks",
            //       so for now we set data bank register to 7e and use long addr for ROM
            // TODO: ensure that the stuff is on an address where we can address both RAM and ROM
            var nmiHook = new byte[]
            {
                // the original NMI handler had rep #$30, pha, phx, phy, phd, phb
                // we need some of this, so do it all and JML to 00c0af at the end
                0xc2, 0x30, // REP #30 (A, X, Y = 16bit)
                0x48, // PHA
                0xda, // PHX
                0x5a, // PHY
                0x0b, // PHD
                0x8b, // PHB
                0xe2, 0x20, // SEP #20 (A = 8bit)
                // if ($7e00d0 (event busy) != 0) return
                0xaf, Lo(RamAddr.EventState), Hi(RamAddr.EventState), Ex(RamAddr.EventState), // LDA (long) $7e00d0 (busy)
                0xf0, 0x06, // BEQ (if 0: jump over return, otherwise: early return)
                0xc2, 0x30, // REP #30 (A, X, Y = 16bit)
                0x5c, 0xaf, 0xc0, 0x00, // JML (long) $00c0af (return)
                // if ($7e005c < 0) return (in boss encounter)
                0xaf, Lo(RamAddr.MapState), Hi(RamAddr.MapState), Ex(RamAddr.MapState), // LDA (long) $7e005c (map state)
                0x10, 0x06, // BPL (if >=0: jump over return, otherwise: early return)
                0xc2, 0x30, // REP #30 (A, X, Y = 16bit)
                0x5c, 0xaf, 0xc0, 0x00, // JML (long) $00c0af (return)
                // if ($7eff02 (item queued) == 0) return
                0xaf, Lo(RamAddr.ReceiveStart), Hi(RamAddr.ReceiveStart), Ex(RamAddr.ReceiveStart), // LDA (long) $7eff02 (start)
                0xd0, 0x06, // BNE (if 0: early return, otherwise: jump over return)
                0xc2, 0x30, // REP #30 (A, X, Y = 16bit)
                0x5c, 0xaf, 0xc0, 0x00, // JML (long) $00c0af (return)
                // data bank = 7e (takes 9 cycles, so doing this after the fast returns)
                0xa9, 0x7e, // LDA #$7e
                0x48, // PHA
                0xab, // PLB
                // load 16bit $7eff00 (item id)
                0xc2, 0x30, // REP #30 (A, X, Y = 16bit)
                0xad, Lo(RamAddr.ReceiveItem), Hi(RamAddr.ReceiveItem), // LDA $7eff00 (item id)
                // if ($7eff00 (item id) > last) return
                0xc9, eventCount, 0x00, // CMP (Last + 1)
                0x10, 0xee, // BPL -18 to return above (branch/return if invalid)
                // item id *= 3 (offset into table)
                0x0a, // ASL A (* =2)
                0x6d, Lo(RamAddr.ReceiveItem), Hi(RamAddr.ReceiveItem), // ADC $7eff00 (+= item id)
                0xaa, // TAX
                0xbf, Lo(itemTableStart), Hi(itemTableStart), Ex(itemTableStart, 0xc0),
                // ^ LDA (long) 16bit item table + X
                0x8d, Lo(RamAddr.EventPtr), Hi(RamAddr.EventPtr), // STA 16bit $7e00d1
                0xe2, 0x20, // SEP #20 (A = 8bit)
                // increment item counter/next receive index low byte
                // because of how this is saved, it's not a 16bit integer
                0xee, Lo(RamAddr.ReceiveIndexLo), Hi(RamAddr.ReceiveIndexLo), // INC $7ecf88
                0xd0, 3, // BNE 3 (skip incrementing high byte if not wrapped around)
                // increment item counter/next receive index high byte
                0xee, Lo(RamAddr.ReceiveIndexHi), Hi(RamAddr.ReceiveIndexHi), // INC $7ecf8a
                0xbf, Lo(itemTableStart + 2), Hi(itemTableStart + 2), Ex(itemTableStart + 2, 0xc0),
                // ^ LDA (long) 8bit item table + 2 + X
                0x8d, Lo(RamAddr.EventPtr + 2), Hi(RamAddr.EventPtr + 2), // STA 8bit $7e00d3
                0xa9, 0x01, // LDA 8bit #$01
                0x8d, Lo(RamAddr.EventState), Hi(RamAddr.EventState), // STA 8bit $7e00d0 (1 = start script)
                0x9c, Lo(RamAddr.ReceiveStart), Hi(RamAddr.ReceiveStart), // STZ 8bit $7eff02 (0 = clear item queued flag)
                // done
                0xc2, 0x30, // REP #30 (A, X, Y = 16bit)
                0x5c, 0xaf, 0xc0, 0x00, // JML (long) 00c0af
            };

            if (lenOfScript != nmiHook.Length)
            {
                var msg = "WARNING: Wrong precalculated size: Expected " + nmiHook.Length + ", got " + lenOfScript;
                Logging.log(msg);
                Debug.Assert(false, msg);
            }

            var nmiHookAddr = workingOffset;
            Logging.log("Multiworld: writing nmi hook at 0x" + nmiHookAddr.ToString("X6"));
            AppendBlock(outRom, ref workingOffset, nmiHook);

            if (itemTableStart >> 16 != (workingOffset - 1) >> 16)
            {
                throw new Exception("Bug: NMI hook crosses banks");
            }

            nmiHookAddr |= 0xc00000;
            // NOTE: we do not insert the NMI into the vector table directly, but jump away from the OG code and back
            outRom[0xc0a8] = 0x5c; // JML
            outRom[0xc0a9] = (byte)((nmiHookAddr >>  0) & 0xff);
            outRom[0xc0aa] = (byte)((nmiHookAddr >>  8) & 0xff);
            outRom[0xc0ab] = (byte)((nmiHookAddr >> 16) & 0xff);
            outRom[0xc0ac] = 0xea; // NOP
            outRom[0xc0ad] = 0xea; // NOP
            outRom[0xc0ae] = 0xea; // NOP

            // done.
            context.workingOffset = workingOffset;
        }

        // TODO: move some helpers to a separate file?

        private static byte Lo(int addr)
        {
            return (byte)(addr & 0xff);
        }

        private static byte Lo(RamAddr addr)
        {
            return Lo((int)addr);
        }

        private static byte Hi(int addr)
        {
            return (byte)((addr >> 8) & 0xff);
        }

        private static byte Hi(RamAddr addr)
        {
            return Hi((int)addr);
        }

        private static byte Ex(int addr, byte start = 0)
        {
            return (byte)(start | ((addr >> 16) & 0xff));
        }

        private static byte Ex(RamAddr addr, byte start = 0)
        {
            return Ex((int)addr, start);
        }

        /// <summary>
        /// Append a script to the rom without crossing banks, increment workingOffset and return actual address.
        /// This overload appends an END (0x00) to the end.
        /// </summary>
        /// <param name="outRom">The byte buffer to write to.</param>
        /// <param name="workingOffset">Offset to free space, will be increment during appending.</param>
        /// <param name="data">The data to write to the output, without End. End will be appended automatically.</param>
        /// <returns>The actual offset the data was written to.</returns>
        private static int AppendBlock(byte[] outRom, ref int workingOffset, IEnumerable<byte> data)
        {
            return AppendBlock(outRom, ref workingOffset, data, true);
        }

        /// <summary>
        /// Append a script to the rom without crossing banks, increment workingOffset and return actual address.
        /// </summary>
        /// <param name="outRom">The byte buffer to write to.</param>
        /// <param name="workingOffset">Offset to free space, will be increment during appending.</param>
        /// <param name="data">The data to write to the output, without End. End will be appended automatically.</param>
        /// <param name="addEnd">If true, appends an END (0x00) command to the end.</param>
        /// <returns>The actual offset the data was written to.</returns>
        private static int AppendBlock(byte[] outRom, ref int workingOffset, IEnumerable<byte> data, bool addEnd)
        {
            var arr = data as byte[] ?? data.ToArray();
            CodeGenerationUtils.ensureSpaceInBank(ref workingOffset, arr.Count() + (addEnd ? 1 : 0));
            var res = workingOffset;
            foreach (var b in arr)
            {
                outRom[workingOffset++] = b;
            }

            if (addEnd)
            {
                outRom[workingOffset++] = EventCommandEnum.END.Value;
            }
            return res;
        }

        private static void AppendRomAddrAsBusAddr(byte[] outRom, ref int workingOffset, int addr)
        {
            addr |= 0xc00000;
            outRom[workingOffset++] = (byte)((addr >>  0) & 0xff);
            outRom[workingOffset++] = (byte)((addr >>  8) & 0xff);
            outRom[workingOffset++] = (byte)((addr >> 16) & 0xff);
        }

        private static List<byte> MakeBareMessage(string text)
        {
            return VanillaEventUtil.getBytes(VanillaEventUtil.wordWrapText(text));
        }

        private static IEnumerable<byte> MakeReceivedMessage(string itemName)
        {
            var res = MakeBareMessage("Received " + itemName);
            res.Add(EventCommandEnum.SLEEP_FOR.Value); // wait for button press
            res.Add(0x07); // maybe
            res.Add(EventCommandEnum.CLOSE_DIALOGUE.Value);
            return res;
        }

        private static IEnumerable<byte> MakeTextOnlyEventData(string text)
        {
            return new[]
            {
                EventCommandEnum.OPEN_DIALOGUE.Value,
            }.Concat(MakeReceivedMessage(text));
        }

        private static IEnumerable<byte> MakeWeaponEventData(byte weaponId)
        {
            Debug.Assert(weaponId < 8);
            var flag1 = (byte)(0xc8 + weaponId);
            var flag2 = (byte)(0xc0 + weaponId);
            var inventory = (byte)(0x80 + weaponId * 9);
            var name = "the " + SomVanillaValues.weaponByteToName(weaponId);
            return new byte[]
            {
                EventCommandEnum.OPEN_DIALOGUE.Value,
                EventCommandEnum.EVENT_LOGIC.Value,
                flag1,
                0x1F, // 1..15
                (byte)(EventCommandEnum.JUMP_BASE.Value + 0x03),
                0x96, // event 396 is our "already got it" event
                EventCommandEnum.ADD_INVENTORY_ITEM.Value,
                inventory,
                EventCommandEnum.INCREMENT_FLAG.Value,
                flag2, // sword at L1
                EventCommandEnum.INCREMENT_FLAG.Value,
                flag1, // sword at L1
                EventCommandEnum.NAME_CHARACTER.Value,
                0x08, // used for "update weapons" here
            }.Concat(MakeReceivedMessage(name));
        }

        private static IEnumerable<byte> MakeOrbEventData(byte weaponId)
        {
            Debug.Assert(weaponId < 8);
            var flag1 = (byte)(0xb8 + weaponId);
            var flag2 = (byte)(0xc0 + weaponId);
            var weaponName = SomVanillaValues.weaponByteToName(weaponId);
            var name = (weaponName[0] == 'A') ? ("an " + weaponName + " orb") : ("a " + weaponName + " orb");
            return new byte[]
            {
                EventCommandEnum.OPEN_DIALOGUE.Value,
                EventCommandEnum.EVENT_LOGIC.Value,
                flag1,
                0x08, // 0..8
                EventCommandEnum.INCREMENT_FLAG.Value,
                flag1,
                EventCommandEnum.EVENT_LOGIC.Value,
                flag2,
                0x08, // 0..8
                EventCommandEnum.INCREMENT_FLAG.Value,
                flag2,
            }.Concat(MakeReceivedMessage(name));
        }

        private static IEnumerable<byte> MakeCharEventData(byte charId, byte weaponId)
        {
            Debug.Assert(charId < 3);
            Debug.Assert(weaponId < 8);
            var characterFlag = (byte)(0x0c + charId);
            var weaponFlag1 = (byte)(0xc8 + weaponId);
            // var weaponFlag2 = (byte)(0xc0 + weaponId); // FIXME: read below
            var charType = IdMap.ItemNames[IdMap.Characters[charId]];
            var indicator = "(" + charType.ToLowerInvariant() + ")";
            var name = "the " + charType + ", " + indicator;
            return new byte[]
            {
                EventCommandEnum.OPEN_DIALOGUE.Value,
                EventCommandEnum.EVENT_LOGIC.Value,
                characterFlag,
                0x1f, // 1..15
                (byte)(EventCommandEnum.JUMP_BASE.Value + 0x03),
                0x96, // event 396 is our "already got it" event
                // doing name character with dialogue box open breaks it a bit; close first
                EventCommandEnum.CLOSE_DIALOGUE.Value,
                EventCommandEnum.NAME_CHARACTER.Value,
                charId,
                EventCommandEnum.INCREMENT_FLAG.Value,
                characterFlag,
                // if we have not cheated the weapon, give it now
                EventCommandEnum.EVENT_LOGIC.Value,
                weaponFlag1,
                0x00, // 0..0
                EventCommandEnum.INCREMENT_FLAG.Value,
                weaponFlag1,
                // FIXME: don't we need to set weaponFlag2 as well?
                EventCommandEnum.ADD_CHARACTER.Value,
                charId,
                EventCommandEnum.NAME_CHARACTER.Value,
                0x08, // update weapons
                // TODO: HEAL ?
                // TODO: Don't we also need to unlock spells or something?
                //       Or is this done for all characters during sword pull?
                EventCommandEnum.OPEN_DIALOGUE.Value,
           }.Concat(MakeReceivedMessage(name));
        }

        private static IEnumerable<byte> MakeFlagEventData(byte flag, string text)
        {
            return new byte[]
            {
                EventCommandEnum.OPEN_DIALOGUE.Value,
                EventCommandEnum.EVENT_LOGIC.Value,
                flag,
                0x1F, // 1..15
                (byte)(EventCommandEnum.JUMP_BASE.Value + 0x03),
                0x96, // event 396 is our "already got it" event
                EventCommandEnum.SET_FLAG.Value,
                flag,
                0x01, // unlocked
            }.Concat(MakeReceivedMessage(text));
        }

        private static IEnumerable<byte> MakeInventoryEventData(byte flag, byte inventory, string text)
        {
            return new byte[]
            {
                EventCommandEnum.OPEN_DIALOGUE.Value,
                EventCommandEnum.EVENT_LOGIC.Value,
                flag,
                0x1F, // 1..15
                (byte)(EventCommandEnum.JUMP_BASE.Value + 0x03),
                0x96, // event 396 is our "already got it" event
                EventCommandEnum.SET_FLAG.Value,
                flag,
                0x01, // unlocked
                EventCommandEnum.ADD_INVENTORY_ITEM.Value,
                inventory,
            }.Concat(MakeReceivedMessage(text));
        }

        private static IEnumerable<byte> MakeSeedEventData(byte elementId)
        {
            var flag = (byte)(0x90 + elementId);
            var name = "the " + IdMap.ItemNames[IdMap.Seeds[elementId]];
            return new byte[]
            {
                EventCommandEnum.OPEN_DIALOGUE.Value,
                EventCommandEnum.EVENT_LOGIC.Value,
                flag,
                0x1F, // 1..15
                (byte)(EventCommandEnum.JUMP_BASE.Value + 0x03),
                0x96, // event 396 is our "already got it" event
                EventCommandEnum.SET_FLAG.Value,
                flag,
                0x01, // unlocked
                // increment total mana power
                (byte)(EventCommandEnum.JUMP_SUBR_BASE.Value + 0x05),
                0x94,
            }.Concat(MakeReceivedMessage(name));
        }

        private static IEnumerable<byte> MakeSpellEventData(byte flag, byte attr, string elementName,
            byte boySpells, byte girlSpells, byte spriteSpells)
        {
            // mask off mana magic, which is unlocked later
            // mask off shade spells and lumina spells to skip setting the attr
            const byte dryadAttr = 0xcd;
            const byte shadeAttr = 0xce;
            const byte luminaAttr = 0xcf;
            var spellMask = new Dictionary<byte, byte>
            {
                {dryadAttr, 0x1b}, // no mana magic
                {shadeAttr, 0x07}, // sprite-only
                {luminaAttr, 0x38}, // girl-only
            };
            if (spellMask.TryGetValue(attr, out var mask))
            {
                boySpells &= mask;
                girlSpells &= mask;
                spriteSpells &= mask;
            }

            var name = elementName + " magic";
            var boyUnlock = (boySpells == 0)
                ? new byte[] { }
                : new byte[]
                {
                    EventCommandEnum.SET_CHARACTER_ATTRIBUTE.Value,
                    0x01, // boy
                    attr,
                    boySpells,
                };
            var girlUnlock = (girlSpells == 0)
                ? new byte[] { }
                : new byte[]
                {
                    EventCommandEnum.SET_CHARACTER_ATTRIBUTE.Value,
                    0x02, // girl
                    attr,
                    girlSpells,
                };
            var spriteUnlock = (spriteSpells == 0)
                ? new byte[] { }
                : new byte[]
                {
                    EventCommandEnum.SET_CHARACTER_ATTRIBUTE.Value,
                    0x03, // sprite
                    attr,
                    spriteSpells,
                };
            return new byte[]
            {
                EventCommandEnum.OPEN_DIALOGUE.Value,
                EventCommandEnum.EVENT_LOGIC.Value,
                flag,
                0x1F, // 1..15
                (byte)(EventCommandEnum.JUMP_BASE.Value + 0x03),
                0x96, // event 396 is our "already got it" event
            }.Concat(boyUnlock).Concat(girlUnlock).Concat(spriteUnlock).Concat(new byte[]
            {
                EventCommandEnum.SET_FLAG.Value,
                flag,
                0x01, // unlocked
            }).Concat(MakeReceivedMessage(name));
        }

        private static IEnumerable<byte> MakeGpEventData(int amount)
        {
            var name = amount + " gold";
            return new byte[]
            {
                EventCommandEnum.OPEN_DIALOGUE.Value,
                EventCommandEnum.ADD_GOLD.Value,
                (byte)((amount >> 0) & 0xff),
                (byte)((amount >> 8) & 0xff),
            }.Concat(MakeReceivedMessage(name));
        }
    }
}
