using CalamityMod.Items.Materials;
using CalamityMod.Projectiles.Melee.Shortswords;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.Melee
{
    [LegacyName("ElementalShortsword", "ElementalShiv")]
    public class StarburstShiv : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Melee";

        public override void SetDefaults()
        {
            Item.width = 74;
            Item.height = 94;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.autoReuse = true;
            Item.useStyle = ItemUseStyleID.Rapier;
            Item.damage = 138;
            Item.DamageType = DamageClass.Melee;
            Item.useAnimation = Item.useTime = 13;
            Item.shoot = ModContent.ProjectileType<StarburstShivProj>();
            Item.shootSpeed = 2.4f;
            Item.knockBack = 8.5f;
            Item.UseSound = SoundID.Item1;
            Item.value = CalamityGlobalItem.RarityPurpleBuyPrice;
            Item.rare = ItemRarityID.Purple;
        }

        public override bool MeleePrefix() => true;

        public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI) => Item.DrawItemGlowmaskSingleFrame(spriteBatch, rotation, ModContent.Request<Texture2D>("CalamityMod/Items/Weapons/Melee/StarburstShivGlow").Value);

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient(ItemID.PiercingStarlight).
                AddIngredient(ItemID.LunarBar, 5).
                AddIngredient<LifeAlloy>(5).
                AddIngredient(ItemID.FragmentSolar, 5).
                AddTile(TileID.MythrilAnvil).
                Register();
        }
    }
}
