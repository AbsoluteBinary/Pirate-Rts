using System;
using System.Collections.Generic;

namespace _Project.Scripts.Fleet.Targets
{
    [Serializable]
    public class TargetConditionRecord
    {
        public string targetId;
        public float current;
        public TargetCondition condition = TargetCondition.Intact;
    }

    [Serializable]
    public class TargetsSaveData
    {
        public int version = 1;
        public List<TargetConditionRecord> targets = new List<TargetConditionRecord>();
    }
}