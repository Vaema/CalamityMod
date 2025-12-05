using System;
using System.Collections.Generic;
using MonoMod.Cil;

namespace CalamityMod.ILEditing;

/// <summary>
///     Simple helper object to capture multiple IL edits to apply in a single
///     pass to avoid additional overhead.
/// </summary>
/// <remarks>
///     Since multiple edits are applied at once, a failure from one edit will
///     cause every edit to fail.  Use with caution.
/// </remarks>
internal readonly ref struct ManipulatorBatch() : IDisposable
{
    public required Action<ILContext.Manipulator> ManipulatorProvider { get; init; }

    private readonly List<ILContext.Manipulator> manipulators = [];

    /// <summary>
    ///     Adds a manipulator to be applied when this object is disposed.
    /// </summary>
    public void Add(ILContext.Manipulator manipulator)
    {
        manipulators.Add(manipulator);
    }

    /// <summary>
    ///     Applies the queued manipulators.
    /// </summary>
    public void Dispose()
    {
        var theManipulators = manipulators;

        ManipulatorProvider?.Invoke(
            il =>
            {
                foreach (var manipulator in theManipulators)
                {
                    manipulator?.Invoke(il);
                }
            }
        );
    }

    /// <summary>
    ///     Initializes a batch from 
    /// </summary>
    /// <param name="manipulatorProvider"></param>
    /// <returns></returns>
    public static ManipulatorBatch From(Action<ILContext.Manipulator> manipulatorProvider)
    {
        ArgumentNullException.ThrowIfNull(manipulatorProvider);

        return new ManipulatorBatch { ManipulatorProvider = manipulatorProvider };
    }
}
