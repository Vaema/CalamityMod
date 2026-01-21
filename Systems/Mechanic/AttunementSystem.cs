using CalamityMod.DataStructures;
using Terraria.ModLoader;

namespace CalamityMod.Systems
{
    public sealed class AttunementSystem : ModSystem
    {
        public static Attunement[] Registry;
        public static Attunement Empty => Registry[EmptyID];
        public static int EmptyID => Registry.Length - 1;

        public override void OnModLoad()
        {
            // This probably could be replaced to ReflectionHelper.IterateCalamityModTypes<Attunement>
            Registry = [
                
                // Broken Biome Blade
                new DefaultAttunement(), new HotAttunement(), new ColdAttunement(), new EvilAttunement(),

                // Biome Blade
                new TrueDefaultAttunement(), new TrueHotAttunement(), new TrueColdAttunement(), new TrueTropicalAttunement(), new TrueEvilAttunement(), new HolyAttunement(),
                
                // True Biome Blade
                new WhirlwindAttunement(), new FlailBladeAttunement(), new SuperPogoAttunement(), new ShockwaveAttunement(),

                // Galaxia
                new PhoenixAttunement(), new AriesAttunement(), new PolarisAttunement(), new AndromedaAttunement(),

                // Empty Slot
                null
             ];
        }

        public override void Unload()
        {
            Registry = null;
        }

        public static Attunement FindOrNull(int index)
        {
            if (index < 0)
                return Empty;

            if (index >= Registry.Length)
                return Empty;

            return Registry[index];
        }

        public static Attunement FindOrNull(AttunementID id)
        {
            return FindOrNull((int)id);
        }
    }
}
