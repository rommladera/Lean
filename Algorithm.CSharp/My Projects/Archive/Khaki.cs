// This uses child quants

using Accord.MachineLearning;
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
using System.Security.Cryptography;

namespace QuantConnect.Algorithm.CSharp
{
    public class Khaki : QCAlgorithm
    {
        private Khaki self;
        private EquityExchange Market = new EquityExchange();
        private Dictionary<string, Quant> Quants = new Dictionary<string, Quant>();
        public Security Security;
        public SimpleMovingAverage Price;

        private bool isTradingTime
        {
            get
            {
                try
                {
                    // var result = Market.DateTimeIsOpen(Time.AddMinutes(-20)) && Market.DateTimeIsOpen(Time) && Market.DateTimeIsOpen(Time.AddMinutes(40));
                    var result = Market.DateTimeIsOpen(Time.AddMinutes(-1)) && Market.DateTimeIsOpen(Time) && Market.DateTimeIsOpen(Time.AddMinutes(1));
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

                self = this;

                SetBrokerageModel(BrokerageName.Alpaca, AccountType.Margin);
                SetTimeZone(TimeZones.NewYork);
                SetStartDate(DateTime.Now.AddDays(-28).Date);
                // SetEndDate(DateTime.Now.AddDays(-6).Date);
                SetCash(1000000);

                UniverseSettings.Resolution = Resolution.Minute;
                Security = AddEquity("SPY", Resolution.Minute);

                // Create Quants
                for (int atrInterval = 10; atrInterval <= 10; atrInterval++)
                {
                    for (decimal buyThreshhold = 8.0m; buyThreshhold <= 8.0m; buyThreshhold += 0.5m)
                    {
                        var q = new Quant(this, atrInterval, buyThreshhold, buyThreshhold);
                        Quants.Add(q.Tag, q);

                        //for (decimal sellThreshhold = 1.0m; sellThreshhold <= 15.0m; sellThreshhold += 0.5m)
                        //{
                        //    var q = new Quant(this, atrInterval, buyThreshhold, sellThreshhold);
                        //    Quants.Add(q.Tag, q);
                        //}
                    }
                }

                //var q = new Quant(this, 10, 1.5m, 8.5m);
                //Quants.Add(q.Tag, q);
                //q = new Quant(this, 10, 1.5m, 9.0m);
                //Quants.Add(q.Tag, q);
                //q = new Quant(this, 10, 1.5m, 11.5m);
                //Quants.Add(q.Tag, q);
                //q = new Quant(this, 10, 1.5m, 13.5m);
                //Quants.Add(q.Tag, q);
                //q = new Quant(this, 10, 2.0m, 12.0m);
                //Quants.Add(q.Tag, q);
                //q = new Quant(this, 10, 2.0m, 13.5m);
                //Quants.Add(q.Tag, q);

                //var q = new Quant(this, 10, 1.5m, 9.0m);
                //Quants.Add(q.Tag, q);

                SetWarmUp(30);

                Schedule.On(DateRules.EveryDay(), TimeRules.Every(TimeSpan.FromMinutes(1)), () =>
                {
                    if (isTradingTime) OnTrading();
                    if (!isTradingTime) OffTrading();
                    OnTickMinute();
                });

                Schedule.On(DateRules.EveryDay(), TimeRules.BeforeMarketClose(Security.Symbol, -10), () =>
                {
                    AfterMarketClose();
                });
            }
            catch (Exception ex)
            {
                Logger($"Initialize Exception: {ex}");
                throw;
            }
        }

        private void OnTrading()
        {
            try
            {
                foreach (var quant in Quants.Values)
                {
                    quant.OnTrading();
                }
            }
            catch (Exception ex)
            {
                Logger($"OnTrading Exception: {ex}");
                throw;
            }
        }

        private void OffTrading()
        {
            try
            {
                foreach (var quant in Quants.Values)
                {
                    quant.OffTrading();
                }
            }
            catch (Exception ex)
            {
                Logger($"OffTrading Exception: {ex}");
                throw;
            }
        }

        private void OnTickMinute()
        {
            try
            {
                // Plotter("Result", "Actual", Security.Price);

                foreach (var quant in Quants.Values)
                {
                    quant.OnTickMinute();
                }
            }
            catch (Exception ex)
            {
                Logger($"OnTickMinute Exception: {ex}");
                throw;
            }
        }

        private void AfterMarketClose()
        {
            try
            {
                //Plotter("Result", "Benchmark", Security.Price);

                foreach (var quant in Quants.Values)
                {
                    Plotter($"Profit", $"{quant.Tag}", quant.NetProfit);
                }

                //foreach (var quant in Quants.Values)
                //{
                //    Logger($"AfterMarketClose,Tag,{quant.Tag}, Win Rate,{quant.WinRate},Net Profit,{quant.NetProfit}", true);
                //}

                // log the winner
                var winnerNet = Quants.Values
                    .OrderByDescending(o => o.NetProfit)
                    .Take(1)
                    .SingleOrDefault();

                // Logger($"Winner Net Profit,Tag,{winnerNet.Tag}, Win Rate,{winnerNet.WinRate},Net Profit,{winnerNet.NetProfit}", true);
                //Plotter("Net Profit", "Interval", winnerNet.AtrInterval);
                //Plotter("Net Profit", "Buy", winnerNet.BuyThreshhold);
                //Plotter("Net Profit", "Sell", winnerNet.SellThreshhold);
                //Plotter("Net Profit", "Profit", winnerNet.NetProfit);
                //Plotter("Net Profit", "Win", winnerNet.WinRate);

                var winnerWinRate = Quants.Values
                    .OrderByDescending(o => o.WinRate)
                    .Take(1)
                    .SingleOrDefault();

                // Logger($"Winner Win Rate,Tag,{winnerWinRate.Tag}, Win Rate,{winnerWinRate.WinRate},Net Profit,{winnerWinRate.NetProfit}", true);
                //Plotter("Win Rate", "Interval", winnerWinRate.AtrInterval);
                //Plotter("Win Rate", "Buy", winnerWinRate.BuyThreshhold);
                //Plotter("Win Rate", "Sell", winnerWinRate.SellThreshhold);
                //Plotter("Win Rate", "Profit", winnerWinRate.NetProfit);
                //Plotter("Win Rate", "Win", winnerWinRate.WinRate);
            }
            catch (Exception ex)
            {
                Logger($"AfterMarketClose Exception: {ex}");
                throw;
            }
        }

        public override void OnOrderEvent(OrderEvent orderEvent)
        {
            try
            {
                var order = Transactions.GetOrderById(orderEvent.OrderId);
                var tag = order.Tag;
                var quant = Quants[tag];

                quant.OnOrderEvent(orderEvent);
            }
            catch (Exception ex)
            {
                Logger($"OnOrderEvent Exception: {ex}");
                throw;
            }
        }

        #region Logger
        public void Logger(string message, bool force = false)
        {
            //if (!LiveMode && !force) return;
            //Log($",{Time},{message}");
        }
        #endregion

        #region Plottter
        private Dictionary<string, decimal> _plotPoints = new Dictionary<string, decimal>();
        public void Plotter(string chart, string series, decimal value)
        {
            try
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
            catch (Exception ex)
            {
                Logger($"Plotter Plot Exception: {ex}");
                throw;
            }
        }
        #endregion

        #region Quant
        private class Quant
        {
            private Khaki _parent = null;
            private int _totalOrders = 0, _totalWins = 0, _atrInterval;
            private decimal _buyFillPrice;
            private decimal _netProfit = 0;
            private bool _invested = false;
            private string _tag = Guid.NewGuid().ToString();
            private bool _openOrders = false;
            private AverageTrueRange _atr;
            private decimal _signal = 330m, _buyThreshhold, _sellThreshhold;

            public Quant(Khaki parent, int atrInterval, decimal buyThreshhold, decimal sellThreshhold)
            {
                _parent = parent;
                var atrIntervalString = $"{atrInterval}".PadLeft(2, '0');
                var buyThreshholdString = $"{buyThreshhold}".PadLeft(3, '0');
                var sellThreshholdString = $"{sellThreshhold}".PadLeft(3, '0');

                _tag = $"({atrInterval}/{buyThreshholdString}/{sellThreshholdString})";
                _atr = parent.ATR(_parent.Security.Symbol, atrInterval, MovingAverageType.Simple, Resolution.Minute);
                var histATR = _parent.History(_parent.Security.Symbol, _atrInterval, Resolution.Minute);
                foreach (var bar in histATR)
                    _atr.Update(bar);

                _atrInterval = atrInterval;
                _buyThreshhold = buyThreshhold;
                _sellThreshhold = sellThreshhold;

                // _signal = _parent.Security.Price + (_atr * _buyThreshhold);
            }

            public string Tag
            {
                get
                {
                    return _tag;
                }
            }

            public decimal NetProfit
            {
                get
                {
                    return _netProfit;
                }
            }

            public decimal WinRate
            {
                get
                {
                    if (_totalOrders == 0) return 0;
                    return ((_totalWins * 100.00m) / (_totalOrders * 100.00m));
                }
            }

            public int AtrInterval
            {
                get
                {
                    return _atrInterval;
                }
            }

            public decimal BuyThreshhold
            {
                get
                {
                    return _buyThreshhold;
                }
            }

            public decimal SellThreshhold
            {
                get
                {
                    return _sellThreshhold;
                }
            }

            public void OnTrading()
            {
                // Calculate Signal
                _signal = (!_invested)
                    ? Math.Min(_signal, _parent.Security.Price + (_atr * _buyThreshhold))
                    : Math.Max(_signal, _parent.Security.Price - (_atr * _sellThreshhold));

                // Plot
                //_parent.Plotter($"{Tag} Result", "Price", security.Price);
                //_parent.Plotter($"{Tag} Result", "Signal", _signal);

                // _parent.Plotter($"{Tag} Portfolio", "Profit", _netProfit);

                // _parent.Plotter($"{Tag} Invested", "Value", (_invested) ? 1 : 0);

                _parent.Logger($"{Tag},{_parent.Security.Price},{_signal},{_invested},{_openOrders},{_atr}");

                if (_openOrders) return;

                if (!_invested)
                {
                    // buy logic
                    if (_parent.Security.Price >= _signal)
                    {
                        _openOrders = true;
                        _parent.MarketOrder(_parent.Security.Symbol, 2, true, _tag);
                        // _parent.SetHoldings(_parent.Security.Symbol, 1, false, _tag);
                    }
                }
                else
                {
                    // sell loic
                    if (_parent.Security.Price <= _signal)
                    {
                        _openOrders = true;
                        _parent.MarketOrder(_parent.Security.Symbol, -1, true, _tag);
                        // _parent.SetHoldings(_parent.Security.Symbol, 0, false, _tag);
                    }
                }
            }

            public void OffTrading()
            {
                if (_invested)
                {
                    // always sell all assets outside of trading times
                    _openOrders = true;
                    _parent.MarketOrder(_parent.Security.Symbol, -1, true, _tag);
                    // _parent.SetHoldings(_parent.Security.Symbol, 0, false, _tag);

                    //// Plot
                    //_parent.Plotter($"{Tag} Result", "Price", security.Price);
                    //_parent.Plotter($"{Tag} Result", "Signal", _signal);
                    //_parent.Plotter($"{Tag} Invested", "Value", (_invested) ? 1 : 0);
                }
            }

            public void OnTickMinute()
            {
                // Plot
                // _parent.Plotter($"Result", "Signal", _signal);
                // _parent.Plotter($"ATR {Tag}", "ATR", _atr);
            }


            public void OnOrderEvent(OrderEvent orderEvent)
            {
                var order = _parent.Transactions.GetOrderById(orderEvent.OrderId);

                // Check if order is mine
                if (order.Tag != _tag) return;

                _parent.Logger($"{Tag}," +
                    $"Status={order.Status}," +
                    $"Id={order.Id}," +
                    $"Direction={order.Direction}," +
                    $"Price={order.Price}");

                if (orderEvent.Status != OrderStatus.Filled) return;

                // _parent.Logger($"{Tag},{orderEvent.Direction},{orderEvent.FillPrice}");

                switch (orderEvent.Direction)
                {
                    case OrderDirection.Buy:
                        _invested = true;
                        _buyFillPrice = orderEvent.FillPrice;
                        _signal = _parent.Security.Price - (_atr * _sellThreshhold);
                        break;
                    case OrderDirection.Sell:
                        _invested = false;
                        _totalOrders++;
                        _netProfit += (orderEvent.FillPrice - _buyFillPrice);
                        _totalWins += (orderEvent.FillPrice > _buyFillPrice) ? 1 : 0;
                        _signal = _parent.Security.Price + (_atr * _buyThreshhold);
                        break;
                    case OrderDirection.Hold:
                    default:
                        break;
                }

                _openOrders = false;

                _parent.Logger($"{Tag}," +
                    $"Signal = {_signal}," +
                    $"Price = {_parent.Security.Price}," +
                    $"ATR = {_atr}");

                // Plot
                //_parent.Plotter($"{Tag} Result", "Price", security.Price);
                //_parent.Plotter($"{Tag} Result", "Signal", _signal);
                //_parent.Plotter($"{Tag} Invested", "Value", (_invested) ? 1 : 0);
            }
        }
        #endregion

    }
}