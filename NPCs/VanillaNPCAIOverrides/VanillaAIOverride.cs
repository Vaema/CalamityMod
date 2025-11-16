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

        public virtual void SendExtraAI(BitWriter bitWriter, BinaryWriter binaryWriter)
        {

        }

        public virtual void ReceiveExtraAI(BitReader bitReader, BinaryReader binaryReader)
        {

        }
    }
}
