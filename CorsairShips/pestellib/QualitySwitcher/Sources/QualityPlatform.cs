using System;

namespace PestelLib.QualitySwitcher
{
    [Flags]
    public enum QualityPlatform
    {
        Editor      = 1 << 0,
        Standalone  = 1 << 1,
        iOS         = 1 << 2,
        Android     = 1 << 3,
    }
}