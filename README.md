# Parallax-Engine
Unity Project for 2.5d Parallax RPG Engine



## Notes To Remember TODOs
### Tile Replacements / Credits
* Switch grass & tree tiles / find credit for them
* Finalize flowers

### Tile Additions / Edits
* Add shadows in
* Large ferns / foliage
* More mushrooms

### Tile object coding
* Slide on ice
* Tiles that you can't jump in (marsh, deep sand, etc.) - includes water and underwater - jump button should probably be used to dive/rise in water?
* Underwater tiles could have color tint in palette view - or some indicator of being underwater tiles
* Same thing for shore tiles
* Animate still water edges
* Forage tiles - "is_forage" flag with fields for replacement "foraged" tile, and what item(s) it drops -> Probably should be events
* Animate the fading/hiding/showing of tiles
* Reflection bug when walking up into blocked tile while showing reflection below, head gets cut off - setting new mask tiles without actually moving

### Map Managing Code
* Buttons for static functions to load/save maps as well
* Map object stores background music
* Nice to have - brush code for water / terraforming - automatically apply correct tiles to multiple layers, add/delete layers dynamically
* Would be really nice to have map managing code visible in same editor window as tile palette for map editing
* Fog effects, weather, etc. are in map definition
* Map creation code -> new file

### Eventing Code
* Look into tasks/jobs instead of coroutines
* Better way to find current map in MoveableObject
* "Selected Event" concept. Doesn't need to be directly in front, could be on ground, under, look ahead, etc.
* More advanced falling physics
* Intermittent bug that happens on stairs sometimes where on_tile is null
* Shrink shadows when jumping!
* Consider splitting up sprite / animation / visual code out of moveable object

### Settings Code
* Switch to a settings asset?

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
* Perks / stats/ point buy etc. No forced classes, build out what you care about. Invest directly in base stats.



## Ideas
### Map Regions
* Dragon area as beautiful valleys, white stone, green grass, maybe some brown mountain mixed in or as caves. Rive/lake that is a dragon shap with island cave?
* Rome/Greece area should have fall colors
* Bamboo in dark mountain caves exterior
* Mini games / quests in each region/area
* In tidepool tropics area (and maybe others) - some islands connected by shallow waters you can walk across, maybe tides change too? Make it accessible only sometimes

### Temples / Leaders Etc.
* Grass temple - combo of water and fire and grass to modify the nature
* Grow / shrink vines, etc., maybe burn some stuff
* Ariados Guy - scene in my head, rope bridge in jungle cliffs
* Crobat Guy

### People Ideas
* Rival tribes - feebas / magikarp groups
* Salandit bandits



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
* KleinStudio & Richard PT

### Asset Packs / Scripts
* Odin Inspector
* JDSherbert SerializableDictionary

### Music
* Andrew LiVecchi (MasterOfRevels)