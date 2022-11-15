using System;

namespace ServerShared.Messaging
{
    // нельзя обфусцировать используется как JSON
    [System.Reflection.Obfuscation(Exclude = true)]
    public enum UnitySystemLanguage
    {
        Afrikaans = 0,
        Arabic = 1,
        Basque = 2,
        Belarusian = 3,
        Bulgarian = 4,
        Catalan = 5,
        Chinese = 6,
        Czech = 7,
        Danish = 8,
        Dutch = 9,
        English = 10, // 0x0000000A
        Estonian = 11, // 0x0000000B
        Faroese = 12, // 0x0000000C
        Finnish = 13, // 0x0000000D
        French = 14, // 0x0000000E
        German = 15, // 0x0000000F
        Greek = 16, // 0x00000010
        Hebrew = 17, // 0x00000011
        [Obsolete("Use SystemLanguage.Hungarian instead (UnityUpgradable) -> Hungarian", true)] Hugarian = 18, // 0x00000012
        Hungarian = 18, // 0x00000012
        Icelandic = 19, // 0x00000013
        Indonesian = 20, // 0x00000014
        Italian = 21, // 0x00000015
        Japanese = 22, // 0x00000016
        Korean = 23, // 0x00000017
        Latvian = 24, // 0x00000018
        Lithuanian = 25, // 0x00000019
        Norwegian = 26, // 0x0000001A
        Polish = 27, // 0x0000001B
        Portuguese = 28, // 0x0000001C
        Romanian = 29, // 0x0000001D
        Russian = 30, // 0x0000001E
        SerboCroatian = 31, // 0x0000001F
        Slovak = 32, // 0x00000020
        Slovenian = 33, // 0x00000021
        Spanish = 34, // 0x00000022
        Swedish = 35, // 0x00000023
        Thai = 36, // 0x00000024
        Turkish = 37, // 0x00000025
        Ukrainian = 38, // 0x00000026
        Vietnamese = 39, // 0x00000027
        ChineseSimplified = 40, // 0x00000028
        ChineseTraditional = 41, // 0x00000029
        Unknown = 42, // 0x0000002A
    }
}
