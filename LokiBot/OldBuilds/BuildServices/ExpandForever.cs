using Sharky;

namespace LokiBot.Builds.BuildServices
{
    public class ExpandForever
    {
        private MacroData MacroData;
        private UnitCountService UnitCountService;

        public ExpandForever(Sharky.LokiBot.LokiBot lokiBot)
        {
            UnitCountService = lokiBot.UnitCountService;
            MacroData = lokiBot.MacroData;
        }

        public void OnFrame()
        {
            if (MacroData.Minerals > 650)
            {
                if (MacroData.DesiredProductionCounts[UnitTypes.TERRAN_COMMANDCENTER] <= UnitCountService.EquivalentTypeCount(UnitTypes.TERRAN_COMMANDCENTER))
                {
                    MacroData.DesiredProductionCounts[UnitTypes.TERRAN_COMMANDCENTER]++;
                }
            }

            MorphCommandCenters();
        }

        private void MorphCommandCenters()
        {
            if (UnitCountService.EquivalentTypeCompleted(UnitTypes.TERRAN_COMMANDCENTER) >= 2)
            {
                if (UnitCountService.EquivalentTypeCompleted(UnitTypes.TERRAN_BARRACKS) > 0)
                {
                    if (MacroData.DesiredMorphCounts[UnitTypes.TERRAN_ORBITALCOMMAND] < 2)
                    {
                        MacroData.DesiredMorphCounts[UnitTypes.TERRAN_ORBITALCOMMAND] = 2;
                    }
                }
            }

            if (UnitCountService.EquivalentTypeCompleted(UnitTypes.TERRAN_COMMANDCENTER) > 3)
            {
                if (MacroData.DesiredTechCounts[UnitTypes.TERRAN_ENGINEERINGBAY] < 1)
                {
                    MacroData.DesiredTechCounts[UnitTypes.TERRAN_ENGINEERINGBAY] = 1;
                }
                if (UnitCountService.EquivalentTypeCompleted(UnitTypes.TERRAN_ENGINEERINGBAY) > 0)
                {
                    if (MacroData.DesiredMorphCounts[UnitTypes.TERRAN_PLANETARYFORTRESS] < UnitCountService.EquivalentTypeCompleted(UnitTypes.TERRAN_COMMANDCENTER) - 3)
                    {
                        MacroData.DesiredMorphCounts[UnitTypes.TERRAN_PLANETARYFORTRESS] = UnitCountService.EquivalentTypeCompleted(UnitTypes.TERRAN_COMMANDCENTER) - 3;
                    }
                }
            }
        }
    }
}