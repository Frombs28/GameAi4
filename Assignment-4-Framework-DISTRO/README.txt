Stephen Frombach: Flocking, Flocking with Path Finding, Collision Detection/Prediction
Simon Hopkins: Flocking, Flocking through Obstacle Course

In general, our flocking utilizes several behaviors from previous assignments that we re-use for the flocking.
These can all be found in SteeringBehvior.cs, and they are called in NPCController.cs.

-Separation uses 7 raycasts to determine if any flock members are too close to this boid, and if any flock members are too close,
it calls basic Evade, where the "target" (the one the boid is moving away from) is whichever flock member is closest.

-Alignment simply uses align and pursue to ensure the boids are all moving towards the flock leader.

-Cohesion finds the centroid of the flock, or the middle point of all memebers, and calls basic seek on the flock members, with the target
being this centroid.

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