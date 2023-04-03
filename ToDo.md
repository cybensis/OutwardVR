#ToDo
- Moving and looking around to rotate the body is doing some funky stuff, fix that
- Rework the helmet mesh hiding thing, im pretty sure it would only work on the tutorial since it relies on the helmet being at index 8 or something
- Figure out how to make the character move side to side instead of just forwards. Character can move sideways when locked on so look at that for ideas
- Can't use inv properly when using arrows, as opening inv nocks an arrow and prevents you from switching it out
- Make it so looking up or down rotates the character backwards or forwards so if there are two items on the same X axis but ones above the other, then you can look up or down to choose which one to pick up
- try to make it so when the player bends over to pick something up or crouches, the camera moves with it
- Figure out why the HUD keeps glitching into the camera when moving
- Make a better control scheme
	- At least figure out some map for the roll button temporarily
- Find a way to make the HUD not so clogged up
- Figure out why there's like a half second delay between control input and action
- For big menus like inventory and pause menus, maybe make it so its position is static from your location when you first open said menu so its not tracking to the headset
	- To do this maybe just have the UI update patcher stop updating HUD position when a menu is open so it freezes in place. I don't think it would be possible to freeze just the menu and not the rest of the HUD
- Make the main menu screen work in VR and move the controller init and update functions to somewhere they can be used in the main menu
- You can access the X button context menu when looting something, find a fix to prevent that
- Make the inventory context menu pop up next to the inv item you're using it on
- Fix camera during crouch movement

### Done
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