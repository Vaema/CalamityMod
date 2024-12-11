using CalamityMod.Items.SummonItems;
using CalamityMod.NPCs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Boss
{
    public class HolyProfanedCore : ModProjectile, ILocalizedModType
    {
        public override LocalizedText DisplayName => CalamityUtils.GetItemName(ModContent.ItemType<ProfanedCore>());
        public override string Texture => "CalamityMod/Items/SummonItems/ProfanedCore";

        public const int Lifetime = 180;
        public ref float Timer => ref Projectile.ai[0];

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 20;
            Projectile.hostile = true;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = Lifetime;
        }

        public override void AI()
        {
            var Prov = CalamityGlobalNPC.holyBoss;
            if (Prov == -1)
            {
                Projectile.active = false;
                return;
            }

            Timer++;

            if (Timer <= 30)
            {
                Projectile.velocity.Y = -4.5f;
            }
            else if (Timer > 30 && Timer <= Lifetime)
            {
                Projectile.velocity = Vector2.Zero;
                Projectile.Center += (Main.npc[Prov].Center + new Vector2(0f, 40f) - Projectile.Center) * 0.0375f;
            }

            Projectile.scale = MathHelper.Lerp(1f, 1.5f, Timer / (float)Lifetime);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = TextureAssets.Projectile[Type].Value;
            float shakeAmt = MathHelper.Clamp(MathHelper.Lerp(0f, 5f, (Timer - 100) / 80f), 0f, 5f);
            Vector2 drawPos = Projectile.Center + Main.rand.NextVector2CircularEdge(shakeAmt, shakeAmt);

            Projectile.DrawProjectileWithBackglow(new Color(255, 255, 25), lightColor, 3.5f, xPos: drawPos.X, yPos: drawPos.Y);
            return false;
        }
    }
}
