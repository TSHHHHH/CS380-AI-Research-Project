using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace BehaviourTrees
{
  public interface IStrategy
  {
    Node.Status Process();

    void Reset()
    {
      // Empty by default
    }
  }

  public class ActionStrategy : IStrategy
  {
    private readonly Action doSomething;

    public ActionStrategy(Action doSomething)
    {
      this.doSomething = doSomething;
    }

    public Node.Status Process()
    {
      doSomething();
      return Node.Status.SUCCESS;
    }
  }

  public class Condition : IStrategy
  {
    private readonly Func<bool> predicate;

    public Condition(Func<bool> predicate)
    {
      this.predicate = predicate;
    }

    public Node.Status Process() => predicate() ? Node.Status.SUCCESS : Node.Status.FALIURE;
  }

  public class PatrolStrategy : IStrategy
  {
    private readonly Transform entity;
    private readonly NavMeshAgent agent;
    private readonly List<Transform> waypoints;
    private readonly float patrolSpeed;
    int currentWaypointIndex = 0;
    bool isPathCalculated;

    public PatrolStrategy(Transform entity, NavMeshAgent agent, List<Transform> waypoints, float patrolSpeed = 2f)
    {
      this.entity = entity;
      this.agent = agent;
      this.waypoints = waypoints;
      this.patrolSpeed = patrolSpeed;
    }

    public Node.Status Process()
    {
      if (currentWaypointIndex == waypoints.Count) return Node.Status.SUCCESS;

      var target = waypoints[currentWaypointIndex];
      agent.SetDestination(target.position);

      //entity.LookAt(target);

      if(isPathCalculated && agent.remainingDistance < 0.1f)
      {
        currentWaypointIndex++;
        isPathCalculated = false;
      }

      if (agent.pathPending)
      {
        isPathCalculated = true;
      }

      return Node.Status.RUNNING;
    }

    public void Reset() => currentWaypointIndex = 0;
  }
}
