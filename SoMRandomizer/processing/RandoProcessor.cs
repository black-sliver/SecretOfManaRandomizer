using SoMRandomizer.config.settings;
using SoMRandomizer.logging;
using SoMRandomizer.processing.common;
using System;

namespace SoMRandomizer.processing
{
    /// <summary>
    /// Superclass for a toggle-able hack or data processor for other hacks
    /// </summary>
    /// 
    /// <remarks>Author: Moppleton</remarks>
    public abstract class RandoProcessor
    {
        public void add(byte[] origRom, byte[] outRom, String seed, RandoSettings settings, RandoContext context)
        {
            int prevOffset = context.workingOffset;
            try
            {
                bool success = process(origRom, outRom, seed, settings, context);
                if (success)
                {
                    int offsetAfter = context.workingOffset;
                    int hackSize = offsetAfter - prevOffset;
                    if (hackSize > 0)
                    {
                        Logging.log("Adding: [" + getName() + "] at 0x" + prevOffset.ToString("X6") + " -> 0x" + (offsetAfter - 1).ToString("X6") + "(0x" + hackSize.ToString("X") + ") bytes");
                    }
                    else
                    {
                        Logging.log("Adding: [" + getName() + "]");
                    }
                }
                else
                {
                    Logging.log("Skipping: [" + getName() + "]");
                }
            }
            catch (Exception e)
            {
                Logging.log("EXCEPTION in: [" + getName() + "]: " + e.Message + "\nStack trace:\n" + e.StackTrace);
                throw e; // caught by higher-level stuff to fail the rom generation
            }
        }

        /// <summary>
        /// Prepare the processor. All changes a processor does to settings and context should here.
        /// </summary>
        /// <param name="origRom">The original ROM data.</param>
        /// <param name="seed">The seed value for randomization.</param>
        /// <param name="settings">The settings.</param>
        /// <param name="context">The context.</param>
        // ReSharper disable once InconsistentNaming
        public virtual void prepare(byte[] origRom, string seed, RandoSettings settings, RandoContext context)
        {
        }

        /// <summary>
        /// Applies a processor. Settings and context should only be read here.
        /// </summary>
        /// <param name="origRom">The original ROM data.</param>
        /// <param name="outRom">The output buffer for the new ROM.</param>
        /// <param name="seed">The seed value for randomization.</param>
        /// <param name="settings">The settings.</param>
        /// <param name="context">The context.</param>
        /// <returns>True if the processor was applied, false if it was skipped.</returns>
        // ReSharper disable once InconsistentNaming
        protected abstract bool process(byte[] origRom, byte[] outRom, String seed, RandoSettings settings, RandoContext context);

        protected abstract string getName();
    }
}
