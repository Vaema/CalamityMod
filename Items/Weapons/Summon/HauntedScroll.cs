using CalamityMod.Buffs.Summon;
using CalamityMod.Projectiles.Summon;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.Summon;

public class HauntedScroll : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Weapons.Summon";
    public override void SetStaticDefaults()
    {
        ItemID.Sets.AnimatesAsSoul[Type] = true;
        Main.RegisterItemAnimation(Type, new DrawAnimationVertical(6, 6));
    }

    public override void SetDefaults()
    {
        Item.width = 36;
        Item.height = 48;
        Item.damage = 25;
        Item.mana = 10;
        Item.useAnimation = Item.useTime = 36;
        Item.useStyle = ItemUseStyleID.HoldUp;
        Item.noMelee = true;
        Item.knockBack = 3f;
        Item.value = CalamityGlobalItem.RarityLightRedBuyPrice;
        Item.rare = ItemRarityID.LightRed;
        Item.UseSound = SoundID.Item60;
        Item.autoReuse = true;
        Item.buffType = ModContent.BuffType<HauntedDishesBuff>();
        Item.shoot = ModContent.ProjectileType<HauntedDishes>();
        Item.DamageType = DamageClass.Summon;
    }

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        player.AddBuff(Item.buffType, 2);
        var minion = Projectile.NewProjectileDirect(source, player.ClampedMouseWorld(), Vector2.Zero, type, damage, knockback, player.whoAmI, 0f, 30f);
        minion.originalDamage = Item.damage;
        //projectile.ai[1] is attack cooldown.  Setting it here prevents immediate attacks
        return false;
    }

    public override void AddRecipes()
    {
        CreateRecipe().
            AddRecipeGroup("Wood", 10).
            AddIngredient(ItemID.SoulofLight, 5).
            AddIngredient(ItemID.SoulofNight, 5).
            AddIngredient(ItemID.FoodPlatter, 3).
            AddTile(TileID.Anvils).
            Register();
    }
}
