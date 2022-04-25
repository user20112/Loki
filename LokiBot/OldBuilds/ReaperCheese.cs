using SC2APIProtocol;
using Sharky;
using Sharky.Builds;
using Sharky.Builds.Terran;
using Sharky.MicroControllers;
using Sharky.MicroTasks;
using Sharky.Proxy;

namespace LokiBot.Builds
{
    public class ReaperCheese : TerranSharkyBuild
    {
        private bool OpeningAttackChatSent;
        private ProxyLocationService ProxyLocationService;
        private ProxyTask ProxyTask;

        public ReaperCheese(Sharky.LokiBot.BaseLokiBot lokiBot, IIndividualMicroController scvMicroController) : base(lokiBot)
        {
            ProxyLocationService = lokiBot.ProxyLocationService;

            OpeningAttackChatSent = false;
            ProxyTask = new ProxyTask(lokiBot.SharkyUnitData, false, 0.9f, MacroData, string.Empty, lokiBot.MicroTaskData, lokiBot.DebugService, lokiBot.ActiveUnitData, scvMicroController);
            ProxyTask.ProxyName = GetType().Name;
        }

        public override void EndBuild(int frame)
        {
            AttackData.UseAttackDataManager = true;
            ProxyTask.Disable();
        }

        public override void OnFrame(ResponseObservation observation)
        {
            SetAttack();

            if (MacroData.FoodUsed >= 15)
            {
                if (MacroData.DesiredSupplyDepots < 1)
                {
                    MacroData.DesiredSupplyDepots = 1;
                }
            }

            if (UnitCountService.EquivalentTypeCompleted(UnitTypes.TERRAN_SUPPLYDEPOT) > 0)
            {
                if (MacroData.Proxies[ProxyTask.ProxyName].DesiredProductionCounts[UnitTypes.TERRAN_BARRACKS] < 1)
                {
                    MacroData.Proxies[ProxyTask.ProxyName].DesiredProductionCounts[UnitTypes.TERRAN_BARRACKS] = 1;
                }
            }

            if (UnitCountService.EquivalentTypeCompleted(UnitTypes.TERRAN_SUPPLYDEPOT) > 0 && UnitCountService.Count(UnitTypes.TERRAN_BARRACKS) > 0)
            {
                if (MacroData.DesiredGases < 1)
                {
                    MacroData.DesiredGases = 1;
                }

                if (MacroData.DesiredProductionCounts[UnitTypes.TERRAN_BARRACKS] < 3)
                {
                    MacroData.DesiredProductionCounts[UnitTypes.TERRAN_BARRACKS] = 3;
                }
            }

            if (UnitCountService.Completed(UnitTypes.TERRAN_BARRACKS) > 0)
            {
                if (MacroData.DesiredUnitCounts[UnitTypes.TERRAN_REAPER] < 10)
                {
                    MacroData.DesiredUnitCounts[UnitTypes.TERRAN_REAPER] = 10;
                }

                if (MacroData.DesiredSupplyDepots < 2)
                {
                    MacroData.DesiredSupplyDepots = 2;
                }
            }

            if (UnitCountService.Completed(UnitTypes.TERRAN_BARRACKS) > 0 && UnitCountService.Count(UnitTypes.TERRAN_COMMANDCENTER) > 0 && UnitCountService.EquivalentTypeCompleted(UnitTypes.TERRAN_SUPPLYDEPOT) >= 2)
            {
                if (MacroData.DesiredMorphCounts[UnitTypes.TERRAN_ORBITALCOMMAND] < 1)
                {
                    MacroData.DesiredMorphCounts[UnitTypes.TERRAN_ORBITALCOMMAND] = 1;
                }

                BuildOptions.StrictSupplyCount = false;
            }

            if (UnitCountService.Count(UnitTypes.TERRAN_ORBITALCOMMAND) > 0)
            {
                BuildOptions.StrictWorkerCount = false;
                BuildOptions.StrictGasCount = false;
            }

            if (MacroData.Minerals > 500)
            {
                if (MacroData.DesiredProductionCounts[UnitTypes.TERRAN_COMMANDCENTER] <= UnitCountService.EquivalentTypeCount(UnitTypes.TERRAN_COMMANDCENTER))
                {
                    MacroData.DesiredProductionCounts[UnitTypes.TERRAN_COMMANDCENTER]++;
                }
            }
        }

        public override void StartBuild(int frame)
        {
            base.StartBuild(frame);

            BuildOptions.StrictGasCount = true;
            BuildOptions.StrictSupplyCount = true;
            BuildOptions.StrictWorkerCount = true;

            MacroData.DesiredUnitCounts[UnitTypes.TERRAN_SCV] = 15;

            var defenseSquadTask = (DefenseSquadTask)MicroTaskData.MicroTasks["DefenseSquadTask"];
            defenseSquadTask.Enable();

            MicroTaskData.MicroTasks[GetType().Name] = ProxyTask;
            var proxyLocation = ProxyLocationService.GetCliffProxyLocation();
            MacroData.Proxies[ProxyTask.ProxyName] = new ProxyData(proxyLocation, MacroData);
            ProxyTask.Enable();

            AttackData.CustomAttackFunction = true;
            AttackData.UseAttackDataManager = false;
        }

        public override bool Transition(int frame)
        {
            if (UnitCountService.EquivalentTypeCount(UnitTypes.TERRAN_COMMANDCENTER) >= 2)
            {
                return true;
            }

            return false;
        }

        private void SetAttack()
        {
            if (UnitCountService.Completed(UnitTypes.TERRAN_REAPER) > 5)
            {
                AttackData.Attacking = true;
                if (!OpeningAttackChatSent)
                {
                    ChatService.SendChatType("ReaperCheese-FirstAttack");
                    OpeningAttackChatSent = true;
                }
            }
        }
    }
}