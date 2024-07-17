using Newtonsoft.Json;
using System;
using System.Collections;
using System.Diagnostics;
using System.Dynamic;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace RauchTech.Logging.Services
{
    internal static class Helper
    {
        public static string ToSnakeCase(this string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return text;
            }

            StringBuilder builder = new StringBuilder(text.Length + Math.Min(2, text.Length / 5));
            UnicodeCategory? previousCategory = default;

            for (int currentIndex = 0; currentIndex < text.Length; currentIndex++)
            {
                char currentChar = text[currentIndex];
                if (currentChar == '_')
                {
                    builder.Append('_');
                    previousCategory = null;
                    continue;
                }

                UnicodeCategory currentCategory = char.GetUnicodeCategory(currentChar);
                switch (currentCategory)
                {
                    case UnicodeCategory.UppercaseLetter:
                    case UnicodeCategory.TitlecaseLetter:
                        if (previousCategory == UnicodeCategory.SpaceSeparator ||
                            previousCategory == UnicodeCategory.LowercaseLetter ||
                            previousCategory != UnicodeCategory.DecimalDigitNumber &&
                            previousCategory != null &&
                            currentIndex > 0 &&
                            currentIndex + 1 < text.Length &&
                            char.IsLower(text[currentIndex + 1]))
                        {
                            builder.Append('_');
                        }

                        currentChar = char.ToLower(currentChar, CultureInfo.InvariantCulture);
                        break;

                    case UnicodeCategory.LowercaseLetter:
                    case UnicodeCategory.DecimalDigitNumber:
                        if (previousCategory == UnicodeCategory.SpaceSeparator)
                        {
                            builder.Append('_');
                        }
                        break;

                    default:
                        if (previousCategory != null)
                        {
                            previousCategory = UnicodeCategory.SpaceSeparator;
                        }
                        continue;
                }

                builder.Append(currentChar);
                previousCategory = currentCategory;
            }

            return builder.ToString();
        }

        public static string NameOfCallingClass(bool fromExtension = false)
        {
            string fullName;
            Type declaringType;
            int skipFrames = fromExtension ? 4 : 2;
            do
            {
                MethodBase method = new StackFrame(skipFrames, false).GetMethod()!;
                declaringType = method.DeclaringType!;
                if (declaringType is null)
                {
                    return method.Name;
                }
                skipFrames++;
            }
            while (declaringType.Module.Name.Equals("mscorlib.dll", StringComparison.OrdinalIgnoreCase));

            fullName = declaringType.ReflectedType?.FullName ?? declaringType.FullName!;

            return fullName;
        }

        public static IEnumerable<(string Key, object Value)> GetIdProperties(string key, object obj, string[] keyParameters)
        {
            var json = JsonConvert.SerializeObject(obj);

            var regex = new Regex(@"""(\w+)"":\s*(?:""([^""]*)""|(\d+))", RegexOptions.Compiled);
            var matches = regex.Matches(json);

            foreach (Match match in matches)
            {
                var outputKey = $"{key}.{match.Groups[1].Value}";
                var value = match.Groups[2].Success ? match.Groups[2].Value : match.Groups[3].Value;

                if (keyParameters.Any(x => outputKey.EndsWith(x)))
                {
                    yield return (outputKey, value);
                }
            }
        }

        public static (string Key, object? Value) RemoveBannedProperties(string key, object obj, string[] bannedParameters)
        {
            if (obj is null) return (key, default);

            var json = JsonConvert.SerializeObject(obj);

            foreach (var bannedParam in bannedParameters)
            {
                var regex = new Regex($@"""{bannedParam}"":\s*(?:""[^""]*""|\d+|null|true|false|\[[^\]]*\]|\{{[^\}}]*\}})\s*,?", RegexOptions.Compiled);
                json = regex.Replace(json, string.Empty);
            }

            json = json.Trim().TrimEnd(',');

            // Clean up any trailing commas
            json = Regex.Replace(json, @",\s*}", "}");
            json = Regex.Replace(json, @",\s*]", "]");

            return (key, JsonConvert.DeserializeObject(json, obj.GetType()));
        }
    }
}