using System.Text;
using KioskBrains.Common.Contracts;

namespace KioskBrains.Waf.Helpers.Text
{
    public static class NamingCaseConverter
    {
        public static string PascalToKebabCase(this string pascalCaseName)
        {
            Assure.ArgumentNotNull(pascalCaseName, nameof(pascalCaseName));
            var kebabCaseNameBuilder = new StringBuilder();
            for (var i = 0; i < pascalCaseName.Length; i++)
            {
                var nameSymbol = pascalCaseName[i];
                if (char.IsUpper(nameSymbol))
                {
                    if (i != 0)
                    {
                        kebabCaseNameBuilder.Append("-");
                    }
                    kebabCaseNameBuilder.Append(char.ToLower(nameSymbol));
                }
                else
                {
                    kebabCaseNameBuilder.Append(nameSymbol);
                }
            }
            return kebabCaseNameBuilder.ToString();
        }
    }
}