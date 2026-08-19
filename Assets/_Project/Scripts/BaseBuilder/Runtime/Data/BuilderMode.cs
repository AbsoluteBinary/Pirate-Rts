namespace _Project.Scripts.BaseBuilder.Runtime.Data
{
    public enum BuilderMode
    {
        Observation,
        Select,
        BuildOnWater,   // Land Grid + land tiles
        BuildOnLand,     // Object Grid + walls / buildings / etc.
        PickUp,
        Delete,
        Lock,
        Unlock
    }
}
