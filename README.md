TerrainCopyPasteTool
A Unity tool for copying terrain data (height, textures, grass, and trees) from one terrain area to another, with support for undo operations. This tool allows you to specify copy shapes (rectangular or circular), set offsets for terrain height adjustments, and visualize the source and destination areas in the Unity Editor.

Features
Copy height data, textures, grass, and trees from a source terrain to a target terrain.
Adjustable terrain height offset to add custom heights to the copied data.
Support for rectangular or circular selection shapes.
Real-time gizmo visualization of source and destination areas that updates based on terrain height.
Undo support to revert any copy operation.
Installation
Clone or download this repository.
Open your Unity project and import the script into your Assets folder.
Ensure the script is inside an Editor folder if using any editor-specific utilities.
Usage
Attach the TerrainCopyPasteTool Component:

Add the TerrainCopyPasteTool component to any GameObject in your scene (e.g., an empty GameObject for organizational purposes).
Assign Source and Destination Terrains:

In the component's inspector, assign the Source Terrain (the terrain to copy data from) and the Destination Terrain (the terrain to paste data into).
Set Center Positions:

Define the center positions (local coordinates) for the source and destination areas.
Source Center Local Position: Center of the area to copy.
Destination Center Local Position: Center of the area to paste.
Choose Shape and Dimensions:

Select the copy shape (rectangle or circle).
For Rectangle:
Set Width and Height (measured in terrain grid units).
For Circle:
Set the Radius.
Adjust Terrain Height Offset (Optional):

Use Height Offset to add or subtract height values to the copied data.
This offset applies to the destination terrain only.
Copy Terrain Data:

Click the Copy Terrain Data button in the Inspector to execute the copy.
The data will copy based on the settings you defined.
Undo Operation:

Use Unity’s Undo function (Ctrl + Z or Cmd + Z) to revert the copy operation.
Visual Gizmos
The tool provides real-time visual feedback in the scene view:
Source Area: Displays the copy area on the source terrain in Green.
Destination Area: Displays the paste area on the destination terrain in Blue.
The gizmos dynamically adjust to follow terrain height changes using Terrain.SampleHeight, ensuring accurate positioning.
Example
To copy a circular area from one terrain to another with a radius of 5 units and a height offset of 0.5, follow these steps:

Set Source Center Local Position and Destination Center Local Position.
Choose Circle as the shape and set Radius to 5.
Set Height Offset to 0.5.
Click Copy Terrain Data to copy the data from the source to the destination.
Requirements
Unity 2019.4 or later
Compatible with Unity’s built-in Terrain component.
Contributing
Pull requests are welcome. For major changes, please open an issue first to discuss what you would like to change or add.



TerrainModifierTool
TerrainModifierTool is a Unity tool designed to adjust the height of specific areas within a Terrain component. This tool enables easy, in-editor terrain height modification within a circular or rectangular area. Additionally, it supports Undo operations, allowing safe and reversible height adjustments.

Features
Modify terrain height within a specified circular or rectangular area
Adjust height in meters
Supports Unity’s Undo functionality
Visualize the modification area using customizable Gizmos
Requirements
Unity 2020.3 or later
A project with a Terrain component
Installation
Clone or download this repository.
Place TerrainModifierTool.cs in the Assets/Scripts folder or any other script folder in your Unity project.
Usage
1. Adding the Script
In Unity, select the target Terrain object and attach the TerrainModifierTool script to it.
Configure the settings for the script in the Inspector.
2. Inspector Settings
Terrain: Select the target Terrain object.
Center: Specify the center coordinates (x and z) of the modification area.
Radius (for circular shape): Set the radius of the affected area.
Rect Size (for rectangular shape): Set the width and height of the affected area.
Height Delta Meters: Enter the height adjustment value in meters. Positive values increase height, and negative values decrease it.
Gizmo Color: Select the color for the Gizmo representing the modification area (default is red).
Shape: Choose Circle or Rectangle to define the shape of the modification area.
3. Modifying Terrain
To apply the height modification, click the Modify Terrain Height button in the Inspector. The changes will immediately apply to the specified terrain area. Modifications are also Undoable, allowing you to use "Edit > Undo" or Ctrl + Z to revert changes.

4. Visualizing the Affected Area
The specified modification area is visualized in the Scene view using Gizmos. The color and shape of the Gizmo reflect the settings in the Inspector.

Notes
Be cautious with large values for Height Delta Meters, as they may create abrupt terrain changes.
While this tool does not support smoothing the modified area, it is optimized for height adjustments with Undo support.
License
This project is licensed under the MIT License. See the LICENSE file for details.



Author
Isemira
If you have feedback or suggestions, please open an issue in the repository.
