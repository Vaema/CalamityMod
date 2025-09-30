using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

//This is for vanilla buffs/debuffs that need icon items
//Calamity ones are stored at the bottom of their debuff file
namespace CalamityMod.Items
{

    public class PotionSicknessIconItem : ModItem
    {
        private string BuffName = "PotionSickness";
        public override string Texture => $"Terraria/Images/Buff_{BuffID.PotionSickness}";
        public override LocalizedText DisplayName => Language.GetText($"BuffName.{BuffName}");
        public override LocalizedText Tooltip => Language.GetOrRegister($"Mods.Terraria.Buffs.{BuffName}.ItemTooltip");
    }
    public class VenomIconItem : ModItem
    {
        private string BuffName = "Venom";
        public override string Texture => $"Terraria/Images/Buff_{BuffID.Venom}";
        public override LocalizedText DisplayName => Language.GetText($"BuffName.{BuffName}");
        public override LocalizedText Tooltip => Language.GetOrRegister($"Mods.Terraria.Buffs.{BuffName}.ItemTooltip");
    }
    public class BetsysCurseIconItem : ModItem
    {
        private string BuffName = "BetsysCurse";
        public override string Texture => $"CalamityMod/ExtraTextures/VanillaBuffs/BetsysCurse";
        public override LocalizedText DisplayName => Language.GetText($"BuffName.{BuffName}");
        public override LocalizedText Tooltip => Language.GetOrRegister($"Mods.Terraria.Buffs.{BuffName}.ItemTooltip");
    }

    public class CursedInfernoIconItem : ModItem
    {
        private string BuffName = "CursedInferno";
        public override string Texture => $"Terraria/Images/Buff_{BuffID.CursedInferno}";
        public override LocalizedText DisplayName => Language.GetText($"BuffName.{BuffName}");
        public override LocalizedText Tooltip => Language.GetOrRegister($"Mods.Terraria.Buffs.{BuffName}.ItemTooltip");
    }
    public class Frostburn2IconItem : ModItem
    {
        private string BuffName = "Frostburn2";
        public override string Texture => $"Terraria/Images/Buff_{BuffID.Frostburn2}";
        public override LocalizedText DisplayName => Language.GetText($"BuffName.{BuffName}");
        public override LocalizedText Tooltip => Language.GetOrRegister($"Mods.Terraria.Buffs.{BuffName}.ItemTooltip");
    }
    public class FrostburnIconItem : ModItem
    {
        private string BuffName = "Frostburn";
        public override string Texture => $"Terraria/Images/Buff_{BuffID.Frostburn}";
        public override LocalizedText DisplayName => Language.GetText($"BuffName.{BuffName}");
        public override LocalizedText Tooltip => Language.GetOrRegister($"Mods.Terraria.Buffs.{BuffName}.ItemTooltip");
    }
    public class OnFireIconItem : ModItem
    {
        private string BuffName = "OnFire";
        public override string Texture => $"Terraria/Images/Buff_{BuffID.OnFire}";
        public override LocalizedText DisplayName => Language.GetText($"BuffName.{BuffName}");
        public override LocalizedText Tooltip => Language.GetOrRegister($"Mods.Terraria.Buffs.{BuffName}.ItemTooltip");
    }

    public class OnFire3IconItem : ModItem
    {
        private string BuffName = "OnFire3";
        public override string Texture => $"Terraria/Images/Buff_{BuffID.OnFire3}";
        public override LocalizedText DisplayName => Language.GetText($"BuffName.{BuffName}");
        public override LocalizedText Tooltip => Language.GetOrRegister($"Mods.Terraria.Buffs.{BuffName}.ItemTooltip");
    }
    public class IchorIconItem : ModItem
    {
        private string BuffName = "Ichor";
        public override string Texture => $"Terraria/Images/Buff_{BuffID.Ichor}";
        public override LocalizedText DisplayName => Language.GetText($"BuffName.{BuffName}");
        public override LocalizedText Tooltip => Language.GetOrRegister($"Mods.Terraria.Buffs.{BuffName}.ItemTooltip");
    }
    public class OiledIconItem : ModItem
    {
        private string BuffName = "Oiled";
        public override string Texture => $"CalamityMod/ExtraTextures/VanillaBuffs/Oiled";
        public override LocalizedText DisplayName => Language.GetText($"BuffName.{BuffName}");
        public override LocalizedText Tooltip => Language.GetOrRegister($"Mods.Terraria.Buffs.{BuffName}.ItemTooltip");
    }
    public class PoisonedIconItem : ModItem
    {
        private string BuffName = "Poisoned";
        public override string Texture => $"Terraria/Images/Buff_{BuffID.Poisoned}";
        public override LocalizedText DisplayName => Language.GetText($"BuffName.{BuffName}");
        public override LocalizedText Tooltip => Language.GetOrRegister($"Mods.Terraria.Buffs.{BuffName}.ItemTooltip");
    }
    public class SlimedIconItem : ModItem
    {
        private string BuffName = "Slimed";
        public override string Texture => $"Terraria/Images/Buff_{BuffID.Slimed}";
        public override LocalizedText DisplayName => Language.GetText($"BuffName.{BuffName}");
        public override LocalizedText Tooltip => Language.GetOrRegister($"Mods.Terraria.Buffs.{BuffName}.ItemTooltip");
    }
    public class WetIconItem : ModItem
    {
        private string BuffName = "Wet";
        public override string Texture => $"Terraria/Images/Buff_{BuffID.Wet}";
        public override LocalizedText DisplayName => Language.GetText($"BuffName.{BuffName}");
        public override LocalizedText Tooltip => Language.GetOrRegister($"Mods.Terraria.Buffs.{BuffName}.ItemTooltip");
    }
    public class ElectrifiedIconItem : ModItem
    {
        private string BuffName = "Electrified";
        public override string Texture => $"Terraria/Images/Buff_{BuffID.Electrified}";
        public override LocalizedText DisplayName => Language.GetText($"BuffName.{BuffName}");
        public override LocalizedText Tooltip => Language.GetOrRegister($"Mods.Terraria.Buffs.{BuffName}.ItemTooltip");
    }
}
