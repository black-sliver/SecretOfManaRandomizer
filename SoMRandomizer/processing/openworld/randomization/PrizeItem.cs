using System.Threading;

namespace SoMRandomizer.processing.openworld.randomization
{
    /// <summary>
    /// An open world "check" prize.
    /// </summary>
    /// 
    /// <remarks>Author: Moppleton</remarks>
    public class PrizeItem
    {
        // this is so i can stick them in dictionaries and not rely on them all to have unique names, because they do not
        private static int PRIZE_UID = 0;
        /// <summary>
        /// Unique identifier of the effect of finding the prize. Multiple copies of the same may exist in a game.
        /// </summary>
        public readonly ItemId itemId;
        // name of the prize
        public string prizeName;
        // i think this is actually not used currently and can probably be removed
        public string prizeType;
        // event data to inject (including dialogue) to give prize
        public byte[] eventData;
        // anything else needed here?
        public string hintName;
        // event flag to flip to 1 when we got the prize
        public byte gotItemEventFlag;
        public double value; // higher = more important
        private readonly int uid;

        public PrizeItem(ItemId id, string name, string type, byte[] data, string hint, byte eventFlag, double prizeValue)
        {
            itemId = id;
            prizeName = name;
            prizeType = type;
            eventData = data;
            hintName = hint;
            gotItemEventFlag = eventFlag;
            value = prizeValue;
            uid = Interlocked.Increment(ref PRIZE_UID);
        }

        public PrizeItem(ItemId id, string type, byte[] data, string hint, byte eventFlag, double prizeValue)
        : this(id, IdMap.ItemNames[id], type, data, hint, eventFlag, prizeValue)
        {
        }

        public override bool Equals(object obj)
        {
            return obj is PrizeItem && uid == ((PrizeItem)obj).uid;
        }

        public override int GetHashCode()
        {
            return uid;
        }

    }
}
