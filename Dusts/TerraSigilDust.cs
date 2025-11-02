using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;

namespace CalamityMod.Dusts
{
    public class TerraSigilDust : ModDust
    {
        public override void OnSpawn(Dust dust)
        {
            dust.noLight = true;
            dust.rotation = Main.rand.NextFloat(MathHelper.TwoPi);
            dust.fadeIn = 1f;
            // Randomly select one of the three variants
            dust.frame = new Rectangle(0, Main.rand.Next(3) * 12, 10, 12);
        }

        public override bool Update(Dust dust)
        {
            dust.velocity.Y += 0.2f;
            dust.velocity *= 0.983f;
            dust.position += dust.velocity;

            dust.alpha += 1;
            if (dust.alpha > 255)
            {
                dust.active = false;
            }
            return false;
        }
    }
}
