public static partial class Extensions {
    public static string RemoveClonePostfix(string name) {
        return name.Replace("(Clone)", "");
    }
}