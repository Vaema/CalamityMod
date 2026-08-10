using Terraria.ModLoader;

namespace CalamityMod;

public class AllClassDamageClass : DamageClass
{
    internal static AllClassDamageClass Instance;

    public override void Load() => Instance = this;
    public override void Unload() => Instance = null;

    public override StatInheritanceData GetModifierInheritance(DamageClass damageClass)
    {
        return StatInheritanceData.Full;
    }
    public override bool GetEffectInheritance(DamageClass damageClass) => true;
}
