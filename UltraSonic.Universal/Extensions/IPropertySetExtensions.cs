using Windows.Foundation.Collections;

namespace UltraSonic.Extensions
{
    public static class PropertySetExtensions
    {
        public static T TryGetProperty<T>(this IPropertySet propertySet, string propertyName, T defaultValue)
        {
            object result;

            if (!propertySet.TryGetValue(propertyName, out result))
            {
                return defaultValue;
            }

            T returnValue;

            try
            {
                returnValue = (T)result;
            }
            catch
            {
                returnValue = defaultValue;
            }

            return returnValue;
        }
    }
}
