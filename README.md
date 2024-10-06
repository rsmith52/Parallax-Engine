# Parallax-Engine
Unity Project for 2.5d Parallax RPG Engine



## Notes To Remember TODOs
### Tile Replacements / Credits
* Switch grass & tree tiles / find credit for them
* More mushrooms
* Finalize flowers
* Beach decorations

### Tile Additions / Edits
* Sand for Beach
* Beach Water / Shore
* Add shadows in

### Tile object coding
* Reflections on ice, still water, etc.
* Slide on ice
* Ice ramps?
* Sand ramps / landslides?
* Tiles that you can't jump in (marsh, deep sand, etc.) - includes water and underwater - jump button should probably be used to dive/rise in water?
* Underwater tiles could have color tint in palette view - or some indicator of being underwater tiles
* Animate still water edges

### Map Managing Code
* Buttons for static functions to load/save maps as well
* New layers could be full or empty by default? Nice to have
* Map object stores map size? background music
* Nice to have - brush code for water / terraforming - automatically apply correct tiles to multiple layers, add/delete layers dynamically
* Would be really nice to have map managing code visible in same editor window as tile palette for map editing
* Fog effects, weather, etc. are in map definition

### Eventing Code
* Look into tasks/jobs instead of coroutines
* Diagonal movement... maybe
* Consider what script sprite appearance and layer handling is owned by... moveable object may not make the most sense
* Better way to find current map in MoveableObject
* Make shadow not jump
* Settings for jump / swim enabled - can be unlocked?
* Setting for allow jumping up ledges / over rocks? again can be unlocked?
* "Selected Event" concept. Doesn't need to be directly in front, could be on ground, under, look ahead, etc.
* More advanced falling physics
* Intermittent bug that happens on stairs sometimes where on_tile is null
* Make see through tiles fade in/out rather than abrubtly change
* Switch to blank humanoid template character, animate it

### UI Code
* Simple message boxes
* Speech bubbles

### AI Code
* Path Finding will need custom logic, gonna be hard

### NLP Code
* Consider GPT or...
* Custom NLP Engine 
* Will need a few types of input to process - requests/commands I'm familiar with, responses are another category

### Other things to do
* Region map - Melanite Style, LoP Region Concept
* Improve comments / documentation in code
* Message system - UI should have separate border / fill elements so opacity can be set differently
* Look at "Garbage Gold" reddit post for inspo
* Reddit post pokemon "eternal ties" inspiration
* Reddit post UI pixel font
* Consider white outlines on characters / events

### Mechanics
* Level caps -> Store excess XP
* Additional pokedex info / research levels like in arceus
* Defeating wild monsters -> Drop items same as if they were caught and had held items, more common in general
* Crafting of net new items for the first time to unlock them - repels, etc.
* Poisoned, no items, limits steps before blacking out, need to take optimal path
* Skyrim esque random encounters
* Followers / companions



## Ideas
### Map Regions
* Dragon area as beautiful valleys, white stone, green grass, maybe some brown mountain mixed in or as caves. Rive/lake that is a dragon shap with island cave?
* Rome/Greece area should have fall colors
* Bamboo in dark mountain caves exterior
* Mini games / quests in each region/area
* In tidepool tropics area (and maybe others) - some islands connected by shallow waters you can walk across, maybe tides change too? Make it accessible only sometimes



## Pokemon to include
* Marshadow
* Applin line
* Sneasel line and all evolutions
* New primeape evo line
* H Zoroark
* Wyrdeer



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
* Flurmimon
* Manuxd789
* UltimoSpriter
* Tyranitar Dark
* Alucus
* ChaoticCherryCake
* iametrine = Dewitty
* Kymotonian
* Speedialga
* Thurpok

### Asset Packs / Scripts
* Odin Inspector
* JDSherbert SerializableDictionary

### Music
* Andrew LiVecchi (MasterOfRevels)