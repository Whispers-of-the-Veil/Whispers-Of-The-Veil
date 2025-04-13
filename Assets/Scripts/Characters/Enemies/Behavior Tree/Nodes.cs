using System;
using System.Collections.Generic;
using System.Linq;

namespace Characters.Enemies.Behavior_Tree {
    // Continue running so long as we don't fail
    public class UntilFail : Nodes {
        public UntilFail(string name, int priority = 0) : base(name, priority) { }

        public override Status Process() {
            if (children[0].Process() == Status.Failure) {
                Reset();
                return Status.Failure;
            }

            return Status.Running;
        }
    }
    
    // Logical Not
    public class Inverter : Nodes {
        public Inverter(string name, int priority = 0) : base(name, priority) { }

        public override Status Process() {
            switch (children[0].Process()) {
                case Status.Running:
                    return Status.Running;
                case Status.Failure:
                    return Status.Success;
                default:
                    return Status.Failure;
            }
        }
    }
    
    public class RandomSelector : PrioritySelector {
        private static Random rng;
        
        protected override List<Nodes> SortChildren() => Shuffle(children).ToList();
        
        public RandomSelector(string name) : base(name) { }

        private static IList<T> Shuffle<T>(IList<T> list) {
            if (rng == null) rng = new Random();
            int count = list.Count;
            while (count > 1)
            {
                --count;
                int index = rng.Next(count + 1);
                (list[index], list[count]) = (list[count], list[index]);
            }
            
            return list;
        }
    }
    
    // Try to get a list of nodes to succeed based on priority
    public class PrioritySelector : Selector {
        List<Nodes> sortedChildren;
        List<Nodes> SortedChildren => sortedChildren ??= SortChildren();
        
        protected virtual List<Nodes> SortChildren() => children.OrderByDescending(child => child.priority).ToList();
        
        public PrioritySelector(string name) : base(name) { }

        public override void Reset() {
            base.Reset();
            sortedChildren = null;
        }

        public override Status Process() {
            foreach (var child in SortedChildren) {
                switch (child.Process()) {
                    case Status.Running:
                        return Status.Running;
                    case Status.Success:
                        Reset();
                        return Status.Success;
                    default:
                        continue;
                }
            }

            Reset();
            return Status.Failure;
        }
    }
    
    // So long as one of the children pass, we will return a success: logical OR
    // This is used to determine if any number of sequences returns a Success
    public class Selector : Nodes {
        public Selector(string name, int priority = 0) : base(name, priority) { }

        public override Status Process() {
            if (currentChild < children.Count) {
                switch (children[currentChild].Process()) {
                    case Status.Running:
                        return Status.Running;
                    case Status.Success:
                        Reset();
                        return Status.Success;
                    default:
                        currentChild++;
                        return Status.Running;
                }
            }
            
            Reset();
            return Status.Failure;
        }
    }
    
    // Makes sure that every child gets executed; if a child fails, every child fails: logical AND
    public class Sequence : Nodes {
        public Sequence(string name, int priority = 0) : base(name, priority) { }

        public override Status Process() {
            if (currentChild < children.Count) {
                switch (children[currentChild].Process()) {
                    case Status.Running:
                        return Status.Running;
                    case Status.Failure:
                        Reset();
                        return Status.Failure;
                    default:
                        currentChild++;
                        return currentChild == children.Count ? Status.Success : Status.Running;
                }
            }
            
            Reset();
            return Status.Success;
        }
    }
    
    public class Leaf : Nodes {
        readonly IStrategy strategy;

        public Leaf(string name, IStrategy strategy, int priority = 0) : base(name, priority) {
            this.strategy = strategy;
        }
        
        public override Status Process() => strategy.Process();
        
        public override void Reset() => strategy.Reset();
    }
    
    public class Nodes {
        public enum Status { Success, Failure, Running }
        public readonly string name;
        public readonly int priority;
        public readonly List<Nodes> children = new();
        protected int currentChild;

        public Nodes(string name = "Node", int priority = 0) {
            this.name = name;
            this.priority = priority;
        }
        
        public void AddChild(Nodes child) => children.Add(child);
        
        public virtual Status Process() => children[currentChild].Process();

        public virtual void Reset() {
            currentChild = 0;
            
            foreach (var child in children) {
                child.Reset();
            }
        }
    }

    public class BehaviorTree : Nodes {
        public BehaviorTree(string name) : base(name) { }

        public override Status Process() {
            while (currentChild < children.Count) {
                var status = children[currentChild].Process();
                if (status != Status.Success) {
                    return status;
                }
                currentChild++;
            }
            
            Reset();
            return Status.Success;
        }
    }
}