namespace Sharky.Builds
{
    public interface IMacroBalancer
    {
        void BalanceAddOns();

        void BalanceDefensiveBuildings();

        void BalanceGases();

        void BalanceGasWorkers();

        void BalanceMorphs();

        void BalanceProduction();

        void BalanceProductionBuildings();

        void BalanceSupply();

        void BalanceTech();
    }
}