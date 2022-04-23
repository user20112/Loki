using System.Collections.Generic;

namespace Sharky.Counter
{
    public class CounterInfoService
    {
        public CounterInfoService(ZergCounterInfoService zergCounterInfoService, TerranCounterInfoService terranCounterInfoService, ProtossCounterInfoService protossCounterInfoService)
        {
            CounterInfoData = new Dictionary<UnitTypes, CounterInfo>();

            zergCounterInfoService.PopulateZergInfo(CounterInfoData);
            protossCounterInfoService.PopulateProtossInfo(CounterInfoData);
            terranCounterInfoService.PopulateTerranInfo(CounterInfoData);
        }

        public Dictionary<UnitTypes, CounterInfo> CounterInfoData { get; private set; }
    }
}