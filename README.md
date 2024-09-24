# Parallax-Engine
Unity Project for 2.5d Parallax RPG Engine



## Notes To Remember TODOs
### Tile Replacements / Credits
* Switch grass & tree tiles / find credit for them
* Same with rocks/misc. minor decorations

### Tile Additions / Edits
* Sand for Beach
* Beach Water / Shore
* Enable thin hills / mountains
* Should bridges be ground of objects? New layer altogether?

### Tile object coding
* "No Pass" tag tiles - should be easy to show in editor but not in runtime
* 2nd Black tile, on cover layer always, probably doable with terrain tag
* Reflections on ice, still water, etc.
* Slide on ice
* Ice ramps?
* Sand ramps / landslides?
* Utility code for calculating sorting layer to not have repeat code in a bunch of places

### Map Managing Code
* Map awareness can just go in Map object - get positions (pos including Z and or layer as params)
* Need sight of layers up and down
* Buttons calling to methods (maybe static) to create new layers above/below
* Buttons for static functions to load/save maps as well
* New layers could be full or empty by default? Nice to have
* Map layers stored in Dict rather than array
* Map object stores map size? background music
* Nice to have - brush code for water / terraforming - automatically apply correct tiles to multiple layers, add/delete layers dynamically

### Eventing Code
* Look into tasks/jobs instead of coroutines
* Diagonal movement... maybe

### Other things to do
* Region map - Melanite Style, LoP Region Concept
* Improve comments / documentation in code
* Message system - UI should have separate border / fill elements so opacity can be set differently
* Consider white outlines on characters / events

### Mechanics
* Level caps -> Store excess XP
* Additional pokedex info / research levels like in arceus



## Ideas
### Map Regions
* Dragon area as beautiful valleys, white stone, green grass, maybe some brown mountain mixed in or as caves. Rive/lake that is a dragon shap with island cave?
* Rome/Greece area should have fall colors
* Bamboo in dark mountain caves exterior



## Credits
### Tilesets
* PeekyChew
* The-Red-eX
* EVoLiNa
* Magiscarf
* KingTapir
* Phyromatical
* zetavares852
* KKKaito
* BoOmxBiG
* WesleyFG
* Kyle-Dove
* zerudez
* Hydragirium
* Alistair

### Asset Packs
* Odin Inspector

### Music
* Andrew LiVecchi (MasterOfRevels)