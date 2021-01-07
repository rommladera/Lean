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
            Quant TestQuant = null;

            //TestQuant = new Quant(
            //    "SPY",
            //    5000.00m, // Initial Cash
            //    q =>
            //    {
            //        if (core.MarketOpenTime()) // First trade
            //        {
            //            if (!q.Holdings.ContainsKey("SPY"))
            //            {
            //                q.SetHoldings("SPY", 1.0m); // Risk only 10 %
            //            }
            //        }

            //        if (core.MarketCloseTime(-1)) // Last trade
            //        {
            //        }

            //        return true;
            //    });
            //TestQuant.Plot = false;
            //TestQuant.Live = false;
            // Quants.Add(TestQuant.Tag, TestQuant);

            //TestQuant = new Quant(
            //    "SPY-AB",
            //    5000.00m, // Initial Cash
            //    q =>
            //    {
            //        if (core.MarketOpenTime()) // First trade
            //        {
            //            if (!q.Holdings.ContainsKey("SPY"))
            //            {
            //                q.SetHoldings("SPY", 1.00m); // Risk only 10 %
            //            }
            //        }

            //        if (core.MarketCloseTime(-1)) // Last trade
            //        {
            //            q.Liquidate();
            //        }

            //        return true;
            //    });
            //TestQuant.Plot = false;
            //TestQuant.Live = false;
            // Quants.Add(TestQuant.Tag, TestQuant);

            TestQuant = new Quant(
                "10AB",
                5000.00m, // Initial Cash
                q =>
                {
                    if (core.MarketOpenTime()) // First trade
                    {
                        // Select Item
                        var item = TopUniverse.Values
                            .Where(w => w.Security.HasData)
                            .Where(w => w.MOMP_Minute_01 > 0)
                            .OrderByDescending(o => o.MOMP_Daily_10)
                            .Take(1)
                            .SingleOrDefault();

                        if (item != null)
                        {
                            var symbol = item.Security.Symbol.Value;
                            Logger($"Security {symbol} selected.");

                            if (!q.Holdings.ContainsKey(symbol))
                            {
                                q.Liquidate();
                                q.SetHoldings(symbol, 1.00m);
                            }
                        }
                        else
                        {
                            Logger("No security selected.");
                        }
                    }

                    if (core.MarketCloseTime(-1)) // Last trade
                    {
                        Logger("Liquidating all.");
                        q.Liquidate();
                    }

                    return true;
                });
            TestQuant.Plot = true;
            TestQuant.Live = true;
            Quants.Add(TestQuant.Tag, TestQuant);



            #region Quant Tests

            //TestQuant = new Quant(
            //    "01A",
            //    10000.00m, // Initial Cash
            //    q =>
            //    {
            //        if (core.MarketOpenTime()) // First trade
            //        {
            //            // Select Item
            //            var item = TopUniverse.Values
            //                .Where(w => w.Security.HasData)
            //                .OrderByDescending(o => o.MOMP_Daily_01)
            //                .Take(1)
            //                .SingleOrDefault();

            //            if (item != null)
            //            {
            //                var symbol = item.Security.Symbol.Value;
            //                Logger($"Security {symbol} selected.");

            //                if (!q.Holdings.ContainsKey(symbol))
            //                {
            //                    q.Liquidate();
            //                    q.SetHoldings(symbol, 1.00m);
            //                }
            //            }
            //            else
            //            {
            //                Logger("No security selected. Liquidating all.");
            //                q.Liquidate();
            //            }
            //        }

            //        if (core.MarketCloseTime(-1)) // Last trade
            //        {
            //        }

            //        return true;
            //    });
            //TestQuant.Plot = true;
            //TestQuant.Live = false;
            //Quants.Add(TestQuant.Tag, TestQuant);

            //TestQuant = new Quant(
            //    "01B",
            //    10000.00m, // Initial Cash
            //    q =>
            //    {
            //        if (core.MarketOpenTime()) // First trade
            //        {
            //        }

            //        if (core.MarketCloseTime(-1)) // Last trade
            //        {
            //            // Select Item
            //            var item = TopUniverse.Values
            //                .Where(w => w.Security.HasData)
            //                .OrderByDescending(o => o.MOMP_Daily_01)
            //                .Take(1)
            //                .SingleOrDefault();

            //            if (item != null)
            //            {
            //                var symbol = item.Security.Symbol.Value;
            //                Logger($"Security {symbol} selected.");

            //                if (!q.Holdings.ContainsKey(symbol))
            //                {
            //                    q.Liquidate();
            //                    q.SetHoldings(symbol, 1.00m);
            //                }
            //            }
            //            else
            //            {
            //                Logger("No security selected. Liquidating all.");
            //                q.Liquidate();
            //            }
            //        }

            //        return true;
            //    });
            //TestQuant.Plot = true;
            //TestQuant.Live = false;
            //Quants.Add(TestQuant.Tag, TestQuant);

            //TestQuant = new Quant(
            //    "01AB",
            //    10000.00m, // Initial Cash
            //    q =>
            //    {
            //        if (core.MarketOpenTime()) // First trade
            //        {
            //            // Select Item
            //            var item = TopUniverse.Values
            //                .Where(w => w.Security.HasData)
            //                .OrderByDescending(o => o.MOMP_Daily_01)
            //                .Take(1)
            //                .SingleOrDefault();

            //            if (item != null)
            //            {
            //                var symbol = item.Security.Symbol.Value;
            //                Logger($"Security {symbol} selected.");

            //                if (!q.Holdings.ContainsKey(symbol))
            //                {
            //                    q.Liquidate();
            //                    q.SetHoldings(symbol, 1.00m);
            //                }
            //            }
            //            else
            //            {
            //                Logger("No security selected.");
            //            }
            //        }

            //        if (core.MarketCloseTime(-1)) // Last trade
            //        {
            //            Logger("Liquidating all.");
            //            q.Liquidate();
            //        }

            //        return true;
            //    });
            //TestQuant.Plot = true;
            //TestQuant.Live = false;
            //Quants.Add(TestQuant.Tag, TestQuant);



            //TestQuant = new Quant(
            //     "05A",
            //     10000.00m, // Initial Cash
            //     q =>
            //     {
            //         if (core.MarketOpenTime()) // First trade
            //         {
            //             // Select Item
            //             var item = TopUniverse.Values
            //                  .Where(w => w.Security.HasData)
            //                  .OrderByDescending(o => o.MOMP_Daily_05)
            //                  .Take(1)
            //                  .SingleOrDefault();

            //             if (item != null)
            //             {
            //                 var symbol = item.Security.Symbol.Value;
            //                 Logger($"Security {symbol} selected.");

            //                 if (!q.Holdings.ContainsKey(symbol))
            //                 {
            //                     q.Liquidate();
            //                     q.SetHoldings(symbol, 1.00m);
            //                 }
            //             }
            //             else
            //             {
            //                 Logger("No security selected. Liquidating all.");
            //                 q.Liquidate();
            //             }
            //         }

            //         if (core.MarketCloseTime(-1)) // Last trade
            //         {
            //         }

            //         return true;
            //     });
            //TestQuant.Plot = true;
            //TestQuant.Live = false;
            //Quants.Add(TestQuant.Tag, TestQuant);

            //TestQuant = new Quant(
            //    "05B",
            //    10000.00m, // Initial Cash
            //    q =>
            //    {
            //        if (core.MarketOpenTime()) // First trade
            //        {
            //        }

            //        if (core.MarketCloseTime(-1)) // Last trade
            //        {
            //            // Select Item
            //            var item = TopUniverse.Values
            //                .Where(w => w.Security.HasData)
            //                .OrderByDescending(o => o.MOMP_Daily_05)
            //                .Take(1)
            //                .SingleOrDefault();

            //            if (item != null)
            //            {
            //                var symbol = item.Security.Symbol.Value;
            //                Logger($"Security {symbol} selected.");

            //                if (!q.Holdings.ContainsKey(symbol))
            //                {
            //                    q.Liquidate();
            //                    q.SetHoldings(symbol, 1.00m);
            //                }
            //            }
            //            else
            //            {
            //                Logger("No security selected. Liquidating all.");
            //                q.Liquidate();
            //            }
            //        }

            //        return true;
            //    });
            //TestQuant.Plot = true;
            //TestQuant.Live = false;
            //Quants.Add(TestQuant.Tag, TestQuant);

            //TestQuant = new Quant(
            //    "05AB",
            //    10000.00m, // Initial Cash
            //    q =>
            //    {
            //        if (core.MarketOpenTime()) // First trade
            //        {
            //            // Select Item
            //            var item = TopUniverse.Values
            //                .Where(w => w.Security.HasData)
            //                .OrderByDescending(o => o.MOMP_Daily_05)
            //                .Take(1)
            //                .SingleOrDefault();

            //            if (item != null)
            //            {
            //                var symbol = item.Security.Symbol.Value;
            //                Logger($"Security {symbol} selected.");

            //                if (!q.Holdings.ContainsKey(symbol))
            //                {
            //                    q.Liquidate();
            //                    q.SetHoldings(symbol, 1.00m);
            //                }
            //            }
            //            else
            //            {
            //                Logger("No security selected.");
            //            }
            //        }

            //        if (core.MarketCloseTime(-1)) // Last trade
            //        {
            //            Logger("Liquidating all.");
            //            q.Liquidate();
            //        }

            //        return true;
            //    });
            //TestQuant.Plot = true;
            //TestQuant.Live = false;
            //Quants.Add(TestQuant.Tag, TestQuant);




            //TestQuant = new Quant(
            //     "10A",
            //     10000.00m, // Initial Cash
            //     q =>
            //     {
            //         if (core.MarketOpenTime()) // First trade
            //         {
            //             // Select Item
            //             var item = TopUniverse.Values
            //                  .Where(w => w.Security.HasData)
            //                  .OrderByDescending(o => o.MOMP_Daily_10)
            //                  .Take(1)
            //                  .SingleOrDefault();

            //             if (item != null)
            //             {
            //                 var symbol = item.Security.Symbol.Value;
            //                 Logger($"Security {symbol} selected.");

            //                 if (!q.Holdings.ContainsKey(symbol))
            //                 {
            //                     q.Liquidate();
            //                     q.SetHoldings(symbol, 1.00m);
            //                 }
            //             }
            //             else
            //             {
            //                 Logger("No security selected. Liquidating all.");
            //                 q.Liquidate();
            //             }
            //         }

            //         if (core.MarketCloseTime(-1)) // Last trade
            //         {
            //         }

            //         return true;
            //     });
            //TestQuant.Plot = true;
            //TestQuant.Live = false;
            //Quants.Add(TestQuant.Tag, TestQuant);

            //TestQuant = new Quant(
            //    "10B",
            //    10000.00m, // Initial Cash
            //    q =>
            //    {
            //        if (core.MarketOpenTime()) // First trade
            //        {
            //        }

            //        if (core.MarketCloseTime(-1)) // Last trade
            //        {
            //            // Select Item
            //            var item = TopUniverse.Values
            //                .Where(w => w.Security.HasData)
            //                .OrderByDescending(o => o.MOMP_Daily_10)
            //                .Take(1)
            //                .SingleOrDefault();

            //            if (item != null)
            //            {
            //                var symbol = item.Security.Symbol.Value;
            //                Logger($"Security {symbol} selected.");

            //                if (!q.Holdings.ContainsKey(symbol))
            //                {
            //                    q.Liquidate();
            //                    q.SetHoldings(symbol, 1.00m);
            //                }
            //            }
            //            else
            //            {
            //                Logger("No security selected. Liquidating all.");
            //                q.Liquidate();
            //            }
            //        }

            //        return true;
            //    });
            //TestQuant.Plot = true;
            //TestQuant.Live = false;
            //Quants.Add(TestQuant.Tag, TestQuant);

            //TestQuant = new Quant(
            //    "10AB",
            //    10000.00m, // Initial Cash
            //    q =>
            //    {
            //        if (core.MarketOpenTime()) // First trade
            //        {
            //            // Select Item
            //            var item = TopUniverse.Values
            //                .Where(w => w.Security.HasData)
            //                .OrderByDescending(o => o.MOMP_Daily_10)
            //                .Take(1)
            //                .SingleOrDefault();

            //            if (item != null)
            //            {
            //                var symbol = item.Security.Symbol.Value;
            //                Logger($"Security {symbol} selected.");

            //                if (!q.Holdings.ContainsKey(symbol))
            //                {
            //                    q.Liquidate();
            //                    q.SetHoldings(symbol, 1.00m);
            //                }
            //            }
            //            else
            //            {
            //                Logger("No security selected.");
            //            }
            //        }

            //        if (core.MarketCloseTime(-1)) // Last trade
            //        {
            //            Logger("Liquidating all.");
            //            q.Liquidate();
            //        }

            //        return true;
            //    });
            //TestQuant.Plot = true;
            //TestQuant.Live = false;
            //Quants.Add(TestQuant.Tag, TestQuant);



            //TestQuant = new Quant(
            //     "10A",
            //     100000.00m, // Initial Cash
            //     q =>
            //     {
            //         if (core.MarketOpenTime()) // First trade
            //         {
            //             // Select Item
            //             var item = TopUniverse.Values
            //                  .Where(w => w.Security.HasData)
            //                  .Where(w => w.MOMP_Minute_01 > 0)
            //                  .OrderByDescending(o => o.MOMP_Daily_10)
            //                  .Take(1)
            //                  .SingleOrDefault();

            //             if (item != null)
            //             {
            //                 var symbol = item.Security.Symbol.Value;
            //                 Logger($"Security {symbol} selected.");

            //                 if (!q.Holdings.ContainsKey(symbol))
            //                 {
            //                     q.Liquidate();
            //                     q.SetHoldings(symbol, 1.00m);
            //                 }
            //             }
            //             else
            //             {
            //                 Logger("No security selected. Liquidating all.");
            //                 q.Liquidate();
            //             }
            //         }

            //         if (core.MarketCloseTime(-1)) // Last trade
            //         {
            //         }

            //         return true;
            //     });
            //TestQuant.Plot = true;
            //TestQuant.Live = false;
            //Quants.Add(TestQuant.Tag, TestQuant);

            //TestQuant = new Quant(
            //    "10B",
            //    100000.00m, // Initial Cash
            //    q =>
            //    {
            //        if (core.MarketOpenTime()) // First trade
            //        {
            //        }

            //        if (core.MarketCloseTime(-1)) // Last trade
            //        {
            //            // Select Item
            //            var item = TopUniverse.Values
            //                .Where(w => w.Security.HasData)
            //                .Where(w => w.MOMP_Minute_01 > 0)
            //                .OrderByDescending(o => o.MOMP_Daily_10)
            //                .Take(1)
            //                .SingleOrDefault();

            //            if (item != null)
            //            {
            //                var symbol = item.Security.Symbol.Value;
            //                Logger($"Security {symbol} selected.");

            //                if (!q.Holdings.ContainsKey(symbol))
            //                {
            //                    q.Liquidate();
            //                    q.SetHoldings(symbol, 1.00m);
            //                }
            //            }
            //            else
            //            {
            //                Logger("No security selected. Liquidating all.");
            //                q.Liquidate();
            //            }
            //        }

            //        return true;
            //    });
            //TestQuant.Plot = true;
            //TestQuant.Live = false;
            //Quants.Add(TestQuant.Tag, TestQuant);

            //TestQuant = new Quant(
            //    "10AB",
            //    5000.00m, // Initial Cash
            //    q =>
            //    {
            //        if (core.MarketOpenTime()) // First trade
            //        {
            //            // Select Item
            //            var item = TopUniverse.Values
            //                .Where(w => w.Security.HasData)
            //                .Where(w => w.MOMP_Minute_01 > 0)
            //                .OrderByDescending(o => o.MOMP_Daily_10)
            //                .Take(1)
            //                .SingleOrDefault();

            //            if (item != null)
            //            {
            //                var symbol = item.Security.Symbol.Value;
            //                Logger($"Security {symbol} selected.");

            //                if (!q.Holdings.ContainsKey(symbol))
            //                {
            //                    q.Liquidate();
            //                    q.SetHoldings(symbol, 1.00m);
            //                }
            //            }
            //            else
            //            {
            //                Logger("No security selected.");
            //            }
            //        }

            //        if (core.MarketCloseTime(-1)) // Last trade
            //        {
            //            Logger("Liquidating all.");
            //            q.Liquidate();
            //        }

            //        return true;
            //    });
            //TestQuant.Plot = true;
            //TestQuant.Live = true;
            //Quants.Add(TestQuant.Tag, TestQuant);




            //TestQuant = new Quant(
            //     "20A",
            //     10000.00m, // Initial Cash
            //     q =>
            //     {
            //         if (core.MarketOpenTime()) // First trade
            //         {
            //             // Select Item
            //             var item = TopUniverse.Values
            //                  .Where(w => w.Security.HasData)
            //                  .OrderByDescending(o => o.MOMP_Daily_20)
            //                  .Take(1)
            //                  .SingleOrDefault();

            //             if (item != null)
            //             {
            //                 var symbol = item.Security.Symbol.Value;
            //                 Logger($"Security {symbol} selected.");

            //                 if (!q.Holdings.ContainsKey(symbol))
            //                 {
            //                     q.Liquidate();
            //                     q.SetHoldings(symbol, 1.00m);
            //                 }
            //             }
            //             else
            //             {
            //                 Logger("No security selected. Liquidating all.");
            //                 q.Liquidate();
            //             }
            //         }

            //         if (core.MarketCloseTime(-1)) // Last trade
            //         {
            //         }

            //         return true;
            //     });
            //TestQuant.Plot = true;
            //TestQuant.Live = false;
            //Quants.Add(TestQuant.Tag, TestQuant);

            //TestQuant = new Quant(
            //    "20B",
            //    10000.00m, // Initial Cash
            //    q =>
            //    {
            //        if (core.MarketOpenTime()) // First trade
            //        {
            //        }

            //        if (core.MarketCloseTime(-1)) // Last trade
            //        {
            //            // Select Item
            //            var item = TopUniverse.Values
            //                .Where(w => w.Security.HasData)
            //                .OrderByDescending(o => o.MOMP_Daily_20)
            //                .Take(1)
            //                .SingleOrDefault();

            //            if (item != null)
            //            {
            //                var symbol = item.Security.Symbol.Value;
            //                Logger($"Security {symbol} selected.");

            //                if (!q.Holdings.ContainsKey(symbol))
            //                {
            //                    q.Liquidate();
            //                    q.SetHoldings(symbol, 1.00m);
            //                }
            //            }
            //            else
            //            {
            //                Logger("No security selected. Liquidating all.");
            //                q.Liquidate();
            //            }
            //        }

            //        return true;
            //    });
            //TestQuant.Plot = true;
            //TestQuant.Live = false;
            //Quants.Add(TestQuant.Tag, TestQuant);

            //TestQuant = new Quant(
            //    "20AB",
            //    10000.00m, // Initial Cash
            //    q =>
            //    {
            //        if (core.MarketOpenTime()) // First trade
            //        {
            //            // Select Item
            //            var item = TopUniverse.Values
            //                .Where(w => w.Security.HasData)
            //                .OrderByDescending(o => o.MOMP_Daily_20)
            //                .Take(1)
            //                .SingleOrDefault();

            //            if (item != null)
            //            {
            //                var symbol = item.Security.Symbol.Value;
            //                Logger($"Security {symbol} selected.");

            //                if (!q.Holdings.ContainsKey(symbol))
            //                {
            //                    q.Liquidate();
            //                    q.SetHoldings(symbol, 1.00m);
            //                }
            //            }
            //            else
            //            {
            //                Logger("No security selected.");
            //            }
            //        }

            //        if (core.MarketCloseTime(-1)) // Last trade
            //        {
            //            Logger("Liquidating all.");
            //            q.Liquidate();
            //        }

            //        return true;
            //    });
            //TestQuant.Plot = true;
            //TestQuant.Live = false;
            //Quants.Add(TestQuant.Tag, TestQuant);

            #endregion
        }
    }
}
