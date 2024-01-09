using CalamityMod.Buffs.Pets;
using CalamityMod.Projectiles.Pets;
using CalamityMod.Rarities;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Pets
{
    public class StarSwallowerUnit : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Pets";
        public override void SetDefaults()
        {
            Item.width = 42;
            Item.height = 30;
            Item.damage = 0;
            Item.useAnimation = 20;
            Item.useTime = 20;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.noMelee = true;

            Item.value = Item.sellPrice(gold: 1);
            Item.rare = ModContent.RarityType<DarkOrange>();

            Item.shoot = ModContent.ProjectileType<StarSwallowerPet>();
            Item.buffType = ModContent.BuffType<StarSwallowerPetBuff>();
            Item.UseSound = SoundID.Item10;
        }

        public override void UseStyle(Player player, Rectangle heldItemFrame)
        {
            if (player.whoAmI == Main.myPlayer && player.itemTime == 0)
            {
                player.AddBuff(Item.buffType, 15, true);
            }
        }
    }
}
