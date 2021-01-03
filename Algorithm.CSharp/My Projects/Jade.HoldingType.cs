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
        private class HoldingType
        {
            public UniverseType UniverseItem { get; set; }
            public int InvestedQuantity { get; set; }
            public decimal AverageBoughtPrice { get; set; }
            public decimal SoldPrice { get; set; }

            public decimal TotalValue
            {
                get
                {
                    return (UniverseItem.Security.Price * InvestedQuantity);
                }
            }

            public HoldingType()
            {
            }
        }
    }
}
