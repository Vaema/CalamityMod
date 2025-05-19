using CalamityMod.Items.Placeables.Furniture;
using CalamityMod.Items.Potions;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.TreasureBags.MiscGrabBags
{
    public class AbyssalTreasure : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.TreasureBags";
        internal static readonly int[] AbyssalTreasurePotions = new int[]
        {
            // Exploration potions
            ModContent.ItemType<AnechoicCoating>(),
            ItemID.FlipperPotion,
            ItemID.GillsPotion,
            ItemID.ShinePotion,
            ItemID.PotionOfReturn,
            // Other higher tier potions
            ItemID.EndurancePotion,
            ItemID.GravitationPotion,
            ItemID.HeartreachPotion,
            ItemID.LifeforcePotion,
            ItemID.SpelunkerPotion
        };

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 10;
            ItemID.Sets.ShimmerTransformToItem[Type] = ModContent.ItemType<SulphuricTreasure>();
        }

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.maxStack = Item.CommonMaxStack;
            Item.consumable = true;
            Item.rare = ItemRarityID.Blue; //Blue for thematics
        }

        public override void ModifyResearchSorting(ref ContentSamples.CreativeHelper.ItemGroup itemGroup)
        {
            itemGroup = ContentSamples.CreativeHelper.ItemGroup.GoodieBags;
        }

        public override bool CanRightClick() => true;

        public override void ModifyItemLoot(ItemLoot itemLoot)
        {
            LeadingConditionRule LCRsinglePlayer = new LeadingConditionRule(DropHelper.If(() => Main.netMode != NetmodeID.MultiplayerClient));
            LeadingConditionRule LCRexpert = new LeadingConditionRule(DropHelper.If(() => Main.expertMode));

            // Buff Potions @ 10%
            var buffPotions = itemLoot.Add(new OneFromOptionsNotScaledWithLuckDropRule(10, 1, AbyssalTreasurePotions));

            // Wormhole Potions @ 3% (Multiplayer only)
            var wormholePotion = ItemDropRule.ByCondition(DropHelper.If(() => Main.netMode == NetmodeID.MultiplayerClient), ItemID.WormholePotion, 30);

            // Pots can normally contain hearts here... but we turned it into a grab bag (so... coins!)
            // 4-12/5-18 Biome Torches @ 12.43% (87/700)
            var torches = DropHelper.NormalVsExpertQuantity(ModContent.ItemType<KelpTorch>(), 1, 4, 12, 5, 18);

            // 10-20 Hellfire Arrows @ 12.43%
            var ammo = ItemDropRule.NotScalingWithLuck(ItemID.HellfireArrow, 1, 10, 20);

            // 1 Healing Potion (33.33% in Expert for +1) @ 12.43%
            var healPot = ItemDropRule.NotScalingWithLuck(ItemID.HealingPotion, 1);
            var healPotExtra = ItemDropRule.NotScalingWithLuck(ItemID.HealingPotion, 3);

            // 1-4/1-7 Dynamites @ 12.43%
            var bombs = DropHelper.NormalVsExpertQuantity(ItemID.Dynamite, 1, 1, 4, 1, 7);

            // Ropes substituted by coins (assuming you don't need it)
            // 4-18/12-54 Silver Coins (500% coin modifier) @ 37.29%
            // Expert multiplier is a simple 3x because vanilla coin modifiers are awful
            var coins = DropHelper.NormalVsExpertQuantity(ItemID.SilverCoin, 1, 4, 18, 12, 54);

            OneFromRulesRule otherDrops = new OneFromRulesRule(1, new IItemDropRule[] { coins, torches, ammo, healPot, bombs, coins, coins });

            buffPotions.OnFailedRoll(wormholePotion).OnFailedRoll(otherDrops);
            buffPotions.OnFailedRoll(LCRsinglePlayer).OnSuccess(otherDrops);
            healPot.OnSuccess(LCRexpert).OnSuccess(healPotExtra);
        }
    }
}
