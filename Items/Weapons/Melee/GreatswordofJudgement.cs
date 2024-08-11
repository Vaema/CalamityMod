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
        private int useTime = 20;
        private int opacityAdjust = 0;
        private float smearOpacity = 0;
        private bool smearGrowth = true;
        public override void SetDefaults()
        {
            Item.width = 78;
            Item.height = 78;
            Item.damage = 155;
            Item.DamageType = DamageClass.Melee;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useAnimation = Item.useTime = useTime;
            Item.useTurn = true;
            Item.knockBack = 7f;
            Item.UseSound = SoundID.Item60;
            Item.autoReuse = true;
            Item.value = CalamityGlobalItem.RarityPurpleBuyPrice;
            Item.rare = ItemRarityID.Purple;
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
            swingRoatation += swordDirection == 1 ? 0.13f : -0.13f;

            float Rot = (swordDirection == -1 ? 1.8f : -1.8f) + swingRoatation;
            if (swordDirection != player.direction)
            {
                swingRoatation *= -1;
                swordDirection = player.direction;
            }
            Particle Smear = new SemiCircularSmearFade(player.Center, Vector2.Zero, mainColor * smearOpacity, Rot, Main.rand.NextFloat(1.95f, 2.2f), new Vector2(1, 1), 2, true, false, true, player.direction);
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
        public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
        {
            Item.DrawItemGlowmaskSingleFrame(spriteBatch, rotation, ModContent.Request<Texture2D>("CalamityMod/Items/Weapons/Melee/GreatswordofJudgementGlow").Value);
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
