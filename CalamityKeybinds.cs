using System;
using System.Linq;
using CalamityMod.Systems.Collections;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod
{
    public class CalamityKeybinds : ModSystem
    {
        public static ModKeybind AdrenalineHotKey { get; private set; }
        public static ModKeybind ArmorSetBonusHotKey { get; private set; }
        public static ModKeybind DashHotkey { get; private set; }
        public static ModKeybind ExoChairSlowdownHotkey { get; private set; }
        public static ModKeybind GodSlayerDashHotKey { get; private set; }
        public static ModKeybind NormalityRelocatorHotKey { get; private set; }
        public static ModKeybind RageHotKey { get; private set; }
        public static ModKeybind SwitchGravityHotkey { get; private set; }
        public static ModKeybind ExpandDebuffInfo { get; private set; }


        public static ModKeybind HeldItem { get; private set; }
        public static ModKeybind Accessory1 { get; private set; }
        public static ModKeybind Accessory2 { get; private set; }
        public static ModKeybind Accessory3 { get; private set; }
        public static ModKeybind Accessory4 { get; private set; }
        public static ModKeybind Accessory5 { get; private set; }
        public static ModKeybind Accessory6 { get; private set; }
        public static ModKeybind Accessory7 { get; private set; }

        public static ModKeybind[] AccessoryKeybinds = [];

        public override void Load()
        {
            // Register keybinds            
            AdrenalineHotKey = KeybindLoader.RegisterKeybind(Mod, "AdrenalineMode", "B");
            ArmorSetBonusHotKey = KeybindLoader.RegisterKeybind(Mod, "ArmorSetBonus", "Y");
            DashHotkey = KeybindLoader.RegisterKeybind(Mod, "DashDoubleTapOverride", "F");
            ExoChairSlowdownHotkey = KeybindLoader.RegisterKeybind(Mod, "ExoChairSlowDown", "RightShift");
            GodSlayerDashHotKey = KeybindLoader.RegisterKeybind(Mod, "GodSlayerDash", "H");
            NormalityRelocatorHotKey = KeybindLoader.RegisterKeybind(Mod, "NormalityRelocator", "Z");
            RageHotKey = KeybindLoader.RegisterKeybind(Mod, "RageMode", "V");
            SwitchGravityHotkey = KeybindLoader.RegisterKeybind(Mod, "GravitySwapOverride", "T");
            ExpandDebuffInfo = KeybindLoader.RegisterKeybind(Mod, "ExpandDebuffInfo", "LeftControl");

            HeldItem = KeybindLoader.RegisterKeybind(Mod, "HeldItem", "NumPad0");
            Accessory1 = KeybindLoader.RegisterKeybind(Mod, "Accessory1", "NumPad1");
            Accessory2 = KeybindLoader.RegisterKeybind(Mod, "Accessory2", "NumPad2");
            Accessory3 = KeybindLoader.RegisterKeybind(Mod, "Accessory3", "NumPad3");
            Accessory4 = KeybindLoader.RegisterKeybind(Mod, "Accessory4", "NumPad4");
            Accessory5 = KeybindLoader.RegisterKeybind(Mod, "Accessory5", "NumPad5");
            Accessory6 = KeybindLoader.RegisterKeybind(Mod, "Accessory6", "NumPad6");
            Accessory7 = KeybindLoader.RegisterKeybind(Mod, "Accessory7", "NumPad7");

            AccessoryKeybinds = [
                Accessory1,
                Accessory2,
                Accessory3,
                Accessory4,
                Accessory5,
                Accessory6,
                Accessory7
           ];
        }

        public override void Unload()
        {
            AdrenalineHotKey = null;
            ArmorSetBonusHotKey = null;
            DashHotkey = null;
            ExoChairSlowdownHotkey = null;
            GodSlayerDashHotKey = null;
            NormalityRelocatorHotKey = null;
            RageHotKey = null;
            ExpandDebuffInfo = null;

            Accessory1 = null;
            Accessory2 = null;
            Accessory3 = null;
            Accessory4 = null;
            Accessory5 = null;
            Accessory6 = null;
            Accessory1 = null;
            Accessory7 = null;

            AccessoryKeybinds = null;
        }
    }

    public static class CalamityKeybindsExtensions
    {
        /// <summary>
        /// Gets if the Calamity keybind for the given item was just pressed. <br/>
        /// Automatically adjusts between Held Item and Accessory Ability keybinds based on if the item is held and equip order of accessories
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static bool JustPressedKeybind(this Item item)
        {
            if (item == Main.LocalPlayer.HeldItem)
                return CalamityKeybinds.HeldItem.JustPressed;

            if (!CalamityItemSets.HasAccessoryKeybind[item.type])
                throw new InvalidOperationException("Item is not marked as having a Calamity keybind in the HasAccessoryKeybind item set");

            int keybindToUse = 0;

            for (var i = 0; i < Main.LocalPlayer.armor.Length; i++)
            {
                if (Main.LocalPlayer.armor[i].type == item.type)
                    break;
                if (CalamityItemSets.HasAccessoryKeybind[Main.LocalPlayer.armor[i].type])
                    keybindToUse++;
            }
            if (keybindToUse > 6)
                return false;
            return CalamityKeybinds.AccessoryKeybinds[keybindToUse].JustPressed;
        }

        /// <summary>
        /// Gets if the Calamity keybind for the given item was just released. <br/>
        /// Automatically adjusts between Held Item and Accessory Ability keybinds based on if the item is held and equip order of accessories
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static bool JustReleasedKeybind(this Item item)
        {

            if (item == Main.LocalPlayer.HeldItem)
                return CalamityKeybinds.HeldItem.JustReleased;

            if (!CalamityItemSets.HasAccessoryKeybind[item.type])
                throw new InvalidOperationException("Item is not marked as having a Calamity keybind in the HasAccessoryKeybind item set");

            int keybindToUse = 0;

            for (var i = 0; i < Main.LocalPlayer.armor.Length; i++)
            {
                if (Main.LocalPlayer.armor[i].type == item.type)
                    break;
                if (CalamityItemSets.HasAccessoryKeybind[Main.LocalPlayer.armor[i].type])
                    keybindToUse++;
            }

            if (keybindToUse > 6)
                return false;
            return CalamityKeybinds.AccessoryKeybinds[keybindToUse].JustReleased;
        }


        /// <summary>
        /// Gets if the Calamity keybind for the given item is currently held down. <br/>
        /// Automatically adjusts between Held Item and Accessory Ability keybinds based on if the item is held and equip order of accessories
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static bool CurrentlyPressingKeybind(this Item item)
        {
            if (item == Main.LocalPlayer.HeldItem)
                return CalamityKeybinds.HeldItem.Current;

            if (!CalamityItemSets.HasAccessoryKeybind[item.type])
                throw new InvalidOperationException("Item is not marked as having a Calamity keybind in the HasAccessoryKeybind item set");

            int keybindToUse = 0;

            for (var i = 0; i < Main.LocalPlayer.armor.Length; i++)
            {
                if (Main.LocalPlayer.armor[i].type == item.type)
                    break;
                if (CalamityItemSets.HasAccessoryKeybind[Main.LocalPlayer.armor[i].type])
                    keybindToUse++;
            }
            if (keybindToUse > 6)
                return false;

            return CalamityKeybinds.AccessoryKeybinds[keybindToUse].Current;
        }


        /// <summary>
        /// Gets the Calamity keybind for the given item. <br/>
        /// Automatically adjusts between Held Item and Accessory Ability keybinds based on if the item is held and equip order of accessories
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static ModKeybind? GetDynamicModHotkey(this Item item)
        {

            if (!CalamityItemSets.HasAccessoryKeybind[item.type])
                return CalamityKeybinds.HeldItem;

            if (!Main.LocalPlayer.armor.Any(x => x.type == item.type))
                return null;

            int keybindToUse = 0;

            for (var i = 0; i < Main.LocalPlayer.armor.Length; i++)
            {
                if (Main.LocalPlayer.armor[i].type == item.type)
                    break;
                if (CalamityItemSets.HasAccessoryKeybind[Main.LocalPlayer.armor[i].type])
                    keybindToUse++;
            }

            if (keybindToUse > 6)
                return null;

            return CalamityKeybinds.AccessoryKeybinds[keybindToUse];
        }
    }
}
