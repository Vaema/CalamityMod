using CalamityMod.Buffs.Pets;
using CalamityMod.Items.Materials;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Accessories
{
    public class DaawnlightSpiritOrigin : ModItem, ILocalizedModType, IHoldShiftTooltipItem
    {
        public new string LocalizationCategory => "Items.Accessories";

        public bool HidesNormalTooltip => true;
        public bool HasFlavorTooltip => true;
        public Color? FlavorTooltipColor => new(149, 28, 235); // #951CEB

        // "Despite the seemingly insane numbers here, I think this item might actually be underpowered"
        // hindsight: the item was not underpowered. Ozzatron 05NOV2021
        // Memes 03SEP2024: Old comments kept for record.

        #region Balancing Variables

        /// <summary>
        /// The bullseye's total lifespan while it is not hit.
        /// </summary>
        public const int BullseyeIdleLifetime = 600;

        /// <summary>
        /// The bullseye's lifespan when hit.
        /// </summary>
        public const int BullseyeHitLifetime = 90;

        /// <summary>
        /// The minimum amount of critcal strike chance lost per decrease.
        /// </summary>
        public const int MinCritLossPerFrame = 1;

        /// <summary>
        /// The minimum rate at which the extra critcal strike chance decreases.<br/>
        /// This means that it'll decrease every X frames.<br/>
        /// <br/>
        /// When there's no critical strike chance, this will be the loss rate,<br/>
        /// and it'll linearly scale to <see cref="MaximumLossRate"/>.
        /// </summary>
        public const int MinimumLossRate = 4;

        /// <summary>
        /// The maximum rate at which the extra critcal strike chance decreases.<br/>
        /// This means that it'll decrease every X frames.<br/>
        /// <br/>
        /// When the extra critical strike chance reaches <see cref="ExtraCritHardCap"/>,<br/>
        /// this value will be the loss rate.
        /// </summary>
        public const int MaximumLossRate = 1;

        /// <summary>
        /// The amount of extra critcal strike chance at which the hard scaling starts applying.
        /// </summary>
        public const int ExtraCritHardCap = 75;

        /// <summary>
        /// When the extra critical strike chance is past <see cref="ExtraCritHardCap"/>,<br/>
        /// every <see cref="CritHardCapScalingInterval"/> more, it'll start decreasing by <see cref="CritLossPerFrameIncreasePerInterval"/> more.
        /// </summary>
        public const int CritHardCapScalingInterval = 25;

        /// <summary>
        /// The amount of extra critical strike chance lost every <see cref="CritLossPerFrameIncreasePerInterval"/> past <see cref="ExtraCritHardCap"/>.
        /// </summary>
        public const int CritLossPerFrameIncreasePerInterval = 4;

        // These were very carefully calculated, please don't change them.
        internal const float RegularEnemyBullseyeRadius = 8f;
        internal const float BossBullseyeRadius = 18f;

        // Special search radius for coin ricoshots that only applies to DSO targets.
        public static readonly float RicoshotSearchDistance = 2800f;

        #endregion

        public override void SetDefaults()
        {
            Item.width = 22;
            Item.height = 38;
            Item.accessory = true;
            Item.rare = ItemRarityID.Purple;
            Item.value = CalamityGlobalItem.RarityPurpleBuyPrice;
            Item.Calamity().donorItem = true;
        }

        // The pet is purely visual and does not affect the functionality of the item.
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.Calamity().spiritOrigin = true;

            // If visibility is disabled, despawn the pet.
            if (hideVisual)
            {
                if (player.FindBuffIndex(ModContent.BuffType<ArcherofLunamoon>()) != -1)
                    player.ClearBuff(ModContent.BuffType<ArcherofLunamoon>());
            }
            // If visibility is enabled, spawn the pet.
            else if (player.whoAmI == Main.myPlayer)
            {
                if (player.FindBuffIndex(ModContent.BuffType<ArcherofLunamoon>()) == -1)
                    player.AddBuff(ModContent.BuffType<ArcherofLunamoon>(), 18000, true);
            }
        }

        public override void UpdateVanity(Player player)
        {
            // Summon anime girl if it's in vanity slot as the pet is purely vanity
            // It's possible for other "pet" items like Fungal Clump or HotE to summon a passive version of their "pets" with some tweaks though
            player.Calamity().spiritOriginVanity = true;
            if (player.whoAmI == Main.myPlayer)
            {
                if (player.FindBuffIndex(ModContent.BuffType<ArcherofLunamoon>()) == -1)
                    player.AddBuff(ModContent.BuffType<ArcherofLunamoon>(), 18000, true);
            }
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<DeadshotBrooch>().
                AddIngredient<MysteriousCircuitry>(15).
                AddIngredient<DubiousPlating>(15).
                AddIngredient(ItemID.LunarBar, 10).
                AddIngredient<GalacticaSingularity>(4).
                AddTile(TileID.LunarCraftingStation).
                Register();
        }
    }
}
