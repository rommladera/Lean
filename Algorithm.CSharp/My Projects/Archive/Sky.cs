using QuantConnect.Brokerages;
using QuantConnect.Data;
using QuantConnect.Indicators;
using QuantConnect.Orders;
using QuantConnect.Securities.Equity;
using System;

namespace QuantConnect.Algorithm.CSharp
{
    public class Sky : QCAlgorithm
    {
        private const int _aInterval = 2, _bInterval = 4, _cInterval = 8, _dInterval = 16;
        private const decimal _holdingPercent = 1.00m;
        private string _symbol = "SPY";
        private EquityExchange Market = new EquityExchange();
        private bool _wentBelow = false;

        // BollingerBands _bb;
        RelativeStrengthIndex _aRsi, _bRsi, _cRsi, _dRsi;
        // AverageTrueRange _atr;
        // ExponentialMovingAverage _ema;
        SimpleMovingAverage _aSma, _bSma, _cSma, _dSma;
        // MovingAverageConvergenceDivergence _macd;
        MomentumPercent _aMomp, _bMomp, _cMomp, _dMomp;

        public override void Initialize()
        {
            Log("=========================================================");
            var now = DateTime.Now;
            Log("Initialize: " + now);

            //Brokerage model and account type:
            SetBrokerageModel(BrokerageName.Alpaca, AccountType.Margin);

            SetTimeZone(TimeZones.NewYork);

            SetStartDate(now.AddDays(-5));
            SetCash(30000);

            // Equity Setup    
            // AddEquity(_symbol, Resolution.Minute);
            AddSecurity(SecurityType.Equity, _symbol, Resolution.Minute);

            //Set up Indicators:
            //_bb = BB(_symbol, 20, 1, MovingAverageType.Simple, Resolution.Minute);
            _aRsi = RSI(_symbol, _aInterval, MovingAverageType.Simple, Resolution.Minute);
            _bRsi = RSI(_symbol, _bInterval, MovingAverageType.Simple, Resolution.Minute);
            _cRsi = RSI(_symbol, _cInterval, MovingAverageType.Simple, Resolution.Minute);
            _dRsi = RSI(_symbol, _dInterval, MovingAverageType.Simple, Resolution.Minute);
            //_atr = ATR(_symbol, 14, MovingAverageType.Simple, Resolution.Minute);
            //_ema = EMA(_symbol, 10, Resolution.Minute);
            _aSma = SMA(_symbol, _aInterval, Resolution.Minute);
            _bSma = SMA(_symbol, _bInterval, Resolution.Minute);
            _cSma = SMA(_symbol, _cInterval, Resolution.Minute);
            _dSma = SMA(_symbol, _dInterval, Resolution.Minute);
            //_macd = MACD(_symbol, 12, 26, 9, MovingAverageType.Simple, Resolution.Minute);
            _aMomp = MOMP(_symbol, _aInterval, Resolution.Minute);
            _bMomp = MOMP(_symbol, _bInterval, Resolution.Minute);
            _cMomp = MOMP(_symbol, _cInterval, Resolution.Minute);
            _dMomp = MOMP(_symbol, _dInterval, Resolution.Minute);
        }

        public override void OnData(Slice data)
        {
            //if (!_bb.IsReady) return;
            if (!_aRsi.IsReady) return;
            if (!_bRsi.IsReady) return;
            if (!_cRsi.IsReady) return;
            if (!_dRsi.IsReady) return;
            //if (!_atr.IsReady) return;
            //if (!_ema.IsReady) return;
            if (!_aSma.IsReady) return;
            if (!_bSma.IsReady) return;
            if (!_cSma.IsReady) return;
            if (!_dSma.IsReady) return;
            //if (!_macd.IsReady) return;
            if (!_aMomp.IsReady) return;
            if (!_bMomp.IsReady) return;
            if (!_cMomp.IsReady) return;
            if (!_dMomp.IsReady) return;

            var isMarketOpen = Market.DateTimeIsOpen(Time.AddMinutes(-15)) && Market.DateTimeIsOpen(Time) && Market.DateTimeIsOpen(Time.AddMinutes(15));

            if (isMarketOpen)
            {
                var _data = data[data.Keys[0]];
                var _price = _data.Close;

                if (!Portfolio.Invested)
                {

                    if
                        (_bMomp > 0 && _cRsi < 50)
                        SetHoldings(_symbol, _holdingPercent);

                }
                else
                {

                    if
                        (_bMomp < 0)
                        SetHoldings(_symbol, 0.00);

                }

                _wentBelow = (_aSma < _bSma);

                // Plot("Price", "Price", _price);
                Plot("Price", "A", _aSma);
                Plot("Price", "B", _bSma);
                Plot("Price", "C", _cSma);
                Plot("Price", "D", _dSma);

                // Plot("Momentum", "Momentum", _momp);

                Plot("Invested", "Invested", (Portfolio.Invested) ? 1 : 0);

                Plot("RSI", "a", _aRsi);
                Plot("RSI", "b", _bRsi);
                Plot("RSI", "c", _cRsi);
                Plot("RSI", "d", _dRsi);

                // Plot("BB", "Price", _price);
                // Plot("BB", "UpperBand", _bb.UpperBand);
                // Plot("BB", "MiddleBand", _bb.MiddleBand);
                // Plot("BB", "LowerBand", _bb.LowerBand);

                // Plot("ATR", "Price", _price);
                // Plot("ATR", "ATR", _atr);

                // Plot("MACD", "Price", _price);
                // Plot("MACD", "Fast", _macd.Fast);
                // Plot("MACD", "Slow", _macd.Slow);

                // Plot("EMA", "Price", _price);
                // Plot("EMA", "EMA", _ema);

                //Plot("RSI", "Overbought", 70);
                //Plot("RSI", "Mid", 50);
                //Plot("RSI", "Oversold", 30);
            }

            if (!isMarketOpen && Portfolio.Invested)
            {
                Log("=========================================================");
                Log($"{Time} End of Day Liquidate");
                Liquidate();
            }
        }

        public override void OnOrderEvent(OrderEvent orderEvent)
        {
            Log("=========================================================");
            Log($"{Time} OnOrderEvent");
            Log($"{Time} OnOrderEvent orderEvent = {orderEvent}");
            var order = Transactions.GetOrderById(orderEvent.OrderId);
            Log($"{Time} OnOrderEvent order = {order}");
        }
    }
}