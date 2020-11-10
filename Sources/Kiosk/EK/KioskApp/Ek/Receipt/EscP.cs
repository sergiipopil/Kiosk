namespace KioskApp.Ek.Receipt
{
    public static class EscP
    {
        public const string INIT = "\x1B\x40";
        public const string SET_A_MODE = "\x1B\x21\x00";
        public const string SET_B_MODE = "\x1B\x21\x01";
        public const string SET_LEFT_MARGIN = "\x1D\x4C\x12\x00";
        public const string ALIGN_LEFT = "\x1B\x61\x00";
        public const string ALIGN_CENTER = "\x1B\x61\x01";
        public const string ALIGN_RIGHT = "\x1B\x61\x02";
        public const string CLEAR_BOLD = "\x1B\x45\x00";
        public const string SET_BOLD = "\x1B\x45\x01";
        public const string TOTAL_CUT = "\x1B\x69";
        public const string FORM_FEED = "\x0C";
    }
}