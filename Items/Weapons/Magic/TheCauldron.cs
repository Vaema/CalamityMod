using CalamityMod.Projectiles.Magic;
using CalamityMod.Projectiles.Rogue;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.Magic
{
    public class TheCauldron : ModItem, ILocalizedModType
    {
        float manaReductionMult = 0.2f;
        public new string LocalizationCategory => "Items.Weapons.Magic";
        public static Asset<Texture2D> Glow;
        public override void SetStaticDefaults()
        {
            if (!Main.dedServ)
            {
                Glow = ModContent.Request<Texture2D>(Texture + "Glow");
            }
        }
        public override void SetDefaults()
        {
            Item.width = 46;
            Item.height = 46;
            Item.damage = 64;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useAnimation = 46;
            Item.useTime = 46;
            Item.knockBack = 8f;
            Item.mana = 18;
            Item.UseSound = SoundID.DD2_MonkStaffSwing;
            Item.autoReuse = true;
            Item.value = CalamityGlobalItem.RarityOrangeBuyPrice;
            Item.rare = ItemRarityID.Orange;
            Item.shoot = ModContent.ProjectileType<CauldronProj>();
            Item.shootSpeed = 12f;
            Item.DamageType = DamageClass.Magic;
            Item.Calamity().donorItem = true;
        }

        // Reduce mana cost while in lava or the underworld
        public override void ModifyManaCost(Player player, ref float reduce, ref float mult)
        {
            if (player.lavaWet || player.ZoneUnderworldHeight)
            {
                mult = manaReductionMult;
            }
        }
        public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
        {
            Item.DrawItemGlowmaskSingleFrame(spriteBatch, rotation, Glow.Value);
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.LavaBucket, 1)
                .AddIngredient(ItemID.HellstoneBar, 8)
                .AddIngredient(ItemID.Obsidian, 20)
                .AddIngredient(ItemID.AshBlock, 20)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
}
