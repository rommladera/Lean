using QuantConnect.Brokerages;
using QuantConnect.Data;
using QuantConnect.Data.UniverseSelection;
using QuantConnect.Indicators;
using QuantConnect.Orders;
using QuantConnect.Securities;
using QuantConnect.Securities.Equity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace QuantConnect.Algorithm.CSharp
{
    public class Rose : QCAlgorithm
    {
        private const int _universeSize = 50;
        private const int _rsiInterval = 10, _rsiLimitMin = 30, _rsiLimitMax = 50;
        private const int _mompInterval = 8, _atrInterval = 8, _smaInterval = 2;
        private const decimal _holdingPercentage = 1.00m;
        private const decimal _threshholdBuy = 2.000m, _threshholdSell = 3.000m;
        private EquityExchange Market = new EquityExchange();
        private Dictionary<Symbol, MyUniverseType> MyUniverse = new Dictionary<Symbol, MyUniverseType>();
        private MyUniverseType MyUniverseInvested = null;

        public override void Initialize()
        {
            // Debug("=========================================================");
            // var now = DateTime.Now;
            // Debug("Initialize: " + now);

            //Brokerage model and account type:
            SetBrokerageModel(BrokerageName.Alpaca, AccountType.Margin);

            SetTimeZone(TimeZones.NewYork);

            SetStartDate(DateTime.Now.AddMonths(-1));
            SetCash(100000);

            UniverseSettings.Resolution = Resolution.Minute;
            AddUniverse(Universe.DollarVolume.Top(_universeSize));
        }

        public override void OnSecuritiesChanged(SecurityChanges changes)
        {
            // Debug("=========================================================");
            // Debug($"{Time} SecurityChanges: {changes}");

            // Loop through securities removed from the universe
            foreach (var security in changes.RemovedSecurities)
            {
                if (MyUniverse.ContainsKey(security.Symbol))
                    MyUniverse.Remove(security.Symbol);

                // Debug($"Indicator deleted for {security.Symbol}");
            }

            foreach (var security in changes.AddedSecurities)
            {
                var myUniverse = new MyUniverseType();
                myUniverse.Security = security;

                myUniverse.RSI = RSI(security.Symbol, _rsiInterval, MovingAverageType.Simple, Resolution.Minute);
                var histRSI = History(security.Symbol, _rsiInterval, Resolution.Minute);
                foreach (var bar in histRSI)
                    myUniverse.RSI.Update(bar.EndTime, bar.Close);

                myUniverse.MOMP = MOMP(security.Symbol, _mompInterval, Resolution.Minute);
                var histMOMP = History(security.Symbol, _mompInterval, Resolution.Minute);
                foreach (var bar in histMOMP)
                    myUniverse.MOMP.Update(bar.EndTime, bar.Close);

                myUniverse.ATR = ATR(security.Symbol, _atrInterval, MovingAverageType.Simple, Resolution.Minute);
                var histATR = History(security.Symbol, _atrInterval, Resolution.Minute);
                foreach (var bar in histATR)
                    myUniverse.ATR.Update(bar);

                myUniverse.SMA = SMA(security.Symbol, _smaInterval, Resolution.Minute);
                var histSMA = History(security.Symbol, _smaInterval, Resolution.Minute);
                foreach (var bar in histSMA)
                    myUniverse.SMA.Update(bar.EndTime, bar.Close);

                // Add the indicator to the dictionary, keyed by the security's Symbol
                MyUniverse.Add(security.Symbol, myUniverse);

                // Debug($"Indicator created for {security.Symbol}");
            }
        }

        public override void OnData(Slice data)
        {
            // Debug("=========================================================");
            // Debug($"{Time} Symbol: {data}");

            // Check if indicators are ready
            foreach (var myUniverse in MyUniverse)
                if (!myUniverse.Value.RSI.IsReady
                    || !myUniverse.Value.MOMP.IsReady
                    || !myUniverse.Value.ATR.IsReady
                    || !myUniverse.Value.SMA.IsReady)
                    return;

            // Market Open transactions
            var isMarketOpen = Market.DateTimeIsOpen(Time.AddMinutes(-15)) && Market.DateTimeIsOpen(Time) && Market.DateTimeIsOpen(Time.AddMinutes(15));

            if (isMarketOpen)
            {
                foreach (var myUniverse in MyUniverse)
                    myUniverse.Value.Update();

                if (!Portfolio.Invested)
                {
                    // Buy logic
                    var filter = MyUniverse
                        .Where(w => w.Value.JustBrokeOut)
                        .OrderByDescending(o => o.Value.Security.Price)
                        .Take(1)
                        .ToList();

                    if (filter.Count > 0)
                    {
                        MyUniverseInvested = filter[0].Value;
                        // Debug($"{Time} Top Symbol: {MyUniverseInvested.Security.Symbol}, {MyUniverseInvested.RSI}");

                        SetHoldings(MyUniverseInvested.Security.Symbol, _holdingPercentage);

                        Plot("Result", "SMA", MyUniverseInvested.SMA);
                        Plot("Result", "Signal", MyUniverseInvested.Signal);
                    }
                    //else
                    //{
                    //    // Debug($"{Time} No Symbol Selected");
                    //}
                }
                else
                {
                    Plot("Result", "SMA", MyUniverseInvested.SMA);
                    Plot("Result", "Signal", MyUniverseInvested.Signal);

                    // Sell logic
                    if (MyUniverseInvested.Signal > MyUniverseInvested.SMA)
                    {
                        SetHoldings(MyUniverseInvested.Security.Symbol, 0.00);
                        MyUniverseInvested = null;
                    }
                }

                // MyUniverseInvested = MyUniverse.Values.First();

                //Plot("Result", "SMA", MyUniverseInvested.SMA);
                //Plot("Result", "Signal", MyUniverseInvested.Signal);

                //Plot("Breakout", "Breakout", (MyUniverseInvested.JustBrokeOut) ? 1 : 0);

                Plot("Invested", "Invested", (Portfolio.Invested) ? 1 : 0);
            }

            if (!isMarketOpen && Portfolio.Invested)
            {
                //Debug("=========================================================");
                //Debug($"{Time} End of Day Liquidate");
                //Liquidate();
            }
        }

        public override void OnOrderEvent(OrderEvent orderEvent)
        {
            //Debug("=========================================================");
            //Debug($"{Time} OnOrderEvent");
            //Debug($"{Time} OnOrderEvent orderEvent = {orderEvent}");
            //var order = Transactions.GetOrderById(orderEvent.OrderId);
            //Debug($"{Time} OnOrderEvent order = {order}");
        }
    }

    public class MyUniverseType
    {
        private const decimal _threshholdBuy = 2.000m, _threshholdSell = 2.000m;

        public MyUniverseType()
        {
            Signal = 9999m;
            Increasing = false;
            JustBrokeOut = false;
        }

        public Security Security { get; set; }
        public RelativeStrengthIndex RSI { get; set; }
        public MomentumPercent MOMP { get; set; }
        public AverageTrueRange ATR { get; set; }
        public SimpleMovingAverage SMA { get; set; }
        public Decimal Signal { get; set; }
        public bool Increasing { get; set; }
        public bool JustBrokeOut { get; set; }

        //public decimal ATRP
        //{
        //    get
        //    {
        //        var price = Security.Close;

        //        return 0.00m;
        //    }

        //}

        public void Update()
        {
            JustBrokeOut = false;

            var price = SMA;
            // Check if passed signal
            if (Increasing)
            {
                if (price < Signal)
                {
                    Increasing = false;
                }
            }
            else
            {
                if (price > Signal)
                {
                    Increasing = true;
                    JustBrokeOut = true;
                }
            }

            // Calculate Signal
            if (Increasing)
            {
                Signal = Math.Max(Signal, SMA - (ATR * _threshholdSell));

            }
            else
            {
                Signal = Math.Min(Signal, SMA + (ATR * _threshholdBuy));
            }
        }
    }
}
