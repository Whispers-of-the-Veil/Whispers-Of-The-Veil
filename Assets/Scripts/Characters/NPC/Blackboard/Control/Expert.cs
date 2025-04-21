namespace Characters.NPC.BlackboardSystem.Control {
    public interface IExpert {
        int GetInsistence(Blackboard blackboard);
        void Execute(Blackboard blackboard);
    }
}