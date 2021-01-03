﻿using QuantConnect.Brokerages;
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

                core.Debug($",{core.Time}, Quant {Tag}, SetHoldings symbol={symbol}, percentage={percentage}");

                var universeItem = core.MyUniverse[symbol];
                var security = universeItem.Security;

                var totalHoldingsValue = Holdings.Values.Sum(s => s.TotalValue);
                var totalPortfolioValue = totalHoldingsValue + Cash;
                var targetValue = totalPortfolioValue * percentage;
                core.Debug($",{core.Time}, Quant {Tag}, totalHoldingsValue={totalHoldingsValue}, totalPortfolioValue={totalPortfolioValue}, targetValue={targetValue}");

                // If we currently have it, find the price to adust, and determine how many quantity to get there
                decimal price;
                int quantityAdjust;
                var totalHoldingValue = Holdings.Values
                    .Where(w => w.UniverseItem.Security.Symbol.Value == symbol)
                    .Sum(s => s.TotalValue);
                var targetAdjustValue = targetValue - totalHoldingValue;
                core.Debug($",{core.Time}, Quant {Tag}, totalHoldingValue={totalHoldingValue}, targetAdjustValue={targetAdjustValue}");
                if (targetAdjustValue == 0)
                {
                    // Already there
                    return;
                }
                else if (targetAdjustValue > 0)
                {
                    // we buy to get to target
                    price = security.AskPrice;
                    quantityAdjust = (int)(targetAdjustValue / price);
                }
                else
                {
                    // we sell to get to target
                    price = security.BidPrice;
                    quantityAdjust = (int)(targetAdjustValue / price);
                    quantityAdjust -= 1; // one more to go below the target
                }
                core.Debug($",{core.Time}, Quant {Tag}, price={price}, quantityAdjust={quantityAdjust}");

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

                        // calculate average bought price
                        var totalBoughtPrice = holding.InvestedQuantity * holding.AverageBoughtPrice;
                        holding.AverageBoughtPrice = (buyTotalPrice + totalBoughtPrice) / (holding.InvestedQuantity + quantity);
                    }
                    else
                    {
                        holding = new HoldingType();
                        holding.UniverseItem = universeItem;
                        holding.AverageBoughtPrice = buyPrice;
                        Holdings.Add(symbol, holding);
                    }

                    holding.InvestedQuantity += quantity;

                    core.Debug($",{core.Time}, Quant {Tag}, Bought {quantity} of {symbol} at {buyPrice} for {buyTotalPrice} total.");
                }
                else
                {
                    // sell
                    // quantity = Math.Abs(quantity); // make positive
                    core.Debug($",{core.Time}, Quant {Tag}, Selling {-quantity} of {symbol}.");

                    // Check if symbol in Holdings
                    if (!Holdings.Keys.Contains(symbol))
                    {
                        core.Debug($",{core.Time}, Quant {Tag}, symbol not in holdings.");
                        return;
                    }

                    // check holding quantity
                    var investedQuantity = Holdings[symbol].InvestedQuantity;
                    if (investedQuantity < -quantity)
                    {
                        core.Debug($",{core.Time}, Quant {Tag}, Not enough quantity to sell, only have {investedQuantity} in holdings. So selling all");
                        quantity = -investedQuantity;
                    }

                    // passed, time to sell
                    var soldPrice = security.BidPrice;
                    var soldTotalPrice = soldPrice * -quantity;

                    var holding = Holdings[symbol];

                    TotalOrders++;
                    TotalWins += (holding.AverageBoughtPrice < soldPrice) ? 1 : 0;
                    Cash += soldTotalPrice;

                    holding.InvestedQuantity += quantity;
                    if (holding.InvestedQuantity == 0) Holdings.Remove(symbol);

                    core.Debug($",{core.Time}, Quant {Tag}, Sold {-quantity} of {symbol} at {soldPrice} for {soldTotalPrice} total.");

                    // quantity = -quantity;
                }

                if (Live)
                {
                    core.Debug($",{core.Time}, Quant {Tag}, Live MarketOrder for {quantity} of {symbol}.");
                    var sec = core.MyUniverse[symbol].Security;
                    // if (core.Securities.ContainsKey(symbol))
                        core.MarketOrder(sec.Symbol, quantity);
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
