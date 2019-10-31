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

public class FieldMapManager : MonoBehaviour {

    public List<GameObject> boids;
    public GameObject player;
    public Text narrator;

    void Start() {
        narrator.text = "Part 1: Flocking Behavior. The blue boids follow the red player character, flocking appropriately.";
        foreach(GameObject boid in boids){
            NPCController npc = boid.GetComponent<NPCController>();
            npc.NewTarget(player.GetComponent<NPCController>());
        }
        
    }
}
