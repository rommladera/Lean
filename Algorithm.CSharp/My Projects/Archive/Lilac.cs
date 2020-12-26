// Uses MACD to determine breakout, EMA200 to determine trend, and uses only SPY

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

namespace QuantConnect.Algorithm.CSharp
{
    public class Lilac : QCAlgorithm
    {
        private Equity _spy;
        private static decimal LastSpyPrice = 0.00m;
        private const int _universeSize = 50;
        private const decimal _holdingPercentage = 1.00m;
        private const int _emaMinuteInterval = 200;
        private const int _macdFastInterval = 12, _macdSlowInterval = 26, _macdSignalInterval = 9;
        private EquityExchange Market = new EquityExchange();
        private Dictionary<Symbol, MyUniverseType> MyUniverse = new Dictionary<Symbol, MyUniverseType>();
        private static decimal LastTotalPortfolioValue = 0.00m;
        private bool wentBelowSignal = false;

        private bool isTradingTime
        {
            get
            {
                try
                {
                    var result = Market.DateTimeIsOpen(Time.AddMinutes(-20)) && Market.DateTimeIsOpen(Time) && Market.DateTimeIsOpen(Time.AddMinutes(40));
                    return result;
                }
                catch (Exception ex)
                {
                    Logger($"isTradingTime Exception: {ex}");
                    throw;
                }
            }
        }

        public override void Initialize()
        {
            try
            {
                Logger("=========================================================");
                Logger("Initialize");

                SetBrokerageModel(BrokerageName.Alpaca, AccountType.Margin);
                SetTimeZone(TimeZones.NewYork);
                SetStartDate(DateTime.Now.AddDays(-28));
                SetCash(60000);

                UniverseSettings.Resolution = Resolution.Minute;
                _spy = AddEquity("SPY", Resolution.Minute);
                AddUniverse(Universe.DollarVolume.Top(_universeSize));

                Schedule.On(DateRules.EveryDay(), TimeRules.Every(TimeSpan.FromMinutes(1)), () =>
                {
                    OnTickMinute();
                });

                Schedule.On(DateRules.EveryDay(), TimeRules.Every(TimeSpan.FromMinutes(1)), () =>
                {
                    if (isTradingTime) OnTrading();
                });

                Schedule.On(DateRules.EveryDay(), TimeRules.BeforeMarketClose("SPY", 10), () =>
                {
                    BeforeMarketClose();
                });
            }
            catch (Exception ex)
            {
                Logger($"Initialize Exception: {ex}");
                throw;
            }
        }

        public override void OnSecuritiesChanged(SecurityChanges changes)
        {
            try
            {
                Logger("=========================================================");
                Logger($"SecurityChanges: {changes}");

                foreach (var security in changes.RemovedSecurities)
                {
                    if (MyUniverse.ContainsKey(security.Symbol))
                        MyUniverse.Remove(security.Symbol);
                }

                foreach (var security in changes.AddedSecurities)
                {
                    if (!MyUniverse.ContainsKey(security.Symbol))
                    {
                        var myUniverse = new MyUniverseType(security, this);
                        MyUniverse.Add(security.Symbol, myUniverse);
                    }
                }

                // Handle SPY
                if (!MyUniverse.ContainsKey(_spy.Symbol))
                {
                    var myUniverse = new MyUniverseType(_spy, this);
                    MyUniverse.Add(_spy.Symbol, myUniverse);
                }
            }
            catch (Exception ex)
            {
                Logger($"OnSecuritiesChanged Exception: {ex}");
                throw;
            }
        }

        private void OnTrading()
        {
            try
            {
                var security = MyUniverse["SPY"];

                Plot("Result", "Price", security.Security.Price);
                Plot("Result", "EMA200", security.MinuteEMA);

                Plot("MACD", "MACD", security.MinuteMACD);
                Plot("MACD", "Signal", security.MinuteMACD.Signal);

                // Sell Logic
                if (
                    Portfolio.Invested && security.MinuteMACD <= security.MinuteMACD.Signal
                    )
                {
                    SetHoldings(security.Security.Symbol, 0.00);
                }

                // Buy Logic
                if (
                    !Portfolio.Invested
                    && security.MinuteMACD >= security.MinuteMACD.Signal
                    && wentBelowSignal
                    && security.MinuteMACD.Signal < 0
                    && security.MinuteMACD < 0
                    && security.Security.Price > security.MinuteEMA
                    )
                {
                    SetHoldings(security.Security.Symbol, _holdingPercentage);
                }

                // Update wentBelow
                wentBelowSignal = (security.MinuteMACD < security.MinuteMACD.Signal);
            }
            catch (Exception ex)
            {
                Logger($"OnTrading Exception: {ex}");
                throw;
            }
        }

        private void OnTickMinute()
        {
            Logger($"OnTickMinute");

            try
            {

                if (LastSpyPrice != _spy.Price)
                {
                    Plot("SPY", "Price", _spy.Price);
                    LastSpyPrice = _spy.Price;
                }

                if (LastTotalPortfolioValue != Portfolio.TotalPortfolioValue)
                {
                    Plot("Portfolio", "Value", Portfolio.TotalPortfolioValue);
                    LastTotalPortfolioValue = Portfolio.TotalPortfolioValue;
                }

                //if (Portfolio.Invested)
                //{
                //    foreach (var a in Portfolio)
                //    {
                //        if (a.Value.Invested && a.Value.UnrealizedProfitPercent < -_minThreshhold)
                //        {
                //            Logger($"{Time} Went below threshhold, liquidating...");
                //            Liquidate();
                //        }
                //        // Logger($"{Time} Symbol={a.Value.Symbol}, UnrealizedProfitPercent={a.Value.UnrealizedProfitPercent}");
                //        // Plot("UnrealizedProfitPercent", "UnrealizedProfitPercent", a.Value.UnrealizedProfitPercent);
                //    }
                //}

                // If market is closed and we are still invested, liquidate all of it
                if (!isTradingTime && Portfolio.Invested)
                {
                    Logger($"Liquidate");
                    Liquidate();
                }
            }
            catch (Exception ex)
            {
                Logger($"OnTickMinute Exception: {ex}");
                throw;
            }
        }

        private void BeforeMarketClose()
        {
            Logger($"BeforeMarketClose");

            try
            {
            }
            catch (Exception ex)
            {
                Logger($"BeforeMarketClose Exception: {ex}");
                throw;
            }
        }

        public override void OnData(Slice data)
        {
            try
            {
            }
            catch (Exception ex)
            {
                Logger($"OnData Exception: {ex}");
                throw;
            }
        }

        public override void OnOrderEvent(OrderEvent orderEvent)
        {
            try
            {
                var order = Transactions.GetOrderById(orderEvent.OrderId);
                Logger("=========================================================");
                Logger($"OnOrderEvent");
                Logger($"OnOrderEvent orderEvent = {orderEvent}");
                Logger($"OnOrderEvent order = {order}");
            }
            catch (Exception ex)
            {
                Logger($"OnOrderEvent Exception: {ex}");
                throw;
            }
        }

        public void Logger(string message, bool force = false)
        {
            // Log($"{Time} {message}");
            if (!LiveMode && !force) return;
            Log($"{message}");
        }

        private class MyUniverseType
        {
            public MyUniverseType(Security security, Lilac parent)
            {
                try
                {
                    Security = security;

                    MinuteEMA = parent.EMA(security.Symbol, _emaMinuteInterval, Resolution.Minute);
                    MinuteMACD = parent.MACD(security.Symbol, _macdFastInterval, _macdSlowInterval, _macdSignalInterval, MovingAverageType.Exponential, Resolution.Minute);
                    MinuteSTO = parent.STO(security.Symbol, 5, 3, 3, Resolution.Minute);
                }
                catch (Exception ex)
                {
                    parent.Logger($"MyUniverseType Initialize Exception: {ex}");
                    throw;
                }
            }

            public Security Security { get; set; }
            public ExponentialMovingAverage MinuteEMA { get; set; }
            public Stochastic MinuteSTO { get; set; }
            public MovingAverageConvergenceDivergence MinuteMACD { get; set; }
        }

    }
}