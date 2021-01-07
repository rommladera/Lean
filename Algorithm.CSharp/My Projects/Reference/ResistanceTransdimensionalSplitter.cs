using QuantConnect.Brokerages;
using QuantConnect.Data;
using QuantConnect.Data.UniverseSelection;
using QuantConnect.Indicators;
using QuantConnect.Orders;
using QuantConnect.Securities.Equity;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QuantConnect.Algorithm.CSharp
{
    public class ResistanceTransdimensionalSplitter : QCAlgorithm
    {
        private Dictionary<Symbol, Indicators.SimpleMovingAverage> _indicators = new Dictionary<Symbol, Indicators.SimpleMovingAverage>();

        public override void Initialize()
        {
            SetStartDate(2018, 11, 5);  //Set Start Date
            SetCash(100000);             //Set Strategy Cash
            UniverseSettings.Resolution = Resolution.Daily;
            // AddEquity("SPY", Resolution.Minute);

            AddUniverse(coarse =>
            {
                var sortedByDollarVolume = coarse
                .Where(x => x.HasFundamentalData)
                .OrderByDescending(x => x.DollarVolume);
                // take the top entries from our sorted collection
                var selection = sortedByDollarVolume.Take(1000);
                // we need to return only the symbol objects
                return selection.Select(x => x.Symbol);
            }, fine =>
            {
                var sortedByPeRatio = fine.OrderByDescending(x => x.ValuationRatios.PERatio);
                // take the top entries from our sorted collection
                var topFine = sortedByPeRatio.Take(5);
                // we need to return only the symbol objects
                return topFine.Select(x => x.Symbol);
            });
        }

        /// OnData event is the primary entry point for your algorithm. Each new data point will be pumped in here.
        /// Slice object keyed by symbol containing the stock data
        public override void OnData(Slice data)
        {
            foreach (var key in data.Keys)
            {
                if (Time.Hour == 15 && Time.Minute == 50)
                {
                    if (_indicators.ContainsKey(key))
                    {
                        // You can use the indicator here by indexing the dictionary
                        // i.e.
                        var indicator = _indicators[key];
                        if (indicator.IsReady)
                        {
                            Log($"{key} SMA current value: {indicator}");
                        };
                    }
                }
            }
        }
        // sort the data by daily dollar volume and take the top 'NumberOfSymbols'

        public override void OnSecuritiesChanged(SecurityChanges changes)
        {
            // Loop through securities added to the universe
            foreach (var security in changes.AddedSecurities)
            {
                // Create SMA indicator (or other indicator, depending on what you want in the algorithm)
                var indicator = SMA(security.Symbol, 14, Resolution.Daily);

                // Warm-up the indicator using a historical data call
                var history = History(security.Symbol, 14, Resolution.Daily);
                foreach (var bar in history)
                {
                    indicator.Update(bar.EndTime, bar.Close);
                };
                // Add the indicator to the dictionary, keyed by the security's Symbol
                _indicators.Add(security.Symbol, indicator);
                Log($"Indicator created for {security.Symbol}");
            }

            // Loop through securities removed from the universe
            foreach (var security in changes.RemovedSecurities)
            {
                // If the Symbol has an indicator
                if (_indicators.ContainsKey(security.Symbol))
                {
                    // Remove the Symbol's indicator from the dictionary
                    _indicators.Remove(security.Symbol);
                }
                Log($"Indicator deleted for {security.Symbol}");
            }
        }
    }
}