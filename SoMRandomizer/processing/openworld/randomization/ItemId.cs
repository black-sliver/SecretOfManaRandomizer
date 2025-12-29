namespace SoMRandomizer.processing.openworld.randomization
{
    /// <summary>
    /// Unique IDs of items for use with multiworld. They are roughly in order of event flags.
    /// Do not change or reorder. Only append or fill holes.
    /// </summary>
    public enum ItemId : byte
    {
        /// <summary>
        /// This matches AP's builtin "Nothing"
        /// </summary>
        Nothing = 0,
        /// <summary>Pseudo Item</summary>
        StarterWeaponMain = 1,
        /// <summary>Pseudo Item</summary>
        StarterWeaponAlt = 2,
        /// <summary>Reserved, unused</summary>
        StarterWeaponAlt2 = 3,
        GloveOrb = 4, // match event id
        SwordOrb = 5,
        AxeOrb = 6,
        SpearOrb = 7,
        WhipOrb = 8,
        BowOrb = 9,
        BoomerangOrb = 10,
        JavelinOrb = 11,
        Boy = 12, // match event id
        Girl = 13,
        Sprite = 14,
        SeaHareTail = 15,
        GoldTowerKey = 16,
        MidgeMallet = 17,
        MoogleBelt = 18,
        FlammieDrum = 19,
        // MagicRope = 20,
        WaterSeed = 21,
        EarthSeed = 22,
        WindSeed = 23,
        FireSeed = 24,
        LightSeed = 25,
        DarkSeed = 26,
        MoonSeed = 27,
        DryadSeed = 28,
        UndineSpells = 29,
        GnomeSpells = 30,
        SylphidSpells = 31,
        SalamandoSpells = 32,
        LuminaSpells = 33,
        ShadeSpells = 34,
        LunaSpells = 35,
        DryadSpells = 36,
        Glove = 37,
        Sword = 38,
        Axe = 39,
        Spear = 40,
        Whip = 41,
        Bow = 42,
        Boomerang = 43,
        Javelin = 44,
        Gp0 = 50,
        Gp1 = 51,
        Gp2 = 52,
        Gp3 = 53,
        Gp4 = 54,
        Gp5 = 55,
        Gp6 = 56,
        Gp7 = 57,
        Gp8 = 58,
        Gp9 = 59,
        Gp10 = 60,
        Gp11 = 61,
        Gp12 = 62,
        Gp13 = 63,
        Gp14 = 64,
        Gp15 = 65,
        Gp16 = 66,
        Last = Gp16,
    }
}
