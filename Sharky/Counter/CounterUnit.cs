namespace Sharky.Counter
{
    public class CounterUnit
    {
        public CounterUnit(UnitTypes unitTypes, float count)
        {
            UnitType = unitTypes;
            Count = count;
        }

        public float Count { get; set; }
        public UnitTypes UnitType { get; set; }
    }
}