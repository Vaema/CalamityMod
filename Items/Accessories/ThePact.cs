using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Items.Accessories
{
    public class ThePact : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Accessories";

        public static float MaxLifeMult => 2;
        public static float ChanceToBeCrit => 0.25f;
        public static float CritDmgTaken => 2.25f;
        public static float HealingPotionBoost => 1.5f;
        public static int BoostDuration => 900;
        public static int DebuffInflictionDuration => 60;
        public static float NaturalRegenBoost => 1.5f;
        public static int RegenTimeBoost => 3;
        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(MaxLifeMult,ChanceToBeCrit.ToPercent(),CritDmgTaken);

        public override void SetStaticDefaults()
        {
            Main.RegisterItemAnimation(Item.type, new DrawAnimationVertical(4, 4));
            ItemID.Sets.AnimatesAsSoul[Type] = true;
        }

        public override void SetDefaults()
        {
            Item.width = 40;
            Item.height = 50;
            Item.value = CalamityGlobalItem.RarityLightRedBuyPrice;
            Item.rare = ItemRarityID.LightRed;
            Item.accessory = true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.Calamity().CanBeCritByThePact = true;
        }

        public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            CalamityUtils.DrawInventoryCustomScale(
                spriteBatch,
                texture: TextureAssets.Item[Type].Value,
                position,
                frame,
                drawColor,
                itemColor,
                origin,
                scale,
                wantedScale: 0.5f,
                drawOffset: new(0f, 0f)
            );
            return false;
        }
    }
}
