using Newtonsoft.Json;

namespace RethinkDb.Newtonsoft
{
    internal class Demand
    {
        public static void Require(bool assertion, string message)
        {
            if (!assertion)
            {
                throw new JsonException(message);
            }
        }

        public static void Require(bool assertion, string format, params object[] param)
        {
            Require(assertion, string.Format(format, param));
        }
    }
}