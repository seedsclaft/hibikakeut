///**************************************************
/// Ariadne 3D Dungeon Maker
/// © 2018 Explorers' Lab
/// Version 1.5.1
///**************************************************

Ariadne 3D Dungeon Maker is a powerful asset 
to create 3D dungeons.

In the extension, you can make grid-based map data in MapEditor.
And set it to the Game Controller prefab, 
Ariadne 3D Dungeon Maker produces dungeons 
according to your map data at runtime.

This asset includes the MoveController that enables 
movement in the dungeon.
The controller enables processing events, too.


# Table of Contents
1. Features
2. Demo
3. Workflow
4. Support
5. Version history


## 1. Features
- Powerful map data editor on Unity Editor.
The dungeon prefabs are instantiated at runtime 
according to your map.  

- The following type of events can define on each point on the map.
    - Increase item
    - Increase player money
    - Move to another position (such as upstairs)
    - Exiting from the dungeon
    - Show messages to the screen
    - Play animation
    - Move player to the front
    - Wait
  In addition, you can add your event by creating scripts for your event.

- Showing map data on the screen. 
That indicates the player position and attributes of the map.

- Including a controller for movement in the dungeon.

- Including a demo dungeon to play.


## 2. Demo
As a tour of Ariadne 3D Dungeon Maker, there is a demo scene.
The scene file is placed on [Ariadne/Demo] folder.

In the dungeon, you can control the player 
by using arrow keys to move and using a space key to deciding. 

You can also control by using buttons on the screen. 
Those buttons correspond to control at mobile devices.


## 3. Workflow
First, create map data by using MapEditor.
You can open the MapEditor from [Window] -> [Ariadne] -> [MapEditor]
in the menu bar.

On the MapEditor, you can set attributes of the map 
by using draw tools.
To add an event, select a position using select tool 
and assign event data at the position.
You can create new event data, and you can also assign existing data.
After assigning an event data, press [Open Event Editor] button.

On the EventEditor, you can define the contents of events 
and starting conditions.

After saving the map data, 
create a dungeon data from 
[Create] -> [Ariadne] -> [DungeonData] in the context menu.
The dungeon data is a holder of map data. 
Set map data that you created to dungeon data.

Next, set the dungeon data to the GameController object in the Scene. 
The GameController object has a component named DungeonSettings, 
so set the dungeon data to this component.

Template objects are placed in [Ariadne/Prefabs/SceneObjects/Template] folder as prefabs.
When you intend to create a new scene, 
it is useful to duplicate the demo scene and customize it
or instantiate template prefabs.

Finally, ready to explore your dungeon. Execute the game!


For more information, see also Ariadne_manual.pdf.


## 4. Support
Please feel free to contact me if you have any questions or comments.

Web
https://explorers-lab.com/

E-mail
explorers-lab@hotmail.com


## 5. Version history
To check version histories, see the "ReleaseNote" file.
