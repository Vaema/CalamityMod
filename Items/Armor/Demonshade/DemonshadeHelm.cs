using CalamityMod.Buffs.Summon;
using CalamityMod.CalPlayer;
using CalamityMod.Items.Materials;
using CalamityMod.Items.Potions.Alcohol;
using CalamityMod.Projectiles.Typeless;
using CalamityMod.Rarities;
using CalamityMod.Tiles.Furniture.CraftingStations;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace CalamityMod.Items.Armor.Demonshade
{
    [AutoloadEquip(EquipType.Head)]
    public class DemonshadeHelm : ModItem, IExtendedHat, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Armor.PostMoonLord";
        public static readonly SoundStyle ActivationSound = new("CalamityMod/Sounds/Custom/AbilitySounds/DemonshadeEnrage");
        internal static string ShadowScytheEntitySourceContext => "SetBonus_Calamity_Demonshade";

        public static int DevilDamage = 1000;
        public static int BeamDamage => CalamityUtils.ScaleWithDifficulty(300);
        public static int ScytheDamage => CalamityUtils.ScaleWithDifficulty(500);
        public static float MultDamageBoost = 0.5f;
        public static double MultDamageTakenBoost = 0.25D;

        public override void SetDefaults()
        {
            Item.width = 18;
            Item.height = 18;
            Item.defense = 50;
            Item.value = CalamityGlobalItem.RarityHotPinkBuyPrice;
            Item.rare = ModContent.RarityType<HotPink>();
            Item.Calamity().devItem = true;
        }

        public override bool IsArmorSet(Item head, Item body, Item legs)
        {
            return body.type == ModContent.ItemType<DemonshadeBreastplate>() && legs.type == ModContent.ItemType<DemonshadeGreaves>();
        }

        public override void ArmorSetShadows(Player player)
        {
            player.armorEffectDrawShadow = true;
            player.armorEffectDrawOutlines = true;
        }

        public override void UpdateArmorSet(Player player)
        {
            player.setBonus = this.GetLocalization("SetBonus").Format(CalamityUtils.GetArmorSetBonusKey(), (1f + MultDamageBoost).Round(), (1D + MultDamageTakenBoost).Round());
            var modPlayer = player.Calamity();
            modPlayer.dsSetBonus = true;
            modPlayer.wearingRogueArmor = true;
            modPlayer.WearingPostMLSummonerSet = true;
            if (player.whoAmI == Main.myPlayer && !modPlayer.chibii)
            {
                modPlayer.redDevil = true;
                var source = player.GetSource_ItemUse(Item);
                if (player.FindBuffIndex(ModContent.BuffType<DemonshadeSetDevilBuff>()) == -1)
                {
                    player.AddBuff(ModContent.BuffType<DemonshadeSetDevilBuff>(), 3600, true);
                }
                if (player.ownedProjectileCounts[ModContent.ProjectileType<DemonshadeRedDevil>()] < 1)
                {
                    int damage = (int)player.GetTotalDamage<SummonDamageClass>().ApplyTo(DevilDamage);

                    var devil = Projectile.NewProjectileDirect(source, player.Center, -Vector2.UnitY, ModContent.ProjectileType<DemonshadeRedDevil>(), damage, 0f, Main.myPlayer, 2f, 0f);
                    devil.originalDamage = DevilDamage;
                }
            }
            player.GetDamage<SummonDamageClass>() += 1f;
            player.maxMinions += 10;
        }

        public override void UpdateEquip(Player player)
        {
            player.GetDamage<GenericDamageClass>() += 0.3f;
            player.GetCritChance<GenericDamageClass>() += 15;
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<ShadowspecBar>(12).
                AddTile<DraedonsForge>().
                SortBeforeFirstRecipesOf(ModContent.ItemType<DemonshadeBreastplate>()).
                Register();
        }

        public string ExtensionTexture => "CalamityMod/Items/Armor/Demonshade/DemonshadeHelm_Extension";
        public Vector2 ExtensionSpriteOffset(PlayerDrawSet drawInfo) => new Vector2(0, -4f);
    }
}
