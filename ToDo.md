#ToDo
- Can open inventory in the rest menu and probably other menus so fix that
- Can't navigate the rest menu, fix it
- Seems dialogue isn't working properly
- The world keeps rotating for some reason when exiting tutorial and I think when starting a game?? really annoying
- Fix the character creation screen by re-position the camera and the UI, make sure the controllers buttons work, and need  to re-enable the head mesh just for this
- On the main menu screen, you have to press B to start use the UI, fix that
- After exiting from the game to main menu, the menu and camera is all wonky and i don't think controls work anymore
- Make loading screen show up on headset
- Make the strafing and backwards movement less janky, I think I can do this by just increasing the walk/run speed
- When you're locked onto someone and kill them the camera rotates 180 degrees so fix that
- Moving and looking around to rotate the body is doing some funky stuff, fix that
- Looking around with hmd then rotating cam with joystick is also doing some funky stuff
- Make it so looking up or down rotates the character backwards or forwards so if there are two items on the same X axis but ones above the other, then you can look up or down to choose which one to pick up
- try to make it so when the player bends over to pick something up or crouches, the camera moves with it
- Figure out why the HUD keeps glitching into the camera when moving
- Figure out why there's like a half second delay between control input and action
- You can access the X button context menu when looting something, find a fix to prevent that
- Make the inventory context menu pop up next to the inv item you're using it on
- Fix camera during crouch movement
- Go through the console, find the errors and fix them
- Try and improve performance

### Done
- ~~Rework the helmet mesh hiding thing, im pretty sure it would only work on the tutorial since it relies on the helmet being at index 8 or something~~
- ~~Make the main menu screen work in VR and move the controller init and update functions to somewhere they can be used in the main menu~~
- ~~When bow and arrows are equipped, trying to open the inv with current binding won't work properly so either fix that or rebind inv to something else, maybe right joystick down~~
- ~~Make it so the player can move diagonally and backwards, rather than just forward or just left and right~~
- ~~Make a better control scheme~~
- ~~Figure out how to make the character move side to side instead of just forwards. Character can move sideways when locked on so look at that for ideas~~
- ~~Using the same DeltaAngle stuff for rotating the body, make it so the HUD stays in the same x (i think) coord, then when you've looked beyond a certain point, recenter it. Also make it so the HUD doesn't rotate up or down, keep it centered. This will all help with reducing the need to strain to look at the corners of the UI by keeping the HUD in place~~
- ~~Position camera slightly to the right and forward when crouched~~
- ~~Lock the camera Y axis to the player body, and for the X and Z axis, make it so moving the headset left/right or forward/backwards moves the player too~~
- ~~Work on the player body rotating left and right when looking around with headset. Right now it's much too jaggy~~
- ~~Change the X look rotation multiplier when stationary, its way too high right now~~
- ~~Map is rotated all wonky, so set its rotation to 0~~
- ~~Reposition the status' HUD elements and the bandage element~~
- ~~In pause menu, pickup item menu and inv, A and X buttons don't work for some reason so fix that~~

#Future Goals
- Figure out if you can track arms and weapon to VR controllers somehow
	- After figuring this out, maybe try adding motion controls, e.g. when shield is within a Y range close to camera (meaning its around the same height as the camera), then trigger blocking.
		- Look into how the hitbox system works so if I get motion controls working, see if I can get the motion controlled sword to interact with an enemy hitbox when swinging controller.
- To prevent the HUD getting clogged up, think about tracking HUD elements to worldplace objects, e.g. health and stamina to the wrist or something
- For pickup item HUD elements, maybe track them to the actual object and make it rotate around the object facing the player
- Maybe add a laser pointer for the controller when in menus like the inventory or pause menu
	- Tried doing this but to work with normal raycasts UI elements need a collider which these ones don't have. You can use EventSystem.current.raycastAll() or GraphicRaycaster apparently but I couldn't get them to work with a worldspace UI
- Maybe add the laser pointer for picking up items instead. It will be 100 times easier than the UI stuff since its interacting with real world objects