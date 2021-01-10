using Accord.Math;
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
        public EquityExchange Market = new EquityExchange();

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
            // SetStartDate(DateTime.Now.AddDays(-192).Date);
            SetStartDate(DateTime.Now.AddDays(-38).Date);
            // SetStartDate(DateTime.Now.AddDays(-17).Date);
            SetEndDate(DateTime.Now.AddDays(-10).Date);

            if (LocalDevMode)
            {
                SetStartDate(2013, 10, 07);
                SetEndDate(2013, 10, 11);
            }

            UniverseSettings.Resolution = Resolution.Minute;
            AddUniverse(Universe.DollarVolume.Top(10));
            AddEquity("SPY", Resolution.Minute); // Always add SPY

            QuantConfig();

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
                if (LiveMode)
                    Logger($"Hello");
            });

            Logger($"{this.GetType().Name} Initialized");
        }

        public void Logger(string message, bool forcedLog = false)
        {
            if (LiveMode || forcedLog)
                if (Time >= DateTime.Now.AddDays(-16).Date)
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
            if (LiveMode)
            {
                // Plot Portfolio
                if (Portfolio.TotalPortfolioValue > 0)
                {
                    Plot("Portfolio Value", "$", Portfolio.TotalPortfolioValue);
                    Plot("Portfolio Performance", "%", (Portfolio.TotalProfit / Portfolio.TotalPortfolioValue) * 100.00m);
                }
            }

            if (MyUniverse.Keys.Contains("SPY"))
            {
                var spy = MyUniverse["SPY"];

                if (spy.Security.Price != 0.00m)
                {
                    // Plot("SPY", "Price", spy.Security.Price);

                    //Plot("SPY VWAP", "Price", spy.Security.Price);
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

                //// Validator
                //Plot("Validator", "Source1", Portfolio.TotalPortfolioValue);

                //var quant = Quants["Test"];
                //Plot("Validator", "Test", quant.TotalPortfolioValue);
            }
        }

        private void PlotQuant()
        {
            foreach (var quant in Quants.Values)
            {
                if (quant.Plot)
                {
                    Plot("Quant Performance", quant.Tag, quant.Performance);
                    Plot("Quant WinRate", quant.Tag, quant.WinRate);
                }
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
                .Where(w => !TopUniverse.Keys.Contains(w) && Quants.Values.Where(h => h.Holdings.ContainsKey(w)).FirstOrDefault() == null)
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
                PlotQuant();
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
                PlotQuant();

                // Pick Winner
                var quantPerformances = (Quants != null && Quants.Values.Count > 0)
                    ? Quants.Values
                        .OrderByDescending(o => o.Performance)
                        .Take(3)
                    : null;

                var quantWinRates = (Quants != null && Quants.Values.Count > 0)
                    ? Quants.Values
                        .OrderByDescending(o => o.WinRate)
                        .Take(3)
                    : null;

                foreach (var quantPerformance in quantPerformances)
                    if (quantPerformance != null)
                        Logger($"Top Performance {quantPerformance.Tag}, Performance {Decimal.Round(quantPerformance.Performance, 4)}, WinRate {Decimal.Round(quantPerformance.WinRate, 4)}", true);

                foreach (var quantWinRate in quantWinRates)
                    if (quantWinRate != null)
                        Logger($"Top Winrate {quantWinRate.Tag}, WinRate {Decimal.Round(quantWinRate.WinRate, 4)}, Performance {Decimal.Round(quantWinRate.Performance, 4)}", true);
            }
        }
    }
}
