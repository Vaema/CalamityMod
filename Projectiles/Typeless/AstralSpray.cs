using CalamityMod.World;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Typeless
{
    public class AstralSpray : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Typeless";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public ref float Time => ref Projectile.ai[0];
        public bool ShotFromTerraformer => Projectile.ai[1] == 1f;

        public static int ConversionType;
        public override void SetStaticDefaults() => ConversionType = ModContent.GetInstance<AstralConversion>().Type;

        public override void SetDefaults()
        {
            Projectile.DefaultToSpray();
            Projectile.aiStyle = 0;
        }

        public override bool? CanDamage() => false;

        public override void AI()
        {
            if (Projectile.timeLeft > 133)
                Projectile.timeLeft = 133;

            if (Main.myPlayer == Projectile.owner)
            {
                int size = ShotFromTerraformer ? 3 : 2;
                Point tileCenter = Projectile.Center.ToTileCoordinates();
                WorldGen.Convert(tileCenter.X, tileCenter.Y, ConversionType, size);
            }

            float dustStart = ShotFromTerraformer ? 3f : 7f;
            if (Time > dustStart)
            {
                float dustScale = Utils.Remap(Time, dustStart + 1f, dustStart + 5f, 0.2f, 1f);
                int dustArea = 0;
                if (ShotFromTerraformer)
                {
                    dustScale *= 1.2f;
                    dustArea = (int)(12f * dustScale);
                }
                
                Dust spray = Dust.NewDustDirect(Projectile.position - Vector2.One * dustArea, Projectile.width + dustArea * 2, Projectile.height + dustArea * 2, DustID.Ice_Purple, Projectile.velocity.X * 0.4f, Projectile.velocity.Y * 0.4f, 100);
                spray.noGravity = true;
                spray.scale *= 1.75f * dustScale;
            }

            Time++;
            Projectile.rotation += 0.3f * Projectile.direction;
        }
    }
}
