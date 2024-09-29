using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Mapping;
using Utilities;
using Eventing;

namespace Eventing
{
    #region Enums

    public enum MovementSpeeds
    {
        VerySlow,
        Slow,
        Moderate,
        Fast,
        VeryFast
    }

    public enum Directions
    {
        Up,
        Left,
        Right,
        Down
    }

    public enum LayerChange
    {
        None,
        Up,
        Down
    }

    public enum MoveCommands
    {
        TurnUp,
        TurnLeft,
        TurnRight,
        TurnDown,
        Turn90DegreesCW,
        Turn90DegreesCCW,
        Turn180Degrees,
        TurnAtRandom,
        TurnTowardsPlayer,
        MoveUp,
        MoveLeft,
        MoveRight,
        MoveDown,
        StepForward,
        StepBackward,
        MoveAtRandom,
        Jump,
        JumpForward,
        JumpBackward,
        MoveLayerUp,
        MoveLayerDown,
        RiseUp,
        SinkDown,
        SetInvisibleFlag,
        SetThroughFlag,
        SetLockDirectionFlag,
        SetWalkingFlag,
        SetSteppingFlag,
        Wait,
        ChangeSpeed
    }

    #endregion


    #region Structs

    [Serializable]
    public struct JumpData
    {
        public float height;
        public Vector3 direction;
        public int num_tiles;

        public JumpData(float height, Vector3 direction, int num_tiles)
        {
            this.height = height;
            this.direction = direction;
            this.num_tiles = num_tiles;
        }
    }

    #endregion


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
        [HideInInspector]
        public LayerChange layer_change;
        private JumpData jump_data;

        private SpriteRenderer[] sprites;
        private SpriteMask bush_mask;
        private SpriteRenderer shadow;

        [Title("Static Flags")]
        public bool invisible = false;
        public bool move_through_walls = false;
        public bool lock_direction = false;
        public bool walking_animation = true;
        public bool stepping_animation = false;
        public bool always_on_top = false;

        [Title("Awareness")]
        [ReadOnly]
        public bool in_bush;
        [ReadOnly]
        public bool on_stairs;
        [ReadOnly]
        public bool on_water;
        [ReadOnly]
        public bool underwater;
        

        [TabGroup ("Movement")]
        [ReadOnly]
        public bool moving;
        [TabGroup ("Movement")]
        [ReadOnly]
        public bool looked;
        [TabGroup ("Movement")]
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
            speed = Constants.SPEEDS[(int)movement_speed];
            //animator = GetComponentInChildren<Animator>();
            sprites = GetComponentsInChildren<SpriteRenderer>();
            bush_mask = GetComponentInChildren<SpriteMask>();
            foreach (SpriteRenderer sprite in sprites)
            {
                if (sprite.tag == Constants.SHADOW_TAG)
                {
                    shadow = sprite;
                    break;
                }
            }
            moving = true;
            looked = true;
            jumping = false;
            jump_data = new JumpData{};
            layer_change = LayerChange.None;
            falling = false;
            other_moved = false;
            in_move_route = false;
            tile_activated = false;

            // Map Awareness Setup
            map = FindObjectOfType<Map>(); // TODO - better way to find map
            // event_manager = FindObjectOfType<EventManager>();

            // Update Animator
            // animator.SetInteger(Constants.DIRECTION_ANIMATION, (int)direction);
            // if (stepping_animation)
            //     animator.SetBool(Constants.STEP_ANIMATION, true);
        }

        public void OnSpaceEntered()
        {
            other_moved = true;
        }

        private void Update()
        {
            // Update Animator
            // animator.SetInteger(Constants.DIRECTION_ANIMATION, (int)direction);
            // if (walking_animation && !stepping_animation)
            //     animator.SetBool(Constants.WALK_ANIMATION, moving);
            // else if (stepping_animation)
            //     animator.SetBool(Constants.WALK_ANIMATION, true);
            
            speed = (jumping || falling) ? (
                jump_data.num_tiles > 0 ? Constants.SPEEDS[(int)MovementSpeeds.Fast] : Constants.SPEEDS[(int)MovementSpeeds.Moderate]) : 
                Constants.SPEEDS[(int)movement_speed];
            
            // Apply Movement
            if (moving)
            {
                Vector3 move_dir = target_pos - transform.position;
                
                // Activate the tile being moved onto
                if (!tile_activated && !jumping && !falling && layer_change == LayerChange.None)
                {
                    tile_activated = ActivateTile(move_dir);
                }
                else if (layer_change == LayerChange.Up)
                {
                    tile_activated = ActivateTile(neighbor_tiles.above_tile);
                    layer_change = LayerChange.None;
                }
                    
                else if (layer_change == LayerChange.Down)
                {
                    tile_activated = ActivateTile(neighbor_tiles.below_tile);
                    layer_change = LayerChange.None;
                }

                // Move in that direction
                transform.position = Vector3.MoveTowards(transform.position, target_pos, Time.deltaTime * speed);
            }

            // Update Sprites & Sorting Order
            foreach (SpriteRenderer sprite in sprites)
            {
                if (sprite.tag == Constants.PRIORITY_TILE_TAG)
                    sprite.sortingOrder = (int)(layer * Constants.SORTING_LAYERS_PER_MAP_LAYER) + Constants.EVENT_SORTING_LAYER_OFFSET + Constants.PRIORITY_TILE_OFFSET;
                else if (sprite.tag == Constants.DEPRIORITY_TILE_TAG || sprite.tag == Constants.SHADOW_TAG)
                    sprite.sortingOrder = (int)(layer * Constants.SORTING_LAYERS_PER_MAP_LAYER) + Constants.EVENT_SORTING_LAYER_OFFSET - Constants.PRIORITY_TILE_OFFSET;
                else
                    sprite.sortingOrder = (int)(layer * Constants.SORTING_LAYERS_PER_MAP_LAYER) + Constants.EVENT_SORTING_LAYER_OFFSET;
                if (on_stairs)
                    sprite.sortingOrder += Constants.PRIORITY_TILE_OFFSET;
                
                if (invisible)
                    sprite.enabled = false;
                else
                    sprite.enabled = true;
            }          
            if (in_bush && !jumping && !falling)
                bush_mask.enabled = true;
            else
                bush_mask.enabled = false;

            // Tilemap and Event awareness check
            if ((other_moved || moving) && transform.position == target_pos)
            {
                if (jumping)
                {
                    target_pos += ((jump_data.height * Vector3.forward) + (jump_data.height * Vector3.down) + (jump_data.direction * (float)jump_data.num_tiles / 2));
                    jumping = false;
                    falling = true;
                }
                else
                {
                    // Get neighboring tiles
                    neighbor_tiles = map.GetNeighborTiles(this);

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
                    falling = false;
                    jump_data = new JumpData{};
                    other_moved = false;
                    if (!tile_activated)
                    {
                        tile_activated = ActivateTile(neighbor_tiles.on_tile);
                    }  
                }   
            }
            // Update facing & look ahead tile
            else if (looked)
            {
                neighbor_tiles = map.GetNeighborTiles(this, true);
                looked = false;
            }
            // Falling physics when in air
            else if (neighbor_tiles.on_tile == null && !move_through_walls && !moving && !jumping && !falling)
            {
                SinkDown(true);
                // target_pos += Vector3.forward + Vector3.down;
                // moving = true;
                // falling = true;
            }

            // Fall to ground
            // if (neighbor_tiles.on_tile == null && !moving && !falling)
            // {
            //     target_pos += Vector3.forward + Vector3.down;
            //     falling = true;
            // }
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
            if (verbose) Debug.Log("Activating tile: " + neighbor_tiles.on_tile);

            // No tile, must be moving through walls
            if (tile == null)
                return true;

            // Don't activate tile until jump has ended
            if (jumping)
                return false;

            // Bush Flag
            if (tile.is_bush)
                in_bush = true; // TODO - bush animation
            else
                in_bush = false;

            // On Stairs Flag
            if (ParallaxTerrain.IsStairTile(tile) || ParallaxTerrain.IsStairTile(neighbor_tiles.on_tile))
                on_stairs = true;
            else   
                on_stairs = false;

            // On Water Flag
            if (ParallaxTerrain.IsWaterTile(tile))
                on_water = true;
            else
                on_water = false;

            // Underwater Flag
            if (ParallaxTerrain.IsUnderwaterTile(tile))
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
                    if (neighbor_tiles.right_tile == null) return false;
                    // Move Onto Right Stairs
                    if (neighbor_tiles.right_tile.terrain_tag == TerrainTags.StairRight)
                    {
                        CancelMovement();
                        if (MoveUpRight()) 
                        {
                            MoveLayerUp();
                            return ActivateTile(neighbor_tiles.up_right_tile);
                        }
                    }
                    // Move Off Left Stairs
                    else if (neighbor_tiles.on_tile.terrain_tag == TerrainTags.StairLeft)
                    {
                        CancelMovement();
                        if (MoveDownRight())
                        {
                            MoveLayerDown();
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
                    // Move Onto Water Right
                    else if (!ParallaxTerrain.IsWaterTile(neighbor_tiles.on_tile) && ParallaxTerrain.IsWaterTile(neighbor_tiles.right_tile) &&
                            ParallaxTerrain.IsWaterTile(neighbor_tiles.up_right_tile))
                    {
                        CancelMovement();
                        if (JumpForward(2, true))
                            return ActivateTile(neighbor_tiles.look_ahead_tile);
                    }
                    // Move Off Water Right
                    else if (ParallaxTerrain.IsWaterTile(neighbor_tiles.on_tile) && !ParallaxTerrain.IsWaterTile(neighbor_tiles.look_ahead_tile))
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
                    if (neighbor_tiles.left_tile == null) return false;
                    // Move Onto Left Stairs
                    if (neighbor_tiles.left_tile.terrain_tag == TerrainTags.StairLeft)
                    {
                        CancelMovement();
                        if (MoveUpLeft())
                        {
                            MoveLayerUp();
                            return ActivateTile(neighbor_tiles.up_left_tile);
                        }
                    }
                    // Move Off Right Stairs
                    else if (neighbor_tiles.on_tile.terrain_tag == TerrainTags.StairRight)
                    {
                        CancelMovement();
                        if (MoveDownLeft())
                        {
                            MoveLayerDown();
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
                    // Move Onto Water Left
                    else if (!ParallaxTerrain.IsWaterTile(neighbor_tiles.on_tile) && ParallaxTerrain.IsWaterTile(neighbor_tiles.left_tile) &&
                            ParallaxTerrain.IsWaterTile(neighbor_tiles.up_left_tile))
                    {
                        CancelMovement();
                        if (JumpForward(2, true))
                            return ActivateTile(neighbor_tiles.look_ahead_tile);
                    }
                    // Move Off Water Left
                    else if (ParallaxTerrain.IsWaterTile(neighbor_tiles.on_tile) && !ParallaxTerrain.IsWaterTile(neighbor_tiles.look_ahead_tile))
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
                    if (neighbor_tiles.up_tile == null) return false;
                    // Move Off Up Stairs
                    if (neighbor_tiles.on_tile.terrain_tag == TerrainTags.StairUp)
                    {
                        CancelMovement();
                        if (MoveUp())
                        {
                            MoveLayerUp();
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
                    // Move Onto Water Up
                    else if (!ParallaxTerrain.IsWaterTile(neighbor_tiles.on_tile) && ParallaxTerrain.IsWaterTile(neighbor_tiles.up_tile) &&
                            ParallaxTerrain.IsWaterTile(neighbor_tiles.up_left_tile) && ParallaxTerrain.IsWaterTile(neighbor_tiles.up_right_tile))
                    {
                        CancelMovement();
                        if (JumpForward(1, true))
                            return ActivateTile(neighbor_tiles.up_tile);
                    }
                    // Move Off Water Up
                    else if (ParallaxTerrain.IsWaterTile(neighbor_tiles.on_tile) && !ParallaxTerrain.IsWaterTile(neighbor_tiles.look_ahead_tile))
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
                    if (neighbor_tiles.down_tile == null) return false;
                    // Move Onto Up Stairs
                    if (neighbor_tiles.down_tile.terrain_tag == TerrainTags.StairUp)
                    {
                        CancelMovement();
                        if (MoveDown())
                        {
                            MoveLayerDown();
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
                    // Move Onto Water Down
                    else if (!ParallaxTerrain.IsWaterTile(neighbor_tiles.on_tile) && ParallaxTerrain.IsWaterTile(neighbor_tiles.down_tile))
                    {
                        CancelMovement();
                        if (JumpForward(2, true))
                            return ActivateTile(neighbor_tiles.down_tile);
                    }
                    // Move Off Water Down
                    else if (ParallaxTerrain.IsWaterTile(neighbor_tiles.on_tile) && !ParallaxTerrain.IsWaterTile(neighbor_tiles.down_tile) &&
                            ParallaxTerrain.IsWaterTile(neighbor_tiles.left_tile) && ParallaxTerrain.IsWaterTile(neighbor_tiles.right_tile))
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
            target_pos = transform.position;
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
                (!ParallaxTerrain.IsWaterTile(neighbor_tiles.up_tile) || neighbor_tiles.up_left_tile == null || (neighbor_tiles.up_left_tile != null && (ParallaxTerrain.IsWaterTile(neighbor_tiles.up_tile) == ParallaxTerrain.IsWaterTile(neighbor_tiles.up_left_tile)))) &&
                (!ParallaxTerrain.IsWaterTile(neighbor_tiles.up_tile) || neighbor_tiles.up_right_tile == null || (neighbor_tiles.up_right_tile != null && (ParallaxTerrain.IsWaterTile(neighbor_tiles.up_tile) == ParallaxTerrain.IsWaterTile(neighbor_tiles.up_right_tile)))) &&
                (!ParallaxTerrain.IsWaterTile(neighbor_tiles.up_tile) || !ParallaxTerrain.IsWaterTile(neighbor_tiles.look_ahead_tile) || neighbor_tiles.look_ahead_cw_tile == null || (neighbor_tiles.look_ahead_cw_tile != null && (ParallaxTerrain.IsWaterTile(neighbor_tiles.look_ahead_tile) == ParallaxTerrain.IsWaterTile(neighbor_tiles.look_ahead_cw_tile)))) &&
                (!ParallaxTerrain.IsWaterTile(neighbor_tiles.up_tile) || !ParallaxTerrain.IsWaterTile(neighbor_tiles.look_ahead_tile) || neighbor_tiles.look_ahead_ccw_tile == null || (neighbor_tiles.look_ahead_ccw_tile != null && (ParallaxTerrain.IsWaterTile(neighbor_tiles.look_ahead_tile) == ParallaxTerrain.IsWaterTile(neighbor_tiles.look_ahead_ccw_tile))))
                ))
            {
                target_pos += Vector3.up;
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
                (!ParallaxTerrain.IsWaterTile(neighbor_tiles.left_tile) || neighbor_tiles.up_left_tile == null || (neighbor_tiles.up_left_tile != null && (ParallaxTerrain.IsWaterTile(neighbor_tiles.left_tile) == ParallaxTerrain.IsWaterTile(neighbor_tiles.up_left_tile)))) &&
                (!ParallaxTerrain.IsWaterTile(neighbor_tiles.left_tile) || !ParallaxTerrain.IsWaterTile(neighbor_tiles.look_ahead_tile) || neighbor_tiles.look_ahead_cw_tile == null || (neighbor_tiles.look_ahead_cw_tile != null && (ParallaxTerrain.IsWaterTile(neighbor_tiles.look_ahead_tile) == ParallaxTerrain.IsWaterTile(neighbor_tiles.look_ahead_cw_tile))))
                )
            {
                target_pos += Vector3.left;
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
                (!ParallaxTerrain.IsWaterTile(neighbor_tiles.right_tile) || neighbor_tiles.up_right_tile == null || (neighbor_tiles.up_right_tile != null && (ParallaxTerrain.IsWaterTile(neighbor_tiles.right_tile) == ParallaxTerrain.IsWaterTile(neighbor_tiles.up_right_tile)))) &&
                (!ParallaxTerrain.IsWaterTile(neighbor_tiles.right_tile) || !ParallaxTerrain.IsWaterTile(neighbor_tiles.look_ahead_tile) || neighbor_tiles.look_ahead_ccw_tile == null || (neighbor_tiles.look_ahead_ccw_tile != null && (ParallaxTerrain.IsWaterTile(neighbor_tiles.look_ahead_tile) == ParallaxTerrain.IsWaterTile(neighbor_tiles.look_ahead_ccw_tile))))
                )
            {
                target_pos += Vector3.right;
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
                (!ParallaxTerrain.IsWaterTile(neighbor_tiles.down_tile) || neighbor_tiles.down_left_tile == null || (neighbor_tiles.down_left_tile != null && (ParallaxTerrain.IsWaterTile(neighbor_tiles.down_tile) == ParallaxTerrain.IsWaterTile(neighbor_tiles.down_left_tile)))) &&
                (!ParallaxTerrain.IsWaterTile(neighbor_tiles.down_tile) || neighbor_tiles.down_right_tile == null || (neighbor_tiles.down_right_tile != null && (ParallaxTerrain.IsWaterTile(neighbor_tiles.down_tile) == ParallaxTerrain.IsWaterTile(neighbor_tiles.down_right_tile))))))
            {
                target_pos += Vector3.down;
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
            target_pos += ((height * Vector3.back) + (height * Vector3.up));
            moving = true;
            jumping = true;
            jump_data = new JumpData (height, new Vector3(), 0);
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
                (num_tiles == 1 && check_tile != null && check_tile.allow_passage && check_tile.terrain_tag != TerrainTags.Ledge && !neighbor_tiles.facing_other_level) ||
                (num_tiles == 2 && neighbor_tiles.look_ahead_tile != null && neighbor_tiles.look_ahead_tile.allow_passage && neighbor_tiles.look_ahead_tile.terrain_tag != TerrainTags.Ledge && !neighbor_tiles.look_ahead_other_level)
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
                if (!ledge_dir_allowed) return false;

                target_pos += ((height * Vector3.back) + (height * Vector3.up) + (v * (float)num_tiles / 2));
                moving = true;
                jumping = true;
                jump_data = new JumpData (height, v, num_tiles);
                tile_activated = false;
                return true;
            }
            // Don't jump when trying to enter/leave water that is blocked
            if (ParallaxTerrain.IsWaterTile(neighbor_tiles.facing_tile)) return false;
            // Attempt shorter jump or hop in place if long jump blocked
            else return JumpForward(num_tiles - 1);
        }
        
        public void JumpBackward(int num_tiles) { }

        #endregion


        #region Movement Between Layers

        public void MoveLayerUp()
        {
            target_pos += (Constants.MAP_LAYER_HEIGHT * Vector3.back);
            moving = true;
            layer += 1;
            tile_activated = false;
        }
        public void MoveLayerDown()
        {
            target_pos += (Constants.MAP_LAYER_HEIGHT * Vector3.forward);
            moving = true;
            layer -= 1;
            tile_activated = false;
        }

        [BoxGroup("Debug Actions/Split/Right/Movement")]
        [Button("Rise Up")]
        public bool RiseUp()
        {
            if (neighbor_tiles.above_tile != null && neighbor_tiles.above_tile.allow_passage)
            {
                bool prev_move_through_walls = move_through_walls;
                bool prev_lock_direction = lock_direction;
                MoveThroughWallsOn();
                LockDirectionOn();
                layer_change = LayerChange.Up;
                MoveLayerUp();
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
                bool prev_move_through_walls = move_through_walls;
                bool prev_lock_direction = lock_direction;
                MoveThroughWallsOn();
                LockDirectionOn();
                layer_change = LayerChange.Down;
                MoveLayerDown();
                MoveDown();
                if (!prev_move_through_walls) MoveThroughWallsOff();
                if (!prev_lock_direction) LockDirectionOff();
                return true;
            }
            return false;
        }

        #endregion


        #region Set Flags

        public void InvisibleOn()
        {
            invisible = true;
        }
        public void InvisibleOff()
        {
            invisible = false;
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

        public void SteppingAnimationOn()
        {
            stepping_animation = true;
        }
        public void SteppingAnimationOff()
        {
            stepping_animation = false;
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
    }
}