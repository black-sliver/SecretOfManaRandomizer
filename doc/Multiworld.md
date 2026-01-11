# SoMR Multiworld

## How does it work?

It uses [Archipelago](https://archipelago.gg) to synchronize locations and items across games.

Each Item has an ID. For each "sendable" item, we create an Event Script that can be queued.

Opposed to solo SoMR, each location has a fixed Event Flag (+Value), which can be checked by reading WRAM.
For locations that do not have a unique flag in solo SoMR, this is done by putting a "fake" item in each location, that
has the flag set to one of the unused Event Flags; we use the solo SoMR Weapon Orb flags for that.

The SoMRandomizer.processing.hacks.openworld.Multiworld applies changes to the ROM to make it work.

SoMR itself is built into a native DLL in SoMRandomizer.api that is then bundled and used by pysomr.

pysomr is a native CPython package that can be installed and used from Python; this is used by the SoM APWorld.
