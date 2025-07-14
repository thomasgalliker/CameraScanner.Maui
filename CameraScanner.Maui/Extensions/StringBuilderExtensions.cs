using System.Text;

namespace CameraScanner.Maui.Extensions
{
    internal static class StringBuilderExtensions
    {
        internal static void AppendIfNotNullOrEmpty(this StringBuilder stringBuilder, string value)
        {
            if (value != null)
            {
                stringBuilder.Append('\n');
                stringBuilder.Append(value);
            }
        }

        internal static void AppendIfNotNullOrEmpty(this StringBuilder stringBuilder, string[] values)
        {
            if (values != null)
            {
                foreach (var s in values)
                {
                    stringBuilder.Append('\n');
                    stringBuilder.Append(s);
                }
            }
        }
    }
}