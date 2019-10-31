Implementation 1 (map state 13):

I spent the majority of my time trying to implement the flock as the Millington book describes, having 3 separate steering behaviors, utilizing steering behaviors that were used previously. I thought by doing this, I would make a more scalable structure, and that since I built the previous behaviors off of what was in the book, this implementation would work. However, the description of flocking in the book was very vague, and despite implementing the 3 behaviors exactly as Millington detailed them in the book, the behavior still did not act as it was described in the book. I am unsure why, but I will detail how I implemented them. I would also encourage you to look at the Flocking class in MillingtonImplementation.cs, because I do not see where I have made an error.



SteeringOutput Separation()
Finds the center of the local neighborhood of boids within a certain distance. Flees this point. Millington did exactly this in his c++ implementation.

SteeringOutput Cohesion()
Find the center of the local neighborhood of boids within a certain point, seek this point.

SteeringOutput VelocityMatchAndAlign(SteeringOutput output)
Takes in the vector of the Separation() and Cohesion() linear outputs. It then adds the average velocity - the bond's velocity, to get the velocity that should be added to the overall velocity vector. Adds this velocity to the boid.

All of the weights are then applied to these vectors, and a final velocity that is the sum of these 3 behaviors with weights is returned.

This functions perfectly fine if all of the weights are balanced, but once you increase one of the weights, it overwhelms the overall vector, and gets blown out of proportion. This makes sense because if all of the vectors are normalized vectors, and you have A + B + C, if you increase one of those weights without increasing the others, you get a vector that is stronger the corresponding direction, and the flock will deteriorate.

