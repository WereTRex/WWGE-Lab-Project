namespace UnityHFSM
{
    /// <summary> An interface for states that can recieve Events (/Triggers), such as State Machines.</summary>
    public interface ITriggerable<TEvent>
    {
        void Trigger(TEvent trigger);
    }
}