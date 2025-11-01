using System;
using CalamityMod.Buffs.Summon;
using CalamityMod.CalPlayer;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Projectiles.Summon
{
    public class VengefulSunSpiritMinion : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Summon";
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.MinionSacrificable[Type] = true;
            ProjectileID.Sets.MinionTargettingFeature[Type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 50;
            Projectile.height = 50;
            Projectile.netImportant = true;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.minionSlots = 1f;
            Projectile.timeLeft = 18000;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.timeLeft *= 5;
            Projectile.minion = true;
            Projectile.DamageType = DamageClass.Summon;
        }

        public int MinionSlotsToAdd
        {
            get { return (int)Projectile.ai[0]; }
            set { Projectile.ai[0] = value; }
        }
        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            CalamityPlayer modPlayer = player.Calamity();
            player.AddBuff(ModContent.BuffType<SolarGodSpiritBuff>(), 3600);

            #region Add Minion Slots
            if (MinionSlotsToAdd > 0)
            {
                float minionSlotsAvaliable = player.maxMinions;
                foreach (var item in Main.ActiveProjectiles)
                {
                    if (item.owner == Projectile.owner)
                        minionSlotsAvaliable -= item.minionSlots;
                }
                while (minionSlotsAvaliable >= 1 && MinionSlotsToAdd > 0)
                {

                    Projectile.minionSlots++;
                    minionSlotsAvaliable--;
                    MinionSlotsToAdd--;
                }
                MinionSlotsToAdd = 0;
            }
            #endregion

            #region Checking alive
            bool correctMinion = Projectile.type == ModContent.ProjectileType<VengefulSunSpiritMinion>();
            if (correctMinion)
            {
                if (player.dead)
                {
                    modPlayer.vengefulSunMinion = false;
                }
                if (modPlayer.vengefulSunMinion)
                {
                    Projectile.timeLeft = 2;
                }
            }
            #endregion

            #region Positioning
            Projectile.Center = player.Center + Vector2.UnitY * (player.gfxOffY + player.gravDir * -16f);
            #endregion

            Lighting.AddLight(Projectile.Center, (255 - Projectile.alpha) * 0.25f / 255f, (255 - Projectile.alpha) * 0.25f / 255f, (255 - Projectile.alpha) * 0f / 255f);

            NPC target = null;
            int targetID = -1;
            Projectile.Minion_FindTargetInRange(1600, ref targetID, false);
            if (targetID < 0)
                return;

            target = Main.npc[targetID];


            if (Projectile.owner == Main.myPlayer)
            {
                if (Projectile.ai[1] > 0f)
                {
                    Projectile.ai[1] -= 1f;
                    return;
                }
                float shootSpeed = 15f;
                Vector2 source = Projectile.Center;
                var velocity = CalamityUtils.CalculatePredictiveAimToTargetMaxUpdates(Projectile.Center, target, shootSpeed, 2);
                Projectile beam = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center - velocity, velocity, ModContent.ProjectileType<VengefulSunBeam>(), (int)(Projectile.damage * (0.75f + Projectile.minionSlots * 0.25f)), Projectile.knockBack, Projectile.owner);
                beam.DamageType = DamageClass.Summon;
                Projectile.ai[1] += 60f / (0.75f + Projectile.minionSlots * 0.25f);
            }
        }

        public override bool? CanDamage() => false;

        public static Asset<Texture2D> circle;
        public override bool PreDraw(ref Color lightColor)
        {
            var spTex = TextureAssets.Projectile[Type].Value;
            var ciTex = CalamityUtils.GetTextureEfficient(ref circle, "CalamityMod/ExtraTextures/GreyscaleOpenCircleButBigger").Value;

            Main.spriteBatch.Draw(spTex,Projectile.Center-Main.screenPosition,null,Color.White,Main.GlobalTimeWrappedHourly,spTex.Size()*0.5f, 0.75f,SpriteEffects.None,0);
            Main.spriteBatch.SafeBegin(SpriteSortMode.Deferred, BatchSetting.Additive, null, Main.GameViewMatrix.TransformationMatrix, () =>
            {
                Main.EntitySpriteDraw(ciTex, Projectile.Center - Main.screenPosition, null, Color.Gold, Main.GlobalTimeWrappedHourly, ciTex.Size() * 0.5f, 0.075f, SpriteEffects.None);

                float count = MathHelper.Min(Projectile.minionSlots * 2, 20);
                for (var i = 0; i < count; i++)
                {
                    var comp = (i / count);
                    var offset = ((Main.mouseTextColor - 190) / 64f) * 8;
                    if (i % 2 == 0)
                        offset = 8 - offset;
                    CalamityUtils.DrawLineBetter(Main.spriteBatch, Projectile.Center + new Vector2(26 + offset, 0).RotatedBy(MathHelper.TwoPi * comp - Main.GlobalTimeWrappedHourly), Projectile.Center + new Vector2(40 + offset, 0).RotatedBy(MathHelper.TwoPi * comp - Main.GlobalTimeWrappedHourly),  i % 2 == 0 ? Color.OrangeRed : Color.Gold, 2f);
                }
            });
            return false;
        }
    }
}
