# Patch v0.1.1
* Trees and Plants are now correctly filtered by the Marquee Tool.
* Move It is no longer a dependency. If not found, the Reload From Move It button is hidden.
* Updated localized strings to cover Move It's absence.

# Patch v0.1.0
* Own Marquee Selection Tool implemented.
* Changed Marquee Tool button icon to the same as the main panel, and icon changes while the marquee tool has been selected and while its being drawn, toggling back  when the selection is finished.
* Updated localized strings.

# Patch v0.0.9
* List Panel design retouched to be closer to the vanilla panels.
* Marquee Selector Button is now hidden until it gets its proper functionality.

# Patch v0.0.8
* Buttons now have tooltips.
* Reopening the List Panel does not reload selected entities from Move It if there is already a previous selection imported. This prevents unintended renewals of the list allowing the user to persist a curated list, only updating it by clicking the reload button.
* All languages added (many translated via GPT).

# Patch V0.0.7
* Complete revamp of the List Panel.
* New close panel button.
* New Select With Marquee button (not yet functional).
* Reload from Move It button moved to the top bar along with the 2 new buttons.
* Each listed item now includes a Remove button that removes the item from the list, but keeps the original list in memory so it can be reloaded back from Move it, provided it has not changed there.
* Bottom bar is now clickable to expand/collapse the list panel.
* Expanded state of panel is also kept on hidden settings and is restored along with position when the panel is reopened in the same or in a new playing session.

# Patch V0.0.6
* French and Spanish translations.
* More code cleanup.
* Added Thumbnail.
* Detached Git from BB fork tree.
* Further Tweeks to the Long Description of the mod.

# Patch V0.0.5
* Tweeks to the Long Description of the mod.

# Patch V0.0.4
* List Panel is now draggable and the position is kept between game sessions by saving it in the mod's settings.
* Cleanup of some unused code and logging statements.

# Patch V0.0.3
* List panel now contains a button to refresh the list based on the lastest Move It selection, so it no longer requires closing and reopening the list to refresh it.
* Still requires a preselection made inside MoveIt to display the selected assets in the list.
* Panel now shows the names of the assets prefabs if they are available, otherwise it will show the name of the asset itself.
* Instructions are now presented in the list panel when no assets are selected.
* The list panel remains visible and partially functional even when Move It is running, only being unable to hightlight the assets when hovered on the list. Selecting one asset from the list closes Move It and restores full functionality to the list panel.
* List panel continues to be fully functional and listing the selected assets event when the info panel of one is opened, allowing the user to select another asset from the list without having to close the info panel first nor having to reopen the list panel.
* Assets like roads, rails, netlanes, paths that might have been selected in Move It are skipped and not be listed in the list panel.
* The panel's design has been improved to be more visually appealing (I hope), and closer to the vanilla UI style.

# Patch V0.0.2
* Fully working version of the Selected Assets List Panel
* Still requires a preselection made inside MoveIt to display the selected assets in the list, and the list needs to be refreshed manually by closing the list and reopening it.