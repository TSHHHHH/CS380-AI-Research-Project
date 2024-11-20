# CS380-AI-Reaserch-Project

# Research Goal

The primary goal of our research is to explore and implement advanced AI tactics for enemies in shooter games. By doing so, we aim to:

- Dynamically select the most optimal cover based on the current situation.
- Enhances the realism and intelligence of enemy responses.
- Increase the challenge and immersion of gameplay.

Our research addresses common complaints in games like "Battlefield V" and "Halo 5: Guardians," where the AI can sometimes be predictable and lacks depth. By enhancing enemy AI to exhibit sophisticated tactics such as flanking, coordinated attacks, and strategic use of cover, we aim to create a more engaging experience that requires players to think strategically and adapt dynamically.

# Methodology

Our research builds on existing AI techniques and introduces new mechanisms for dynamic and adaptive enemy behavior. The following sections outline the key components of our methodology:

## Dynamic Cover Selection

One of the most crucial aspects of advanced enemy AI is the ability to dynamically select the most optimal cover based on various factors such as player position, visibility, and distance. This section outlines how we achieve dynamic cover selection using a combination of vector mathematics and pathfinding algorithms.

Steps Involved:

### Initial Setup

The game will pre-process the level geometry and compute all corner positions of a wall, each corner will then offset a small distance to get 3 cover points (referred to as “posts”) based on the direction of each corner.

![image](https://github.com/user-attachments/assets/142670f9-f202-4e70-8706-21f3205d8a7c)

### Identify Potential Cover Spots:

Upon alert, the AI agent first searches all corners around themself (constrained by a set view radius) and saves them into memory.

### Calculate Direction Vectors:

For each corner, the AI calculates the direction vector from the corner’s position to the player. This is given by:

$$
d = P_{pos}-C_{pos}
$$

where d is the direction vector, C_pos is the corner position, and P_pos is the player's position.

### Validate Posts:

In a typical firefight scenario, the enemy agent should seek cover that provides a certain amount of protection. Positions located along walls on the player's side will be ineffective.

To validate, we first calculate the normal vector to the direction vector (right side). The normal vector n is obtained using:

$$
n= \begin{bmatrix}
   -d_y \\ d_x
\end{bmatrix}
$$

where $d_x$ and $d_y$ are the components of the direction vector d.

### Cross Product Calculation:

We can use the normal we obtained previously and use it with each post under that corner that is saved by the enemy agent.
The cross product of the normal vector with the vector from the cover spot to each post position helps in determining the relative position and orientation:

$$
c=n \times d
$$

If the result is above 0, it means the post is on the left side of the normal vector; otherwise, it is on the right side (we consider equal to 0 as being on the right side of the normal).

Through this process, we can eliminate posts on the player's side.

### Ray Casting for Visibility:

There is one more step to validate each post. Since we have already eliminated posts that are fully exposed to the player, we now need to check the visibility of each remaining post (since the enemy will need to be able to shoot the player from that post). Each post will cast a ray toward the player to determine if it is obstructed by any obstacles, such as walls. This ray-casting technique assesses whether a cover point is viable by checking if it's blocked (it also helps to eliminate posts that are overlapping with wall colliders).

After this process, we will eliminate all posts that are fundamentally invalid for the enemy agent to use and save the viable ones into a list of potential positions for the enemy agent.

### Post Selection Criteria:

To select the best position for the enemy agent while accommodating a variety of enemy AI designs, we adopted the post-selector technique used in The Last of Us. We created a base class for the post selector that includes multiple evaluator functions, allowing us to derive different enemy AI characteristics from it.

- Valid posts are selected based on criteria such as:
- **Valid Path**: Ensuring the path to the cover post is valid.
- **Distance**: Checking if the post is within a certain distance.
- **Availability**: Verifying that the cover post is not already occupied.
- **Player's Field of View (FOV)**: Ensuring the post is not within the player’s FOV.

The rating of each post can be calculated using:

$$
Rating = \sum_{i} w_i * f_i
$$

where $w_i$ are the weights and $f_i$ are the criteria functions.

```cpp
protected float ShorterDistanceFirst()
{
    distance = Distance(enemyPosition, postPosition);

    distanceScore = Clamp(1 - (distance / maxDistance));

    return distanceScore;
}
```

```cpp
public Transform EvaluatePost()
{
    private float _distanceWt = 0.2f;
    private float _coverInPlayerFOVWt = 0.8f;

    float bestScore = 0f;
    Transform bestPost = null;

    foreach (var post in validPosts)
    {
        float currPostScore = 0f;

        if (IsRunningTowardsPlayer()
            continue;

        int NotInPlayerFOVScore = NotInPlayerFOV();
        float distanceScore = ShorterDistanceFirst();

        currPostScore = sumOfScoresAfterWeight;

        if (currPostScore > bestScore)
        {
            bestScore = currPostScore;
            bestPost = post.transform;
        }
    }

    return bestPost;
}
```

## AI Perception and Field of View (FOV)

Enhanced AI perception systems ensure that enemies can detect and react to the player based on line of sight (LOS) and field of view (FOV) checks. During our research, we discovered the advanced FOV shape used in The Last of Us. Since the article did not provide detailed implementation, we developed our own method.

## First Check - Forward Vector and View Angle:

The enemy's field of view is defined by a forward vector and a specific view angle, typically 60 degrees. The enemy uses this to determine if the player is within its FOV:

$$
\theta = 60\degree
$$

The AI calculates the direction vector from the enemy to the player:

$$
d = P_{pos} - e_{pos}
$$

this vector is normalized to ensure consistent calculations.

To determine if the player is within the enemy's FOV, the dot product of the forward vector f and the direction vector d is calculated:

$$
d \cdot f \le cos\frac{\theta}{2}
$$

If the angle is more than half of the view angle, the player is outside the enemy’s FOV and not visible. Conversely, if the angle is less than half the view angle, the player is within the FOV and visible.
Finally, a ray is cast to check if the player is blocked by an obstacle. This step ensures that even if the player is within the FOV, they are only detected if there is a clear line of sight, as illustrated in the image. The red cross indicates the player is not visible due to an obstruction.

![image](https://github.com/user-attachments/assets/8fd90a1f-ccc2-44f3-bf78-7dca021f4f26)

## Second Check - Enhanced Circular FOV:

After passing the first check, the AI performs a second check using a circular FOV. This is constructed by offsetting a set amount and using that point as the center of the circle. The radius is determined by the offset plus a small "puff size" to cover the area around the enemy (we want the enemy able to sense their surrounding to avoid exploitation by the player).
Like the first check, the enemy agent calculates the direction vector from the center of the circular FOV to the player:

$$
d = P_{pos} - C_{center}
$$

this direction vector is also normalized.
To determine if the player falls within the circular detection range, the AI compares the distance to the radius of the circle. If the distance is greater than the radius, the player is outside the detection range:

$$
Distance > Radius
$$

If the distance is greater than the radius, the player is not detected, as shown in the visual. Otherwise, the player is within the detection range and can be seen by the enemy.
Similar to the first check, the enemy agent casts a ray to the player's position to check if any obstacles block the view. If the ray intersects with obstacles before reaching the player, the player is considered blocked and not visible, ensuring a realistic detection system.

We also built an FOV visualizer to help me understand and debug this enhanced FOV system.

![image](https://github.com/user-attachments/assets/10da9bab-9683-4004-bf2b-d61aeb61800a)


# Implementation and Results

We developed a top-down shooter game to demonstrate our AI tactics. The game includes several advanced AI behaviors:

- **Cover Utilization**: Enemies dynamically select and move to optimal cover points based on real-time analysis of the environment and player position.
- **Flanking Maneuvers**: Enemies attempt to flank the player, using the environment strategically to gain advantageous positions.
- **AI Enhanced Precision**: Enemies have a better FOV system which can improve AI decision-making and significantly improve the gameplay experience.

The results are promising, as the built framework allows game developers like us to focus on designing enemy AI personalities instead of worrying about their precision and varying behaviors. By isolating cover selection from the overall enemy behavior, we can concentrate on creating more engaging and diverse AI interactions.

# Conclusion

In this research, we have delved into the development of advanced AI tactics for enemies in shooter games, inspired by the sophisticated AI systems implemented in games like "The Last of Us." Our goal was to create more immersive, challenging, and repayable gaming experiences by enhancing enemy behavior through dynamic cover selection, advanced perception systems, and adaptive tactics.

Throughout the study, we demonstrated how enemies could dynamically choose optimal cover spots using vector mathematics and pathfinding algorithms. By considering factors such as player position, visibility, and environmental context, enemies can respond effectively to player actions, increasing the realism and challenge of the game. Additionally, we incorporated an enhanced field of view (FOV) system that adjusts based on distance and context. This dynamic FOV, along with a secondary circular FOV, improves enemy detection accuracy and adds a layer of complexity to their perception, making them more formidable opponents.

The adaptive behaviors we introduced allow game developers to create enemies who can react intelligently to changing situations. They can coordinate group tactics, execute flanking maneuvers, and strategically use cover, resulting in a more engaging and unpredictable gameplay experience with the help of optimal cover selection. These advancements not only elevate the challenge for players but also enhance the overall immersion and replayability of the game.

While our methodologies are particularly suited to shooter games, they can be adapted to various game genres and even beyond gaming, such as in simulations, robotics, and virtual training environments.

Looking ahead, integrating machine learning can enable AI to learn and adapt from player interactions, making them even more intelligent and unpredictable. Further refining AI perception systems will improve their ability to detect and respond to a wider range of stimuli, enhancing their situational awareness. Moreover, these advanced AI tactics can create more sophisticated and adaptable systems across various industries.

This research sets a foundation for future innovations in AI for video games, promising to push the boundaries of creating intelligent and dynamic adversaries.

# References

## Journals:

[McIntosh n.d.] McIntosh, T. (n.d.). Human Enemy AI in The Last of Us. In Game AI Pro 2: Collected Wisdom of Game AI Professionals.

## Videos:

[AI and Games 20] AI and Games. (2020, June 17). Endure and Survive: the AI of The Last of Us | AI and Games #52. YouTube. https://youtu.be/7MVzKuQ5SfY?si=ctxQwtvLiQQwDvcQ

[AI and Games 20] AI and Games. (2020, February 27). Designing the Enemy AI of Tom Clancy’s The Division 2 | AI and Games. YouTube. https://youtu.be/6Xv0WguFTFE?si=-bNOPIJxAGNsFRvM
