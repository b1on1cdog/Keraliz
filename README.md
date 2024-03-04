# Keraliz
Basically a Dual Monitor magnifier for Windows.

# What is this for?

I'm getting eyes surgery soon, that's 2 recovery weeks with blurry vision, so what this does is basically use a secondary monitor to display a zoomed version of the primary, the purpose is making text easier to read since i want to play some visual novels during the recovery time.

At 2/3/2024 i haven't tested throughly yet and the code has no dynamic options, it's setup to work on my 2560x1440 primary monitor at 125% scale factor and 1920x1080 with 100% scale factor secondary monitor, it probably won't behave correctly if the setup has noticeable differences, consider this a Proof of Concept for the meanwhile since i can't expect it to work as-is otherwise.

I might or i might not make it easy to use for end users, but if you actually have impaired vision and would think this would help you someway, you should let me know to improve the chances i continue working on it.
# Usage (updated 3/3/2024)
For the meanwhile the app preferences is handled by keyboard.<br />
-UP/DOWN: Zoom In/Out<br />
-W/A/S/D: Move Position<br />
-X: Set this position as Maximum horizontal limit for current Zoom Setting<br />
-Y: Set this position as Maximum vertical limit for current Zoom Setting<br />
-Q: Calibrate cursor to 20 pixels to the left in X axis<br />
-E: Calibrate cursor to 20 pixels to the right in X axis<br />
-R: Calibrate cursor to 20 pixels up in Y axis<br />
-T: Calibrate cursor to 20 pixels down in Y axis<br />
-K: Save all Settings<br />
-L: Delete all axis limits<br />
-C: Clear current Zoom X/Y limit<br />

Note: The focus should be on the App to set any Settings by Keyboards keys, since for setting the Axis you might want to place the cursor on the Primary Monitor do no click anything to not lose focus on the app while settings the axis limits. 

# To-do
-Dynamically tune the app based on user monitors.<br />
-Adding settings so it could be customized from app side.<br />
-Zooming in/out without being inside the app.<br />
