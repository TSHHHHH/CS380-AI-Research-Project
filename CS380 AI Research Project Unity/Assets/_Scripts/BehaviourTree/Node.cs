using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourTrees
{
  public class Node
  {
    public enum Status
    {
      SUCCESS,
      FALIURE,
      RUNNING
    }

    public readonly string name;

    public readonly List<Node> children = new List<Node>();
    protected int currChildIndx;

    public Node(string name = "Node")
    {
      this.name = name;
    }

    public void AddChild(Node child) => children.Add(child);

    // Process the current child
    public virtual Status Process() => children[currChildIndx].Process();

    public virtual void Reset()
    {
      // Reset the current child index
      currChildIndx = 0;

      // Reset all children
      foreach (var child in children)
      {
        child.Reset();
      }
    }
  }

  public class Sequencer : Node
  {
    public Sequencer(string name) : base(name)
    {
    }

    public override Status Process()
    {
      if (currChildIndx < children.Count)
      {
        switch(children[currChildIndx].Process())
        {
          case Status.RUNNING:
            return Status.RUNNING;

          case Status.FALIURE:
            Reset();
            return Status.FALIURE;

          default:
            currChildIndx++;
            return currChildIndx == children.Count ? Status.SUCCESS : Status.RUNNING;
        }
      }

      Reset();

      return Status.SUCCESS;
    }
  }

  public class LeafNode : Node
  {
    readonly IStrategy strategy;

    public LeafNode(String name, IStrategy strategy) : base(name)
    {
      this.strategy = strategy;
    }

    public override Status Process() => strategy.Process();

    public override void Reset() => strategy.Reset();
  }
}
