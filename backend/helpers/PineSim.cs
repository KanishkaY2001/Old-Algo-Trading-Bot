namespace TradingBot
{
    public class PineSim
    {
        public static decimal? GetPivotHighLow(Project project, bool high)
        {
            var data = project.data;
            int left = project.leftBars;
            int right = project.rightBars;
            int dataSize = data.Count;

            if (dataSize < left + right + 1)
                return null;

            int midIdx = dataSize - right - 1;
            Candle mid = data[midIdx];

            for (int i = 0; i < Math.Max(left, right); ++i)
            {
                Candle adj;
                int leftIdx = midIdx - i - 1;
                int rightIdx = midIdx + i + 1;

                if (leftIdx >= 0)
                {
                    adj = data[leftIdx];
                    if ((high && mid.high < adj.high) || (!high && mid.low > adj.low))
                        return null;
                }

                if (rightIdx < dataSize)
                {
                    adj = data[rightIdx];
                    if ((high && mid.high <= adj.high) || (!high && mid.low >= adj.low))
                        return null;
                }
            }

            if (high)
                return (mid.pivotHi = mid.high);

            return (mid.pivotLo = mid.low);
        }

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

            if (info == null || val == null)
                throw new Exception($"-- Invalid Info: '{info}', or Property: '{prop}' --");

            info.SetValue(candle, val);
        }

        public static void GetValueWhen(List<Candle> data, string cond)
        {
            
        }

        public static decimal? GetPastValue(List<Candle> data, string src, int back)
        {
            if (back >= data.Count)
                return null;

            Candle candle = data[data.Count - 1 - back];
            return (decimal?)GetPropVal(candle, src);
        }

        public static decimal? GetSma(List<Candle> data, string src, int len)
        {
            int min = Math.Min(len, data.Count);
            decimal? sum = 0;

            for (int i = 0; i < min; ++i)
            {
                sum += GetPastValue(data, src, i);
            }
            return sum / len;
        }

        public static decimal? GetEma(List<Candle> data, string src, string prop, int len)
        {
            if (data.Count < len)
                return null;

            Candle candle = data[data.Count - 1];
            decimal multi = 2m / (len + 1);
            var srcVal = GetPastValue(data, src, 0);
            var emaPrev = GetPastValue(data, prop, 1);
            decimal? calc = srcVal * multi + emaPrev * (1 - multi);
            var emaNow = emaPrev == null ? GetSma(data, src, len) : calc;
            SetPropVal(candle, prop, emaNow);

            return emaNow;
        }

        public static object? Iff(bool cond, object? _then, object? _else)
        {
            return cond ? _then : _else;
        }

        public static decimal? Iff(bool cond, decimal? _then, decimal? _else)
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