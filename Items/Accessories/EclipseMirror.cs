using CalamityMod.Balancing;
using CalamityMod.CalPlayer;
using CalamityMod.Cooldowns;
using CalamityMod.Items.Materials;
using CalamityMod.Projectiles.Rogue;
using CalamityMod.Rarities;
using CalamityMod.Tiles.Furniture.CraftingStations;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Accessories
{
    public class EclipseMirror : ModItem, ILocalizedModType, IHoldShiftTooltipItem
    {
        public new string LocalizationCategory => "Items.Accessories";
        public bool HasFlavorTooltip => true;

        public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 46;
            Item.value = CalamityGlobalItem.RarityDarkBlueBuyPrice;
            Item.rare = ModContent.RarityType<CosmicPurple>();
            Item.accessory = true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            CalamityPlayer modPlayer = player.Calamity();
            modPlayer.stealthGenStandstill += 0.25f;
            modPlayer.rogueStealthMax += 0.1f;
            modPlayer.eclipseMirror = true;
            modPlayer.stealthStrikeHalfCost = true;
            player.GetCritChance<ThrowingDamageClass>() += 6;
            player.GetDamage<ThrowingDamageClass>() += 0.06f;
            player.aggro -= 700;
            modPlayer.DodgeEffects.Add(EclipseMirrorDodge);
        }

        public string EclipseMirrorDodge(Player Player, Player.HurtInfo info)
        {

            // 17APR2024: Ozzatron: Eclipse Mirror is a dodge. It uses vanilla dodge iframes and benefits from Cross Necklace.
            int eclipseMirrorDodgeIFrames = Player.ComputeDodgeIFrames();
            Player.GiveUniversalIFrames(eclipseMirrorDodgeIFrames, true);

            Player.Calamity().rogueStealth += 0.5f;
            SoundEngine.PlaySound(SoundID.Item68, Player.Center);

            var source = Player.GetSource_Accessory(Player.Calamity().FindAccessory(ModContent.ItemType<EclipseMirror>()));
            int damage = (int)Player.GetTotalDamage<RogueDamageClass>().ApplyTo(2000);

            int eclipse = Projectile.NewProjectile(source, Player.Center, Vector2.Zero, ModContent.ProjectileType<EclipseMirrorBurst>(), damage, 0, Player.whoAmI);
            if (eclipse.WithinBounds(Main.maxProjectiles))
                Main.projectile[eclipse].DamageType = DamageClass.Generic;

            // TODO -- Calamity dodges should probably not send a vanilla dodge packet considering that causes Tabi dust
            NetMessage.SendData(MessageID.Dodge, -1, -1, null, Player.whoAmI, 1f, 0f, 0f, 0, 0, 0);
            return "eclipsemirror";
        }


        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<AbyssalMirror>().
                AddIngredient<DarkMatterSheath>().
                AddIngredient<DarksunFragment>(20).
                AddTile<CosmicAnvil>().
                Register();
        }
    }
}
