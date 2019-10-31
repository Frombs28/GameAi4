using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// MapStateManager is the place to keep a succession of events or "states" when building 
/// a multi-step AI demo. Note that this is a way to manage 
/// 
/// State changes could happen for one of two reasons:
///     when the user has pressed a number key 0..9, desiring a new phase
///     when something happening in the game forces a transition to the next phase
/// 
/// One use will be for AI demos that are switched up based on keyboard input. For that, 
/// the number keys 0..9 will be used to dial in whichever phase the user wants to see.
/// </summary>

public class ForestMapManager : MonoBehaviour
{
    public List<GameObject> boids1;
    public GameObject follow1;
    public GameObject[] Path1;
    public Text narrator;

    void Start()
    {
        narrator.text = "Part 3: RayCasting for Obstacle Avoidance";
        foreach (GameObject boid in boids1)
        {
            NPCController npc = boid.GetComponent<NPCController>();
            npc.NewTarget(follow1.GetComponent<NPCController>());
            if (npc.gameObject.tag == "Red")
            {
                npc.NewTarget(Path1[0].GetComponent<NPCController>());
            }
        }
    }
}
