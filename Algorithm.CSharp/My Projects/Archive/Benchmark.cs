using QuantConnect.Brokerages;
using QuantConnect.Data;
using QuantConnect.Indicators;
using QuantConnect.Orders;
using QuantConnect.Securities;
using QuantConnect.Securities.Equity;
using System;

namespace QuantConnect.Algorithm.CSharp
{
    public class Benchmark : QCAlgorithm
    {
        private EquityExchange Market = new EquityExchange();
        private Security security;

        public override void Initialize()
        {
            SetBrokerageModel(BrokerageName.Alpaca, AccountType.Margin);
            SetTimeZone(TimeZones.NewYork);
            SetStartDate(DateTime.Now.AddDays(-7));
            SetCash(100000);

            // Equity Setup    
            security = AddEquity("SPY", Resolution.Minute);

            Schedule.On(DateRules.EveryDay(), TimeRules.Every(TimeSpan.FromMinutes(1)), () =>
            {
                OnTickMinute();
            });
        }

        public override void OnData(Slice data)
        {
            if (!Portfolio.Invested)
                SetHoldings(security.Symbol, 1.00);

            //var isMarketOpen = Market.DateTimeIsOpen(Time.AddMinutes(-15)) && Market.DateTimeIsOpen(Time) && Market.DateTimeIsOpen(Time.AddMinutes(15));

            //if (isMarketOpen)
            //{
            //    if (!Portfolio.Invested)
            //    {
            //        SetHoldings(security.Symbol, 1.00);
            //    }

            //    Plot("Portfolio", "Value", Portfolio.TotalPortfolioValue);
            //    Plot("Price", "Value", security.Price);
            //}
        }

        private void OnTickMinute()
        {
            Plot("Portfolio", "Value", Portfolio.TotalPortfolioValue);
            if (security.Price != 0)
                Plot("Price", "Value", security.Price);
        }

    }
}