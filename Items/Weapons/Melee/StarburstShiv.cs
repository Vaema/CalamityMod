using CalamityMod.Cooldowns;
using CalamityMod.Items.Materials;
using CalamityMod.Projectiles.BaseProjectiles;
using CalamityMod.Projectiles.DraedonsArsenal;
using CalamityMod.Projectiles.Melee;
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

        public static int MaxEnergy = 100;

        public override void SetDefaults()
        {
            Item.width = 74;
            Item.height = 94;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.damage = 183;
            Item.DamageType = DamageClass.Melee;
            Item.useAnimation = Item.useTime = 20;
            Item.shootSpeed = 10f;
            Item.knockBack = 9f;
            Item.value = CalamityGlobalItem.RarityPurpleBuyPrice;
            Item.rare = ItemRarityID.Purple;
            Item.shoot = ModContent.ProjectileType<StarburstShivHoldout>();


            Item.noMelee = true;
            Item.noUseGraphic = true;

            Item.autoReuse = true;
            Item.channel = true;
            base.SetDefaults();
        }

        public override bool AltFunctionUse(Player player)
        {
            return true;
        }


        public override void HoldItem(Player player)
        {
            if (player.Calamity().cooldowns.TryGetValue(ElementalMastery.ID, out var cooldown))
            {
                cooldown.timeLeft = player.Calamity().starburstShivElementalMastery;
            }
            else
            {
                player.AddCooldown(ElementalMastery.ID, 0);
            }
        }
        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient(ItemID.PiercingStarlight).
                AddIngredient<Lucrecia>().
                AddIngredient(ItemID.LunarBar, 5).
                AddIngredient<LifeAlloy>(5).
                AddIngredient(ItemID.FragmentSolar, 5).
                AddTile(TileID.MythrilAnvil).
                Register();
        }
    }
}
