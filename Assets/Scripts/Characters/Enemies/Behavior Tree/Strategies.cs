
// Contains all the logic for different stragies that the AI can execute

using System;

namespace Characters.Enemies.Behavior_Tree {
    public interface IStrategy {
        Nodes.Status Process();

        void Reset() {
            // Noop
        }
    }

    // Preform some action; fire and forget the action. Returns success regardless of status of action
    public class ActionStrategy : IStrategy {
        readonly Action doSomething;

        public ActionStrategy(Action doSomething) {
            this.doSomething = doSomething;
        }

        public Nodes.Status Process() {
            doSomething();
            return Nodes.Status.Success;
        }
    }

    // Logical evaluator; returns Success or Failure for the node depending on if the function returns true or false
    // If evaluated to false, all proceeding children will fail, and wont execute
    public class Condition : IStrategy {
        private readonly Func<bool> predicate;

        public Condition(Func<bool> predicate) {
            this.predicate = predicate;
        }
        
        public Nodes.Status Process() => predicate() ? Nodes.Status.Success : Nodes.Status.Failure;
    }
}