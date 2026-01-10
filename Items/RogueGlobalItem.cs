using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Items
{
    public sealed class RogueGlobalItem : GlobalItem
    {
        public override bool InstancePerEntity => true;

        /// <summary> Tracks the stealth strike damage modifier for this item, derived from its prefix. </summary>
        internal float StealthStrikePrefixBonus = 0.0f;

        public override GlobalItem Clone(Item from, Item to)
        {
            RogueGlobalItem myClone = (RogueGlobalItem)base.Clone(from, to);

            // Rogue
            myClone.StealthStrikePrefixBonus = StealthStrikePrefixBonus;

            return myClone;
        }

        public override bool AppliesToEntity(Item entity, bool lateInstantiation)
        {
            return entity.CountsAsClass<RogueDamageClass>();
        }

        public override void PreReforge(Item item)
        {
            StealthStrikePrefixBonus = 0f;
        }
    }
}
