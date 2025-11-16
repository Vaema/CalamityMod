using System.IO;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace CalamityMod.NPCs.VanillaNPCAIOverrides
{
    public abstract class VanillaAIOverride
    {
        public NPC NPC { get; set; }

        public abstract bool AI(Mod mod);

        public virtual void PostAI(Mod mod)
        {

        }

        public virtual void SendExtraAI(BitWriter bitWriter, BinaryWriter binaryWriter)
        {

        }

        public virtual void ReceiveExtraAI(BitReader bitReader, BinaryReader binaryReader)
        {

        }

        /// <summary>
        /// This Method should be Implemented If we added our custom field to AI Overrides
        /// </summary>
        /// <returns></returns>
        public virtual VanillaAIOverride Clone()
        {
            return (VanillaAIOverride)this.MemberwiseClone();
        }
    }
}
