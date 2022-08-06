namespace TradingBot
{
    public class ExitStratOpt
    {
        public bool prevStrat { get; set; } = false;
        public int sl { get; set; }
        public int rr { get; set; }
        public int trailStop { get; set; }
        public int trailOffset { get; set; }

        public int takeProfit { get; set; }
        public int stopLoss { get; set; }
        //profit= Take, loss=Stop,trail_points=trailstop*10,trail_offset=trailofset*10

        public ExitStratOpt(int _sl, int _rr, int st, int os, int tp, int stl)
        {
            sl = _sl;
            rr = _rr;
            trailStop = st;
            trailOffset = os;
            takeProfit = tp;
            stopLoss = stl;
        }
    }

    public class ExitStrat
    {
        // profit = 0.01 * 100 * 10 = $10
        // loss = 0.01 * 10 * 20 = $2
        // trail_points = 10 * 10 = 100 OR maybe $1
        // trail_offset = 
        //https://www.tradingview.com/pine-script-reference/v4/#fun_strategy{dot}exit
        public static void ApplyIndicator(Project project)
        {
            
            /*
            //@version=3
            strategy("Exit Strategy is important than Entry Strategy!", overlay=true,default_qty_value=100000,initial_capital=10000,currency=currency.USD, pyramiding=1,calc_on_every_tick=true)
            plot(ema(high,34))
            plot(ema(low,34))
            if (close>ema(high,34) and open<ema(high,34))
                strategy.entry("Buy",strategy.long)
            if(close<ema(low,34) and open>ema(low,34))
                strategy.entry("Sell",strategy.short)
            */
            //Console.Write(Helper.UnixToDate())
            
            /*
            Candle candle = project.data[project.data.Count - 1];
            var emaHigh = project.Ema("high", "high",34);
            var emaLow = project.Ema("low", "low",34);
            var prev = project.exitSOpt.prevStrat;

            string strategy = "-";
            if (!prev && candle.close > emaHigh && candle.open < emaHigh)
            {
                strategy = "Buy";
                project.exitSOpt.prevStrat = !prev;

            } else if (prev && candle.close < emaLow && candle.open > emaLow)
            {
                strategy = "Sell";
                project.exitSOpt.prevStrat = !prev;
            }    
            
            Console.WriteLine($"{Helper.UnixToDate(candle.unix)} | {strategy}");
            */

            /*
            // TP and SL///
            SL = input(defval=100.00, title="Take Profit (Pip)", type=float, step=1)
            rr= input(defval=20.00,title="Stop Loss (Pip)",type=float, step=1)
            trailstop=input(defval=10.00, title="Trailing Stop (Pip)",type=float,step=1)
            trailofset=input(defval=10.00, title="Trailing Offset (Pip)",type=float,step=1)
            useTPandSL = input(defval = true, title = "Use exit order strategy?")
            Stop = rr*10
            Take=SL*10
            Q = 100
            if(useTPandSL)
                strategy.exit("Exit Long", "Buy", qty_percent=Q, profit= Take, loss=Stop,trail_points=trailstop*10,trail_offset=trailofset*10)
                strategy.exit("Exit Short", "Sell", qty_percent=Q, profit= Take, loss=Stop,trail_points=trailstop*10,trail_offset=trailofset*10)
            */










        }
    }
}