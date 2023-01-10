using System;

namespace Natsurainko.Toolkits.Values
{
    public static class DateTimeExtension
    {
        public static DateTime ToDateTime(this long timeStamp)
        {
            var dateTime = new DateTime(1970, 1, 1, 8, 0, 0, 0, DateTimeKind.Utc);

            return dateTime.AddMilliseconds(timeStamp);
        }
    }
}
