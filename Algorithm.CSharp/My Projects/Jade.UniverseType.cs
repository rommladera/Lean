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
        private class UniverseType
        {
            public Security Security { get; set; }

            public VolumeWeightedAveragePriceIndicator VWAP_01, VWAP_02, VWAP_04, VWAP_08, VWAP_16;
            public MomentumPercent MOMP_Minute_01, MOMP_Minute_02, MOMP_Minute_04, MOMP_Minute_08, MOMP_Minute_16;
            public MomentumPercent MOMP_Daily_01, MOMP_Daily_05, MOMP_Daily_10, MOMP_Daily_20, MOMP_Daily_40;
            public ExponentialMovingAverage EMA_Minute_02, EMA_Minute_04, EMA_Minute_08, EMA_Minute_16;
            public RelativeStrengthIndex RSI_01, RSI_02, RSI_04, RSI_08, RSI_16;

            public UniverseType(Security security)
            {
                Security = security;

                VWAP_01 = core.VWAP(security.Symbol, 1, Resolution.Minute);
                VWAP_02 = core.VWAP(security.Symbol, 2, Resolution.Minute);
                VWAP_04 = core.VWAP(security.Symbol, 4, Resolution.Minute);
                VWAP_08 = core.VWAP(security.Symbol, 8, Resolution.Minute);
                VWAP_16 = core.VWAP(security.Symbol, 16, Resolution.Minute);

                MOMP_Minute_01 = core.MOMP(security.Symbol, 1, Resolution.Minute);
                MOMP_Minute_02 = core.MOMP(security.Symbol, 2, Resolution.Minute);
                MOMP_Minute_04 = core.MOMP(security.Symbol, 4, Resolution.Minute);
                MOMP_Minute_08 = core.MOMP(security.Symbol, 8, Resolution.Minute);
                MOMP_Minute_16 = core.MOMP(security.Symbol, 16, Resolution.Minute);

                MOMP_Daily_01 = core.MOMP(security.Symbol, 1, Resolution.Daily);
                MOMP_Daily_05 = core.MOMP(security.Symbol, 5, Resolution.Daily);
                MOMP_Daily_10 = core.MOMP(security.Symbol, 10, Resolution.Daily);
                MOMP_Daily_20 = core.MOMP(security.Symbol, 20, Resolution.Daily);
                MOMP_Daily_40 = core.MOMP(security.Symbol, 40, Resolution.Daily);

                EMA_Minute_02 = core.EMA(security.Symbol, 2, Resolution.Minute);
                EMA_Minute_04 = core.EMA(security.Symbol, 4, Resolution.Minute);
                EMA_Minute_08 = core.EMA(security.Symbol, 8, Resolution.Minute);
                EMA_Minute_16 = core.EMA(security.Symbol, 16, Resolution.Minute);

                RSI_01 = core.RSI(security.Symbol, 1, MovingAverageType.Exponential, Resolution.Minute);
                RSI_02 = core.RSI(security.Symbol, 2, MovingAverageType.Exponential, Resolution.Minute);
                RSI_04 = core.RSI(security.Symbol, 4, MovingAverageType.Exponential, Resolution.Minute);
                RSI_08 = core.RSI(security.Symbol, 8, MovingAverageType.Exponential, Resolution.Minute);
                RSI_16 = core.RSI(security.Symbol, 16, MovingAverageType.Exponential, Resolution.Minute);

                var hist = core.History(security.Symbol, 32, Resolution.Minute);
                foreach (var bar in hist)
                {
                    MOMP_Minute_01.Update(bar.EndTime, bar.Close);
                    MOMP_Minute_02.Update(bar.EndTime, bar.Close);
                    MOMP_Minute_04.Update(bar.EndTime, bar.Close);
                    MOMP_Minute_08.Update(bar.EndTime, bar.Close);
                    MOMP_Minute_16.Update(bar.EndTime, bar.Close);

                    EMA_Minute_02.Update(bar.EndTime, bar.Close);
                    EMA_Minute_04.Update(bar.EndTime, bar.Close);
                    EMA_Minute_08.Update(bar.EndTime, bar.Close);
                    EMA_Minute_16.Update(bar.EndTime, bar.Close);

                    RSI_01.Update(bar.EndTime, bar.Close);
                    RSI_02.Update(bar.EndTime, bar.Close);
                    RSI_04.Update(bar.EndTime, bar.Close);
                    RSI_08.Update(bar.EndTime, bar.Close);
                    RSI_16.Update(bar.EndTime, bar.Close);
                }

                hist = core.History(security.Symbol, 40, Resolution.Daily);
                foreach (var bar in hist)
                {
                    MOMP_Daily_01.Update(bar.EndTime, bar.Close);
                    MOMP_Daily_05.Update(bar.EndTime, bar.Close);
                    MOMP_Daily_10.Update(bar.EndTime, bar.Close);
                    MOMP_Daily_20.Update(bar.EndTime, bar.Close);
                    MOMP_Daily_40.Update(bar.EndTime, bar.Close);
                }
            }
        }
    }
}
