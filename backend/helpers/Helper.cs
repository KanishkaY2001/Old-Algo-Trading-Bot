namespace TradingBot
{
    public class Helper
    {
        public static string UnixToDate(long unix)
        {
            return DateTimeOffset.FromUnixTimeSeconds(unix).ToLocalTime().ToString();
        }

        public static long GetUnix()
        {
            return DateTimeOffset.Now.ToUnixTimeSeconds();
        }
    }
}