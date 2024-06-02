using CalamityMod.Projectiles.Typeless;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace CalamityMod.Items.Fishing.SunkenSeaCatches
{
    public class SerpentsBite : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Fishing";
        public override void SetDefaults()
        {
            // Instead of copying these values, we can clone and modify the ones we want to copy
            Item.CloneDefaults(ItemID.AmethystHook);
            Item.width = 30;
            Item.height = 32;
            Item.shootSpeed = SerpentsBiteHook.LaunchSpeed; // how quickly the hook is shot.
            Item.shoot = ProjectileType<SerpentsBiteHook>();
            Item.value = CalamityGlobalItem.RarityLightRedBuyPrice;
            Item.rare = ItemRarityID.LightRed;
        }
    }
}
