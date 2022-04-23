using SC2APIProtocol;
using System.Linq;
using System.Numerics;

namespace Sharky.Builds.BuildingPlacement
{
    public class ResourceCenterLocator
    {
        private ActiveUnitData ActiveUnitData;
        private BaseData BaseData;
        private BuildingService BuildingService;
        private BuildOptions BuildOptions;
        private TargetingData TargetingData;

        public ResourceCenterLocator(Sharky.LokiBot.LokiBot lokiBot)
        {
            ActiveUnitData = lokiBot.ActiveUnitData;
            BaseData = lokiBot.BaseData;
            BuildingService = lokiBot.BuildingService;
            BuildOptions = lokiBot.BuildOptions;
            TargetingData = lokiBot.TargetingData;
        }

        public Point2D GetResourceCenterLocation(bool canHaveCreep)
        {
            var resourceCenters = ActiveUnitData.SelfUnits.Values.Where(u => u.UnitClassifications.Contains(UnitClassification.ResourceCenter));
            var openBases = BaseData.BaseLocations.Where(b => !resourceCenters.Any(r => Vector2.DistanceSquared(r.Position, new Vector2(b.Location.X, b.Location.Y)) < 25 || r.Unit.Orders.Any(o => o.TargetWorldSpacePos != null && o.TargetWorldSpacePos.X == b.Location.X && o.TargetWorldSpacePos.Y == b.Location.Y)));

            foreach (var openBase in openBases)
            {
                if (BuildingService.AreaBuildable(openBase.Location.X, openBase.Location.Y, 2) && !BuildingService.Blocked(openBase.Location.X, openBase.Location.Y, 2.5f, 0))
                {
                    // TODO: check if area safe
                    if (!BuildOptions.EncroachEnemyMainWithExpansions)
                    {
                        var vector = new Vector2(TargetingData.EnemyMainBasePoint.X, TargetingData.EnemyMainBasePoint.Y);
                        if (Vector2.DistanceSquared(vector, new Vector2(openBase.Location.X, openBase.Location.Y)) < 900)
                        {
                            continue;
                        }
                    }

                    if (!canHaveCreep && BuildingService.HasAnyCreep(openBase.Location.X, openBase.Location.Y, 2.5f / 2.0f))
                    {
                        continue;
                    }
                    return openBase.Location;
                }
            }
            return null;
        }
    }
}