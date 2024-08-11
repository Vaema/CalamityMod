using CalamityMod.Items.Materials;
using CalamityMod.Projectiles.Melee;
using CalamityMod.Rarities;
using CalamityMod.Tiles.Furniture.CraftingStations;
using Terraria.Audio;
using Terraria.ID;
using Microsoft.Xna.Framework;
using Terraria.ModLoader;
using CalamityMod.Particles;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Basic.Reference.Assemblies;
using System;
using Mono.Cecil;

namespace CalamityMod.Items.Weapons.Melee
{
    [LegacyName("GreatswordofBlah")]
    public class Orderbringer : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Melee";
        private float swingRoatation = 0;
        private int time = 0;
        private Color mainColor;
        private int swordDirection;
        private int useTime = 18;
        private int opacityAdjust = 0;
        private float smearOpacity = 0;
        private bool smearGrowth = true;
        public override void SetDefaults()
        {
            Item.width = Item.height = 108;
            Item.damage = 400;
            Item.DamageType = DamageClass.Melee;
            Item.useAnimation = Item.useTime = useTime;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTurn = true;
            Item.knockBack = 8f;
            Item.UseSound = SoundID.Item60;
            Item.autoReuse = true;
            Item.value = CalamityGlobalItem.RarityDarkBlueBuyPrice;
            Item.rare = ModContent.RarityType<DarkBlue>();
            Item.shoot = ModContent.ProjectileType<OrderbringerWaveProj>();
            Item.shootSpeed = 5.5f;
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Projectile.NewProjectile(source, position, velocity, type, (int)(damage * 0.5f), knockback, player.whoAmI, 0f);

            for (int i = 0; i < 4; i++)
            {
                Vector2 vel = (velocity * 2.5f).RotatedByRandom(0.6f);
                Projectile.NewProjectile(source, Main.MouseWorld - (velocity * 40) + Main.rand.NextVector2Circular(130, 130), vel * Main.rand.NextFloat(0.8f, 1.2f), ModContent.ProjectileType<StarofJudgement>(), (int)(damage * 0.25f), knockback * 0.2f, player.whoAmI, 0, 0, 1);
            }
            return false;
        }
        public override void UseItemHitbox(Player player, ref Rectangle hitbox, ref bool noHitbox)
        {
            float scale = 1.2f;
            Vector2 newSize = new Point(hitbox.Width, hitbox.Height).ToVector2() * scale;
            hitbox = new Rectangle(hitbox.X - (int)((newSize.X - hitbox.Width) / 2f), hitbox.Y - (int)((newSize.Y - hitbox.Height) / 2f), (int)newSize.X, (int)newSize.Y);
        }
        public override void UseAnimation(Player player)
        {
            swordDirection = player.direction;
            time = 0;
            swingRoatation = 0;
            mainColor = Main.rand.NextBool() ? Color.MediumPurple : Color.MediumOrchid;
            opacityAdjust = 0;
            smearOpacity = 0;
        }
        public override void MeleeEffects(Player player, Rectangle hitbox)
        {
            opacityAdjust++;
            if (opacityAdjust >= 5 && opacityAdjust <= 15 && smearOpacity < 0.9f)
                smearOpacity += 0.1f;
            else if (smearOpacity > 0)
                smearOpacity -= 0.2f;

            time++;
            swingRoatation += swordDirection == 1 ? 0.17f : -0.17f;

            float Rot = (swordDirection == -1 ? 1.8f : -1.8f) + swingRoatation;
            if (swordDirection != player.direction)
            {
                swingRoatation *= -1;
                swordDirection = player.direction;
            }
            Particle Smear = new SemiCircularSmearFade(player.Center, Vector2.Zero, mainColor * smearOpacity, Rot, Main.rand.NextFloat(2.8f, 3.2f), new Vector2(1, 1), 2, true, false, true, player.direction);
            GeneralParticleHandler.SpawnParticle(Smear);

            if (Main.rand.NextBool())
            {
                Vector2 dustVel = new Vector2(5 * swordDirection, -5).RotatedByRandom(1.55f) * Main.rand.NextFloat(0.7f, 1.3f) * 2;
                Dust dust = Dust.NewDustPerfect(player.Center + dustVel * 9, 278);
                dust.scale = Main.rand.NextFloat(0.5f, 0.75f);
                dust.velocity = dustVel * 0.55f;
                dust.color = Main.rand.NextBool() ? Color.MediumPurple : Color.MediumOrchid;
                dust.noGravity = true;
            }
        }
        public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone)
        {
            int beamDamage = player.CalcIntDamage<MeleeDamageClass>(Item.damage * 1.5f);
            for (int i = 0; i < 2; i++)
            {
                Vector2 targetPos = Main.MouseWorld;
                NPC target2 = Main.MouseWorld.ClosestNPCAt(650);
                if (target2 != null)
                    targetPos = target2.Center + target2.velocity * 14;

                Vector2 spawnPos = Main.MouseWorld + new Vector2(Main.rand.NextFloat(-300, 300), -900);
                Vector2 vel = (targetPos - spawnPos).SafeNormalize(Vector2.UnitY) * 10;
                Projectile.NewProjectile(player.GetSource_ItemUse(Item), spawnPos, vel, ModContent.ProjectileType<OrderbringerBeam>(), beamDamage, 0, player.whoAmI);
            }
        }
        public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
        {
            Item.DrawItemGlowmaskSingleFrame(spriteBatch, rotation, ModContent.Request<Texture2D>("CalamityMod/Items/Weapons/Melee/GreatswordofJudgementGlow").Value);
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<GreatswordofJudgement>().
                AddIngredient<CosmiliteBar>(8).
                AddIngredient<EndothermicEnergy>(20).
                AddTile<CosmicAnvil>().
                Register();
        }
    }
}
