namespace iWorkTech.Orleans.Common
{
    public static class CacheKeys
    {
        public static readonly string Entry = "_Entry";
        public static readonly string CallbackEntry = "_Callback";
        public static readonly string CallbackMessage = "_CallbackMessage";
        public static readonly string Parent = "_Parent";
        public static readonly string Child = "_Child";
        public static readonly string DependentMessage = "_DependentMessage";
        public static readonly string DependentCts = "_DependentCTS";
        public static readonly string Games = "_Games";
        public static string Ticks => "_Ticks";
        public static string CancelMsg => "_CancelMsg";
        public static string CancelTokenSource => "_CancelTokenSource";
    }
}