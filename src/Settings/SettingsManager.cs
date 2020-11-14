using System.Collections.Generic;
using System.Linq;

namespace IntegratedCalc.Settings
{
    public static class SettingsManager
    {
        private static IList<ISettingsProvider> s_providers = new List<ISettingsProvider>();
        public static IEnumerable<ISettingsProvider> Providers => s_providers;
        public static int Count => s_providers.Count;

        public static bool Add<T>(SettingsProvider<T> item) where T : new()
        {
            if (s_providers.Any(x => x is SettingsProvider<T>))
                return false;
            s_providers.Add(item);
            return true;
        }

        public static SettingsProvider<T> Get<T>() where T : new() => s_providers.First(x => x is SettingsProvider<T>) as SettingsProvider<T>;

        public static void Clear() => s_providers.Clear();
        public static bool Contains(ISettingsProvider item) => s_providers.Contains(item);
        public static void CopyTo(ISettingsProvider[] array, int arrayIndex) => s_providers.CopyTo(array, arrayIndex);
        public static bool Remove<T>(SettingsProvider<T> item) where T : new()
        {
            return s_providers.Remove(item);
        }
    }
}
