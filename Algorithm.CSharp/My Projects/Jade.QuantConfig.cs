using QuantConnect.Brokerages;
using QuantConnect.Data.UniverseSelection;
using QuantConnect.Indicators;
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
        private void QuantConfig()
        {
            var TestQuant = new Quant(
                "01",
                10000.00m, // Initial Cash
                q =>
                {
                    if (core.MarketCloseTime(-1)) // Last trade
                    {
                        // Select Item
                        var item = MyUniverse.Values
                            .Where(w => w.Security.HasData)
                            .OrderByDescending(o => o.MOMP_Daily_01)
                            .Take(1)
                            .SingleOrDefault();

                        if (item != null)
                        {
                            var symbol = item.Security.Symbol.Value;
                            Logger($"Security {symbol} selected.");

                            if (!q.Holdings.ContainsKey(symbol))
                            {
                                q.Liquidate();
                                q.SetHoldings(symbol, 0.10m); // Risk only 10 %
                            }
                        }
                        else
                        {
                            Logger("No security selected. Liquidating all.");
                            q.Liquidate();
                        }
                    }

                    return true;
                });
            TestQuant.Plot = true;
            TestQuant.Live = true;
            Quants.Add(TestQuant.Tag, TestQuant);

        }
    }
}
