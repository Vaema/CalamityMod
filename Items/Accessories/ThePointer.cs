using System.Collections.Generic;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.CalPlayer;
using CalamityMod.Items.Materials;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Items.Accessories
{
    public class ThePointer : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Accessories";
        public override void SetDefaults()
        {
            Item.width = 26;
            Item.height = 26;
            Item.value = CalamityGlobalItem.RarityBlueBuyPrice;
            Item.rare = ItemRarityID.Blue;
            Item.accessory = true;
        }

        public override void ModifyTooltips(List<TooltipLine> list) => list.IntegrateHotkey(CalamityKeybinds.ThePointerLock);

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            LockOnHelper.ForceUsability = true;
            if (CalamityKeybinds.ThePointerLock.JustPressed)
            {
                LockOnHelper.Toggle();
                if (LockOnHelper.AimedTarget != null)
                {
                    Vector2 dir = player.DirectionTo(LockOnHelper.AimedTarget.Center);
                    int loops = (int)((player.Center.Distance(LockOnHelper.AimedTarget.Center))/30f)+1;
                    for (var i = 1; i < loops; i++)
                    {
                        var pos = player.Center + dir*i*30f;
                        if (i >= loops-1)
                            pos = LockOnHelper.AimedTarget.Center - dir*30f;
                        var x = new LineParticle(pos, dir, false, 10, 1, new Color(70,70,255));
                        GeneralParticleHandler.SpawnParticle(x);
                    }
                }
            }
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient(ItemID.Glass).
                AddRecipeGroup("AnyCopperBar", 2).
                AddRecipeGroup("AnyIronBar",3).
                AddIngredient(ItemID.ManaCrystal).
                AddTile(TileID.WorkBenches).
                Register();
        }
    }
}
