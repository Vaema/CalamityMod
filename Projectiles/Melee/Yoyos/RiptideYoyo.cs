using System.IO;
using CalamityMod.Items.Weapons.Melee;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Melee.Yoyos
{
    public class RiptideYoyo : ModProjectile
    {
        public override LocalizedText DisplayName => CalamityUtils.GetItemName<Riptide>();

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.YoyosLifeTimeMultiplier[Type] = Riptide.Duration;
            ProjectileID.Sets.YoyosMaximumRange[Type] = Riptide.Reach;
            ProjectileID.Sets.YoyosTopSpeed[Type] = Riptide.Speed;
        }

        public override void SetDefaults()
        {
            Projectile.aiStyle = ProjAIStyleID.Yoyo;
            Projectile.width = Projectile.height = 22;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.MeleeNoSpeed;
            Projectile.penetrate = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(Projectile.localAI[1]);
            writer.Write(Projectile.localAI[2]);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            Projectile.localAI[1] = reader.ReadSingle();
            Projectile.localAI[2] = reader.ReadSingle();
        }

        public override void AI()
        {
            if ((Projectile.position - Main.player[Projectile.owner].position).Length() > 3200f) // 200 blocks
                Projectile.Kill();

            Projectile.localAI[2]++;
            if (Projectile.localAI[2] % 10 == 0)
            {
                Projectile proj = Projectile.NewProjectileDirect(new EntitySource_ItemUse(Main.player[Projectile.owner], Main.player[Projectile.owner].HeldItem),
                    Projectile.Center,
                    Vector2.Zero,
                    ModContent.ProjectileType<RiptideWave>(),
                    Projectile.damage,
                    0f,
                    Projectile.owner,
                    Projectile.whoAmI,
                    Projectile.localAI[1]);
                (proj.ModProjectile as RiptideWave).RotationDirection = Projectile.localAI[2] % 20 == 0 ? -1 : 1;
            }
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            Projectile.localAI[1] = MathHelper.Lerp(Projectile.localAI[1], 2f, 0.09f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Asset<Texture2D> glowTexture = ModContent.Request<Texture2D>("CalamityMod/Particles/BloomCircle");

            Main.EntitySpriteDraw(glowTexture.Value, Projectile.Center - Main.screenPosition, glowTexture.Frame(), new Color(0.05f, 0.1f, 0.25f, 0f), 0f, glowTexture.Frame().Size() / 2, 0.3f * Projectile.localAI[1], SpriteEffects.None);

            return true;
        }
    }
}
