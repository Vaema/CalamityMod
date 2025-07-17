using CalamityMod.CalPlayer;
using CalamityMod.Projectiles.Typeless;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Fishing.FishingRods
{
    public class SlurperPole : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Fishing";
        public override void SetStaticDefaults()
        {
            ItemID.Sets.CanFishInLava[Type] = true;
        }

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 28;
            Item.useAnimation = 8;
            Item.useTime = 8;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.UseSound = SoundID.Item1;
            Item.fishingPole = 25;
            Item.shootSpeed = 14f;
            Item.shoot = ModContent.ProjectileType<SlurperBobber>();
            Item.value = CalamityGlobalItem.RarityOrangeBuyPrice;
            Item.rare = ItemRarityID.Orange;
            Item.accessory = true;
        }

        public override bool AllowPrefix(int pre)
        {
            if (pre == 0)
                return true;
            return false;
        }

        public override bool CanReforge()
        {
            return false;
        }
        public override void HoldItem(Player player)
        {
            if (player.Calamity().SelectedFishingMinigame == CalamityPlayer.FishingMinigames.None)
                player.Calamity().SelectedFishingMinigame = CalamityPlayer.FishingMinigames.SlurperPole;
        }

        public override void UpdateEquip(Player player)
        {
            player.Calamity().SelectedFishingMinigame = CalamityPlayer.FishingMinigames.SlurperPole;
        }

        public override void ModifyFishingLine(Projectile bobber, ref Vector2 lineOriginOffset, ref Color lineColor)
        {
            lineOriginOffset = new Vector2(45f, -43f);
            lineColor = new Color(227, 79, 79, 100);
        }
    }
}
