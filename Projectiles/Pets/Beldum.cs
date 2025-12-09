using System;
using CalamityMod.Buffs.Pets;
using CalamityMod.CalPlayer;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Pets
{
    public class Beldum : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Pets";
        public override void SetStaticDefaults()
        {
            Main.projPet[Type] = true;

            ProjectileID.Sets.CharacterPreviewAnimations[Type] = ProjectileID.Sets.SimpleLoop(0, 0, 1)
            .WithOffset(-8f, -20f).WithSpriteDirection(-1).WhenNotSelected(0, 0);
        }

        public override void SetDefaults()
        {
            Projectile.netImportant = true;
            Projectile.width = 32;
            Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft *= 5;
            Projectile.tileCollide = false;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            Vector2 perfcenter = Projectile.Center;
            Vector2 vectorperf = player.Center - perfcenter;
            float playerdistance = vectorperf.Length();
            if (!player.active)
            {
                Projectile.active = false;
                return;
            }

            //Delete the projectile if the player doesnt have the buff or is very far away (dunno if this needs to be deleted)
            if (!player.HasBuff(ModContent.BuffType<BeldumBuff>()) || playerdistance >= 4000f)
            {
                Projectile.Kill();
            }

            CalamityPlayer modPlayer = player.Calamity();
            if (player.dead)
            {
                modPlayer.beldum = false;
            }
            if (modPlayer.beldum)
            {
                Projectile.timeLeft = 2;
            }

            Projectile.FloatingPetAI(false, 0);

            Projectile.ai[0]++;
            Projectile.rotation = MathF.Sin(Projectile.ai[0] * 0.05f) * MathHelper.ToRadians(20);
            Projectile.spriteDirection = -Projectile.velocity.X.DirectionalSign();
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = TextureAssets.Projectile[Type].Value;
            Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition + Vector2.UnitY * MathF.Cos(Projectile.ai[0] * 0.05f) * 10, null, Projectile.GetAlpha(lightColor), Projectile.rotation, tex.Size() / 2, Projectile.scale, Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None);
            return false;
        }
    }
}
