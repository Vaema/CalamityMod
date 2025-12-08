using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace CalamityMod.NPCs.VanillaNPCAIOverrides
{
    public abstract class VanillaAIOverride : ILoadable
    {
        public NPC NPC { get; set; }
        public bool DisableMultiplayerSmoothing { get; set; }

        public void Load(Mod mod)
        {
            CalamityVanillaAIOverrideNPC.RegisterNetID(this);
        }

        public void Unload()
        {

        }

        public abstract bool AI(Mod mod);

        public virtual void SetDefaults(Mod mod) { }

        public virtual void OnSpawn(Mod mod) { }

        public virtual void PostAI(Mod mod)
        {

        }

        public virtual void SendExtraAI(BitWriter bitWriter, BinaryWriter binaryWriter)
        {

        }

        public virtual void ReceiveExtraAI(BitReader bitReader, BinaryReader binaryReader)
        {

        }

        public virtual void HitEffect(Mod mod, NPC.HitInfo hit) { }

        public virtual void FindFrame(Mod mod, int frameHeight) { }

        public virtual bool PreDraw(Mod mod, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) => true;

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
