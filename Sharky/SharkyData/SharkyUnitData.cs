﻿using Google.Protobuf.Collections;
using SC2APIProtocol;
using System.Collections.Generic;

namespace Sharky
{
    public class SharkyUnitData
    {
        public RepeatedField<Effect> Effects;
        public RepeatedField<uint> ResearchedUpgrades;

        /// <summary>
        /// key is ability, value is the unitType it belongs to
        /// </summary>
        public Dictionary<Abilities, UnitTypes> UnitAbilities;

        public Dictionary<Abilities, float> AbilityCooldownTimes { get; set; }
        public HashSet<UnitTypes> AbilityDetectionTypes { get; set; }
        public Dictionary<UnitTypes, TrainingTypeData> AddOnData { get; set; }
        public HashSet<UnitTypes> AirSplashDamagers { get; set; }
        public Dictionary<UnitTypes, BuildingTypeData> BuildingData { get; set; }
        public HashSet<Buffs> CarryingMineralBuffs { get; set; }
        public HashSet<Buffs> CarryingResourceBuffs { get; set; }
        public HashSet<UnitTypes> CloakableAttackers { get; set; }
        public Dictionary<Point2D, uint> CorrosiveBiles { get; set; }
        public HashSet<UnitTypes> DefensiveStructureTypes { get; set; }
        public HashSet<UnitTypes> DetectionTypes { get; set; }
        public HashSet<UnitTypes> GasGeyserRefineryTypes { get; set; }
        public HashSet<UnitTypes> GasGeyserTypes { get; set; }
        public HashSet<Abilities> GatheringAbilities { get; set; }
        public HashSet<UnitTypes> GroundSplashDamagers { get; set; }
        public HashSet<UnitTypes> MineralFieldTypes { get; set; }
        public HashSet<Abilities> MiningAbilities { get; set; }
        public Dictionary<UnitTypes, TrainingTypeData> MorphData { get; set; }
        public HashSet<UnitTypes> NoWeaponCooldownTypes { get; set; }
        public HashSet<UnitTypes> ProtossTypes { get; set; }
        public HashSet<UnitTypes> ReactorTypes { get; set; }
        public HashSet<UnitTypes> ResourceCenterTypes { get; set; }
        public HashSet<UnitTypes> TechLabTypes { get; set; }
        public HashSet<UnitTypes> TerranTypes { get; set; }
        public Dictionary<UnitTypes, TrainingTypeData> TrainingData { get; set; }
        public HashSet<UnitTypes> UndeadTypes { get; set; }
        public Dictionary<UnitTypes, UnitTypeData> UnitData { get; set; }
        public Dictionary<Upgrades, TrainingTypeData> UpgradeData { get; set; }
        public Dictionary<Abilities, float> WarpInCooldownTimes { get; set; }
        public HashSet<UnitTypes> ZergTypes { get; set; }
    }
}