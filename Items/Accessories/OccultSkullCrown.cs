using CalamityMod.CalPlayer;
using CalamityMod.Items.Materials;
using CalamityMod.Rarities;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Accessories
{
    [AutoloadEquip(EquipType.Face)]
    public class OccultSkullCrown : ModItem, ILocalizedModType, IHoldShiftTooltipItem
    {
        public new string LocalizationCategory => "Items.Accessories";

        public bool HasFlavorTooltip => true;
        public Color? TooltipExtensionColor => new(195, 223, 255);

        public override void SetStaticDefaults()
        {

            if (!Main.dedServ)
            {
                int equipSlot = EquipLoader.GetEquipSlot(Mod, Name, EquipType.Face);
                ArmorIDs.Face.Sets.PreventHairDraw[equipSlot] = true;
                ArmorIDs.Face.Sets.OverrideHelmet[equipSlot] = true;
            }
        }

        public override void SetDefaults()
        {
            Item.width = 82;
            Item.height = 62;
            Item.defense = 5;
            Item.rare = ModContent.RarityType<Turquoise>();
            Item.value = CalamityGlobalItem.RarityTurquoiseBuyPrice;
            Item.accessory = true;
            Item.SetRevExclusive();
        }

        public override void UpdateEquip(Player player)
        {
            CalamityPlayer modPlayer = player.Calamity();
            modPlayer.laudanum = true;
            modPlayer.heartOfDarkness = true;
            modPlayer.stressPills = true;
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<HeartofDarkness>().
                AddIngredient<Laudanum>().
                AddIngredient<StressPills>().
                AddIngredient<TwistingNether>(3).
                AddTile(TileID.MythrilAnvil).
                Register();
        }
    }
}
