using QuantConnect.Brokerages;
using QuantConnect.Data;
using QuantConnect.Indicators;
using QuantConnect.Orders;
using QuantConnect.Securities.Equity;
using System;

namespace QuantConnect.Algorithm.CSharp
{
    public class Indigo : QCAlgorithm
    {
        private const decimal _threshholdBuy = 2.000m, _threshholdSell = 2.000m;
        private const int _smaInterval = 2;
        private const int _atrInterval = 8, _rsiInterval = 16, _mompInterval = 8;

        private const decimal _holdingPercent = 1.00m;
        private bool _justCrossed = false;
        private DirectionEnum _direction = DirectionEnum.Down;

        private string _symbol = "SPY";
        private EquityExchange Market = new EquityExchange();
        private decimal _signal = decimal.MaxValue;

        private RelativeStrengthIndex _rsi;
        private AverageTrueRange _atr;
        private SimpleMovingAverage _sma;
        private MomentumPercent _momp;

        public override void Initialize()
        {
            Debug("=========================================================");
            var now = DateTime.Now;
            Debug("Initialize: " + now);

            //Brokerage model and account type:
            SetBrokerageModel(BrokerageName.Alpaca, AccountType.Margin);

            SetTimeZone(TimeZones.NewYork);

            SetStartDate(now.AddDays(-2));
            SetCash(30000);

            // Equity Setup    
            AddEquity(_symbol, Resolution.Minute);

            //Set up Indicators:
            _rsi = RSI(_symbol, _rsiInterval, MovingAverageType.Simple, Resolution.Minute);
            _atr = ATR(_symbol, _atrInterval, MovingAverageType.Simple, Resolution.Minute);
            _sma = SMA(_symbol, _smaInterval, Resolution.Minute);
            _momp = MOMP(_symbol, _mompInterval, Resolution.Minute);

            Schedule.On(DateRules.EveryDay(), TimeRules.Every(TimeSpan.FromMinutes(10)), () =>
            {
                OnTick();
            });
        }

        public void OnTick()
        {
            var isMarketOpen = Market.DateTimeIsOpen(Time);

            if (isMarketOpen)
            {
                //Plot("Tick", "Tick", 1);
                //Plot("Tick", "Tick", 0);
                Debug($"{Time} Tick");
            }
        }

        public override void OnData(Slice data)
        {
            if (!_rsi.IsReady) return;
            if (!_atr.IsReady) return;
            if (!_sma.IsReady) return;
            if (!_momp.IsReady) return;

            var isMarketOpen = Market.DateTimeIsOpen(Time.AddMinutes(-15)) && Market.DateTimeIsOpen(Time) && Market.DateTimeIsOpen(Time.AddMinutes(15));

            if (isMarketOpen)
            {
                var _data = data[_symbol];
                var _price = _data.Close;

                // See if just crossed
                if (_direction == DirectionEnum.Down)
                {
                    if (_sma > _signal)
                    {
                        _direction = DirectionEnum.Up;
                        _justCrossed = true;
                        _signal = decimal.MinValue;
                    }
                }
                else
                {
                    if (_sma < _signal)
                    {
                        _direction = DirectionEnum.Down;
                        _justCrossed = true;
                        _signal = decimal.MaxValue;
                    }
                }

                // Calculate Signal
                // _sma is the price
                _signal = (_direction == DirectionEnum.Up)
                    ? Math.Max(_signal, _sma - (_atr * _threshholdSell))
                    : Math.Min(_signal, _sma + (_atr * _threshholdBuy));

                // Calculate Investment
                if (!Portfolio.Invested)
                {
                    // Do we buy
                    if (_justCrossed && _direction == DirectionEnum.Up && _momp > 0)
                    {
                        SetHoldings(_symbol, _holdingPercent);
                    }
                }
                else
                {
                    // Do we sell
                    if (_signal > _sma)
                    {
                        SetHoldings(_symbol, 0.00);
                    }
                }

                Plot("Portfolio", "Value", Portfolio.TotalPortfolioValue);

                Plot("Result", "Price", _price);
                Plot("Result", "SMA", _sma);
                Plot("Result", "Signal", _signal);

                Plot("MOMP", "MOMP", _momp);

                Plot("RSI", "RSI", _rsi);

                _justCrossed = false;
            }

            if (!isMarketOpen && Portfolio.Invested)
            {
                // Debug("=========================================================");
                // Debug($"{Time} End of Day Liquidate");
                Liquidate();
            }
        }

        public override void OnOrderEvent(OrderEvent orderEvent)
        {
            // Debug("=========================================================");
            // Debug($"{Time} OnOrderEvent");
            // Debug($"{Time} OnOrderEvent orderEvent = {orderEvent}");
            // var order = Transactions.GetOrderById(orderEvent.OrderId);
            // Debug($"{Time} OnOrderEvent order = {order}");
        }

        public enum DirectionEnum
        {
            Up,
            Down
        }

    }
}