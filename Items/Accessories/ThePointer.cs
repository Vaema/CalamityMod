using System.Collections.Generic;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Accessories
{
    public class ThePointer : ModItem, ILocalizedModType
    {
        public static Asset<Texture2D> ActiveTexture;
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
                    int loops = (int)((player.Center.Distance(LockOnHelper.AimedTarget.Center)) / 30f) + 1;
                    for (var i = 1; i < loops; i++)
                    {
                        var pos = player.Center + dir * i * 30f;
                        if (i >= loops - 1)
                            pos = LockOnHelper.AimedTarget.Center - dir * 30f;
                        var x = new LineParticle(pos, dir, false, 10, 1, new Color(238, 2, 52));
                        GeneralParticleHandler.SpawnParticle(x);
                    }
                }
            }
        }

        public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            if (LockOnHelper.AimedTarget == null)
                return true;
            var tex = ActiveTexture ??= ModContent.Request<Texture2D>("CalamityMod/Items/Accessories/ThePointer_Active");
            CalamityUtils.DrawInventoryCustomScale(
                spriteBatch,
                tex.Value,
                position,
                tex.Frame(),
                drawColor,
                itemColor,
                tex.Size() * 0.5f,
                0.1f,
                wantedScale: 0.75f);
            return false;
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient(ItemID.Glass).
                AddRecipeGroup("AnyCopperBar", 2).
                AddRecipeGroup("IronBar", 3).
                AddIngredient(ItemID.ManaCrystal).
                AddTile(TileID.WorkBenches).
                Register();
        }
    }
}
