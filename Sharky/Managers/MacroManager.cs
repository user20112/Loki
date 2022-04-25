using SC2APIProtocol;
using Sharky.Builds;
using Sharky.Builds.MacroServices;
using Sharky.Macro;
using System.Collections.Generic;

namespace Sharky.Managers
{
    public class MacroManager : SharkyManager
    {
        private AddOnBuilder AddOnBuilder;
        private BuildAddOnSwapService BuildAddOnSwapService;
        private BuildDefenseService BuildDefenseService;
        private BuildingCancelService BuildingCancelService;
        private BuildingMorpher BuildingMorpher;
        private BuildProxyService BuildProxyService;
        private BuildPylonService BuildPylonService;
        private int LastRunFrame;
        private MacroData MacroData;
        private MacroSetup MacroSetup;
        private ProductionBuilder ProductionBuilder;
        private SupplyBuilder SupplyBuilder;
        private TechBuilder TechBuilder;
        private UnfinishedBuildingCompleter UnfinishedBuildingCompleter;
        private UnitBuilder UnitBuilder;
        private UpgradeResearcher UpgradeResearcher;
        private VespeneGasBuilder VespeneGasBuilder;

        public MacroManager(LokiBot.BaseLokiBot lokiBot)
        {
            MacroSetup = lokiBot.MacroSetup;
            MacroData = lokiBot.MacroData;

            BuildPylonService = lokiBot.BuildPylonService;
            BuildDefenseService = lokiBot.BuildDefenseService;
            BuildProxyService = lokiBot.BuildProxyService;
            BuildAddOnSwapService = lokiBot.BuildAddOnSwapService;
            BuildingCancelService = lokiBot.BuildingCancelService;

            VespeneGasBuilder = lokiBot.VespeneGasBuilder;
            UnitBuilder = lokiBot.UnitBuilder;
            UpgradeResearcher = lokiBot.UpgradeResearcher;
            SupplyBuilder = lokiBot.SupplyBuilder;
            ProductionBuilder = lokiBot.ProductionBuilder;
            TechBuilder = lokiBot.TechBuilder;
            AddOnBuilder = lokiBot.AddOnBuilder;
            BuildingMorpher = lokiBot.BuildingMorpher;
            UnfinishedBuildingCompleter = lokiBot.UnfinishedBuildingCompleter;

            MacroData.DesiredUpgrades = new Dictionary<Upgrades, bool>();

            LastRunFrame = -10;
            RunFrequency = 5;
        }

        public override bool NeverSkip { get => true; }
        public int RunFrequency { get; set; }

        public override IEnumerable<SC2APIProtocol.Action> OnFrame(ResponseObservation observation)
        {
            var actions = new List<Action>();

            MacroData.FoodUsed = (int)observation.Observation.PlayerCommon.FoodUsed;
            MacroData.FoodLeft = (int)observation.Observation.PlayerCommon.FoodCap - MacroData.FoodUsed;
            MacroData.FoodArmy = (int)observation.Observation.PlayerCommon.FoodArmy;
            MacroData.Minerals = (int)observation.Observation.PlayerCommon.Minerals;
            MacroData.VespeneGas = (int)observation.Observation.PlayerCommon.Vespene;
            MacroData.Frame = (int)observation.Observation.GameLoop;

            if (LastRunFrame + RunFrequency > observation.Observation.GameLoop)
            {
                return actions;
            }
            LastRunFrame = (int)observation.Observation.GameLoop;

            actions.AddRange(BuildProxyService.BuildPylons());
            actions.AddRange(BuildProxyService.MorphBuildings());
            actions.AddRange(BuildProxyService.BuildAddOns());
            actions.AddRange(BuildProxyService.ResumePausedBuilds());
            actions.AddRange(BuildProxyService.BuildDefensiveBuildings());
            actions.AddRange(BuildProxyService.BuildProductionBuildings());
            actions.AddRange(BuildProxyService.BuildTechBuildings());
            // TODO: send new SCVs to any incomplete proxy building without one

            actions.AddRange(BuildAddOnSwapService.BuildAndSwapAddons());

            if (MacroData.Minerals >= 100)
            {
                actions.AddRange(BuildPylonService.BuildPylonsAtEveryMineralLine());
                actions.AddRange(BuildPylonService.BuildPylonsAtDefensivePoint());
                actions.AddRange(BuildPylonService.BuildPylonsAtEveryBase());
                actions.AddRange(BuildPylonService.BuildPylonsAtNextBase());
            }

            actions.AddRange(SupplyBuilder.BuildSupply());

            actions.AddRange(BuildDefenseService.BuildDefensiveBuildingsAtEveryMineralLine());
            actions.AddRange(BuildDefenseService.BuildDefensiveBuildingsAtDefensivePoint());
            actions.AddRange(BuildDefenseService.BuildDefensiveBuildingsAtEveryBase());
            actions.AddRange(BuildDefenseService.BuildDefensiveBuildingsAtNextBase());
            actions.AddRange(BuildDefenseService.BuildDefensiveBuildings());

            actions.AddRange(VespeneGasBuilder.BuildVespeneGas());

            actions.AddRange(BuildingMorpher.MorphBuildings());
            actions.AddRange(AddOnBuilder.BuildAddOns());
            actions.AddRange(ProductionBuilder.BuildProductionBuildings());
            actions.AddRange(TechBuilder.BuildTechBuildings());
            actions.AddRange(UnfinishedBuildingCompleter.SendScvToFinishIncompleteBuildings());

            actions.AddRange(UpgradeResearcher.ResearchUpgrades());
            actions.AddRange(UnitBuilder.ProduceUnits());

            actions.AddRange(BuildingCancelService.CancelBuildings());

            return actions;
        }

        public override void OnStart(ResponseGameInfo gameInfo, ResponseData data, ResponsePing pingResponse, ResponseObservation observation, uint playerId, string opponentId)
        {
            foreach (var playerInfo in gameInfo.PlayerInfo)
            {
                if (playerInfo.PlayerId == playerId)
                {
                    MacroData.Race = playerInfo.RaceActual;
                }
            }

            MacroSetup.SetupMacro(MacroData);
        }
    }
}