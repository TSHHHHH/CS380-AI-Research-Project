using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviourTrees;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
  [SerializeField] private List<Transform> wayPoints = new ();

  private NavMeshAgent agent;

  private BehaviourTree tree;

  private void Awake()
  {
    agent = GetComponent<NavMeshAgent>();

    tree = new BehaviourTree("Enemy");
    tree.AddChild(new LeafNode("Patrol", new PatrolStrategy(transform, agent, wayPoints)));
  }

  // Start is called before the first frame update
  private void Start()
  {
  }

  // Update is called once per frame
  private void Update()
  {
    tree.Process();
  }
}