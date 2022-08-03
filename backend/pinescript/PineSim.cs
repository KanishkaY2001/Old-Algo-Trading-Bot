namespace TradingBot
{
    public class PineSim
    {
        public static object? GetPropVal(Candle candle, string prop)
        {
            var info = candle.GetType().GetProperty(prop);
            return info == null? null : info.GetValue(candle);
        }

        public static void SetPropVal(Candle candle, string prop, object? val)
        {
            var info = candle.GetType().GetProperty(prop);

            val = val is decimal? (decimal)val : val;
            val = val is decimal? ? (decimal?)val : val;
            val = val is string ? (string)val : null;

            if (info != null && val != null)
            {
                info.SetValue(candle, val);
                return;
            } 
            throw new Exception($"-- Invalid Info: '{info}', or Property: '{prop}' --");
        }

        public static object? Iff(bool cond, object? _then, object? _else)
        {
            return cond ? _then : _else;
        }

        public static decimal Nz(decimal? value)
        {
            return value ?? 0;
        }

        public static bool Na(object item)
        {
            return item == null;
        }
    }
}