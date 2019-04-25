# Joystick Camera Control
A mod for Cities: Skylines to control the camera using an analog joystick/gamepad. Should work with just about any joystick.

# How to Use
Enable the mod and go to the Options menu. It displays a list of default input settings.
* Input Axis: Selects which axis to use.
  * You can use mouse inputs too. This could be useful with modifiers; eg "while holding Ctrl, the mouse moves the camera".
* Action: Selects what this axis controls.
* Movement Speed: Selects how fast the movement will be.
* Invert: Moves in the opposite direction of the input.
* Dead Zone: Inputs less than this will be ignored. Useful for old joysticks that jitter when not actually being moved.
* Offset: Added to the input (before dead zone). Useful for joysticks that have a slider whose neutral point isn't the centre.
* Smoothing: Whether to use Unity's input smoothing feature.

Also, the current inputs are shown, so you can see which is which. You can also reset the camera to a sane state in case it gets stuck somewhere.

## Modifiers
You can add modifier keys to each input, to use one axis for multiple controls. Each modifier specifies a button, and whether the input is active when that button is held, or not held. The joystick's buttons can also be used as modifiers.

For example, you could set the Horizontal and Vertical axes to Move Left/Right and Move Forward/Backward when Shift is not held, and to Move East/West and Move North/South when shift is held.

By adding modifiers to the mouse inputs, you can use the mouse to pan and rotate while holding a key, which is useful even without a joystick.

# Known Issues
* Only five input axes (excluding the mouse) can be used (see Issue #1) because Unity is silly.
* Moving the camera up/down is usually ignored by the game.
* To build on Linux/Mac, you must set `STEAMPATH` environment variable, eg `/home/steam/`.

# Possible Future Features
* Different profiles per joystick.
* Control more than just camera; eg use a slider to adjust game speed.
  * Bind buttons to game functions. (Steam already has this built in though.)
  * Let one axis control the movement speed of another. (eg for joysticks that have a non-spring-loaded slider)
* Use extra mice/drawing tablets/... as inputs.
  * Accept inputs over local TCP/UDP port.
* Force feedback in whatever appropriate situation.
* Map camera position/angle directly to an axis (ie the camera position is exactly the joystick position).

# Thanks to
* Egi, boformer, Elektrix for help with the API.
* tomarus for the UI code I "borrowed" from TerrainGen.
* andrief for ilspymono, and the ilspy developers, for tools necessary to find axis definitions.
* Icons from [Gnome project](https://commons.wikimedia.org/wiki/File:Gnome-joystick.svg) and [The Noun Project](https://commons.wikimedia.org/wiki/File:Video_Camera_-_The_Noun_Project.svg).
* My cat for reminding me to take breaks.
