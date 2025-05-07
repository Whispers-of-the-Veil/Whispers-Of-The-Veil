// Lucas

using Characters.NPC.BlackboardSystem;

namespace Characters.NPC.Commands {
    public interface ICommand {
        void Execute(Blackboard blackboard) { }
    }
}