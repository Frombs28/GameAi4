Stephen Frombach: Flocking, Flocking with Path Finding, Collision Detection/Prediction
Simon Hopkins: Flocking, Flocking through Obstacle Course

In general, our flocking utilizes several behaviors from previous assignments that we re-use for the flocking.
These can all be found in SteeringBehvior.cs, and they are called in NPCController.cs.

The flocking behaviors is that of Millington's implementation. As he states in his book, you add 3 different behaviors together, separation, cohesion, and velocity alignment. You then multiply those behaviors be the respective weights, depending on how you would like your flock to behave.

Our 3 scenes are called: Part1, Part2, and Forest. 1 will take you to Part1, 2 to Part2, and 3 to Forest. S will restart your current scene.

Part 1 contains a flock following the Red player. However, it also contains flock members that the player can pick up by going near them. Although at first the individual
flock members will be wary of joining the flock (they will move away), they will quickly join the flock.


1) What are the weights of the three steering behaviors in your flocking model?
I recommend “surfacing those weights as public variables you can edit in the Inspector.

The weights of the three steering behavoirs are:

Separation: 10
Align: 5
Cohesion: 2

We followed the general rule of separation > alignment/velocity matching > cohesion and played with the weights until we were happy with them.


2) In Part 2, what did you do for avoiding a group of agents? What are the weights of
path following and evade behavior? Did you use a separation algorithm, and what were its parameters?



3) In Part 3, how many rays did you use in your ray-casting, and why?

We are using four rays for ray-casting. This allows us to check in front of us and to the sides.