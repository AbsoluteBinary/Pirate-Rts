using _Project.Scripts.Harbour.ShipBuilder.Data;

namespace _Project.Scripts.Fleet
{
    public static class PendingLaunch
    {
        public const int Size = 5;
        public static readonly ShipBlueprint[] Ships = new ShipBlueprint[Size];
        public static int FlagShipIndex = 2;

        public static void Capture(ShipBlueprint[] slots, int flagIndex)
        {
            for (int i = 0; i < Size; i++)
                Ships[i] = slots != null && i < slots.Length ? slots[i] : null;
            FlagShipIndex = flagIndex;
        }
    }
}