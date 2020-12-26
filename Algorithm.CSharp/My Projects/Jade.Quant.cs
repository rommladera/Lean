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
        private class Quant
        {
            public string Tag = "";
            private decimal Cash = 0.00m;
            public Func<Quant, bool> QuantLogic;


            public bool Live = false;
            public bool Plot = false;

            public Dictionary<string, HoldingType> Holdings = new Dictionary<string, HoldingType>();

            //public UniverseType InvestedUniverse = null;
            //public int InvestedQuantity = 0;
            //public decimal InvestmentBoughtPrice, InvestmentSoldPrice;

            public int TotalOrders = 0;
            public int TotalWins = 0;

            //public decimal TotalHoldingsValue
            //{
            //    get
            //    {
            //        return Holdings.Values.Sum(s => s.UniverseItem.Security.Price);
            //    }
            //}

            //public decimal TotalPortfolioValue
            //{
            //    get
            //    {
            //        return TotalHoldingsValue + Cash;
            //    }
            //}

            // public decimal Return

            public Quant(string tag, decimal cash, Func<Quant, bool> quantLogic)
            {
                Tag = tag;
                Cash = cash;
                QuantLogic = quantLogic;

                core.Schedule.On(core.DateRules.EveryDay(), core.TimeRules.Every(TimeSpan.FromMinutes(1)), () =>
                {
                    if (core.IsWarmingUp) return;
                    OnTick();
                });
            }

            public void SetHoldings(string symbol, decimal percentage)
            {
                // check for valid symbol
                if (symbol == null || symbol == "")
                {
                    core.Debug($",{core.Time}, Quant {Tag}, Invalid symbol.");
                    return;
                }

                // check if universe contain symbol
                if (!core.MyUniverse.Keys.Contains(symbol))
                {
                    core.Debug($",{core.Time}, Quant {Tag}, Universe does not contain {symbol}.");
                    return;
                }

                var universeItem = core.MyUniverse[symbol];
                var security = universeItem.Security;

                var totalHoldingsValue = Holdings.Values.Sum(s => s.UniverseItem.Security.Price);
                var totalPortfolioValue = totalHoldingsValue + Cash;
                var targetValue = totalPortfolioValue * percentage;

                var price = security.Price;
                var targetQuantity = (int)(targetValue / price);

                var investedQuantity = (Holdings.Keys.Contains(symbol))
                    ? Holdings[symbol].InvestedQuantity
                    : 0;

                var quantityAdjust = targetQuantity - investedQuantity;

                if (quantityAdjust == 0) return; // we're good

                MarketOrder(symbol, quantityAdjust);
            }

            public void MarketOrder(string symbol, int quantity)
            {
                if (symbol == null || symbol == "" || quantity == 0)
                {
                    core.Debug($",{core.Time}, Quant {Tag}, Invalid symbol or qunatity.");
                    return;
                }

                // check if universe contain symbol
                if (!core.MyUniverse.Keys.Contains(symbol))
                {
                    core.Debug($",{core.Time}, Quant {Tag}, Universe does not contain {symbol}.");
                    return;
                }
                var security = core.MyUniverse[symbol].Security;

                if (quantity > 0)
                {
                    // buy
                    core.Debug($",{core.Time}, Quant {Tag}, Buying {quantity} of {symbol}.");

                    var universeItem = core.MyUniverse[symbol];

                    var buyPrice = universeItem.Security.AskPrice;
                    var buyTotalPrice = buyPrice * quantity;

                    if (Cash < buyTotalPrice)
                    {
                        core.Debug($",{core.Time}, Quant {Tag}, Not enough cash. Total buy price of {buyTotalPrice} with only {Cash} on hand.");
                        return;
                    }

                    // passed, time to buy
                    Cash -= buyTotalPrice;

                    HoldingType holding;
                    if (Holdings.Keys.Contains(symbol))
                    {
                        holding = Holdings[symbol];
                    }
                    else
                    {
                        holding = new HoldingType();
                        Holdings.Add(symbol, holding);
                    }

                    holding.InvestedQuantity += quantity;
                    holding.BoughtPrice = buyPrice;
                }
                else
                {
                    // sell
                    quantity = Math.Abs(quantity); // make positive
                    core.Debug($",{core.Time}, Quant {Tag}, Selling {quantity} of {symbol}.");

                    // Check if symbol in Holdings
                    if (!Holdings.Keys.Contains(symbol))
                    {
                        core.Debug($",{core.Time}, Quant {Tag}, symbol not in holdings.");
                        return;
                    }

                    // check holding quantity
                    var investedQuantity = Holdings[symbol].InvestedQuantity;
                    if (investedQuantity < quantity)
                    {
                        core.Debug($",{core.Time}, Quant {Tag}, Not enought quantity to sell, only have {investedQuantity} in holdings.");
                        return;
                    }

                    // passed, time to sell
                    var soldPrice = security.BidPrice;
                    var soldTotalPrice = soldPrice * quantity;

                    core.Debug($",{core.Time}, Quant {Tag}, Sold {quantity} of {symbol} at {soldPrice} for {soldTotalPrice} total.");

                    var holding = Holdings[symbol];

                    TotalOrders++;
                    TotalWins += (holding.BoughtPrice < soldPrice) ? 1 : 0;
                    Cash += soldTotalPrice;

                    holding.InvestedQuantity -= quantity;
                    if (holding.InvestedQuantity == 0) Holdings.Remove(symbol);
                }
            }

            // public void Liquidate()

            private void OnTick()
            {
                QuantLogic(this);
            }
        }
    }
}
