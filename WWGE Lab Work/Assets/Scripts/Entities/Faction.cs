/// <summary> A enumerator representing the factions of an entity.</summary>
[System.Serializable, System.Flags]
public enum Faction
{
    Unaligned = 0,
    Player = 1 << 1,
    Enemy = 1 << 2
}
