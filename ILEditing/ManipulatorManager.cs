#nullable enable

using System;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.ILEditing;

internal readonly record struct ManipulatorContext(
    ManipulatorBatch PlayerUpdate
);

/// <summary>
///     
/// </summary>
internal sealed class ManipulatorManager : ModSystem
{
    private static bool hasAppliedEdits;
    private static event Action<ManipulatorContext>? WrappedApplyEdits;

    public static event Action<ManipulatorContext> ApplyEdits
    {
        add
        {
            if (hasAppliedEdits)
            {
                throw new InvalidOperationException("Cannot add to ApplyEdits after edits have already been applied! Call it earlier.");
            }

            WrappedApplyEdits += value;
        }

        // No reason to actually use this, but we're contractually obligated to.
        remove
        {
            WrappedApplyEdits -= value;
        }
    }

    public override void OnModLoad()
    {
        base.OnModLoad();

        hasAppliedEdits = true;

        using var playerUpdate = ManipulatorBatch.From(manipulator => IL_Player.Update += manipulator);
        var ctx = new ManipulatorContext(playerUpdate);

        WrappedApplyEdits?.Invoke(ctx);
    }
}
