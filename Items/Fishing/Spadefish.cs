using System;
using System.Linq;
using CalamityMod.Graphics.Renderers;
using CalamityMod.Items.Weapons.Rogue;
using CalamityMod.Systems;
using CalamityMod.Utilities.Daybreak;
using CalamityMod.Utilities.Daybreak.Buffers;
using Microsoft.CodeAnalysis;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics;
using Terraria.Graphics.Renderers;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Fishing
{
    public class Spadefish : RogueWeapon, ILocalizedModType
    {

        public static float SpinsToThrow => 3;
        public new string LocalizationCategory => "Items.Fishing";
        public override void SetDefaults()
        {
            Item.width = 46;
            Item.height = 44;
            Item.damage = 15;
            Item.knockBack = 2f;
            Item.useTime = 40;
            Item.useAnimation = 40;
            Item.shootSpeed = 10;

            Item.DamageType = RogueDamageClass.Instance;
            Item.useTurn = true;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.value = CalamityGlobalItem.RarityGreenBuyPrice;
            Item.rare = ItemRarityID.Green;
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.useStyle = ItemUseStyleID.Thrust;
            Item.shoot = ModContent.ProjectileType<SpadefishThrown>();
            Item.noMelee = true;
            Item.noUseGraphic = true;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.Calamity().StealthStrikeAvailable())
            {
                type = ModContent.ProjectileType<SpadefishHoldout>();
                Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
                //This proj sets StealthStrike on its own setdefaults
                return false;
            }
            return true;
        }
    }

    public class SpadefishThrown : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Rogue";
        public override string Texture => "CalamityMod/Items/Fishing/Spadefish";
        public override void SetDefaults()
        {
            Projectile.DamageType = RogueDamageClass.Instance;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 30;
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.stopsDealingDamageAfterPenetrateHits = true;
            Projectile.timeLeft = 180;
        }

        public override void AI()
        {
            if (Projectile.ai[0] <= 1)
                Projectile.velocity.Y += 0.2f;
            if (Projectile.ai[0] == 0)
                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;
            else if (Projectile.ai[0] == 1)
                Projectile.rotation += Projectile.velocity.X * 0.15f;
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (Projectile.ai[0] == 1)
                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;
            Projectile.velocity = Vector2.Zero;
            Projectile.Center += oldVelocity * 2;
            Projectile.ai[0] = 2;
            return false;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            SoundEngine.PlaySound(SoundID.Item178, Projectile.Center);

            Projectile.velocity.X = Projectile.velocity.X.DirectionalSign() * -2;
            Projectile.velocity.Y = -3f;
            Projectile.ai[0] = 1;
            Projectile.netUpdate = true;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            var tex = TextureAssets.Projectile[Type];
            Main.EntitySpriteDraw(tex.Value, Projectile.Center - Main.screenPosition, null, lightColor, Projectile.rotation, tex.Size() * 0.5f, Projectile.scale, 0);
            return false;
        }
    }

    public class SpadefishHoldout : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Rogue";
        public override string Texture => "CalamityMod/Items/Fishing/Spadefish";

        ref float spin => ref Projectile.ai[0];
        ref float startingDir => ref Projectile.ai[1];

        float throwSpeed = 12;

        bool reset = false;

        float spinSin => MathF.Sin(spin);
        public override void SetDefaults()
        {
            Projectile.DamageType = RogueDamageClass.Instance;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            Projectile.width = 64;
            Projectile.height = 64;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.Calamity().stealthStrike = true;
            Projectile.tileCollide = false;
        }

        public override void Load()
        {
            On_LegacyPlayerRenderer.DrawPlayer += spinThePlayer;
        }

        private void spinThePlayer(On_LegacyPlayerRenderer.orig_DrawPlayer orig, LegacyPlayerRenderer self, Terraria.Graphics.Camera camera, Player drawPlayer, Vector2 position, float rotation, Vector2 rotationOrigin, float shadow, float scale)
        {
            var t = ModContent.ProjectileType<SpadefishHoldout>();
            if (drawPlayer.ownedProjectileCounts[t] <= 0)
            {
                orig(self, camera, drawPlayer, position, rotation, rotationOrigin, shadow, scale);
                return;
            }

            var Spade = Main.projectile.FirstOrDefault(p => p.active && p.owner == drawPlayer.whoAmI && p.type == t)?.ModProjectile<SpadefishHoldout>();

            if (Spade is null || Spade.startingDir == 0)
            {
                orig(self, camera, drawPlayer, position, rotation, rotationOrigin, shadow, scale);
                return;
            }

            using var lease = ScreenspaceTargetPool.Shared.Rent(Main.instance.GraphicsDevice);
            using (lease.Scope(clearColor: Color.Transparent))
            {
                orig(self, camera, drawPlayer, position, rotation, rotationOrigin, shadow, scale);

                bool flip = (Spade.spinSin * Spade.startingDir) < 0;
                var tex = TextureAssets.Projectile[Type];
                Main.EntitySpriteDraw(tex.Value, drawPlayer.Center + new Vector2(40,0) - Main.screenPosition, null, Lighting.GetColor(Spade.Projectile.Center.ToTileCoordinates()), Spade.Projectile.rotation - (flip ? MathHelper.PiOver2 : 0), tex.Size() * 0.5f, new Vector2(Spade.Projectile.scale, Spade.Projectile.scale), flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None);
            }
            float width = Spade.spinSin * Spade.startingDir * drawPlayer.direction;

            using (Main.spriteBatch.Scope())
            {
                Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Matrix.identity);
                Main.spriteBatch.Draw(lease.Target, position - Main.screenPosition, null, Color.White, 0, position - Main.screenPosition, new Vector2(MathF.Abs(width), 1), width < 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0f);
                Main.spriteBatch.End();
            }
        }

        public override void AI()
        {
            var player = Main.player[Projectile.owner];
            if (startingDir == 0)
            {
                startingDir = Projectile.velocity.X.DirectionalSign();
                throwSpeed = Projectile.velocity.Length();
                Projectile.velocity= Vector2.Zero;
                player.Calamity().ConsumeStealthByAttacking();
            }
            player.direction = 1;
            player.SetDummyItemTime(40);
            Projectile.Center = player.Center + new Vector2(48 * spinSin, 0 * MathF.Cos(spin)) * startingDir;
            Projectile.rotation = Projectile.DirectionFrom(player.Center).ToRotation() + MathHelper.PiOver4;
            if (spin == 0)
            {
                spin = MathHelper.TwoPi;
            }
            spin *= 1.0175f;

            if (spinSin < 0.25f && spinSin > -0.25f)
            {
                if (reset)
                    Projectile.ResetLocalNPCHitImmunity();
                reset = false;
            }
            else reset = true;

            if (spin > MathHelper.TwoPi * (1+Spadefish.SpinsToThrow))
            {
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), player.Center, player.Calamity().mouseRotationFromPlayer.ToRotationVector2() * throwSpeed * 1.2f, ModContent.ProjectileType<SpadefishThrown>(), Projectile.damage, Projectile.knockBack, player.whoAmI);
                Projectile.Kill();
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            return false;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            SoundEngine.PlaySound(SoundID.Item178, Projectile.Center);
        }
    }
}
