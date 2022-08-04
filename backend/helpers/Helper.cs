namespace TradingBot
{
    public class Helper
    {
        public static string UnixToDate(long unix)
        {
            return DateTimeOffset.FromUnixTimeSeconds(unix).ToLocalTime().ToString();
        }
    }
}