using System.Text.RegularExpressions;

namespace TradingBot
{
    public class HighLow
    {
        private static volatile Dictionary<string, Regex> buyPatterns = new Dictionary<string, Regex>
        {
            { "e1" , new Regex("(ll )(- )*(lh )+((- )|(lh ))*(hl$)", RegexOptions.Compiled) }, /* LL -> LH -> HL */
            { "e2" , new Regex("(hl )(- )*(lh )+((- )|(lh ))*(hl$)", RegexOptions.Compiled) }, /* HL -> LH -L HL */
            { "e3" , new Regex("(ll )(- )*(hh )+((- )|(hh ))*(hl$)", RegexOptions.Compiled) }  /* LL -> HH -> HL */
        };

        /* Sell Patterns */
        private static volatile Dictionary<string, Regex> sellPatterns = new Dictionary<string, Regex>
        {
            { "x1" , new Regex("ll$", RegexOptions.Compiled) }, /* LL */
        };

        public static void ApplyIndicator(Project p)
        {
            int idx = p.data.Count - 1;
            if (idx < 0 || idx < p.rightBars + p.leftBars)
                return;

            Candle candle = p.data[idx];
            Candle target = p.data[idx - p.rightBars];

            decimal? ph = p.PivotHigh();
            decimal? pl = p.PivotLow();
            
            /*
            target.hl = PineSim.Iff(ph != null, 1, PineSim.Iff(pl != null, -1, null));
            target.zz = PineSim.Iff(ph != null, ph, PineSim.Iff(pl != null, pl, null));
            target.zz = PineSim.Iff(pl != null && target.hl== -1 && p.prevHl == -1 && pl > p.prevZz, null, target.zz);
            target.zz = PineSim.Iff(ph != null && target.hl== 1 && p.prevHl == 1  && ph < p.prevZz, null, target.zz);
            
            target.hl= PineSim.Iff(target.hl== -1 && p.prevHl == 1 && target.zz > p.prevZz, null, target.hl);
            target.hl= PineSim.Iff(target.hl== 1 && p.prevHl == -1 && target.zz < p.prevZz, null, target.hl);
            
            target.zz = PineSim.Iff(target.hl== null, null, target.zz);

            if (target.hl != null) p.prevHl = target.hl;
            if (target.zz != null) p.prevZz = target.zz;
            
            decimal[] previous = FindPrevious(project, target, PineScript.Iff(target.hlc.hl == 1, -1, 1));
            */

            Console.WriteLine($"unix: {Helper.UnixToDate(target.unix)} | pivotHigh: {ph}        | pivotLow: {pl} ");
        }
    }
}