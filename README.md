I made a mini minecraft game, for now it only has the basic mechanics- precedurally generated 3D open world made of minable, craftable and placeable blocks.

The only premade assets I used are the "First Person Character" we used in the course, and an inventory system asset called "Inventory Master". (Although I heavily editted the inventory system, it had many bugs..)

Most of the work went into the world generation part.

Initialy, I have written my own terrain generation algorithm (block by block generation) but then switched to using perlin noise to create a smoother surface.

**My algorithm is still in the code and available as an option in the unity editor under the generator object.

In addition I made an algorithm that creates a forest on top of the terrain surface made of randomly sized trees. The basic tree shape is also handwritten.

To minimize lag, only the surface of the terrain is generated. There are no cube objects in the game instead there are quad meshes, one mesh for each of the 6 faces of the blocks. But, only the quads/faces that touch an empty "air" block are actually generated to the game. This method creates the thinest surface possible while maintaining an illusion of a full world.
This surface is monitored and updated in real time so there won't be duplicate or redundant meshes when mining or placing a block. A table keeps track of the block types in every spot on the 3D map so that the correct mesh is created when a block is placed or mined (stone mesh for stone block type, etc..).

Each block type has its own breaking speed and unique breaking sound.

Theres an inventory and crafting system with many different recepies.

All the wood and stone tools (axe, pickaxe etc..) are craftable and usable, each has its own mining preferences (axes are better for wood, pickaxes for stone, shovel for dirt etc..), and of course, tools of better grade have faster mining speed (stone>wood>hand)

The game meets all the criteria for a final project in this course:
There are several game states (main menu, play state, inventory pause state, win state) , there are means to quit the game at any time, theres a timer with a few missions that act as a win and lose condition. The game is around 80% handwritten, and it is at least as complex as any other game we had in the course.
