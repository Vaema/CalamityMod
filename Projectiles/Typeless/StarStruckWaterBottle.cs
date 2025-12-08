using CalamityMod.Dusts;
using CalamityMod.World;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Typeless
{
    public class StarStruckWaterBottle : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Typeless";
        public override string Texture => "CalamityMod/Items/Weapons/Typeless/StarStruckWater";

        public static int ConversionType;
        public override void SetStaticDefaults() => ConversionType = ModContent.GetInstance<AstralConversion>().Type;

        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.aiStyle = ProjAIStyleID.ThrownProjectile;
            Projectile.friendly = true;
            Projectile.penetrate = 1;
        }

        public override void AI()
        {
            Projectile.ai[0] += 1f;
            if (Projectile.ai[0] >= 10f)
            {
                Projectile.velocity.Y += 0.1f;
                Projectile.velocity.X *= 0.998f;
            }
        }

        public override void OnKill(int timeLeft)
        {
            if (Projectile.owner == Main.myPlayer)
            {
                SoundEngine.PlaySound(SoundID.Shatter, Projectile.position);
                for (int index = 0; index < 5; ++index)
                    Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Glass, 0f, 0f, 0, new Color(), 1f);
                for (int index1 = 0; index1 < 30; ++index1)
                {
                    int index2 = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, ModContent.DustType<AstralBlue>(), 0f, -2f, 0, new Color(), 1.1f);
                    Dust dust = Main.dust[index2];
                    dust.alpha = 100;
                    dust.velocity.X *= 1.5f;
                    dust.velocity *= 3f;
                }

                if (Main.myPlayer == Projectile.owner)
                {
                    Point tileCenter = Projectile.Center.ToTileCoordinates();
                    WorldGen.Convert(tileCenter.X, tileCenter.Y, ConversionType);
                }
            }
        }

        public override bool? CanCutTiles() => false;
    }
}
