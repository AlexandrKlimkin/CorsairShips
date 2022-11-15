namespace S
{
    // нельзя обфусцировать используется как JSON
    [System.Reflection.Obfuscation(Exclude = true)]
    public class HashMismatchInfo
    {
        public string Commands;
        public string Client;
        public string Server;
    }
}