namespace PestelLib.ChatCommon
{
    // нельзя обфусцировать используется как JSON
    [System.Reflection.Obfuscation(Exclude = true)]
    public enum BanReason
    {
        None,
        Flood,
        BadWord,
        UserReported,
        AdminBan,
        RichText,
        Links,
    }
}
