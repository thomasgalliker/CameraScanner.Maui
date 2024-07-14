namespace CameraDemoApp.Extensions
{
    public static class EnumerableExtensions
    {
        public static T Next<T>(this T enumValue) where T : struct
        {
            if (!typeof(T).IsEnum)
            {
                throw new ArgumentException($"Parameter <T> ({typeof(T).FullName}) is not an Enum");
            }

            var arrayOfEnumValues = (T[])Enum.GetValues(enumValue.GetType());
            var arrayIndex = Array.IndexOf(arrayOfEnumValues, enumValue) + 1;
            return arrayOfEnumValues.Length == arrayIndex ? arrayOfEnumValues[0] : arrayOfEnumValues[arrayIndex];
        }
    }
}