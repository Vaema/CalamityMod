using log4net;

namespace CalamityMod;

public static partial class CalamityUtils
{
    /// <summary>
    /// Log out formatted message for IL edit Failure
    /// <para>Format: IL edit "<u>name</u>" failed! <u>reason</u></para>
    /// </summary>
    /// <param name="logger">Logger to use</param>
    /// <param name="name">Name of the IL Edit</param>
    /// <param name="reason">Reason why this IL Edit has failed</param>
    public static void ILFailure(this ILog logger, string name, string reason)
    {
        logger.Warn($"IL edit \"{name}\" failed! {reason}");
    }
}
