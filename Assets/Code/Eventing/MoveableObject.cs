using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Sirenix.OdinInspector;
using Mapping;
using Mapping.Tiles;
using Utilities;
using Eventing;

namespace Eventing
{
    public class MoveableObject : SerializedMonoBehaviour
    {
        #region Fields

        [Title("Positition")]
        [EnumPaging]
        public MovementSpeeds movement_speed = MovementSpeeds.Moderate;
        [EnumPaging]
        public Directions direction = Directions.Down;
        public int layer;

        private float speed;
        private Map map;
        private Vector3 target_pos;
        private Vector3 last_pos;
        public Vector3 target_rotation;
        private Vector3 main_sprite_target_pos;
        private Vector3 shadow_target_pos;
        private Vector3 shadow_home_pos;
        private Vector3 reflection_mask_target_pos;
        private Vector3 reflection_mask_home_pos;
        private Vector3 reflection_target_pos;
        private Vector3 reflection_home_pos;
  
        private JumpData jump_data;
        private LayerChange layer_change;
        private bool layer_up_delay;
        private bool layer_down_delay;
        private bool layer_changed;
        private bool on_stairs_changed;
        private bool initial_checks_done;

        //[HideInInspector]
        public Animator animator;
        private Animator reflection_animator;

        private SpriteRenderer[] sprites;
        private SortingGroup sprite_group;
        private SpriteRenderer main_sprite;
        private SpriteRenderer shadow;
        private SpriteRenderer reflection;
        private SpriteMask bush_mask;
        private SortingGroup reflection_group;
        private Transform reflection_mask_trans;
        private SpriteMask[] reflection_masks;
        private bool visibility_changed;
        private bool shore_anim;

        [Title("Static Settings")]
        [InfoBox("Not previewed in scene editor")]
        public bool invisible = false;
        public bool move_through_walls = false;
        public bool lock_direction = false;
        public bool walking_animation = true;
        public bool stepping_animation = false;
        public bool always_on_top = false;
        public OutlineColors outline = OutlineColors.None;

        [Title("Awareness")]
        [ReadOnly]
        public bool in_bush;
        [ReadOnly]
        public bool on_stairs;
        [ReadOnly]
        public bool on_water;
        [ReadOnly]
        public bool underwater;
        [ReadOnly]
        public bool show_reflection;
        [ReadOnly]
        public bool under_bridge;
        [ReadOnly]
        public bool behind_prefab;
        [ReadOnly]
        public bool behind_upper_layer;

        [TabGroup ("Movement")]
        [ReadOnly]
        public bool moving;
        [TabGroup ("Movement")]
        [ReadOnly]
        public bool looked;
        [TabGroup("Movement")]
        [ReadOnly]
        public bool rotating;
        [ReadOnly]
        public bool jumping;
        [TabGroup ("Movement")]
        [ReadOnly]
        public bool falling;
        [TabGroup ("Movement")]
        [ReadOnly]
        public bool other_moved;
        [TabGroup ("Movement")]
        [ReadOnly]
        public bool in_move_route;
        [TabGroup ("Movement")]
        [ReadOnly]
        public bool tile_activated;

        [TabGroup ("Tiles")]
        [ReadOnly]
        public NeighborTiles neighbor_tiles;

        //private EventManager event_manager;
        [TabGroup ("Events")]
        [ReadOnly]
        private Event[] neighbor_events = new Event[0];
        [TabGroup ("Events")]
        [ReadOnly]
        public Event on_event;
        [TabGroup ("Events")]
        [ReadOnly]
        public Event up_event;
        [TabGroup ("Events")]
        [ReadOnly]
        public Event left_event;
        [TabGroup ("Events")]
        [ReadOnly]
        public Event right_event;
        [TabGroup ("Events")]
        [ReadOnly]
        public Event down_event;

        #endregion


        #region Accessors
        
        public Vector3 GetCurrentPos()
        {
            return transform.position;
        }
        public Vector3 GetTargetPos()
        {
            return target_pos;
        }
        public bool GetInMoveRoute()
        {
            return in_move_route;
        }

        #endregion


        #region Mono Behavior

        private void Start()
        {
            // Basic Setup
            target_pos = transform.position;
            last_pos = transform.position;
            target_rotation = Constants.DEFAULT_PERSPECTIVE_ANGLE;
            speed = Constants.SPEEDS[(int)movement_speed];
            
            moving = true;
            looked = true;
            rotating = true;
            jumping = false;
            falling = false;
            jump_data = new JumpData{};

            layer_change = LayerChange.None;
            layer_up_delay = false;
            layer_down_delay = false;
            layer_changed = true;
            on_stairs_changed = true;
            visibility_changed = true;
            
            other_moved = false;
            in_move_route = false;
            tile_activated = false;
            shore_anim = false;

            // Map Awareness Setup
            map = FindFirstObjectByType<Map>(); // TODO - better way to find map
            // event_manager = FindObjectOfType<EventManager>();

            // Animator / Sprite Setup
            sprites = GetComponentsInChildren<SpriteRenderer>();
            bush_mask = GetComponentInChildren<SpriteMask>();
            sprite_group = GetComponentInChildren<SortingGroup>();
            reflection_group = GetComponentsInChildren<SortingGroup>().Last();
            reflection_mask_trans = reflection_group.transform.GetChild(1);
            reflection_masks = reflection_group.GetComponentsInChildren<SpriteMask>();
            foreach (SpriteRenderer sprite in sprites)
            {
                if (sprite.tag == Constants.SPRITE_TAG)
                    main_sprite = sprite;
                else if (sprite.tag == Constants.SHADOW_TAG)
                    shadow = sprite;
                else if (sprite.tag == Constants.REFLECTION_TAG)
                    reflection = sprite;
            }
            main_sprite.material = SpriteUtils.GetOutlineMaterial(outline);
            main_sprite_target_pos = Constants.DEFAULT_SPRITE_POS;
            shadow_target_pos = shadow.transform.localPosition;
            shadow_home_pos = shadow.transform.localPosition;
            reflection_mask_target_pos = reflection_mask_trans.localPosition;
            reflection_mask_home_pos = reflection_mask_trans.localPosition;
            reflection_target_pos = reflection.transform.localPosition;
            reflection_home_pos = reflection.transform.localPosition;
            
            animator = GetComponentInChildren<Animator>();
            reflection_animator = GetComponentsInChildren<Animator>().Last();
            AnimSetInt(Constants.ANIM_DIRECTION, (int)direction);
            AnimSetBool(Constants.ANIM_WALK, false);
            AnimSetBool(Constants.ANIM_RUN, false);
            AnimSetBool(Constants.ANIM_SNEAK, false);
            AnimSetBool(Constants.ANIM_JUMP, false);
            if (stepping_animation)
                AnimSetBool(Constants.ANIM_WALK, true);

            // Starting Map Checks
            initial_checks_done = false;
        }

        public void OnSpaceEntered()
        {
            other_moved = true;
        }

        private void Update()
        {
            // Update Animator
            AnimSetInt(Constants.ANIM_DIRECTION, (int)direction);
            if (walking_animation && !stepping_animation && !jumping && !falling && !on_water && !underwater && (target_pos != transform.position))
                 AnimSetBool(Constants.ANIM_WALK, moving);
            else if (stepping_animation)
                AnimSetBool(Constants.ANIM_WALK, true);
            else
                AnimSetBool(Constants.ANIM_WALK, false);
            
            speed = (jumping || falling) ? (
                jump_data.num_tiles > 1 ? Constants.SPEEDS[(int)MovementSpeeds.Fast] : Constants.SPEEDS[(int)MovementSpeeds.Moderate]) : 
                Constants.SPEEDS[(int)movement_speed];
            
            // Apply Movement
            if (moving)
            {
                Vector3 move_dir = target_pos - transform.position;
                
                // Activate the tile being moved onto across layers
                if (!tile_activated)
                {
                    if (!jumping && !falling && layer_change == LayerChange.None)
                    {
                        tile_activated = ActivateTile(move_dir);
                    }
                    else if (layer_change == LayerChange.Up)
                    {
                        tile_activated = ActivateTile(neighbor_tiles.above_tile);
                        layer_change = LayerChange.None;
                        
                        // Under Bridge Flag
                        if (map.HideBridgeAbovePosition(this.transform.position + Vector3.up + (Constants.MAP_LAYER_HEIGHT * Vector3.back)))
                            under_bridge = true;
                        else
                            under_bridge = false;
                        // Behind Terrain Flag
                        if (map.HideLayersAbovePosition(this.transform.position + Vector3.up))
                            behind_upper_layer = true;
                        else   
                            behind_upper_layer = false;
                        // Behind Prefab Flag
                        if (map.HidePrefabBlockingPosition(this.transform.position + Vector3.up + (Constants.MAP_LAYER_HEIGHT * Vector3.back)))
                            behind_prefab = true;
                        else
                            behind_prefab = false;
                    }
                        
                    else if (layer_change == LayerChange.Down)
                    {
                        tile_activated = ActivateTile(neighbor_tiles.below_tile);
                        layer_change = LayerChange.None;

                        // Under Bridge Flag
                        if (map.HideBridgeAbovePosition(this.transform.position + Vector3.down + (Constants.MAP_LAYER_HEIGHT * Vector3.forward)))
                            under_bridge = true;
                        else
                            under_bridge = false;
                        // Behind Terrain Flag
                        if (map.HideLayersAbovePosition(this.transform.position + Vector3.down + (Constants.MAP_LAYER_HEIGHT * Vector3.forward)))
                            behind_upper_layer = true;
                        else   
                            behind_upper_layer = false;
                        // Behind Prefab Flag
                        if (map.HidePrefabBlockingPosition(this.transform.position + Vector3.down + (Constants.MAP_LAYER_HEIGHT * Vector3.forward)))
                            behind_prefab = true;
                        else
                            behind_prefab = false;
                        // Show Reflection Flag
                        if (!underwater && (neighbor_tiles.below_tile != null && neighbor_tiles.below_tile.is_reflective))
                        {
                            if (!show_reflection)
                            {
                                show_reflection = true;
                                visibility_changed = true;
                            }
                            map.SetReflectionMask(target_pos, reflection_masks);
                        }
                        else if (show_reflection)
                        {
                            show_reflection = false;
                            visibility_changed = true;
                        }
                    }
                }

                // Move in that direction
                transform.position = Vector3.MoveTowards(transform.position, target_pos, Time.deltaTime * speed);
            }

            // Handle camera angle rotation changes
            if (rotating && target_rotation != transform.localEulerAngles)
            {
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(target_rotation.x, target_rotation.y, target_rotation.z), Time.deltaTime * speed);
                transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, 0, 0);

                if (Math.Abs(360 - (transform.localEulerAngles.x - target_rotation.x)) < Constants.ROTATION_SMOOTHING_CUTOFF ||
                    Math.Abs(transform.localEulerAngles.x - target_rotation.x) < Constants.ROTATION_SMOOTHING_CUTOFF)
                {
                    transform.localEulerAngles = target_rotation;
                    rotating = false;
                } 
            }

            // Shift player sprite for floating underwater
            if (main_sprite.transform.localPosition != main_sprite_target_pos)
            {
                main_sprite.transform.localPosition = Vector3.MoveTowards(main_sprite.transform.localPosition, main_sprite_target_pos, Time.deltaTime * speed);
            }

            // Move shadow to keep up with object
            if (shadow.transform.localPosition != shadow_target_pos)
                shadow.transform.localPosition = Vector3.MoveTowards(shadow.transform.localPosition, shadow_target_pos, Time.deltaTime * speed);

            // Move reflection mask to keep up with object
            if (reflection_mask_trans.localPosition != reflection_mask_target_pos)
                reflection_mask_trans.localPosition = Vector3.MoveTowards(reflection_mask_trans.localPosition, reflection_mask_target_pos, Time.deltaTime * speed);

            // Move reflection to keep up with object
            if (reflection.transform.localPosition != reflection_target_pos)
            {
                float ref_speed = jump_data.num_tiles > 1 ? speed : Constants.SPEEDS[(int)MovementSpeeds.Moderate];
                reflection.transform.localPosition = Vector3.MoveTowards(reflection.transform.localPosition, reflection_target_pos, Time.deltaTime * ref_speed);
            }
            
            // Update bush flag    
            if (in_bush && !jumping && !falling)
            {
                foreach (SpriteMask mask in bush_mask.GetComponentsInChildren<SpriteMask>())
                    mask.enabled = true;
            }
                
            else
            {
                foreach (SpriteMask mask in bush_mask.GetComponentsInChildren<SpriteMask>())
                    mask.enabled = false;
            }

            // Tilemap and Event awareness check
            if ((other_moved || moving) && transform.position == target_pos)
            {
                if (layer_up_delay)
                {
                    layer += 1;
                    layer_up_delay = false;
                    layer_changed = true;

                    // Show Reflection Flag
                    if (!underwater && (neighbor_tiles.above_tile != null && neighbor_tiles.above_tile.is_reflective))
                    {
                        if (!show_reflection)
                        {
                            show_reflection = true;
                            visibility_changed = true;
                        }
                        map.SetReflectionMask(target_pos, reflection_masks);
                    }
                    else if (show_reflection)
                    {
                        show_reflection = false;
                        visibility_changed = true;
                    }
                }
                else if (layer_down_delay)
                {
                    layer -= 1;
                    layer_down_delay = false;
                    layer_changed = true;
                }
                if (jumping)
                {
                    target_pos += ((jump_data.height * Vector3.forward) + (jump_data.height * Vector3.down) + (jump_data.direction * (float)jump_data.num_tiles / 2f));
                    map.SetReflectionMask(target_pos, reflection_masks);

                    if (jump_data.num_tiles > 1)
                    {
                        if (jump_data.dir == Directions.Left || jump_data.dir == Directions.Right)
                            reflection_mask_trans.localPosition += 0.75f * (jump_data.direction * (float)jump_data.num_tiles / 2f);
                        else if (jump_data.dir == Directions.Down && jump_data.source_reflective)
                            reflection_mask_trans.localPosition -= 0.75f * (jump_data.direction * (float)jump_data.num_tiles / 2f);
                        else if (jump_data.dir == Directions.Up)
                            reflection_mask_trans.localPosition -= 0.75f * (jump_data.direction * (float)jump_data.num_tiles / 2f);
                    }
                    
                    shadow_target_pos = shadow_home_pos;
                    reflection_mask_target_pos = reflection_mask_home_pos;
                    reflection_target_pos = reflection_home_pos;

                    jumping = false;
                    falling = true;
                }
                else
                {
                    // Get neighboring tiles
                    neighbor_tiles = map.GetNeighborTiles(this);

                    // Initial flag setting
                    if (!initial_checks_done)
                    {
                        // Under Bridge Flag
                        if (map.HideBridgeAbovePosition(this.transform.position))
                            under_bridge = true;
                        else
                            under_bridge = false;
                        // Behind Terrain Flag
                        if (map.HideLayersAbovePosition(this.transform.position))
                            behind_upper_layer = true;
                        else   
                            behind_upper_layer = false;
                        // Behind Prefab Flag
                        if (map.HidePrefabBlockingPosition(this.transform.position))
                            behind_prefab = true;
                        else
                            behind_prefab = false;
                        // Show Reflection Flag
                        if (!underwater && 
                            (neighbor_tiles.on_tile != null && neighbor_tiles.on_tile.is_reflective) ||
                            (neighbor_tiles.down_tile != null && neighbor_tiles.down_tile.is_reflective) ||
                            (neighbor_tiles.down_right_tile != null && neighbor_tiles.down_right_tile.is_reflective) ||
                            (neighbor_tiles.down_left_tile != null && neighbor_tiles.down_left_tile.is_reflective))
                        {
                            if (!show_reflection)
                            {
                                show_reflection = true;
                                visibility_changed = true;
                            }
                            map.SetReflectionMask(target_pos, reflection_masks);
                        }
                        else if (show_reflection)
                        {
                            show_reflection = false;
                            visibility_changed = true;
                        }

                        if (underwater)
                        {
                            transform.localEulerAngles = Constants.UNDERWATER_PERSPECTIVE_ANGLE;
                            main_sprite.transform.localPosition = Constants.UNDERWATER_SPRITE_OFFSET;
                            bush_mask.transform.localPosition += Constants.UNDERWATER_BUSH_MASK_OFFSET;
                        }
                        target_rotation = transform.localEulerAngles;
                        main_sprite_target_pos = main_sprite.transform.localPosition;


                        initial_checks_done = true;
                    }

                    // Notify old neighbor events
                    // foreach (Event e in neighbor_events)
                    // {
                    //     if (e != null)
                    //     {
                    //         MoveableCharacter character = e.GetComponent<MoveableCharacter>();
                    //         if (character != null)
                    //             character.other_moved = true;
                    //     }
                    // }

                    // Get neighboring events
                    // neighbor_events = event_manager.GetNeighborEvents(this);
                    // on_event = neighbor_events[0];
                    // up_event = neighbor_events[1];
                    // left_event = neighbor_events[2];
                    // right_event = neighbor_events[3];
                    // down_event = neighbor_events[4];

                    // Notify new neighbor events
                    // foreach (Event e in neighbor_events) {
                    //     if (e != null)
                    //     {
                    //         MoveableCharacter character = e.GetComponent<MoveableCharacter>();
                    //         if (character != null)
                    //             character.other_moved = true;
                    //     }
                    // }

                    moving = false;
                    looked = false;
                    if (falling)
                    {
                        falling = false;
                        AnimSetBool(Constants.ANIM_JUMP, false);
                    }
                    jump_data = new JumpData{};
                    other_moved = false;
                    visibility_changed = true;
                    if (!tile_activated)
                        tile_activated = ActivateTile(neighbor_tiles.on_tile);
                }   
            }
            // Update facing & look ahead tile
            else if (looked && transform.position == target_pos)
            {
                neighbor_tiles = map.GetNeighborTiles(this, true);
                looked = false;
            }
            // Falling physics when in air
            else if (neighbor_tiles.on_tile == null && !move_through_walls && !moving && !jumping && !falling)
            {
                SinkDown(true);
            }

            // Update Sprites & Sorting Order
            if (layer_changed || (on_stairs_changed && !layer_up_delay && !layer_down_delay))
            {
                foreach (SpriteRenderer sprite in sprites)
                {
                    if (sprite.tag == Constants.SHADOW_TAG)
                        sprite.sortingOrder = (int)(layer * Constants.SORTING_LAYERS_PER_MAP_LAYER) + Constants.EVENT_SORTING_LAYER_OFFSET - Constants.PRIORITY_TILE_OFFSET;
                    else if (sprite.tag == Constants.REFLECTION_TAG)
                    {
                        int reflection_layer = (int)(layer * Constants.SORTING_LAYERS_PER_MAP_LAYER) + Constants.OBJECT_LAYER_START_OFFSET;
                        sprite.sortingOrder = reflection_layer;
                        reflection_group.sortingOrder = on_stairs ? reflection_layer + Constants.OBJECT_LAYER_START_OFFSET : reflection_layer;
                    }
                    else
                    {
                        int sprite_layer = (int)(layer * Constants.SORTING_LAYERS_PER_MAP_LAYER + Constants.EVENT_SORTING_LAYER_OFFSET);
                        sprite.sortingOrder = sprite_layer;
                        sprite_group.sortingOrder = on_stairs ? sprite_layer + Constants.OBJECT_LAYER_START_OFFSET : sprite_layer;
                    }
                    
                    if (on_stairs)
                        sprite.sortingOrder += Constants.OBJECT_LAYER_START_OFFSET;
                }
                layer_changed = false;
                on_stairs_changed = false;
            }

            // Update visible status
            if (visibility_changed)
            {
                // Invisibility
                foreach (SpriteRenderer sprite in sprites)
                {
                    if (invisible)
                        sprite.enabled = false;
                    else
                        sprite.enabled = true;
                }

                // Shadow behavior
                if (!invisible) shadow.enabled = !(shore_anim || on_water || underwater);
                // Reflection behavior

                reflection_group.GetComponentInChildren<SpriteRenderer>().enabled = show_reflection;
                foreach (SpriteMask mask in reflection_masks)
                    mask.enabled = show_reflection;
                visibility_changed = false;
            }
        }

        #endregion


        #region Map Interactions

        [TitleGroup("Debug Actions")]
        [HorizontalGroup("Debug Actions/Split")]
        [VerticalGroup("Debug Actions/Split/Left")]
        [BoxGroup("Debug Actions/Split/Left/Activation")]
        [Button("Activate Current Tile")]
        private void ManualActivateTile()
        {
            ActivateTile(neighbor_tiles.on_tile, true);
        }

        private bool ActivateTile(ParallaxTileBase tile, bool verbose = false)
        {
            if (verbose) Debug.Log("Activating tile: " + tile);
            bool sneaking = (speed == Constants.SPEEDS[(int)MovementSpeeds.VerySlow]);

            // No tile, must be moving through walls
            if (tile == null)
                return true;

            // Don't activate tile until jump has ended
            if (jumping)
                return false;

            // Bush Flag
            if (tile.is_bush)
            {
                in_bush = true;
            }
            else
                in_bush = false;

            // Grass Animation
            if (ParallaxTerrain.IsGrassTile(tile, true) && !sneaking)
                StartCoroutine(map.GrassRustleAnimation(target_pos));

            // Footprint Animation
            if (ParallaxTerrain.IsSandTile(tile) || ParallaxTerrain.IsSnowTile(tile))
            {
                StartCoroutine(map.FootprintsAnimation(target_pos, direction, sneaking));
            }

            // Shore Animation
            if (shore_anim && (last_pos != target_pos))
            {
                StartCoroutine(map.KillWaterSplashAnimation(last_pos));
                shore_anim = false;
            }
            if (ParallaxTerrain.IsShoreTile(tile))
            {
                StartCoroutine(map.WaterSplashAnimation(target_pos));
                shore_anim = true;
            }

            // On Stairs Flag
            bool prev_on_stairs = on_stairs;
            if (ParallaxTerrain.IsStairTile(tile) || ParallaxTerrain.IsStairTile(neighbor_tiles.on_tile, false, true))
                on_stairs = true;
            else   
                on_stairs = false;
            if (on_stairs != prev_on_stairs)
                on_stairs_changed = true;

            // On Water Flag
            if (ParallaxTerrain.IsWaterTile(tile))
                on_water = true;
            else
                on_water = false;

            // Underwater Flag
            if (ParallaxTerrain.IsUnderwaterTile(tile) || 
            (layer_change == LayerChange.None && ParallaxTerrain.IsWaterTile(neighbor_tiles.above_tile)) ||
            (layer_change == LayerChange.Up && ParallaxTerrain.IsWaterTile(neighbor_tiles.above_tile, true)) ||
            (layer_change == LayerChange.Down && ParallaxTerrain.IsWaterTile(neighbor_tiles.below_tile, true)))
                underwater = true;
            else
                underwater = false;

            return true;
        }

        private bool ActivateTile(Vector3 move_dir)
        {
            if (neighbor_tiles.on_tile == null) return false;
            if (Mathf.Abs(move_dir.x) > 0)
            {
                // Move Right
                if (move_dir.x > 0)
                {
                    // Under Bridge Flag
                    if (map.HideBridgeAbovePosition(this.transform.position + Vector3.right))
                        under_bridge = true;
                    else
                        under_bridge = false;
                    // Behind Terrain Flag
                    if (map.HideLayersAbovePosition(this.transform.position + Vector3.right))
                        behind_upper_layer = true;
                    else   
                        behind_upper_layer = false;
                    // Behind Prefab Flag
                    if (map.HidePrefabBlockingPosition(this.transform.position + Vector3.right))
                        behind_prefab = true;
                    else
                        behind_prefab = false;
                    // Shadow showing for leaving shore
                    if (ParallaxTerrain.IsShoreTile(neighbor_tiles.on_tile) && !ParallaxTerrain.IsShoreTile(neighbor_tiles.right_tile))
                        visibility_changed = true; // Update shadow enabled or not, desired earlier when leaving shore or entering water
                    // Show Reflection Flag
                    if (!underwater && 
                        ((neighbor_tiles.on_tile != null && neighbor_tiles.on_tile.is_reflective) ||
                        (neighbor_tiles.down_tile != null && neighbor_tiles.down_tile.is_reflective) ||
                        (neighbor_tiles.down_right_tile != null && neighbor_tiles.down_right_tile.is_reflective) ||
                        (neighbor_tiles.down_right_two_tile != null && neighbor_tiles.down_right_two_tile.is_reflective)))
                    {
                        if (!show_reflection)
                        {
                            show_reflection = true;
                            visibility_changed = true;
                        }
                        map.SetReflectionMask(target_pos, reflection_masks);
                    }
                    else if (show_reflection)
                    {
                        show_reflection = false;
                        visibility_changed = true;
                    }

                    if (neighbor_tiles.right_tile == null) return false;
                    // Move Onto Right Stairs
                    if (neighbor_tiles.right_tile.terrain_tag == TerrainTags.StairRight)
                    {
                        CancelMovement();
                        if (MoveUpRight()) 
                        {
                            GameObject go = map.GetGameObjectAtPosition(this.transform.position + Vector3.up + Vector3.right);
                            if (go.tag == Constants.TOP_SIDE_STAIR_TILE) MoveLayerUp(false);
                            else MoveLayerUp(true);
                            return ActivateTile(neighbor_tiles.up_right_tile);
                        }
                    }
                    // Move Off Left Stairs
                    else if (neighbor_tiles.on_tile.terrain_tag == TerrainTags.StairLeft)
                    {
                        CancelMovement();
                        if (MoveDownRight())
                        {
                            GameObject go = map.GetGameObjectAtPosition(this.transform.position);
                            if (go.tag == Constants.TOP_SIDE_STAIR_TILE) MoveLayerDown(true);
                            else MoveLayerDown(false);
                            neighbor_tiles = map.GetNeighborTiles(this, false, true);
                            return ActivateTile(neighbor_tiles.down_right_tile);
                        }
                    }
                    // Move Onto Right Ledge
                    else if (neighbor_tiles.right_tile.terrain_tag == TerrainTags.Ledge)
                    {
                        CancelMovement();
                        if (JumpForward(2))
                            return ActivateTile(neighbor_tiles.look_ahead_tile);
                    }
                    // Move Right onto/off Jump Tile
                    else if (neighbor_tiles.right_tile.is_jump || neighbor_tiles.on_tile.is_jump)
                    {
                        CancelMovement();
                        if (JumpForward(1))
                            return ActivateTile(neighbor_tiles.facing_tile);
                    }
                    // Move Onto Water Right
                    else if (!ParallaxTerrain.IsWaterTile(neighbor_tiles.on_tile) && ParallaxTerrain.IsWaterTile(neighbor_tiles.right_tile) &&
                            ParallaxTerrain.IsWaterTile(neighbor_tiles.up_right_tile) && !underwater)
                    {
                        CancelMovement();
                        if (Settings.ALLOW_SWIMMING)
                        {
                            if (JumpForward(2, true))
                            {
                                on_water = true;
                                visibility_changed = true;
                                return ActivateTile(neighbor_tiles.look_ahead_tile);
                            }
                        }
                    }
                    // Move Off Water Right
                    else if (ParallaxTerrain.IsWaterTile(neighbor_tiles.on_tile) && !ParallaxTerrain.IsWaterTile(neighbor_tiles.look_ahead_tile) &&
                            !underwater)
                    {
                        CancelMovement();
                        if (JumpForward(2, true))
                            return ActivateTile(neighbor_tiles.look_ahead_tile);
                    }
                    return ActivateTile(neighbor_tiles.right_tile);
                }
                // Move Left
                else if (move_dir.x < 0)
                {
                    // Under Bridge Flag
                    if (map.HideBridgeAbovePosition(this.transform.position + Vector3.left))
                        under_bridge = true;
                    else
                        under_bridge = false;
                    // Behind Terrain Flag
                    if (map.HideLayersAbovePosition(this.transform.position + Vector3.left))
                        behind_upper_layer = true;
                    else   
                        behind_upper_layer = false;
                    // Behind Prefab Flag
                    if (map.HidePrefabBlockingPosition(this.transform.position + Vector3.left))
                        behind_prefab = true;
                    else
                        behind_prefab = false;
                    // Shadow showing for leaving shore
                    if (ParallaxTerrain.IsShoreTile(neighbor_tiles.on_tile) && !ParallaxTerrain.IsShoreTile(neighbor_tiles.left_tile))
                        visibility_changed = true; // Update shadow enabled or not, desired earlier when leaving shore
                    // Show Reflection Flag
                    if (!underwater && 
                        ((neighbor_tiles.on_tile != null && neighbor_tiles.on_tile.is_reflective) ||
                        (neighbor_tiles.down_tile != null && neighbor_tiles.down_tile.is_reflective) ||
                        (neighbor_tiles.down_left_tile != null && neighbor_tiles.down_left_tile.is_reflective) ||
                        (neighbor_tiles.down_left_two_tile != null && neighbor_tiles.down_left_two_tile.is_reflective)))
                    {
                        if (!show_reflection)
                        {
                            show_reflection = true;
                            visibility_changed = true;
                        }
                        map.SetReflectionMask(target_pos, reflection_masks);
                    }
                    else if (show_reflection)
                    {
                        show_reflection = false;
                        visibility_changed = true;
                    }

                    if (neighbor_tiles.left_tile == null) return false;
                    // Move Onto Left Stairs
                    if (neighbor_tiles.left_tile.terrain_tag == TerrainTags.StairLeft)
                    {
                        CancelMovement();
                        if (MoveUpLeft())
                        {
                            GameObject go = map.GetGameObjectAtPosition(this.transform.position + Vector3.up + Vector3.left);
                            if (go.tag == Constants.TOP_SIDE_STAIR_TILE) MoveLayerUp(false);
                            else MoveLayerUp(true);
                            return ActivateTile(neighbor_tiles.up_left_tile);
                        }
                    }
                    // Move Off Right Stairs
                    else if (neighbor_tiles.on_tile.terrain_tag == TerrainTags.StairRight)
                    {
                        CancelMovement();
                        if (MoveDownLeft())
                        {
                            GameObject go = map.GetGameObjectAtPosition(this.transform.position);
                            if (go.tag == Constants.TOP_SIDE_STAIR_TILE) MoveLayerDown(true);
                            else MoveLayerDown(false);
                            neighbor_tiles = map.GetNeighborTiles(this, false, true);
                            return ActivateTile(neighbor_tiles.down_left_tile);
                        }
                    }
                    // Move Onto Left Ledge
                    else if (neighbor_tiles.left_tile.terrain_tag == TerrainTags.Ledge)
                    {
                        CancelMovement();
                        if (JumpForward(2))
                            return ActivateTile(neighbor_tiles.look_ahead_tile);
                    }
                    // Move Left onto/off Jump Tile
                    else if (neighbor_tiles.left_tile.is_jump || neighbor_tiles.on_tile.is_jump)
                    {
                        CancelMovement();
                        if (JumpForward(1))
                            return ActivateTile(neighbor_tiles.facing_tile);
                    }
                    // Move Onto Water Left
                    else if (!ParallaxTerrain.IsWaterTile(neighbor_tiles.on_tile) && ParallaxTerrain.IsWaterTile(neighbor_tiles.left_tile) &&
                            ParallaxTerrain.IsWaterTile(neighbor_tiles.up_left_tile) && !underwater)
                    {
                        CancelMovement();
                        if (Settings.ALLOW_SWIMMING)
                        {
                            if (JumpForward(2, true))
                            {
                                on_water = true;
                                visibility_changed = true;
                                return ActivateTile(neighbor_tiles.look_ahead_tile);
                            }
                        }
                    }
                    // Move Off Water Left
                    else if (ParallaxTerrain.IsWaterTile(neighbor_tiles.on_tile) && !ParallaxTerrain.IsWaterTile(neighbor_tiles.look_ahead_tile) &&
                            !underwater)
                    {
                        CancelMovement();
                        if (JumpForward(2, true))
                            return ActivateTile(neighbor_tiles.look_ahead_tile);
                    }
                    return ActivateTile(neighbor_tiles.left_tile);
                }
            }
            else if (Mathf.Abs(move_dir.y) > 0)
            {
                // Move Up
                if (move_dir.y > 0)
                {
                    // Under Bridge Flag
                    if (map.HideBridgeAbovePosition(this.transform.position + Vector3.up))
                        under_bridge = true;
                    else
                        under_bridge = false;
                    // Behind Terrain Flag
                    if (map.HideLayersAbovePosition(this.transform.position + Vector3.up))
                        behind_upper_layer = true;
                    else   
                        behind_upper_layer = false;
                    // Behind Prefab Flag
                    if (map.HidePrefabBlockingPosition(this.transform.position + Vector3.up))
                        behind_prefab = true;
                    else
                        behind_prefab = false;
                    // Shadow showing for leaving shore
                    if (ParallaxTerrain.IsShoreTile(neighbor_tiles.on_tile) && !ParallaxTerrain.IsShoreTile(neighbor_tiles.up_tile))
                        visibility_changed = true; // Update shadow enabled or not, desired earlier when leaving shore
                    // Show Reflection Flag
                    if (!underwater && 
                        ((neighbor_tiles.up_tile != null && neighbor_tiles.up_tile.is_reflective) ||
                        (neighbor_tiles.on_tile != null && neighbor_tiles.on_tile.is_reflective) ||
                        (neighbor_tiles.left_tile != null && neighbor_tiles.left_tile.is_reflective) ||
                        (neighbor_tiles.right_tile != null && neighbor_tiles.right_tile.is_reflective)))
                    {
                        if (!show_reflection)
                        {
                            show_reflection = true;
                            visibility_changed = true;
                        }
                        map.SetReflectionMask(target_pos, reflection_masks);
                    }
                    else if (show_reflection)
                    {
                        show_reflection = false;
                        visibility_changed = true;
                    }

                    if (neighbor_tiles.up_tile == null) return false;
                    // Move Off Up Stairs
                    if (neighbor_tiles.on_tile.terrain_tag == TerrainTags.StairUp)
                    {
                        CancelMovement();
                        if (MoveUp())
                        {
                            MoveLayerUp(true);
                            neighbor_tiles = map.GetNeighborTiles(this, false, false, true);
                            return ActivateTile(neighbor_tiles.up_tile);
                        }
                    }
                    // Move Onto Up Ledge
                    else if (neighbor_tiles.up_tile.terrain_tag == TerrainTags.Ledge)
                    {
                        CancelMovement();
                        if (JumpForward(2))
                            return ActivateTile(neighbor_tiles.look_ahead_tile);
                    }
                    // Move Up onto/off Jump Tile
                    else if (neighbor_tiles.up_tile.is_jump || neighbor_tiles.on_tile.is_jump)
                    {
                        CancelMovement();
                        if (JumpForward(1))
                            return ActivateTile(neighbor_tiles.facing_tile);
                    }
                    // Move Onto Water Up
                    else if (!ParallaxTerrain.IsWaterTile(neighbor_tiles.on_tile) && ParallaxTerrain.IsWaterTile(neighbor_tiles.up_tile) &&
                            ParallaxTerrain.IsWaterTile(neighbor_tiles.up_left_tile) && ParallaxTerrain.IsWaterTile(neighbor_tiles.up_right_tile) &&
                            !underwater)
                    {
                        CancelMovement();
                        if (Settings.ALLOW_SWIMMING)
                        {
                            if (JumpForward(1, true))
                            {
                                on_water = true;
                                visibility_changed = true;
                                return ActivateTile(neighbor_tiles.look_ahead_tile);
                            }
                        }
                    }
                    // Move Off Water Up
                    else if (ParallaxTerrain.IsWaterTile(neighbor_tiles.on_tile) && !ParallaxTerrain.IsWaterTile(neighbor_tiles.look_ahead_tile) &&
                            !underwater)
                    {
                        CancelMovement();
                        if (JumpForward(2, true))
                            return ActivateTile(neighbor_tiles.up_tile);
                    }
                    return ActivateTile(neighbor_tiles.up_tile);
                }
                // Move Down
                else if (move_dir.y < 0)
                {
                    // Under Bridge Flag
                    if (map.HideBridgeAbovePosition(this.transform.position + Vector3.down))
                        under_bridge = true;
                    else
                        under_bridge = false;
                    // Behind Terrain Flag
                    if (map.HideLayersAbovePosition(this.transform.position + Vector3.down))
                        behind_upper_layer = true;
                    else   
                        behind_upper_layer = false;
                    // Behind Prefab Flag
                    if (map.HidePrefabBlockingPosition(this.transform.position + Vector3.down))
                        behind_prefab = true;
                    else
                        behind_prefab = false;
                    // Shadow showing for leaving shore
                    if (ParallaxTerrain.IsShoreTile(neighbor_tiles.on_tile) && !ParallaxTerrain.IsShoreTile(neighbor_tiles.down_tile))
                        visibility_changed = true; // Update shadow enabled or not, desired earlier when leaving shore
                    // Show Reflection Flag
                    if (!underwater &&
                        ((neighbor_tiles.down_tile != null && neighbor_tiles.down_tile.is_reflective) ||
                        (neighbor_tiles.down_two_tile != null && neighbor_tiles.down_two_tile.is_reflective) ||
                        (neighbor_tiles.down_two_left_tile != null && neighbor_tiles.down_two_left_tile.is_reflective) ||
                        (neighbor_tiles.down_two_right_tile != null && neighbor_tiles.down_two_right_tile.is_reflective)))
                    {
                        if (!show_reflection)
                        {
                            show_reflection = true;
                            visibility_changed = true;
                        }
                        map.SetReflectionMask(target_pos, reflection_masks);
                    }
                    else if (show_reflection)
                    {
                        show_reflection = false;
                        visibility_changed = true;
                    }

                    if (neighbor_tiles.down_tile == null) return false;
                    // Move Onto Up Stairs
                    if (neighbor_tiles.down_tile.terrain_tag == TerrainTags.StairUp)
                    {
                        CancelMovement();
                        if (MoveDown())
                        {
                            MoveLayerDown(true);
                            return ActivateTile(neighbor_tiles.down_tile);
                        }
                    }
                    // Move Onto Down Ledge
                    else if (neighbor_tiles.down_tile.terrain_tag == TerrainTags.Ledge)
                    {
                        CancelMovement();
                        if (JumpForward(2))
                            return ActivateTile(neighbor_tiles.look_ahead_tile);
                    }
                    // Move Down onto/off Jump Tile
                    else if (neighbor_tiles.down_tile.is_jump || neighbor_tiles.on_tile.is_jump)
                    {
                        CancelMovement();
                        if (JumpForward(1))
                            return ActivateTile(neighbor_tiles.facing_tile);
                    }
                    // Move Onto Water Down
                    else if (!ParallaxTerrain.IsWaterTile(neighbor_tiles.on_tile) && ParallaxTerrain.IsWaterTile(neighbor_tiles.down_tile) &&
                            !underwater)
                    {
                        CancelMovement();
                        if (Settings.ALLOW_SWIMMING)
                        {
                            if (JumpForward(2, true))
                            {
                                on_water = true;
                                visibility_changed = true;
                                return ActivateTile(neighbor_tiles.look_ahead_tile);
                            }
                        }
                    }
                    // Move Off Water Down
                    else if (ParallaxTerrain.IsWaterTile(neighbor_tiles.on_tile) && !ParallaxTerrain.IsWaterTile(neighbor_tiles.down_tile) &&
                            ParallaxTerrain.IsWaterTile(neighbor_tiles.left_tile) && ParallaxTerrain.IsWaterTile(neighbor_tiles.right_tile) &&
                            !underwater)
                    {
                        CancelMovement();
                        if (JumpForward(1, true))
                            return ActivateTile(neighbor_tiles.down_tile);
                    }
                    return ActivateTile(neighbor_tiles.down_tile);
                }
            }

            return false;
        }

        [BoxGroup("Debug Actions/Split/Left/Activation")]
        [Button("Activate Event")]
        public void ActivateEvent() { }

        #endregion


        #region Move Routing

        public IEnumerator StartMoveRoute(MoveCommands[] moves)
        {
            in_move_route = true;

            foreach (MoveCommands move in moves)
            {
                switch (move)
                {
                    case MoveCommands.TurnUp:
                        TurnUp();
                        yield return new WaitForSeconds(1 / speed);
                        break;
                    case MoveCommands.TurnLeft:
                        TurnLeft();
                        yield return new WaitForSeconds(1 / speed);
                        break;
                    case MoveCommands.TurnRight:
                        TurnRight();
                        yield return new WaitForSeconds(1 / speed);
                        break;
                    case MoveCommands.TurnDown:
                        TurnDown();
                        yield return new WaitForSeconds(1 / speed);
                        break;
                    case MoveCommands.Turn90DegreesCCW:
                        Turn90DegreesCCW();
                        yield return new WaitForSeconds(1 / speed);
                        break;
                    case MoveCommands.Turn90DegreesCW:
                        Turn90DegreesCW();
                        yield return new WaitForSeconds(1 / speed);
                        break;
                    case MoveCommands.Turn180Degrees:
                        Turn180Degrees();
                        yield return new WaitForSeconds(1 / speed);
                        break;
                    case MoveCommands.TurnAtRandom:
                        TurnAtRandom();
                        yield return new WaitForSeconds(1 / speed);
                        break;
                    case MoveCommands.TurnTowardsPlayer:
                        TurnTowardsPlayer();
                        yield return new WaitForSeconds(1 / speed);
                        break;
                    case MoveCommands.MoveUp:
                        MoveUp(); break;
                    case MoveCommands.MoveLeft:
                        MoveLeft(); break;
                    case MoveCommands.MoveRight:
                        MoveRight(); break;
                    case MoveCommands.MoveDown:
                        MoveDown(); break;
                    default:
                        break;
                }

                yield return new WaitUntil(() => !moving);
            }

            in_move_route = false;
        }

        #endregion


        #region Direction Setting

        public void TurnUp()
        {
            if (!lock_direction)
            {
                direction = Directions.Up;
                looked = true;
            }
                
        }
        public void TurnLeft()
        {
            if (!lock_direction)
            {
                direction = Directions.Left;
                looked = true;
            }
                
        }
        public void TurnRight()
        {
            if (!lock_direction)
            {
                direction = Directions.Right;
                looked = true;
            }
                
        }
        public void TurnDown()
        {
            if (!lock_direction)
            {
                direction = Directions.Down;
                looked = true;
            }
        }

        public void Turn90DegreesCCW()
        {
            switch (direction)
            {
                case Directions.Up:
                    TurnLeft(); break;
                case Directions.Left:
                    TurnDown(); break;
                case Directions.Right:
                    TurnUp(); break;
                case Directions.Down:
                    TurnRight(); break;
                default:
                    break;
            }
        }
        public void Turn90DegreesCW()
        {
            switch (direction)
            {
                case Directions.Up:
                    TurnRight(); break;
                case Directions.Left:
                    TurnUp(); break;
                case Directions.Right:
                    TurnDown(); break;
                case Directions.Down:
                    TurnLeft(); break;
                default:
                    break;
            }
        }
        public void Turn180Degrees()
        {
            switch (direction)
            {
                case Directions.Up:
                    TurnDown(); break;
                case Directions.Left:
                    TurnRight(); break;
                case Directions.Right:
                    TurnLeft(); break;
                case Directions.Down:
                    TurnUp(); break;
                default:
                    break;
            }
        }
        public void TurnAtRandom()
        {
            System.Random random = Utilities.Random.GetRandom();
            Directions new_direction = (Directions)random.Next(4);
            while (new_direction == direction)
            {
                new_direction = (Directions)UnityEngine.Random.Range(0, 3);
            }
            if (!lock_direction)
            {
                direction = new_direction;
                looked = true;
            }
                
        }
        public void TurnTowardsPlayer()
        {
            // TODO
        }

        #endregion


        #region Movement Execution

        public void CancelMovement()
        {
            target_pos = last_pos;
            shadow_target_pos = shadow_home_pos;
            reflection_mask_trans.localPosition = reflection_mask_home_pos;
            reflection_mask_target_pos = reflection_mask_home_pos;
            reflection_target_pos = reflection_home_pos;
        }

        [VerticalGroup("Debug Actions/Split/Right")]
        [BoxGroup("Debug Actions/Split/Right/Movement")]
        [Button("Move Up")]
        public bool MoveUp()
        {
            TurnUp();
            if (move_through_walls || 
                (neighbor_tiles.on_tile != null && neighbor_tiles.up_tile != null &&
                (up_event == null || up_event.IsPassable()) && neighbor_tiles.up_tile.allow_passage &&
                neighbor_tiles.up_tile.down_passage && 
                (neighbor_tiles.on_tile.up_passage || (ParallaxTerrain.IsWaterTile(neighbor_tiles.up_tile) && !neighbor_tiles.facing_other_level)) &&
                ((ParallaxTerrain.IsWaterTile(neighbor_tiles.up_tile) && underwater) ||
                (!ParallaxTerrain.IsWaterTile(neighbor_tiles.up_tile) || neighbor_tiles.up_left_tile == null || (neighbor_tiles.up_left_tile != null && (ParallaxTerrain.IsWaterTile(neighbor_tiles.up_tile) == ParallaxTerrain.IsWaterTile(neighbor_tiles.up_left_tile)))) &&
                (!ParallaxTerrain.IsWaterTile(neighbor_tiles.up_tile) || neighbor_tiles.up_right_tile == null || (neighbor_tiles.up_right_tile != null && (ParallaxTerrain.IsWaterTile(neighbor_tiles.up_tile) == ParallaxTerrain.IsWaterTile(neighbor_tiles.up_right_tile)))) &&
                (!ParallaxTerrain.IsWaterTile(neighbor_tiles.up_tile) || !ParallaxTerrain.IsWaterTile(neighbor_tiles.look_ahead_tile) || neighbor_tiles.look_ahead_cw_tile == null || (neighbor_tiles.look_ahead_cw_tile != null && (ParallaxTerrain.IsWaterTile(neighbor_tiles.look_ahead_tile) == ParallaxTerrain.IsWaterTile(neighbor_tiles.look_ahead_cw_tile)))) &&
                (!ParallaxTerrain.IsWaterTile(neighbor_tiles.up_tile) || !ParallaxTerrain.IsWaterTile(neighbor_tiles.look_ahead_tile) || neighbor_tiles.look_ahead_ccw_tile == null || (neighbor_tiles.look_ahead_ccw_tile != null && (ParallaxTerrain.IsWaterTile(neighbor_tiles.look_ahead_tile) == ParallaxTerrain.IsWaterTile(neighbor_tiles.look_ahead_ccw_tile)))) &&
                (ParallaxTerrain.IsStairTile(neighbor_tiles.up_tile, true) == ParallaxTerrain.IsStairTile(neighbor_tiles.on_tile, true))
                )))
            {
                last_pos = transform.position;
                target_pos += Vector3.up;
                reflection_mask_trans.localPosition += Vector3.down;
                moving = true;
                tile_activated = false;
                return true;
            }
            return false;
        }
        
        [BoxGroup("Debug Actions/Split/Right/Movement")]
        [Button("Move Left")]
        public bool MoveLeft()
        {
            TurnLeft();
            if (move_through_walls || neighbor_tiles.on_tile.terrain_tag == TerrainTags.StairRight ||
                (neighbor_tiles.on_tile != null && neighbor_tiles.left_tile != null &&
                (left_event == null || left_event.IsPassable()) && neighbor_tiles.on_tile.left_passage &&
                neighbor_tiles.left_tile.allow_passage && neighbor_tiles.left_tile.right_passage) &&
                ((ParallaxTerrain.IsWaterTile(neighbor_tiles.left_tile) && underwater) ||
                (!ParallaxTerrain.IsWaterTile(neighbor_tiles.left_tile) || neighbor_tiles.up_left_tile == null || (neighbor_tiles.up_left_tile != null && (ParallaxTerrain.IsWaterTile(neighbor_tiles.left_tile) == ParallaxTerrain.IsWaterTile(neighbor_tiles.up_left_tile)))) &&
                (!ParallaxTerrain.IsWaterTile(neighbor_tiles.left_tile) || !ParallaxTerrain.IsWaterTile(neighbor_tiles.look_ahead_tile) || neighbor_tiles.look_ahead_cw_tile == null || (neighbor_tiles.look_ahead_cw_tile != null && (ParallaxTerrain.IsWaterTile(neighbor_tiles.look_ahead_tile) == ParallaxTerrain.IsWaterTile(neighbor_tiles.look_ahead_cw_tile))))
                ))
            {
                last_pos = transform.position;
                target_pos += Vector3.left;
                reflection_mask_trans.localPosition += Vector3.left;
                moving = true;
                tile_activated = false;
                return true;
            }
            return false;
        }
        
        [BoxGroup("Debug Actions/Split/Right/Movement")]
        [Button("Move Right")]
        public bool MoveRight()
        {
            TurnRight();
            if (move_through_walls || neighbor_tiles.on_tile.terrain_tag == TerrainTags.StairLeft ||
                (neighbor_tiles.on_tile != null && neighbor_tiles.right_tile != null &&
                (right_event == null || right_event.IsPassable()) && neighbor_tiles.on_tile.right_passage &&
                neighbor_tiles.right_tile.allow_passage && neighbor_tiles.right_tile.left_passage) &&
                ((ParallaxTerrain.IsWaterTile(neighbor_tiles.right_tile) && underwater) ||
                (!ParallaxTerrain.IsWaterTile(neighbor_tiles.right_tile) || neighbor_tiles.up_right_tile == null || (neighbor_tiles.up_right_tile != null && (ParallaxTerrain.IsWaterTile(neighbor_tiles.right_tile) == ParallaxTerrain.IsWaterTile(neighbor_tiles.up_right_tile)))) &&
                (!ParallaxTerrain.IsWaterTile(neighbor_tiles.right_tile) || !ParallaxTerrain.IsWaterTile(neighbor_tiles.look_ahead_tile) || neighbor_tiles.look_ahead_ccw_tile == null || (neighbor_tiles.look_ahead_ccw_tile != null && (ParallaxTerrain.IsWaterTile(neighbor_tiles.look_ahead_tile) == ParallaxTerrain.IsWaterTile(neighbor_tiles.look_ahead_ccw_tile))))
                ))
            {
                last_pos = transform.position;
                target_pos += Vector3.right;
                reflection_mask_trans.localPosition += Vector3.right;
                moving = true;
                tile_activated = false;
                return true;
            }
            return false;
        }
        
        [BoxGroup("Debug Actions/Split/Right/Movement")]
        [Button("Move Down")]
        public bool MoveDown()
        {
            TurnDown();
            if (move_through_walls || (neighbor_tiles.on_tile != null && neighbor_tiles.down_tile != null &&
                (down_event == null || down_event.IsPassable()) && neighbor_tiles.on_tile.down_passage &&
                neighbor_tiles.down_tile.allow_passage && 
                (neighbor_tiles.down_tile.up_passage || (ParallaxTerrain.IsWaterTile(neighbor_tiles.on_tile) && !neighbor_tiles.facing_other_level)) &&
                ((ParallaxTerrain.IsWaterTile(neighbor_tiles.down_tile) && underwater) ||
                (!ParallaxTerrain.IsWaterTile(neighbor_tiles.down_tile) || neighbor_tiles.down_left_tile == null || (neighbor_tiles.down_left_tile != null && (ParallaxTerrain.IsWaterTile(neighbor_tiles.down_tile) == ParallaxTerrain.IsWaterTile(neighbor_tiles.down_left_tile)))) &&
                (!ParallaxTerrain.IsWaterTile(neighbor_tiles.down_tile) || neighbor_tiles.down_right_tile == null || (neighbor_tiles.down_right_tile != null && (ParallaxTerrain.IsWaterTile(neighbor_tiles.down_tile) == ParallaxTerrain.IsWaterTile(neighbor_tiles.down_right_tile)))) &&
                (ParallaxTerrain.IsStairTile(neighbor_tiles.down_tile, true) == ParallaxTerrain.IsStairTile(neighbor_tiles.on_tile, true))
                )))
            {
                last_pos = transform.position;
                target_pos += Vector3.down;
                reflection_mask_trans.localPosition += Vector3.up;
                moving = true;
                tile_activated = false;
                return true;
            }
            return false;
        }

        public bool MoveUpLeft()
        {
            TurnLeft();
            // TODO Check for Events
            if (move_through_walls || (neighbor_tiles.on_tile != null && neighbor_tiles.up_left_tile != null &&
            neighbor_tiles.up_left_tile.allow_passage))
            {
                last_pos = transform.position;
                target_pos += Vector3.up + Vector3.left;
                moving = true;
                tile_activated = false;
                return true;
            }
            return false;
        }
        public bool MoveUpRight()
        {
            TurnRight();
            // TODO Check for Events
            if (move_through_walls || (neighbor_tiles.on_tile != null && neighbor_tiles.up_right_tile != null &&
            neighbor_tiles.up_right_tile.allow_passage))
            {
                last_pos = transform.position;
                target_pos += Vector3.up + Vector3.right;
                moving = true;
                tile_activated = false;
                return true;
            }
            return false;
        }
        public bool MoveDownLeft()
        {
            TurnLeft();
            // TODO Check for Events
            if (move_through_walls || (neighbor_tiles.on_tile != null && neighbor_tiles.down_left_tile != null &&
            neighbor_tiles.down_left_tile.allow_passage))
            {
                last_pos = transform.position;
                target_pos += Vector3.down + Vector3.left;
                moving = true;
                tile_activated = false;
                return true;
            }
            return false;
        }
        public bool MoveDownRight()
        {
            TurnRight();
            // TODO Check for Events
            if (move_through_walls || (neighbor_tiles.on_tile != null && neighbor_tiles.down_right_tile != null &&
            neighbor_tiles.down_right_tile.allow_passage))
            {
                last_pos = transform.position;
                target_pos += Vector3.down + Vector3.right;
                moving = true;
                tile_activated = false;
                return true;
            }
            return false;
        }

        [BoxGroup("Debug Actions/Split/Right/Movement")]
        [Button("Step Forward")]
        public bool StepForward()
        {
            bool success = false;
            switch (direction)
            {
                case Directions.Up:
                    success = MoveUp();
                    break;
                case Directions.Left:
                    success = MoveLeft();
                    break;
                case Directions.Right:
                    success = MoveRight();
                    break;
                case Directions.Down:
                    success = MoveDown();
                    break;
                default:
                    success = false;
                    break;
            }
            moving = true;
            return success;
        }
        
        [BoxGroup("Debug Actions/Split/Right/Movement")]
        [Button("Step Backward")]
        public bool StepBackward()
        {
            bool prev_lock_direction = lock_direction;
            bool success = false;
            switch (direction)
            {
                case Directions.Up:
                    LockDirectionOn();
                    success = MoveDown();
                    lock_direction = prev_lock_direction;
                    break;
                case Directions.Left:
                    LockDirectionOn();
                    success = MoveRight();
                    lock_direction = prev_lock_direction;
                    break;
                case Directions.Right:
                    LockDirectionOn();
                    success = MoveLeft();
                    lock_direction = prev_lock_direction;
                    break;
                case Directions.Down:
                    LockDirectionOn();
                    success = MoveUp();
                    lock_direction = prev_lock_direction;
                    break;
                default:
                    break;
            }
            moving = true;
            return success;
        }

        [BoxGroup("Debug Actions/Split/Right/Movement")]
        [Button("Move at Random")]
        public bool MoveAtRandom()
        {
            bool success = false;
            System.Random random = Utilities.Random.GetRandom();
            Directions new_direction = (Directions)random.Next(4);
            switch (new_direction)
            {
                case Directions.Up:
                    success = MoveUp();
                    break;
                case Directions.Left:
                    success = MoveLeft();
                    break;
                case Directions.Right:
                    success = MoveRight();
                    break;
                case Directions.Down:
                    success = MoveDown();
                    break;
                default:
                    break;
            }
            moving = true;
            return success;
        }

        [BoxGroup("Debug Actions/Split/Right/Movement")]
        [Button("Jump in Place")]
        public bool JumpInPlace() { 
            float height = Constants.JUMP_HEIGHT;
            last_pos = transform.position;
            if (ParallaxTerrain.IsShoreTile(neighbor_tiles.on_tile))
            {
                StartCoroutine(map.KillWaterSplashAnimation(last_pos));
                shore_anim = false;
            }

            target_pos += (height * Vector3.back) + (height * Vector3.up);
            shadow_target_pos += (height * Vector3.forward) + (height * Vector3.down);
            reflection_mask_target_pos += (height * Vector3.forward) + (height * Vector3.up);
            reflection_target_pos += (1.5f * height * Vector3.up);
            moving = true;
            jumping = true;
            jump_data = new JumpData (height, new Vector3(), 0, Directions.Up, neighbor_tiles.on_tile.is_reflective);
            tile_activated = false;

            return true;
        }
        
        [BoxGroup("Debug Actions/Split/Right/Movement")]
        [Button("Jump Forward")]
        public bool JumpForward(int num_tiles = 1, bool onto_water = false)
        {
            if (num_tiles < 1) return false;
            // TODO Check for Events
            // TODO Check more than 2 tiles ahead
            ParallaxTileBase check_tile = neighbor_tiles.facing_tile;
            if (move_through_walls || (neighbor_tiles.on_tile != null &&
                ((num_tiles >2) ||
                (num_tiles == 1 && check_tile != null && check_tile.allow_passage && !ParallaxTerrain.IsLedgeTile(check_tile) && !neighbor_tiles.facing_other_level && !ParallaxTerrain.IsStairTile(check_tile) && (onto_water || !ParallaxTerrain.IsWaterTile(check_tile))) ||
                (num_tiles == 2 && (Settings.ALLOW_JUMP_OVER_OBJECTS || (check_tile != null && check_tile.allow_passage)) && (check_tile != null && !ParallaxTerrain.IsStairTile(check_tile)) && neighbor_tiles.look_ahead_tile != null && neighbor_tiles.look_ahead_tile.allow_passage && !ParallaxTerrain.IsLedgeTile(neighbor_tiles.look_ahead_tile) && !ParallaxTerrain.IsStairTile(neighbor_tiles.look_ahead_tile) && !neighbor_tiles.look_ahead_other_level && (onto_water || !ParallaxTerrain.IsWaterTile(neighbor_tiles.look_ahead_tile)))
            )))
            {            
                float height = Constants.JUMP_HEIGHT * num_tiles;
                Vector3 v = new Vector3();
                bool ledge_dir_allowed = true;

                switch (direction)
                {
                    case Directions.Up:
                        v = Vector3.up;
                        if (check_tile != null && check_tile.terrain_tag == TerrainTags.Ledge)
                            ledge_dir_allowed = check_tile.down_passage;
                        break;
                    case Directions.Left:
                        v = Vector3.left;
                        if (check_tile != null && check_tile.terrain_tag == TerrainTags.Ledge)
                            ledge_dir_allowed = check_tile.right_passage;
                        break;
                    case Directions.Right:
                        v = Vector3.right;
                        if (check_tile != null && check_tile.terrain_tag == TerrainTags.Ledge)
                            ledge_dir_allowed = check_tile.left_passage;
                        break;
                    case Directions.Down:
                        v = Vector3.down;
                        if (check_tile != null && check_tile.terrain_tag == TerrainTags.Ledge)
                            ledge_dir_allowed = check_tile.up_passage;
                        break;
                    default:
                        break;
                }
                if (!Settings.ALLOW_JUMP_UP_LEDGES && !ledge_dir_allowed) return false;

                last_pos = transform.position;
                if (ParallaxTerrain.IsShoreTile(neighbor_tiles.on_tile))
                {
                    StartCoroutine(map.KillWaterSplashAnimation(last_pos));
                    shore_anim = false;
                }

                target_pos += ((height * Vector3.back) + (height * Vector3.up) + (v * (float)num_tiles / 2f));
                shadow_target_pos += (height * Vector3.forward) + (height * Vector3.down);

                reflection_mask_target_pos += (1.5f * Vector3.back) + (1.5f * Vector3.up);
                reflection_target_pos += (1.5f * height * Vector3.up);

                bool source_reflective = neighbor_tiles.on_tile.is_reflective;
                float jump_mult = source_reflective ? 0.5f : 0.75f;

                if (direction == Directions.Left || direction == Directions.Right)
                    reflection_mask_trans.localPosition += jump_mult * ((v * (float)num_tiles) / 2f);
                else if (direction == Directions.Down && source_reflective)
                    reflection_mask_trans.localPosition -= jump_mult * ((v * (float)num_tiles) / 2f);
                else if (direction == Directions.Down && !source_reflective)
                    reflection_mask_trans.localPosition -= 1.25f * jump_mult * ((v * (float)num_tiles) / 2f);
                else if (direction == Directions.Up && !source_reflective)
                    reflection_mask_trans.localPosition -= 0.75f * (v * (float)num_tiles / 2f);

                moving = true;
                jumping = true;
                jump_data = new JumpData (height, v, num_tiles, direction, source_reflective);

                AnimSetBool(Constants.ANIM_JUMP, true);
                tile_activated = false;
                return true;
            }
            // Don't jump when trying to enter/leave water that is blocked
            // if (ParallaxTerrain.IsWaterTile(neighbor_tiles.facing_tile)) return false;
            // Attempt shorter jump or hop in place if long jump blocked
            else return JumpForward(num_tiles - 1);
        }
        
        public void JumpBackward(int num_tiles) { }

        #endregion


        #region Movement Between Layers

        public void MoveLayerUp(bool delay_until_move_comp = false)
        {
            last_pos = transform.position;
            target_pos += (Constants.MAP_LAYER_HEIGHT * Vector3.back);
            moving = true;
            if (delay_until_move_comp) layer_up_delay = true;
            else 
            {
                layer += 1;
                layer_changed = true;
            }
            tile_activated = false;
        }
        public void MoveLayerDown(bool delay_until_move_comp = false)
        {
            last_pos = transform.position;
            target_pos += (Constants.MAP_LAYER_HEIGHT * Vector3.forward);
            moving = true;
            if (delay_until_move_comp) layer_down_delay = true;
            else 
            {
                layer -= 1;
                layer_changed = true;
            }
            tile_activated = false;
        }

        [BoxGroup("Debug Actions/Split/Right/Movement")]
        [Button("Rise Up")]
        public bool RiseUp()
        {
            if (neighbor_tiles.above_tile != null && neighbor_tiles.above_tile.allow_passage)
            {
                if (underwater && ParallaxTerrain.IsWaterTile(neighbor_tiles.above_tile) && !ParallaxTerrain.IsUnderwaterTile(neighbor_tiles.above_tile))
                {
                    target_rotation = Constants.DEFAULT_PERSPECTIVE_ANGLE;
                    main_sprite_target_pos -= Constants.UNDERWATER_SPRITE_OFFSET;
                    bush_mask.transform.localPosition -= Constants.UNDERWATER_BUSH_MASK_OFFSET;
                    rotating = true;
                }

                bool prev_move_through_walls = move_through_walls;
                bool prev_lock_direction = lock_direction;
                MoveThroughWallsOn();
                LockDirectionOn();
                layer_change = LayerChange.Up;
                MoveLayerUp(true);
                MoveUp();
                if (!prev_move_through_walls) MoveThroughWallsOff();
                if (!prev_lock_direction) LockDirectionOff();
                return true;
            }
            return false;
        }
        
        [BoxGroup("Debug Actions/Split/Right/Movement")]
        [Button("Sink Down")]
        public bool SinkDown(bool fall = false)
        {
            if (fall || (neighbor_tiles.below_tile != null && neighbor_tiles.below_tile.allow_passage))
            {
                if (!underwater && ParallaxTerrain.IsUnderwaterTile(neighbor_tiles.below_tile))
                {
                    target_rotation = Constants.UNDERWATER_PERSPECTIVE_ANGLE;
                    main_sprite_target_pos += Constants.UNDERWATER_SPRITE_OFFSET;
                    bush_mask.transform.localPosition += Constants.UNDERWATER_BUSH_MASK_OFFSET;
                    rotating = true;
                }

                bool prev_move_through_walls = move_through_walls;
                bool prev_lock_direction = lock_direction;
                MoveThroughWallsOn();
                LockDirectionOn();
                MoveLayerDown(false);
                layer_change = LayerChange.Down;
                MoveDown();
                if (!prev_move_through_walls) MoveThroughWallsOff();
                if (!prev_lock_direction) LockDirectionOff();
                return true;
            }
            return false;
        }

        #endregion


        #region Set Flags

        [BoxGroup("Debug Actions/Split/Left/Static Settings")]
        [Button("Invisible On")]
        public void InvisibleOn()
        {
            invisible = true;
            visibility_changed = true;
        }
        [BoxGroup("Debug Actions/Split/Left/Static Settings")]
        [Button("Invisible Off")]
        public void InvisibleOff()
        {
            invisible = false;
            visibility_changed = true;
        }

        public void MoveThroughWallsOn()
        {
            move_through_walls = true;
        }
        public void MoveThroughWallsOff()
        {
            move_through_walls = false;
        }

        public void LockDirectionOn()
        {
            lock_direction = true;
        }
        public void LockDirectionOff()
        {
            lock_direction = false;
        }

        public void WalkingAnimationOn()
        {
            walking_animation = true;
        }
        public void WalkingAnimationOff()
        {
            walking_animation = true;
        }

        [BoxGroup("Debug Actions/Split/Left/Static Settings")]
        [Button("Stepping Anim On")]
        public void SteppingAnimationOn()
        {
            stepping_animation = true;
        }
        [BoxGroup("Debug Actions/Split/Left/Static Settings")]
        [Button("Stepping Anim Off")]
        public void SteppingAnimationOff()
        {
            stepping_animation = false;
        }

        [BoxGroup("Debug Actions/Split/Left/Static Settings")]
        [Button("Outline")]
        public void SetOutline(OutlineColors color)
        {
            main_sprite.material = SpriteUtils.GetOutlineMaterial(color);
        }

        #endregion


        #region Misc. Effects

        public IEnumerator Wait(float seconds)
        {
            yield return new WaitForSeconds(seconds);
        }

        public void ChangeSpeed(MovementSpeeds new_speed)
        {
            movement_speed = new_speed;
        }

        #endregion


        #region Animator Interactions

        public void AnimSetInt(string variable, int value)
        {
            animator.SetInteger(variable, value);
            reflection_animator.SetInteger(variable, value);
        }
        public void AnimSetBool(string variable, bool value)
        {
            animator.SetBool(variable, value);
            reflection_animator.SetBool(variable, value);
        }

        #endregion
    }
}