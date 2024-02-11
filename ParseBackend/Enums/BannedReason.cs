using System.ComponentModel;

namespace ParseBackend.Enums
{
    public enum BannedReason
    {
        [Description("None")] None = -1,

        [Description("Exploiting")] Exploiting,
    }
}
