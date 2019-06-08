Globe Camera Script for Unity
====

This script provides a set of camera control to imitate the view from a person that is observing a globe of a planet, to work with strategy games or space games that allow players to inspect planets. ![](https://media.giphy.com/media/IdTIu9RfDRt27CSu56/giphy.gif)![](https://media.giphy.com/media/jtLVwePP5f6QFcuGze/giphy.gif)

The angle between the viewing direction and the direction of the planet core stays constant during movement.

The viewing direction can be changed, but the up direction of the view will always be towards the general northern direction. 

The camera is not allowed to hover directly above the two pole regions.

The input buttons can be changed in the Input Manager. Attach this script to a camera. It will treat the parent of the camera object as the origin of the spherical system.