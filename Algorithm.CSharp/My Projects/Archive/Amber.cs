// This uses child quants
// Use 3 EMA Indicators
//      Price Line 2
//      Sell Line 4-26 Increment by 2
//      Buy Line 4-26 Increment by 2


// Option 1: Use fixed EMA parameters, find which is the best overall
//      Verdict:
// Option 2: In the beginning of the day, use the EMA with the best overall performance
//      Verdict:
// Option 3: In the beginning of the day, use the EMA with the best overall daily performance from yesterday
//      Verdict:
// Option 4: Dynamically update the EMA with the best overall performer
//      Verdict:
// Option 5: Dynamically update the EMA with the best overall daily performer
//      Verdict:


// TODO: Store indicators in separate collection



using Accord.MachineLearning;
using QuantConnect.Brokerages;
using QuantConnect.Data;
using QuantConnect.Data.Fundamental;
using QuantConnect.Data.UniverseSelection;
using QuantConnect.Indicators;
using QuantConnect.Orders;
using QuantConnect.Securities;
using QuantConnect.Securities.Equity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace QuantConnect.Algorithm.CSharp
{
    public class Amber : QCAlgorithm
    {
        private Amber self;
        private EquityExchange Market = new EquityExchange();
        private Dictionary<string, Quant> Quants = new Dictionary<string, Quant>();
        public Security Security;
        private Quant LiveQuant = null;
        private Dictionary<int, ExponentialMovingAverage> Indicators = new Dictionary<int, ExponentialMovingAverage>();

        private bool isTradingTime
        {
            get
            {
                var result = Market.DateTimeIsOpen(Time.AddMinutes(-1)) && Market.DateTimeIsOpen(Time) && Market.DateTimeIsOpen(Time.AddMinutes(1));
                return result;
            }
        }

        public override void Initialize()
        {
            // Logger("Initialize");

            self = this;

            SetBrokerageModel(BrokerageName.Alpaca, AccountType.Margin);
            SetTimeZone(TimeZones.NewYork);
            SetStartDate(DateTime.Now.AddDays(-28).Date);
            // SetEndDate(DateTime.Now.AddDays(-6).Date);
            SetCash(1000000);

            UniverseSettings.Resolution = Resolution.Minute;
            Security = AddEquity("SPY", Resolution.Minute);

            // Create indicators to be used by the quants
            for (var i = 2; i <= 30; i += 2)
            {
                var ema = EMA(Security.Symbol, i, Resolution.Minute);
                Indicators.Add(i, ema);
            }

            // Create Quants
            for (int signalLine = 4; signalLine <= 30; signalLine += 2)
            {
                var q = new Quant(this, Indicators[2], Indicators[signalLine]);
                q.Tag = $"{signalLine}".PadLeft(2, '0');

                Quants.Add(q.Tag, q);
            }

            // Create live quant
            //LiveQuant = new Quant(this, Indicators[2], Indicators[10], Indicators[28]);
            //LiveQuant.Tag = $"(10/28)";
            //LiveQuant.Live = true;

            // SetWarmUp(30);

            Schedule.On(DateRules.EveryDay(), TimeRules.Every(TimeSpan.FromMinutes(1)), () =>
            {
                if (Time.DayOfWeek == DayOfWeek.Saturday || Time.DayOfWeek == DayOfWeek.Sunday) return;

                if (isTradingTime) OnTrading();
                if (!isTradingTime) OffTrading();
                OnTickMinute();
            });

            Schedule.On(DateRules.EveryDay(), TimeRules.At(1, 0), () =>
            {
                if (Time.DayOfWeek == DayOfWeek.Saturday || Time.DayOfWeek == DayOfWeek.Sunday) return;

                OnDayBegin();
            });

            Schedule.On(DateRules.EveryDay(), TimeRules.At(23, 0), () =>
             {
                 if (Time.DayOfWeek == DayOfWeek.Saturday || Time.DayOfWeek == DayOfWeek.Sunday) return;

                 OnDayEnd();
             });

        }

        private void OnTickMinute()
        {
            //if (Security.Price != 0)
            //    Plot($"Result", "Price", Security.Price);

            //Plot($"Portfolio", "TotalProfit", Portfolio.TotalProfit);

            //// Adjust threshholds to best performer
            //var winnerDailyNetProfit = Quants.Values
            //    .OrderByDescending(o => o.DailyNetProfit)
            //    .Take(1)
            //    .SingleOrDefault();

            //if (winnerDailyNetProfit.TotalOrders > 0)
            //{
            //    LiveQuant.BuyThreshhold = winnerDailyNetProfit.BuyThreshhold;
            //    LiveQuant.SellThreshhold = winnerDailyNetProfit.SellThreshhold;
            //}
        }

        private void OnTrading()
        {
            foreach (var quant in Quants.Values)
                quant.OnTrading();

            if (LiveQuant != null)
                LiveQuant.OnTrading();

            // Plot($"Portfolio", "TotalProfit", Portfolio.TotalProfit);
        }

        private void OffTrading()
        {
            foreach (var quant in Quants.Values)
                quant.OffTrading();

            if (LiveQuant != null)
                LiveQuant.OffTrading();
        }

        private void OnDayBegin()
        {
            foreach (var quant in Quants.Values)
                quant.OnDayBegin();

            if (LiveQuant != null)
                LiveQuant.OnDayBegin();
        }

        private void OnDayEnd()
        {
            //if (Time.DayOfWeek == DayOfWeek.Saturday || Time.DayOfWeek == DayOfWeek.Sunday) return;

            foreach (var quant in Quants.Values)
                quant.OnDayEnd();

            if (LiveQuant != null)
                LiveQuant.OnDayEnd();

            ////Plotter("Result", "Benchmark", Security.Price);

            //foreach (var quant in Quants.Values)
            //{
            //    Plotter($"Profit", $"{quant.Tag}", quant.NetProfit);
            //}

            ////foreach (var quant in Quants.Values)
            ////{
            ////    Logger($"AfterMarketClose,Tag,{quant.Tag}, Win Rate,{quant.WinRate},Net Profit,{quant.NetProfit}", true);
            ////}

            // log the winner
            var winnerNetProfit = Quants.Values
                .OrderByDescending(o => o.NetProfit)
                .Take(1)
                .SingleOrDefault();

            Logger($"Winner Net Profit,Tag,{winnerNetProfit.Tag},Win Rate,{winnerNetProfit.WinRate},Net Profit,{winnerNetProfit.NetProfit}", true);

            // Let's select overall winner
            //if (winnerNetProfit.DailyTotalOrders > 0)
            //{
            //    if (LiveQuant != null) LiveQuant.Live = false;
            //    LiveQuant = winnerNetProfit;
            //    LiveQuant.Live = true;
            //}

            var winnerDailyNetProfit = Quants.Values
                .OrderByDescending(o => o.DailyNetProfit)
                .Take(1)
                .SingleOrDefault();

            Logger($"Winner Daily Net Profit,Tag,{winnerDailyNetProfit.Tag},Win Rate,{winnerDailyNetProfit.DailyWinRate},Net Profit,{winnerDailyNetProfit.DailyNetProfit}", true);

            // Let's select daily winner
            //if (winnerDailyNetProfit.DailyTotalOrders > 0)
            //{
            //    if (LiveQuant != null) LiveQuant.Live = false;
            //    LiveQuant = winnerDailyNetProfit;
            //    LiveQuant.Live = true;
            //}

            //if (LiveQuant != null) LiveQuant.Live = false;
            //LiveQuant = Quants.FirstOrDefault().Value;
            //LiveQuant.Live = true;


            ////Plotter("Net Profit", "Interval", winnerNet.AtrInterval);
            ////Plotter("Net Profit", "Buy", winnerNet.BuyThreshhold);
            ////Plotter("Net Profit", "Sell", winnerNet.SellThreshhold);
            ////Plotter("Net Profit", "Profit", winnerNet.NetProfit);
            ////Plotter("Net Profit", "Win", winnerNet.WinRate);

            //var winnerWinRate = Quants.Values
            //    .OrderByDescending(o => o.WinRate)
            //    .Take(1)
            //    .SingleOrDefault();

            //Logger($"Winner Win Rate,Tag,{winnerWinRate.Tag}, Win Rate,{winnerWinRate.WinRate},Net Profit,{winnerWinRate.NetProfit}", true);
            ////Plotter("Win Rate", "Interval", winnerWinRate.AtrInterval);
            ////Plotter("Win Rate", "Buy", winnerWinRate.BuyThreshhold);
            ////Plotter("Win Rate", "Sell", winnerWinRate.SellThreshhold);
            ////Plotter("Win Rate", "Profit", winnerWinRate.NetProfit);
            ////Plotter("Win Rate", "Win", winnerWinRate.WinRate);
        }

        #region Logger
        public void Logger(string message, bool force = false)
        {
            if (!LiveMode && !force) return;
            Log($",{Time},{message}");
        }
        #endregion

        #region Plottter
        private Dictionary<string, decimal> _plotPoints = new Dictionary<string, decimal>();
        public void Plotter(string chart, string series, decimal value)
        {
            //var key = $"{chart}-{series}";

            //if (_plotPoints.Keys.Contains(key))
            //{
            //    var storedValue = _plotPoints[key];
            //    if (storedValue != value)
            //    {
            //        _plotPoints[key] = value;
            //        Plot(chart, series, value);
            //    }
            //}
            //else
            //{
            //    _plotPoints.Add(key, value);
            //    Plot(chart, series, value);
            //}

            Plot(chart, series, value);
        }
        #endregion

        #region Quant
        private class Quant
        {
            private Amber _parent = null;
            private decimal _buyPrice, _sellPrice;

            public Quant(Amber parent, ExponentialMovingAverage priceLine, ExponentialMovingAverage signalLine)
            {
                _parent = parent;
                //var buyLineString = $"{buyLine}".PadLeft(3, '0');
                //var sellLineString = $"{sellLine}".PadLeft(3, '0');
                //Tag = $"({buyLineString}/{sellLineString})";

                PriceLine = priceLine;
                SignalLine = signalLine;

                Invested = false;
                NetProfit = 0;
                TotalOrders = 0;
                TotalWins = 0;
                DailyNetProfit = 0;
                DailyTotalOrders = 0;
                DailyTotalWins = 0;
                Live = false;
            }

            public string Tag { get; set; }

            public ExponentialMovingAverage PriceLine { get; set; }

            public ExponentialMovingAverage SignalLine { get; set; }

            public bool Invested { get; set; }

            public bool Live { get; set; }

            public decimal NetProfit { get; set; }

            public int TotalOrders { get; set; }

            public int TotalWins { get; set; }

            public decimal WinRate
            {
                get
                {
                    if (TotalOrders == 0) return 0;
                    return ((TotalWins * 100.00m) / (TotalOrders * 100.00m));
                }
            }

            public decimal DailyNetProfit { get; set; }

            public int DailyTotalOrders { get; set; }

            public int DailyTotalWins { get; set; }

            public decimal DailyWinRate
            {
                get
                {
                    if (DailyTotalOrders == 0) return 0;
                    return ((DailyTotalWins * 100.00m) / (DailyTotalOrders * 100.00m));
                }
            }

            private void BuyLogic()
            {
                Invested = true;
                _buyPrice = _parent.Security.Price;

                if (Live)
                    _parent.MarketOrder(_parent.Security.Symbol, 1);
            }

            private void SellLogic()
            {
                Invested = false;
                _sellPrice = _parent.Security.Price;

                TotalOrders++;
                NetProfit += (_sellPrice - _buyPrice);
                TotalWins += (_sellPrice > _buyPrice) ? 1 : 0;

                DailyTotalOrders++;
                DailyNetProfit += (_sellPrice - _buyPrice);
                DailyTotalWins += (_sellPrice > _buyPrice) ? 1 : 0;

                if (Live)
                    _parent.MarketOrder(_parent.Security.Symbol, -1);
            }

            public void OnTrading()
            {
                if (!Invested)
                {
                    // Do we buy?
                    if (PriceLine > SignalLine)
                    {
                        BuyLogic();
                    }
                }
                else
                {
                    // Do we sell?
                    if (PriceLine < SignalLine)
                    {
                        SellLogic();
                    }
                }

                // Plot
                if (Live)
                {
                    //    _parent.Plot($"Portfolio", "TotalProfitBot", NetProfit);
                    _parent.Plot($"Result", "PriceLine", PriceLine);
                    _parent.Plot($"Result", "SignalLine", SignalLine);
                }

            }

            public void OffTrading()
            {
                if (Invested)
                {
                    SellLogic();
                }
            }

            public void OnDayBegin()
            {
                Invested = false;

                DailyNetProfit = 0;
                DailyTotalOrders = 0;
                DailyTotalWins = 0;
            }

            public void OnDayEnd()
            {
            }
        }
        #endregion

    }
}