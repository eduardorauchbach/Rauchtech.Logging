using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Text;

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
                MethodBase method = new StackFrame(skipFrames, false).GetMethod();
                declaringType = method.DeclaringType;
                if (declaringType is null)
                {
                    return method.Name;
                }
                skipFrames++;
            }
            while (declaringType.Module.Name.Equals("mscorlib.dll", StringComparison.OrdinalIgnoreCase));

            fullName = declaringType.ReflectedType?.FullName ?? declaringType.FullName;

            return fullName;
        }

        public static IEnumerable<(string Key, object Value)> GetIdProperties(string key, object obj)
        {
            if (obj is null)
                yield break;

            Type type = obj.GetType();
            // Directly return the value if it's a simple type or a specific case
            if (type.IsPrimitive || type == typeof(string) || type == typeof(Guid) || type.IsEnum)
            {
                if (key.EndsWith("Id") || key == "id")
                    yield return (key, obj);
            }
            else
            {
                // Iterate through all properties of the object
                foreach (PropertyInfo propInfo in type.GetProperties())
                {
                    string propName = propInfo.Name;
                    object propValue = propInfo.GetValue(obj, null);

                    if (propName == "Id" || propName.EndsWith("Id"))
                    {
                        yield return ($"{key}.{propName}", propValue);
                    }
                }
            }
        }
    }
}