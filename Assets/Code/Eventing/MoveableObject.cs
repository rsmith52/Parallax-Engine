using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Mapping;
using Utilities;

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
        SetInvisibleFlag,
        SetThroughFlag,
        SetFixDirectionFlag,
        SetWalkingFlag,
        SetSteppingFlag,
        Wait,
        ChangeSpeed
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

        private SpriteRenderer[] sprites;
        private SpriteMask bush_mask;

        [Title("Static Flags")]
        public bool invisible = false;
        public bool move_through_walls = false;
        public bool fix_direction = false;
        public bool walking_animation = true;
        public bool stepping_animation = false;
        public bool always_on_top = false;

        [Title("Awareness")]
        [ReadOnly]
        public bool in_bush;
        [ReadOnly]
        public bool on_water;

        [TabGroup ("Movement")]
        [ReadOnly]
        public bool moving;
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
            moving = true;
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
            
            speed = Constants.SPEEDS[(int)movement_speed];

            // Apply Movement
            if (moving)
            {
                // Activate the tile being moved onto
                if (!tile_activated)
                {
                    Vector3 move_dir = target_pos - transform.position;
                    tile_activated = ActivateTile(move_dir);
                }

                // Move in that direction
                transform.position = Vector3.MoveTowards(transform.position, target_pos, Time.deltaTime * speed);
            }

            // Update Sprites & Sorting Order
            foreach (SpriteRenderer sprite in sprites)
                if (sprite.tag == Constants.PRIORITY_TILE_TAG)
                    sprite.sortingOrder = (int)(layer * Constants.SORTING_LAYERS_PER_MAP_LAYER) + Constants.EVENT_SORTING_LAYER_OFFSET + Constants.PRIORITY_TILE_OFFSET;
                else if (sprite.tag == Constants.DEPRIORITY_TILE_TAG)
                    sprite.sortingOrder = (int)(layer * Constants.SORTING_LAYERS_PER_MAP_LAYER) + Constants.EVENT_SORTING_LAYER_OFFSET - Constants.PRIORITY_TILE_OFFSET;
                else
                    sprite.sortingOrder = (int)(layer * Constants.SORTING_LAYERS_PER_MAP_LAYER) + Constants.EVENT_SORTING_LAYER_OFFSET;
            
            if (invisible)
                    foreach (SpriteRenderer sprite in sprites)
                        sprite.enabled = false;
            else
                foreach (SpriteRenderer sprite in sprites)
                    sprite.enabled = true;
                     
            if (in_bush)
                bush_mask.enabled = true;
            else
                bush_mask.enabled = false;

            // Tilemap and Event awareness check
            if ((other_moved || moving) && transform.position == target_pos)
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
                other_moved = false;
                if (!tile_activated)
                {
                    tile_activated = ActivateTile(neighbor_tiles.on_tile);
                }     
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
            if (verbose) Debug.Log("Activating tile: " + neighbor_tiles.on_tile);

            // No tile, must be moving through walls
            if (tile == null)
                return true;

            // Bush Flag
            if (tile.is_bush)
                in_bush = true; // TODO - bush animation
            else
                in_bush = false;

            // On Water Flag
            if (ParallaxTerrain.IsWaterTile(tile))
                on_water = true;
            else
                on_water = false;

            return true;
        }

        private bool ActivateTile(Vector3 move_dir)
        {
            if (Mathf.Abs(move_dir.x) > 0)
            {
                // Move Right
                if (move_dir.x > 0)
                {
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
                    return ActivateTile(neighbor_tiles.right_tile);
                }
                // Move Left
                else if (move_dir.x < 0)
                {
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
                    return ActivateTile(neighbor_tiles.left_tile);
                }
            }
            else if (Mathf.Abs(move_dir.y) > 0)
            {
                // Move Up
                if (move_dir.y > 0)
                {
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
                    return ActivateTile(neighbor_tiles.up_tile);
                }
                // Move Down
                else if (move_dir.y < 0)
                {
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
            if (!fix_direction)
                direction = Directions.Up;
        }
        public void TurnLeft()
        {
            if (!fix_direction)
                direction = Directions.Left;
        }
        public void TurnRight()
        {
            if (!fix_direction)
                direction = Directions.Right;
        }
        public void TurnDown()
        {
            if (!fix_direction)
                direction = Directions.Down;
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
            if (!fix_direction)
                direction = new_direction;
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
            if (move_through_walls || (neighbor_tiles.on_tile != null && neighbor_tiles.up_tile != null &&
                (up_event == null || up_event.IsPassable()) && neighbor_tiles.on_tile.up_passage &&
                neighbor_tiles.up_tile.allow_passage && neighbor_tiles.up_tile.down_passage))
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
                neighbor_tiles.left_tile.allow_passage && neighbor_tiles.left_tile.right_passage))
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
                neighbor_tiles.right_tile.allow_passage && neighbor_tiles.right_tile.left_passage))
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
                neighbor_tiles.down_tile.allow_passage && neighbor_tiles.down_tile.up_passage))
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
            bool prev_fix_direction = fix_direction;
            bool success = false;
            switch (direction)
            {
                case Directions.Up:
                    FixDirectionOn();
                    success = MoveDown();
                    fix_direction = prev_fix_direction;
                    break;
                case Directions.Left:
                    FixDirectionOn();
                    success = MoveRight();
                    fix_direction = prev_fix_direction;
                    break;
                case Directions.Right:
                    FixDirectionOn();
                    success = MoveLeft();
                    fix_direction = prev_fix_direction;
                    break;
                case Directions.Down:
                    FixDirectionOn();
                    success = MoveUp();
                    fix_direction = prev_fix_direction;
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
        public void JumpInPlace() { }
        [BoxGroup("Debug Actions/Split/Right/Movement")]
        [Button("Jump Forward")]
        public void JumpForward(int num_tiles) { }
        [BoxGroup("Debug Actions/Split/Right/Movement")]
        [Button("Jump Backward")]
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

        public void FixDirectionOn()
        {
            fix_direction = true;
        }
        public void FixDirectionOff()
        {
            fix_direction = false;
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