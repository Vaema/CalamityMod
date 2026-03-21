using CalamityMod.NPCs;
using CalamityMod.Tiles.Furniture;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Placeables.Furniture
{
    public class TheMonument : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Placeables";
        public override string Texture => "Terraria/Images/Item_1177"; //Placeholder

        public const float MonumentTaxIncrease = 0.5f;
        public const float MonumentHappinessReduction = 0.15f;

        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<TheMonumentTile>());
            Item.rare = ItemRarityID.Pink;
            Item.value = Item.buyPrice(gold: 5);
        }

        public override void Load()
        {
            On_Player.CollectTaxes += AddMonumentTaxes;      
        }

        private static void AddMonumentTaxes(On_Player.orig_CollectTaxes orig, Player self)
        {
            orig(self);

            foreach (NPC n in Main.ActiveNPCs)
            {
                if (!n.homeless && !NPCID.Sets.IsTownPet[n.type] && NPC.TypeToDefaultHeadIndex(n.type) > 0 && n.GetGlobalNPC<CalamityGlobalTownNPC>().AffectedByTheMonument)
                    self.taxMoney += (int)(CalamityGlobalTownNPC.TotalTaxesPerNPC * MonumentTaxIncrease);
            }
            if (self.taxMoney > CalamityGlobalTownNPC.TaxesToCollectLimit)
                self.taxMoney = CalamityGlobalTownNPC.TaxesToCollectLimit;
        }
    }
}
