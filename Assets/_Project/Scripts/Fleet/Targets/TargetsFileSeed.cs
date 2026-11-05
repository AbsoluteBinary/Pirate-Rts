using _Project.Scripts.Fleet.Targets;
using UnityEngine;

public class TargetsFileSeed : MonoBehaviour
{
    [ContextMenu("Seed TargetIron")]
    public void Seed()
    {
        TargetsFile.WriteRecord(new TargetConditionRecord
        {
            targetId = "TargetIron",
            current = 500f,
            condition = TargetCondition.Intact
        });
        Debug.Log($"Seeded → {TargetsFile.FilePath}");
    }
}