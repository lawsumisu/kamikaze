# Kamikaze

Control a gust of wind and try to pick up as many leaves as you can!

## Controls
* `Space`: Increase wind speed
    * As the wind's speed increases, it can pull leaves in that are farther away
    * Releasing `Space` will cause the wind to slow down to it's initial speed.
* Arrow Keys, `WASD`: Move the wind around. The controls are inverted (like for aircraft simulators), so pressing `W` or `Up` will cause the wind to move downwards, and pressing `S` or `Down` will cause the wind to move upwards.
* `Q`: Turn on Debug Lines: The lines will show the path that the wind is taking. The concentric circles represent the bounding volume that leaves will be move in if they collide with it.
Note: The wind always moves in the direction that it is facing.
