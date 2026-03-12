using System;
using System.Collections.Generic;
using System.Linq;
using CalamityMod.Items.Materials;
using CalamityMod.Rarities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;

namespace CalamityMod.Items.Accessories
{

    public enum AlcoholType
    {
        None,
        BloodyMary,
        CaribbeanRum,
        CinnamonRoll,
        Everclear,
        EvergreenGin,
        Fireball,
        GrapeBeer,
        Manhattan,
        Margarita,
        Moonshine,
        MoscowMule,
        OldFashioned,
        PurpleHaze,
        RedWine,
        Rum,
        Screwdriver,
        StarBeamRye,
        Tequila,
        TequilaSunrise,
        Vodka,
        Whiskey,
        WhiteWine,
        Ale // DOES NOT USE IALCOHOLITEM YET
    }

    public interface IAlcoholItem
    {
        Action<Player, float> AlcoholEffect { get; }
        AlcoholType AlcoholVariant { get; }
    }

    public class IVDripOnTheRocks : ModItem, ILocalizedModType
    {
        public int containedAlcoholID = -1;
        public AlcoholType currentAlcoholType = AlcoholType.None;
        public new string LocalizationCategory => "Items.Accessories";
        public static readonly float DamageBoostMultiplier = 1.25f; // (only exists right now so that it compiles, leave it be)
        public static readonly float DamageReductionMultiplier = 0.75f; // (only exists right now so that it compiles, leave it be)

        public override void SetStaticDefaults()
        {
            Main.RegisterItemAnimation(Item.type, new Terraria.DataStructures.DrawAnimationVertical(1, 24));
            ItemID.Sets.AnimatesAsSoul[Item.type] = true;
        }

        public override void SetDefaults()
        {
            Item.width = 58;
            Item.height = 62;
            Item.value = CalamityGlobalItem.RarityLimeBuyPrice;
            Item.rare = ItemRarityID.Lime;
            Item.accessory = true;
        }

        public override void Load()
        {
            On_ItemSlot.OverrideLeftClick += IvDripAlcoholEquip;
        }

        public override void Unload()
        {
            On_ItemSlot.OverrideLeftClick -= IvDripAlcoholEquip;
        }

        private bool IvDripAlcoholEquip(On_ItemSlot.orig_OverrideLeftClick orig, Item[] inv, int context, int slot)
        {
            if (context == ItemSlot.Context.InventoryItem || context == ItemSlot.Context.ChestItem)
            {
                Item mouseItem = Main.mouseItem;
                Item targetItem = inv[slot];

                // Alcohol into this acc
                if (targetItem.ModItem is IVDripOnTheRocks drip && mouseItem.ModItem is IAlcoholItem alcohol)
                {
                    if (drip.containedAlcoholID == -1)
                    {
                        drip.containedAlcoholID = mouseItem.type;
                        drip.currentAlcoholType = alcohol.AlcoholVariant;

                        mouseItem.stack--;
                        if (mouseItem.stack <= 0)
                            mouseItem.TurnToAir();

                        SoundEngine.PlaySound(SoundID.Grab);
                        return true;
                    }
                }

                if (mouseItem.ModItem is IVDripOnTheRocks dripHeld && targetItem.ModItem is IAlcoholItem alcoholTarget)
                {
                    if (dripHeld.containedAlcoholID == -1)
                    {
                        dripHeld.containedAlcoholID = targetItem.type;
                        dripHeld.currentAlcoholType = alcoholTarget.AlcoholVariant;

                        targetItem.stack--;
                        if (targetItem.stack <= 0)
                            targetItem.TurnToAir();

                        SoundEngine.PlaySound(SoundID.Grab);
                        return true;
                    }
                }
            }

            return orig(inv, context, slot);
        }

        #region Shift-Right Click to Empty IV Drip

        // Allow shift-right clicking only if the IV drip actually contains alcohol
        public override bool CanRightClick() => containedAlcoholID != -1 && Main.keyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.LeftShift);

        public override void RightClick(Player player)
        {
            if (containedAlcoholID != -1)
            {
                // Give the alcohol back to the player
                player.QuickSpawnItem(player.GetSource_ItemUse(Item), containedAlcoholID);

                containedAlcoholID = -1;
                currentAlcoholType = AlcoholType.None;

                Item.stack++;

                SoundEngine.PlaySound(SoundID.Grab);
            }
        }
        #endregion

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            IVDripPlayer dripPlayer = player.GetModPlayer<IVDripPlayer>();
            dripPlayer.ivDripEquipped = true;

            if (containedAlcoholID != -1)
            {
                Item alcoholItem = new Item(containedAlcoholID);

                if (alcoholItem.ModItem is IAlcoholItem alcohol)
                    dripPlayer.ApplyAlcoholEffect(alcohol.AlcoholEffect);
            }
        }
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            if (tooltips == null) return;

            var effectLine = tooltips.FirstOrDefault(x => x.Text.Contains("[EFFECT]"));
            var nameLine = tooltips.FirstOrDefault(x => x.Text.Contains("[NAME]"));

            if (containedAlcoholID == -1)
            {
                if (effectLine != null)
                {
                    effectLine.Text = this.GetLocalizedValue("Empty");
                    effectLine.OverrideColor = Color.Gray;
                }
                nameLine?.Hide();
                return;
            }

            if (effectLine != null)
            {
                string effectText = Lang.GetTooltip(containedAlcoholID).ToString();
                effectLine.Text = effectText;
                effectLine.OverrideColor = Color.White;
            }

            if (nameLine != null)
            {
                string alcoholName = Lang.GetItemNameValue(containedAlcoholID);
                nameLine.Text = $"Currently filled with [c/FF81E4:{alcoholName}]";
            }
        }

        // Draw the correct alcohol variant frame
        public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            Texture2D texture = TextureAssets.Item[Item.type].Value;

            int frameHeight = texture.Height / 24;
            int frameIndex = (int)currentAlcoholType;

            Rectangle targetFrame = new Rectangle(0, frameIndex * frameHeight, texture.Width, frameHeight);

            spriteBatch.Draw(texture, position, targetFrame, drawColor, 0f, origin, scale, SpriteEffects.None, 0f);
            return false;
        }
    }

    public class IVDripPlayer : ModPlayer
    {
        public Dictionary<Action<Player, float>, float> alcoholEffects = new();
        public bool ivDripEquipped;

        public override void ResetEffects()
        {
            alcoholEffects.Clear();
            ivDripEquipped = false;
        }

        public void ApplyAlcoholEffect(Action<Player, float> effect)
        {
            if (effect == null)
                return;

            if (alcoholEffects.ContainsKey(effect))
                alcoholEffects[effect] += 1f;
            else
                alcoholEffects[effect] = 1f;
        }

        public override void PostUpdateEquips()
        {
            foreach (var effect in alcoholEffects)
            {
                effect.Key.Invoke(Player, effect.Value);
            }
        }
    }
}
