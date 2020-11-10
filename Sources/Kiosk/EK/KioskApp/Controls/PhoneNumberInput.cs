using System.Linq;
using KioskBrains.Kiosk.Core.Ui.Controls;

namespace KioskApp.Controls
{
    public class PhoneNumberInput : TextInput
    {
        public static readonly int ValidPhoneNumberLength = "+38 0XX XXX XX XX".Length;

        protected override int DefaultMaxLength => ValidPhoneNumberLength;

        private const string FixedPrefix = "+38 0";

        private static readonly int[] SpaceLength =
            {
                6,
                10,
                13,
            };

        protected override void OnTextChanged(string previousValue, string currentValue)
        {
            if (currentValue == null
                || currentValue.Length < FixedPrefix.Length)
            {
                Text = FixedPrefix;
            }
        }

        public override void AddText(string textAddition)
        {
            if (Text != null
                && SpaceLength.Contains(Text.Length))
            {
                // add space
                textAddition = textAddition + " ";
            }

            base.AddText(textAddition);
        }

        public override void ProcessBackspace()
        {
            if (Text != null
                && SpaceLength.Contains(Text.Length - 2))
            {
                // additionally remove space
                Text = Text.Substring(0, Text.Length - 2);
            }
            else
            {
                base.ProcessBackspace();
            }
        }
    }
}