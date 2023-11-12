[System.Serializable, System.Flags]
public enum Faction
{
    Unaligned = 0,
    Player = 1 << 1,
    Enemy = 1 << 2
}
