using SC2APIProtocol;

namespace Sharky
{
    public class AttackData
    {
        public int ArmyFoodAttack { get; set; }
        public int ArmyFoodRetreat { get; set; }
        public Point2D ArmyPoint { get; set; }
        public bool Attacking { get; set; }
        public float AttackTrigger { get; set; }
        public bool CustomAttackFunction { get; set; }
        public float RetreatTrigger { get; set; }
        public TargetPriorityCalculation TargetPriorityCalculation { get; set; }
        public bool UseAttackDataManager { get; set; }
    }
}