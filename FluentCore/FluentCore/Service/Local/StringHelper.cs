using System.Collections.Generic;

namespace FluentCore.Service.Local
{
    public class StringHelper
    {
        public static IEnumerable<string> Replace(List<string> values, Dictionary<string, string> replaceValues)
        {
            for (int i = 0; i < values.Count; i++)
            {
                var value = values[i];

                foreach (var (k, v) in replaceValues)
                    value = value.Replace(k, v);

                yield return value;
            }
        }

        public static string Replace(string value, Dictionary<string, string> replaceValues)
        {
            foreach (var (k, v) in replaceValues)
                value = value.Replace(k, v);

            return value;
        }

    }
}
