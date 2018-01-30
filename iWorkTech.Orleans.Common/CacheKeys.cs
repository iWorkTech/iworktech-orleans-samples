namespace iWorkTech.Orleans.Common
{
    public static class CacheKeys
    {
        public static readonly string ENTRY = "_Entry";
        public static readonly string CALLBACK_ENTRY = "_Callback";
        public static readonly string CALLBACK_MESSAGE = "_CallbackMessage";
        public static readonly string PARENT = "_Parent";
        public static readonly string CHILD = "_Child";
        public static readonly string DEPENDENT_MESSAGE = "_DependentMessage";
        public static readonly string DEPENDENT_CTS = "_DependentCTS";
        public static readonly string GAMES = "_Games";
        public static string Ticks => "_Ticks";
        public static string CancelMsg => "_CancelMsg";
        public static string CancelTokenSource => "_CancelTokenSource";
    }
}