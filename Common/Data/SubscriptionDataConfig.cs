﻿/*
 * QUANTCONNECT.COM - Democratizing Finance, Empowering Individuals.
 * Lean Algorithmic Trading Engine v2.0. Copyright 2014 QuantConnect Corporation.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License"); 
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using QuantConnect.Data.Consolidators;
using QuantConnect.Securities;

namespace QuantConnect.Data
{
    /// <summary>
    /// Subscription data required including the type of data.
    /// </summary>
    public class SubscriptionDataConfig
    {
        /// <summary>
        /// Type of data
        /// </summary>
        public readonly Type Type;
        /// <summary>
        /// Security type of this data subscription
        /// </summary>
        public readonly SecurityType SecurityType;
        /// <summary>
        /// Symbol of the asset we're requesting.
        /// </summary>
        public readonly string Symbol;
        /// <summary>
        /// Resolution of the asset we're requesting, second minute or tick
        /// </summary>
        public readonly Resolution Resolution;
        /// <summary>
        /// Timespan increment between triggers of this data:
        /// </summary>
        public readonly TimeSpan Increment;
        /// <summary>
        /// True if wish to send old data when time gaps in data feed.
        /// </summary>
        public readonly bool FillDataForward;
        /// <summary>
        /// Boolean Send Data from between 4am - 8am (Equities Setting Only)
        /// </summary>
        public readonly bool ExtendedMarketHours;
        /// <summary>
        /// True if the data type has OHLC properties, even if dynamic data
        /// </summary>
        public readonly bool IsTradeBar;
        /// <summary>
        /// True if the data type has a Volume property, even if it is dynamic data
        /// </summary>
        public readonly bool HasVolume;
        /// <summary>
        /// True if this subscription was added for the sole purpose of providing currency conversion rates via <see cref="CashBook.EnsureCurrencyDataFeeds"/>
        /// </summary>
        public readonly bool IsInternalFeed;
        /// <summary>
        /// The subscription index from the SubscriptionManager
        /// </summary>
        public readonly int SubscriptionIndex;

        /// <summary>
        /// Price Scaling Factor:
        /// </summary>
        public decimal PriceScaleFactor;
        /// <summary>
        /// Symbol Mapping: When symbols change over time (e.g. CHASE-> JPM) need to update the symbol requested.
        /// </summary>
        public string MappedSymbol;
        /// <summary>
        /// Consolidators that are registred with this subscription
        /// </summary>
        public readonly List<IDataConsolidator> Consolidators; 

        /// <summary>
        /// Constructor for Data Subscriptions
        /// </summary>
        /// <param name="objectType">Type of the data objects.</param>
        /// <param name="securityType">SecurityType Enum Set Equity/FOREX/Futures etc.</param>
        /// <param name="symbol">Symbol of the asset we're requesting</param>
        /// <param name="resolution">Resolution of the asset we're requesting</param>
        /// <param name="fillForward">Fill in gaps with historical data</param>
        /// <param name="extendedHours">Equities only - send in data from 4am - 8pm</param>
        /// <param name="isTradeBar">Set to true if the objectType has Open, High, Low, and Close properties defines, does not need to directly derive from the TradeBar class
        /// This is used for the DynamicDataConsolidator</param>
        /// <param name="hasVolume">Set to true if the objectType has a Volume property defined. This is used for the DynamicDataConsolidator</param>
        /// <param name="isInternalFeed">Set to true if this subscription is added for the sole purpose of providing currency conversion rates,
        /// setting this flag to true will prevent the data from being sent into the algorithm's OnData methods</param>
        /// <param name="subscriptionIndex">The subscription index from the SubscriptionManager, this MUST equal the subscription's index or all hell will break loose!</param>
        public SubscriptionDataConfig(Type objectType, 
            SecurityType securityType, 
            string symbol, 
            Resolution resolution, 
            bool fillForward, 
            bool extendedHours, 
            bool isTradeBar, 
            bool hasVolume, 
            bool isInternalFeed, 
            int subscriptionIndex)
        {
            Type = objectType;
            SecurityType = securityType;
            Resolution = resolution;
            Symbol = symbol.ToUpper();
            FillDataForward = fillForward;
            ExtendedMarketHours = extendedHours;
            IsTradeBar = isTradeBar;
            HasVolume = hasVolume;
            PriceScaleFactor = 1;
            MappedSymbol = symbol;
            IsInternalFeed = isInternalFeed;
            SubscriptionIndex = subscriptionIndex;
            Consolidators = new List<IDataConsolidator>();

            switch (resolution)
            {
                case Resolution.Tick:
                    //Ticks are individual sales and fillforward doesn't apply.
                    Increment = TimeSpan.FromSeconds(0);
                    FillDataForward = false;
                    break;
                case Resolution.Second:
                    Increment = TimeSpan.FromSeconds(1);
                    break;
                case Resolution.Minute:
                    Increment = TimeSpan.FromMinutes(1);
                    break;
                case Resolution.Hour:
                    Increment = TimeSpan.FromHours(1);
                    break;
                case Resolution.Daily:
                    Increment = TimeSpan.FromDays(1);
                    break;
                default:
                    throw new InvalidEnumArgumentException("Unexpected Resolution: " + resolution);
            }
        }

        /// <summary>
        /// Update the price scaling factor for this subscription:
        /// -> Used for backwards scaling _equity_ prices to adjust for splits and dividends. Unused
        /// </summary>
        public void SetPriceScaleFactor(decimal newFactor) 
        {
            PriceScaleFactor = newFactor;
        }

        /// <summary>
        /// Update the mapped symbol stored here: 
        /// </summary>
        /// <param name="newSymbol"></param>
        public void SetMappedSymbol(string newSymbol) 
        {
            MappedSymbol = newSymbol;
        }
    }
}
