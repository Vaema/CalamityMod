using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.CalPlayer;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Accessories
{
    public class HideofAstrumDeus : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Accessories";

        public static int BlazeDamage => CalamityUtils.ScaleWithDifficulty(50);
        public static int StarDamage => 75;

        public override void SetDefaults()
        {
            Item.width = 26;
            Item.height = 26;
            Item.value = CalamityGlobalItem.RarityCyanBuyPrice;
            Item.rare = ItemRarityID.Cyan;
            Item.accessory = true;
            Item.expert = true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            CalamityPlayer modPlayer = player.Calamity();
            modPlayer.hideOfDeus = true;
            if (modPlayer.hideOfDeusMeleeBoostTimer > 0)
                player.GetDamage<TrueMeleeDamageClass>() += 0.3f;
        }
    }
}
