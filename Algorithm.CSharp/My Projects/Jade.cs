using QuantConnect.Brokerages;
using QuantConnect.Data.UniverseSelection;
using QuantConnect.Indicators;
using QuantConnect.Orders;
using QuantConnect.Securities;
using QuantConnect.Securities.Equity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Cache;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace QuantConnect.Algorithm.CSharp
{
    public partial class Jade : QCAlgorithm
    {
        private static Jade core;
        private bool LocalDevMode = System.Diagnostics.Debugger.IsAttached;
        private bool ShowDebug = true;
        private decimal StartingCash = 100000.00m;
        private decimal PercentRisk = 0.05m;
        private EquityExchange Market = new EquityExchange();

        private Dictionary<string, UniverseType> MyUniverse = new Dictionary<string, UniverseType>();
        private Dictionary<string, UniverseType> TopUniverse = new Dictionary<string, UniverseType>();

        private Dictionary<string, Quant> Quants = new Dictionary<string, Quant>();

        public override void Initialize()
        {
            Logger($"{this.GetType().Name} Initializing");
            core = this;

            SetBrokerageModel(BrokerageName.Alpaca, AccountType.Margin);
            SetTimeZone(TimeZones.NewYork);
            SetCash(StartingCash);
            SetStartDate(DateTime.Now.AddDays(-17).Date);
            SetEndDate(DateTime.Now.AddDays(-10).Date);

            if (LocalDevMode)
            {
                SetStartDate(2013, 10, 07);
                SetEndDate(2013, 10, 11);
            }

            UniverseSettings.Resolution = Resolution.Minute;
            AddUniverse(Universe.DollarVolume.Top(10));
            AddEquity("SPY", Resolution.Minute); // Always add SPY

            //#region SPY
            //var TestQuant = new Quant("SPY-A",
            //    q =>
            //    {
            //        if (core.MarketOpenTime(-1)) // Before First trade
            //        {
            //        }

            //        if (core.MarketOpenTime()) // First trade
            //        {
            //            q.SetHoldings("SPY", PercentRisk);
            //        }

            //        if (core.MarketIsOpen() && !core.MarketOpenTime() && !core.MarketCloseTime(-1)) // Regular trade, not the first trade, not the last trade
            //        {
            //        }

            //        if (core.MarketCloseTime(-1)) // Last trade
            //        {
            //        }

            //        if (core.MarketCloseTime()) // After Last Trade/First Close
            //        {
            //        }

            //        return true;
            //    });
            //TestQuant.Plot = true;
            //TestQuant.Live = false;
            //Quants.Add(TestQuant.Tag, TestQuant);

            //TestQuant = new Quant("SPY-B",
            //    q =>
            //    {
            //        if (core.MarketOpenTime(-1)) // Before First trade
            //        {
            //        }

            //        if (core.MarketOpenTime()) // First trade
            //        {
            //            q.SetHoldings("SPY", PercentRisk);
            //        }

            //        if (core.MarketIsOpen() && !core.MarketOpenTime() && !core.MarketCloseTime(-1)) // Regular trade, not the first trade, not the last trade
            //        {

            //        }

            //        if (core.MarketCloseTime(-1)) // Last trade
            //        {
            //            q.Liquidate();
            //        }

            //        if (core.MarketCloseTime()) // After Last Trade/First Close
            //        {
            //        }

            //        return true;
            //    });
            //TestQuant.Plot = true;
            //TestQuant.Live = false;
            //Quants.Add(TestQuant.Tag, TestQuant);
            //#endregion






            //#region Live
            //TestQuant = new Quant("Live",
            //    q =>
            //    {
            //        if (core.MarketOpenTime()) // First trade
            //        {
            //            var item = MyUniverse.Values
            //                .Where(w => w.Security.HasData)
            //                .OrderByDescending(o => o.MOMP_Daily_01)
            //                .Take(1)
            //                .SingleOrDefault();

            //            if (item != null)
            //                q.SetHoldings(item.Security.Symbol.Value, PercentRisk);
            //        }

            //        if (core.MarketCloseTime(-1)) // Last trade
            //        {
            //            q.Liquidate();
            //        }

            //        return true;
            //    });
            //TestQuant.Plot = true;
            //Quants.Add(TestQuant.Tag, TestQuant);

            //TestQuant = new Quant("Test",
            //    q =>
            //    {
            //        if (core.MarketOpenTime()) // First trade
            //        {
            //            var item = MyUniverse.Values
            //                .Where(w => w.Security.HasData)
            //                .OrderByDescending(o => o.MOMP_Daily_01)
            //                .Take(1)
            //                .SingleOrDefault();

            //            if (item != null)
            //                q.SetHoldings(item.Security.Symbol.Value, PercentRisk);
            //        }

            //        if (core.MarketCloseTime(-1)) // Last trade
            //        {
            //            q.Liquidate();
            //        }

            //        return true;
            //    });
            //TestQuant.Plot = true;
            //Quants.Add(TestQuant.Tag, TestQuant);
            //#endregion

            //#region Test Quants
            //TestQuant = new Quant("A",
            //    q =>
            //    {
            //        if (core.MarketOpenTime()) // First trade
            //        {
            //            var item = MyUniverse.Values
            //                .Where(w => w.Security.HasData)
            //                .OrderByDescending(o => o.MOMP_Daily_01)
            //                .Take(1)
            //                .SingleOrDefault();

            //            if (item != null)
            //                q.SetHoldings(item.Security.Symbol.Value, PercentRisk);
            //        }

            //        if (core.MarketCloseTime(-1)) // Last trade
            //        {
            //            q.Liquidate();
            //        }

            //        return true;
            //    });
            //TestQuant.Plot = false;
            //Quants.Add(TestQuant.Tag, TestQuant);

            //TestQuant = new Quant("B",
            //    q =>
            //    {
            //        if (core.MarketOpenTime()) // First trade
            //        {
            //            var item = MyUniverse.Values
            //                .Where(w => w.Security.HasData)
            //                .OrderBy(o => o.MOMP_Daily_01)
            //                .Take(1)
            //                .SingleOrDefault();

            //            if (item != null)
            //                q.SetHoldings(item.Security.Symbol.Value, PercentRisk);
            //        }

            //        if (core.MarketCloseTime(-1)) // Last trade
            //        {
            //            q.Liquidate();
            //        }

            //        return true;
            //    });
            //TestQuant.Plot = false;
            //Quants.Add(TestQuant.Tag, TestQuant);
            //#endregion

            var TestQuant = new Quant(
                "Test",
                Portfolio.TotalPortfolioValue,
                q =>
                {
                    if (core.MarketOpenTime()) // First trade
                    {
                        var item = MyUniverse.Values
                            .Where(w => w.Security.HasData)
                            .OrderByDescending(o => o.MOMP_Daily_05)
                            .Take(1)
                            .SingleOrDefault();

                        var random = new Random();
                        int num = random.Next(20);
                        decimal percentRisk = num / 100.00m;

                        if (item != null)
                            q.SetHoldings(item.Security.Symbol.Value, percentRisk);
                    }

                    return true;
                });
            TestQuant.Plot = true;
            TestQuant.Live = true;
            Quants.Add(TestQuant.Tag, TestQuant);

            SetWarmUp(TimeSpan.FromDays(40));

            Schedule.On(DateRules.EveryDay(), TimeRules.Every(TimeSpan.FromMinutes(1)), () =>
            {
                if (IsWarmingUp) return;
                OnTick();
            });

            // Hello ping
            Schedule.On(DateRules.EveryDay(), TimeRules.Every(TimeSpan.FromMinutes(10)), () =>
            {
                if (IsWarmingUp) return;
                if (LiveMode) Logger($"Hello");
            });

            Logger($"{this.GetType().Name} Initialized");
        }

        public void Logger(string message)
        {
            Debug($",{Time}, {message}");
        }

        private bool MarketOpenTime(int minutes = 0)
        {
            return (!Market.DateTimeIsOpen(Time.AddMinutes(-minutes - 1)) && Market.DateTimeIsOpen(Time.AddMinutes(-minutes)));
        }

        private bool MarketCloseTime(int minutes = 0)
        {
            return (Market.DateTimeIsOpen(Time.AddMinutes(-minutes - 1)) && !Market.DateTimeIsOpen(Time.AddMinutes(-minutes)));
        }

        private bool MarketIsOpen()
        {
            return (Market.DateTimeIsOpen(Time));
        }

        private void PlotCore()
        {
            if (MyUniverse.Keys.Contains("SPY"))
            {
                var spy = MyUniverse["SPY"];

                if (spy.Security.Price != 0.00m)
                {
                    Plot("SPY", "Price", spy.Security.Price);

                    //if (spy.VWAP_01 != 0) Plot("SPY VWAP", "01", spy.VWAP_01);
                    //if (spy.VWAP_02 != 0) Plot("SPY VWAP", "02", spy.VWAP_02);
                    //if (spy.VWAP_04 != 0) Plot("SPY VWAP", "04", spy.VWAP_04);
                    //if (spy.VWAP_08 != 0) Plot("SPY VWAP", "08", spy.VWAP_08);
                    //if (spy.VWAP_16 != 0) Plot("SPY VWAP", "16", spy.VWAP_16);

                    //Plot("MOMP Minute", "01", spy.MOMP_Minute_01);
                    //Plot("MOMP Minute", "02", spy.MOMP_Minute_02);
                    //Plot("MOMP Minute", "04", spy.MOMP_Minute_04);
                    //Plot("MOMP Minute", "08", spy.MOMP_Minute_08);
                    //Plot("MOMP Minute", "16", spy.MOMP_Minute_16);

                    //Plot("MOMP Daily", "01", spy.MOMP_Daily_01);
                    //Plot("MOMP Daily", "05", spy.MOMP_Daily_05);
                    //Plot("MOMP Daily", "10", spy.MOMP_Daily_10);
                    //Plot("MOMP Daily", "20", spy.MOMP_Daily_20);
                    //Plot("MOMP Daily", "40", spy.MOMP_Daily_40);
                }

                // Plot Portfolio
                // Plot("Portfolio", "TotalPortfolioValue", Portfolio.TotalPortfolioValue);



                // Validator
                Plot("Validator", "Source1", Portfolio.TotalPortfolioValue);

                var quant = Quants["Test"];
                Plot("Validator", "Test1", quant.TotalPortfolioValue);
            }
        }

        public override void OnSecuritiesChanged(SecurityChanges changes)
        {
            // Process MyUniverse First (additions fist)
            if (!MyUniverse.ContainsKey("SPY"))
            {
                if (LiveMode || LocalDevMode || ShowDebug) Logger($"My Universe added SPY");
                MyUniverse.Add("SPY", new UniverseType(Securities["SPY"]));
            }

            var securities = "";
            foreach (var security in changes.AddedSecurities.OrderBy(o => o.Symbol))
            {
                var symbol = security.Symbol.Value;
                if (!MyUniverse.ContainsKey(symbol))
                {
                    securities += (securities != "") ? "," + symbol : symbol;
                    MyUniverse.Add(symbol, new UniverseType(security));
                }
            }
            if ((LiveMode || LocalDevMode || ShowDebug) && securities != "") Logger($"My Universe added {securities}");

            //var symbols = changes.AddedSecurities.OrderBy(o => o.Symbol)
            //    .Where(w => !MyUniverse.ContainsKey(w.Symbol.Value))
            //    .ToArray();
            //foreach (var symbol in symbols)
            //    MyUniverse.Add(symbol.Symbol.Value, new UniverseType(symbol));
            //var symbolsJoined = String.Join(",", symbols.Select(s => s.Symbol.Value).ToArray());
            //if ((LiveMode || LocalDevMode || ShowDebug) && symbolsJoined != "") Logger($"My Universe added {symbolsJoined}");


            // Process TopUniverse referencing MyUniverse
            securities = "";
            foreach (var security in changes.AddedSecurities.OrderBy(o => o.Symbol))
            {
                var symbol = security.Symbol.Value;
                if (!TopUniverse.ContainsKey(symbol)) // necessary, this somehow causes runtime
                {
                    securities += (securities != "") ? "," + symbol : symbol;
                    TopUniverse.Add(symbol, MyUniverse[symbol]);
                }
            }
            if ((LiveMode || LocalDevMode || ShowDebug) && securities != "") Logger($"Top Universe added {securities}");

            securities = "";
            foreach (var security in changes.RemovedSecurities.OrderBy(o => o.Symbol))
            {
                var symbol = security.Symbol.Value;
                if (TopUniverse.ContainsKey(symbol))
                {
                    securities += (securities != "") ? "," + symbol : symbol;
                    TopUniverse.Remove(symbol);
                }
            }
            if ((LiveMode || LocalDevMode || ShowDebug) && securities != "") Logger($"Top Universe removed {securities}");

            securities = "";
            foreach (var security in TopUniverse.Keys.OrderBy(o => o))
                securities += (securities != "") ? "," + security : security;
            if ((LiveMode || LocalDevMode || ShowDebug) && securities != "") Logger($"Top Universe {securities}");



            // Remove Securities from MyUniverse (cleanup unused securities)
            securities = "";
            var removedSymbols = MyUniverse.Keys
                .Where(w => !TopUniverse.Keys.Contains(w))
                .ToArray();
            foreach (var symbol in removedSymbols)
                MyUniverse.Remove(symbol);
            var symbolsJoined = String.Join(",", removedSymbols);
            if ((LiveMode || LocalDevMode || ShowDebug) && symbolsJoined != "") Logger($"My Universe removed {symbolsJoined}");

            securities = "";
            foreach (var security in MyUniverse.Keys.OrderBy(o => o))
                securities += (securities != "") ? "," + security : security;
            if ((LiveMode || LocalDevMode || ShowDebug) && securities != "") Logger($"My Universe {securities}");

        }

        public override void OnOrderEvent(OrderEvent orderEvent)
        {
            var order = Transactions.GetOrderById(orderEvent.OrderId);

            // BUG: split tag with PIPE as sent from MarketOrder
            var tags = order.Tag.Split('|');
            var tag = tags[0];

            // var quant = Quants[tag];

            core.Logger($"Quant {tag}, OrderEvent {orderEvent.Symbol.Value}, {orderEvent.Status}, {orderEvent.FillQuantity}, {orderEvent.FillPrice}");
        }

        private void OnTick()
        {
            if (core.MarketOpenTime(-1)) // Before First trade
            {
                Logger($"Before Market Open");
            }

            if (core.MarketOpenTime()) // First trade
            {
                Logger($"First Trade");
                PlotCore();
            }

            if (core.MarketIsOpen() && !core.MarketOpenTime() && !core.MarketCloseTime(-1)) // Regular trade, not the first trade, not the last trade
            {
                PlotCore();
            }

            if (core.MarketCloseTime(-1)) // Last trade
            {
                Logger($"Last Trade");
                PlotCore();
            }

            if (core.MarketCloseTime()) // After Last Trade/First Close
            {
                Logger($"Market Close");
                PlotCore();
            }
        }
    }
}
