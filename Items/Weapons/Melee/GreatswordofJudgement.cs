using CalamityMod.Dusts;
using CalamityMod.Particles;
using CalamityMod.Projectiles.Melee;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Audio;

namespace CalamityMod.Items.Weapons.Melee
{
    public class GreatswordofJudgement : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Melee";
        private float swingRoatation = 0;
        private int time = 0;
        private Color mainColor;
        private int swordDirection;
        public override void SetDefaults()
        {
            Item.width = 78;
            Item.height = 78;
            Item.damage = 40;
            Item.DamageType = DamageClass.Melee;
            Item.useAnimation = 25;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTime = 25;
            Item.useTurn = true;
            Item.knockBack = 7f;
            Item.UseSound = new SoundStyle("CalamityMod/Sounds/Item/TerratomereSwing") with { Volume = 0.3f, Pitch = 0.5f };
            Item.autoReuse = true;
            Item.value = CalamityGlobalItem.Rarity10BuyPrice;
            Item.rare = ItemRarityID.Red;
            Item.shoot = ModContent.ProjectileType<JudgementProj>();
            Item.shootSpeed = 5f;
        }
        public override void UseItemHitbox(Player player, ref Rectangle hitbox, ref bool noHitbox)
        {
            float scale = 1.1f;
            Vector2 newSize = new Point(hitbox.Width, hitbox.Height).ToVector2() * scale;
            hitbox = new Rectangle(hitbox.X - (int)((newSize.X - hitbox.Width) / 2f), hitbox.Y - (int)((newSize.Y - hitbox.Height) / 2f), (int)newSize.X, (int)newSize.Y);
        }
        public override void UseAnimation(Player player)
        {
            swordDirection = player.direction;
            time = 0;
            swingRoatation = 0;
            mainColor = Main.rand.NextBool() ? Color.MediumPurple : Color.MediumOrchid;
        }
        public override void MeleeEffects(Player player, Rectangle hitbox)
        {
            time++;
            swingRoatation += swordDirection == 1 ? 0.13f : -0.13f;

            float Rot = (swordDirection == -1 ? 1.8f : -1.8f) + swingRoatation;
            if (swordDirection != player.direction)
            {
                swingRoatation *= -1;
                swordDirection = player.direction;
            }
            Particle Smear = new SemiCircularSmearFade(player.Center, Vector2.Zero, mainColor * 0.8f, Rot, Main.rand.NextFloat(1.75f, 1.8f), new Vector2(1, 1), 2, true, false, true, player.direction);
            GeneralParticleHandler.SpawnParticle(Smear);

            if (Main.rand.NextBool(3))
            {
                Vector2 dustVel = new Vector2(5 * swordDirection, -5).RotatedByRandom(1.55f) * Main.rand.NextFloat(0.7f, 1.3f);
                Dust dust = Dust.NewDustPerfect(player.Center + dustVel * 9, 278);
                dust.scale = Main.rand.NextFloat(0.4f, 0.55f);
                dust.velocity = dustVel * 0.55f;
                dust.color = Main.rand.NextBool() ? Color.MediumPurple : Color.MediumOrchid;
                dust.noGravity = true;
            }
        }
        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient(ItemID.LunarBar, 7).
                AddTile(TileID.LunarCraftingStation).
                Register();
        }
    }
}
