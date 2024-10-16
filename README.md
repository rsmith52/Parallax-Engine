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
* Buildings - think of them more like terrain than trees for how they are setup

### Tile object coding
* Slide on ice
* Tiles that you can't jump in (marsh, deep sand, etc.)
* Underwater tiles could have color tint in palette view - or some indicator of being underwater tiles
* Same thing for shore tiles
* Animate still water edges
* Forage tiles - "is_forage" flag with fields for replacement "foraged" tile, and what item(s) it drops -> Probably should be events
* Reflection bug when walking up into blocked tile while showing reflection below, head gets cut off - setting new mask tiles without actually moving
* Consider playing with particle effects for grass rustle, water, etc. Have some randomness involved.
* Water splash doesn't look quite right anymore with the camera rotation
* One specific prefab tile (small normal rock) won't hide properly when on a bridge

### Map Managing Code
* Buttons for static functions to load/save maps as well
* Upon saving, create a cache as well. Map Data objects contain a map and a cache and are saved/loaded together. Cache assigned to map at load time.
* Map object stores background music
* Nice to have - brush code for water / terraforming - automatically apply correct tiles to multiple layers, add/delete layers dynamically
* Would be really nice to have map managing code visible in same editor window as tile palette for map editing
* Fog effects, weather, etc. are in map definition
* Refactor considering what should go in start vs awake.
* Build on cache so it can be generated and saved off in editor
* Hidden bridge case of going from one to another
* Hiding terrain is broken again :(

### Eventing Code
* Look into tasks/jobs instead of coroutines
* Better way to find current map in MoveableObject
* "Selected Event" concept. Doesn't need to be directly in front, could be on ground, under, look ahead, etc.
* More advanced falling physics
* Intermittent bug that happens on stairs sometimes where on_tile is null
* Shrink shadows when jumping!
* Consider splitting up sprite / animation / visual code out of moveable object
* Refactor considering what should go in start vs awake.
* Refactor update method in moveable object to put everything in smaller functions

### Camera
* Reflection could use a bit more work again
* Auto find player and set player follow on map load

### Settings Code
* Switch to a settings asset?

### UI Code
* Speech bubbles -> Pop up automatically as NPCs come into hearing range, trying to start a conversation with you! Can be many popped up at once.
* Handle words that will cut to next line
* Continue past 3 lines
* Main menu scene
* Window scaling
* Map label popup
* HUD
* Pause Menu

### AI Code
* Path Finding will need custom logic, gonna be hard
* State machines - AI behavior
* Characters - NPCs, Monsters, etc.
* Map sections as well - when to spawn something, etc.
* Slightly wider range of sight given to AI objects, awareness of events in order to make simple decisions, choose movement based on what is seen, etc.
* Imagine enemies gathering at the edge of a ledge trying to reach the player but unable to - different creatures have different range of movement, some could jump up that ledge!
* Enemies jumping at you in general, good visual / feel - cool mix of movement options different creatures will have
* State control - idea on how to make it swappable at runtime via enum selection -> public and internal state, if they ever don't match, queue a transition

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
* Courier NPC like in Skyrim
* Level scaling -> Assing difficulty ratings to everyting, from simple to boss. Zones get assigned a scaling based on game objectives completed at that point. Locked at that point.



## Ideas
### Map Regions
* Dragon area as beautiful valleys, white stone, green grass, maybe some brown mountain mixed in or as caves. Rive/lake that is a dragon shap with island cave?
* Rome/Greece area should have fall colors
* Bamboo in dark mountain caves exterior
* Mini games / quests in each region/area
* In tidepool tropics area (and maybe others) - some islands connected by shallow waters you can walk across, maybe tides change too? Make it accessible only sometimes
* In tidepool tropics need a blue hole
* Syrup trees - side quest related to apple pokemon

### Temples / Leaders Etc.
* Grass temple - combo of water and fire and grass to modify the nature
* Grass temple - vileplume's poison vs. bellossom - let sunlight in to cleanse it
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