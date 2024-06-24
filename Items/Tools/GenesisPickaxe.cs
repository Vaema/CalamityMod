using CalamityMod.Items.Materials;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Graphics.Renderers;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities.Terraria.Utilities;

namespace CalamityMod.Items.Tools
{
    [LegacyName("GallantPickaxe")]
    public class GenesisPickaxe : ModItem, ILocalizedModType
    {
        private int swordDirection;
        public int time = 0;
        public float swingRotation = 0;
        public new string LocalizationCategory => "Items.Tools";
        public override void SetDefaults()
        {
            // These stats exactly match vanilla's Luminite pickaxes.
            Item.width = 84;
            Item.height = 80;
            Item.damage = 80;
            Item.knockBack = 5.5f;
            Item.useTime = 6;
            Item.useAnimation = 12;
            Item.pick = 225;
            Item.tileBoost += 4;

            Item.DamageType = DamageClass.Melee;
            Item.useTurn = true;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.value = CalamityGlobalItem.RarityRedBuyPrice;
            Item.rare = ItemRarityID.Red;
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.useTurn = true;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<MeldConstruct>(12)
                .AddIngredient(ItemID.LunarBar, 10)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }

        public override void UseAnimation(Player player)
        {
            swordDirection = (player.Center - player.Calamity().mouseWorld).X > 1 ? -1 : 1;
            time = 0;
            swingRotation = 0;

            //float Rot = (player.direction == -1 ? 5.5f : -5.5f) * Main.rand.NextFloat(0.99f, 1.1f);
            //Particle Smear = new SemiCircularSmearFade(player.Center, Vector2.Zero, Color.LightGreen * 0.9f, Rot, Main.rand.NextFloat(1.7f, 1.83f), new Vector2(1, 1), 6, true, false, true);
            //GeneralParticleHandler.SpawnParticle(Smear);
        }
        public override void MeleeEffects(Player player, Rectangle hitbox)
        {
            player.itemRotation = swingRotation - 1.7f * swordDirection;
            player.itemLocation = player.Center;
            player.direction = swordDirection;

            swingRotation = Utils.Remap(time, 0, player.itemAnimationMax, 0, 2.88f * swordDirection);

            if (Main.rand.NextBool(4))
            {
                Vector2 dustVel = new Vector2(5 * swordDirection, -5).RotatedByRandom(1.55f) * Main.rand.NextFloat(0.7f, 1.3f) * 2;
                Dust dust = Dust.NewDustPerfect(player.Center + dustVel * 9, 278);
                dust.scale = Main.rand.NextFloat(0.5f, 0.75f);
                dust.velocity = dustVel * 0.1f;
                dust.color = Color.LightGreen;
                dust.noGravity = true;
            }
            if (Main.rand.NextBool(4))
            {
                Vector2 dustVel = new Vector2(5 * swordDirection, -5).RotatedByRandom(1.55f) * Main.rand.NextFloat(0.7f, 1.3f) * 2;

                float partScale = Main.rand.NextFloat(0.6f, 1.2f);
                Vector2 partVel = dustVel * Main.rand.NextFloat(0.1f, 0.3f);
                Particle spark3 = new GlowOrbParticle(player.Center + dustVel * 7, partVel, false, 18, partScale, Color.Black, false, false, false);
                GeneralParticleHandler.SpawnParticle(spark3);
                Particle spark2 = new GlowOrbParticle(player.Center + dustVel * 7, partVel, false, 18, partScale * 0.5f, Color.LightGreen, true, false, false);
                GeneralParticleHandler.SpawnParticle(spark2);
            }

            Vector2 dustVel2 = new Vector2(5 * swordDirection, -5).RotatedBy(swingRotation - 1.7f * swordDirection);

            float partScale2 = Main.rand.NextFloat(0.5f, 0.8f);
            Vector2 partVel2 = dustVel2 * Main.rand.NextFloat(0.1f, 0.7f);

            //Particle smoke = new GlowOrbParticle(player.Center + dustVel2 * 12 + Main.rand.NextVector2Circular(12, 12), partVel2.RotatedBy(MathHelper.ToRadians(90f * swordDirection)).RotatedBy(-0.3 * swordDirection) * -5, false, 9, Main.rand.NextFloat(0.3f, 0.5f), Color.LightGreen);
            //GeneralParticleHandler.SpawnParticle(smoke);
            //Particle smoke2 = new GlowOrbParticle(player.Center + dustVel2 * 12 + Main.rand.NextVector2Circular(12, 12), partVel2.RotatedBy(MathHelper.ToRadians(90f * swordDirection)).RotatedBy(-0.3 * swordDirection) * -5, false, 9, Main.rand.NextFloat(0.3f, 0.5f), Color.Black, false, false, false);
            //GeneralParticleHandler.SpawnParticle(smoke2);

            Particle smoke = new HeavySmokeParticle(player.Center + dustVel2 * 12 + Main.rand.NextVector2Circular(8, 8), partVel2.RotatedBy(MathHelper.ToRadians(90f * swordDirection)).RotatedBy(-0.3 * swordDirection) * -5, Color.Black, 13, partScale2, 0.5f, Main.rand.NextFloat(-0.2f, 0.2f), false);
            GeneralParticleHandler.SpawnParticle(smoke);

            time++;
        }
    }
}
