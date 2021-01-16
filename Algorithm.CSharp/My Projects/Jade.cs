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
        private UniverseType SPY = null;

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
            // SetEndDate(DateTime.Now.AddDays(-32).Date); // RCL
            SetEndDate(DateTime.Now.AddDays(-10).Date);


            if (LocalDevMode)
            {
                SetStartDate(2013, 10, 07);
                SetEndDate(2013, 10, 11);
            }

            UniverseSettings.Resolution = Resolution.Minute;
            AddUniverse(Universe.DollarVolume.Top(10));

            AddEquity("SPY", Resolution.Minute); // Always add SPY
            SPY = new UniverseType(Securities["SPY"]);
            MyUniverse.Add("SPY", SPY);

            QuantConfig();

            SetWarmUp(TimeSpan.FromDays(50));

            // Every Minute
            Schedule.On(DateRules.EveryDay(), TimeRules.Every(TimeSpan.FromMinutes(1)), () =>
            {
                if (IsWarmingUp) return;
                OnTick();
            });

            // Every 10 minutes
            Schedule.On(DateRules.EveryDay(), TimeRules.Every(TimeSpan.FromMinutes(10)), () =>
            {
                if (IsWarmingUp) return;
                OnTick10();
            });

            Logger($"{this.GetType().Name} Initialized");
        }

        public void Logger(string message, bool forcedLog = false)
        {
            if (LiveMode || forcedLog)
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

        private void LogIndicatorAndHistory()
        {
            var hist = core.History(SPY.Security.Symbol, 5, Resolution.Minute);
            foreach (var bar in hist)
                Logger($"History Minute,{bar.EndTime},{Decimal.Round(bar.Open, 4)},{Decimal.Round(bar.High, 4)},{Decimal.Round(bar.Low, 4)},{Decimal.Round(bar.Close, 4)},{Decimal.Round(bar.Volume, 4)}");

            hist = core.History(SPY.Security.Symbol, 5, Resolution.Daily);
            foreach (var bar in hist)
                Logger($"History Daily,{bar.EndTime},{Decimal.Round(bar.Open, 4)},{Decimal.Round(bar.High, 4)},{Decimal.Round(bar.Low, 4)},{Decimal.Round(bar.Close, 4)},{Decimal.Round(bar.Volume, 4)}");

            Logger($"SPY,{SPY.Security.LocalTime},{Decimal.Round(SPY.Security.Open, 4)},{Decimal.Round(SPY.Security.High, 4)},{Decimal.Round(SPY.Security.Low, 4)},{Decimal.Round(SPY.Security.Close, 4)},{Decimal.Round(SPY.Security.Volume, 4)}");

            Logger($"MOMP Daily,{Decimal.Round(SPY.MOMP_Daily_01, 4)},{Decimal.Round(SPY.MOMP_Daily_10, 4)},{Decimal.Round(SPY.MOMP_Daily_40, 4)}");
            Logger($"MOMP Minute,{Decimal.Round(SPY.MOMP_Minute_01, 4)},{Decimal.Round(SPY.MOMP_Minute_04, 4)},{Decimal.Round(SPY.MOMP_Minute_16, 4)}");
            Logger($"VWAP,{Decimal.Round(SPY.VWAP_01, 4)},{Decimal.Round(SPY.VWAP_04, 4)},{Decimal.Round(SPY.VWAP_16, 4)}");


        }

        public override void OnSecuritiesChanged(SecurityChanges changes)
        {
            // Process MyUniverse First (additions fist)
            //if (!MyUniverse.ContainsKey("SPY"))
            //{
            //    Logger($"SPY Added", true);
            //    if (LiveMode || LocalDevMode || ShowDebug) Logger($"My Universe added SPY");
            //    MyUniverse.Add("SPY", new UniverseType(Securities["SPY"]));
            //}

            var securities = "";
            foreach (var security in changes.AddedSecurities.OrderBy(o => o.Symbol))
            {
                var symbol = security.Symbol.Value;
                if (!MyUniverse.ContainsKey(symbol))
                {
                    securities += $",{symbol}";
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
                    securities += $",{symbol}";
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
                    securities += $",{symbol}";
                    TopUniverse.Remove(symbol);
                }
            }
            if ((LiveMode || LocalDevMode || ShowDebug) && securities != "") Logger($"Top Universe removed {securities}");

            securities = "";
            foreach (var security in TopUniverse.Values.OrderBy(o => o.Security.Symbol.Value))
                securities += $",{security.Security.Symbol.Value}";
            // securities += $",{security.Security.Symbol.Value}={Decimal.Round(security.MOMP_Daily_01, 4)}";
            if ((LiveMode || LocalDevMode || ShowDebug) && securities != "") Logger($"Top Universe {securities}");

            // Remove Securities from MyUniverse (cleanup unused securities)
            securities = "";
            var removedSymbols = MyUniverse.Keys
                .Where(w => w != "SPY" && !TopUniverse.Keys.Contains(w) && Quants.Values.Where(h => h.Holdings.ContainsKey(w)).FirstOrDefault() == null)
                .ToArray();
            foreach (var symbol in removedSymbols)
                MyUniverse.Remove(symbol);
            var symbolsJoined = String.Join(",", removedSymbols);
            if ((LiveMode || LocalDevMode || ShowDebug) && symbolsJoined != "") Logger($"My Universe removed ,{symbolsJoined}");

            securities = "";
            foreach (var security in MyUniverse.Keys.OrderBy(o => o))
                securities += $",{security}";
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
            if (core.MarketOpenTime(-30) || core.MarketOpenTime(-29) || core.MarketOpenTime(-28) || core.MarketOpenTime(-27) || core.MarketOpenTime(-26)
                || core.MarketOpenTime(-25) || core.MarketOpenTime(-24) || core.MarketOpenTime(-23) || core.MarketOpenTime(-22) || core.MarketOpenTime(-21)
                || core.MarketOpenTime(-20) || core.MarketOpenTime(-19) || core.MarketOpenTime(-18) || core.MarketOpenTime(-17) || core.MarketOpenTime(-16)
                || core.MarketOpenTime(-15) || core.MarketOpenTime(-14) || core.MarketOpenTime(-13) || core.MarketOpenTime(-12) || core.MarketOpenTime(-11)
                || core.MarketOpenTime(-10) || core.MarketOpenTime(-9) || core.MarketOpenTime(-8) || core.MarketOpenTime(-7) || core.MarketOpenTime(-6)
                || core.MarketOpenTime(-5) || core.MarketOpenTime(-4) || core.MarketOpenTime(-3) || core.MarketOpenTime(-2)) // Extended Hours Before Market
            {
                LogIndicatorAndHistory();
            }

            if (core.MarketOpenTime(-1)) // Before First trade
            {
                Logger($"Before Market Open");
                LogIndicatorAndHistory();
            }

            if (core.MarketOpenTime()) // First trade
            {
                Logger($"First Trade");
                PlotCore();
                PlotQuant();
                LogIndicatorAndHistory();
            }

            if (core.MarketIsOpen() && !core.MarketOpenTime() && !core.MarketCloseTime(-1)) // Regular trade, not the first trade, not the last trade
            {
                PlotCore();
                LogIndicatorAndHistory();
            }

            if (core.MarketCloseTime(-1)) // Last trade
            {
                Logger($"Last Trade");
                PlotCore();
                LogIndicatorAndHistory();
            }

            if (core.MarketCloseTime()) // After Last Trade/First Close
            {
                Logger($"Market Close");
                PlotCore();
                PlotQuant();
                LogIndicatorAndHistory();

                // Pick Winner only in the last few days
                if (Time >= DateTime.Now.AddDays(-15).Date)
                {
                    var quantPerformances = (Quants != null && Quants.Values.Count > 0)
                    ? Quants.Values
                        .OrderByDescending(o => o.Performance)
                        .Take(3)
                    : null;

                    if (quantPerformances != null)
                        foreach (var quantPerformance in quantPerformances)
                            if (quantPerformance != null)
                                Logger($"Top Performance {quantPerformance.Tag}, Performance {Decimal.Round(quantPerformance.Performance, 4)}, WinRate {Decimal.Round(quantPerformance.WinRate, 4)}", true);

                    var quantWinRates = (Quants != null && Quants.Values.Count > 0)
                        ? Quants.Values
                            .OrderByDescending(o => o.WinRate)
                            .Take(3)
                        : null;

                    if (quantWinRates != null)
                        foreach (var quantWinRate in quantWinRates)
                            if (quantWinRate != null)
                                Logger($"Top Winrate {quantWinRate.Tag}, WinRate {Decimal.Round(quantWinRate.WinRate, 4)}, Performance {Decimal.Round(quantWinRate.Performance, 4)}", true);
                }
            }

            if (core.MarketCloseTime(1)) // Second Close
            {
                LogIndicatorAndHistory();
            }
        }

        private void OnTick10()
        {
            if (LiveMode)
                Logger($"Hello");

            if (!MarketIsOpen())
                PlotCore();
        }
    }
}
