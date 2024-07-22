using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourTrees
{
  public class BehaviourTree : Node
  {
    public BehaviourTree(string name) : base(name)
    {
    }

    public override Status Process()
    {
      while (currChildIndx < children.Count)
      {
        var status = children[currChildIndx].Process();

        if (status != Status.SUCCESS)
        {
          return status;
        }

        currChildIndx++;
      }

      return Status.SUCCESS;
    }
  }
}