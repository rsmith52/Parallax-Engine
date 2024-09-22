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


    public class MoveableObject : MonoBehaviour
    {
        #region Fields

        [Title("Positition")]
        [EnumPaging]
        public MovementSpeeds movement_speed = MovementSpeeds.Moderate;
        [EnumPaging]
        public Directions direction = Directions.Down;
        [ReadOnly]
        public int layer = 0;

        private float speed;
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

        //private MapManager map_manager;
        [TabGroup ("Tiles")]
        [ReadOnly]
        public ParallaxTileBase on_tile;
        [TabGroup ("Tiles")]
        [ReadOnly]
        public ParallaxTileBase up_tile;
        [TabGroup ("Tiles")]
        [ReadOnly]
        public ParallaxTileBase left_tile;
        [TabGroup ("Tiles")]
        [ReadOnly]
        public ParallaxTileBase right_tile;
        [TabGroup ("Tiles")]
        [ReadOnly]
        public ParallaxTileBase down_tile;

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
            // map_manager = FindObjectOfType<MapManager>();
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

            foreach (SpriteRenderer sprite in sprites)
                if (sprite.tag == Constants.PRIORITY_TILE_TAG)
                    sprite.sortingOrder = (int)(layer * Constants.SORTING_LAYERS_PER_MAP_LAYER) + Constants.OBJECT_SORTING_LAYER_OFFSET + Constants.PRIORITY_TILE_OFFSET;
                else if (sprite.tag == Constants.DEPRIORITY_TILE_TAG)
                    sprite.sortingOrder = (int)(layer * Constants.SORTING_LAYERS_PER_MAP_LAYER) + Constants.OBJECT_SORTING_LAYER_OFFSET - Constants.PRIORITY_TILE_OFFSET;
                else
                    sprite.sortingOrder = (int)(layer * Constants.SORTING_LAYERS_PER_MAP_LAYER) + Constants.OBJECT_SORTING_LAYER_OFFSET;
            
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

            // Tilemap and Event awareness check
            if ((other_moved || moving) && transform.position == target_pos)
            {
                // Get neighboring tiles
                // ParallaxTileBase[] neighbor_tiles = map_manager.GetNeighborTiles(this);
                // on_tile = neighbor_tiles[0];
                // up_tile = neighbor_tiles[1];
                // left_tile = neighbor_tiles[2];
                // right_tile = neighbor_tiles[3];
                // down_tile = neighbor_tiles[4];

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
                    tile_activated = ActivateTile(on_tile);
                }     
            }
        }

        #endregion


        #region Map Interactions

        [Button("Activate Current Tile")]
        private void ManualActivateTile()
        {
            ActivateTile(on_tile);
        }

        private bool ActivateTile(ParallaxTileBase tile)
        {
            // TODO
            return true;
        }

        private bool ActivateTile(Vector3 move_dir)
        {
            // TODO
            return true;
        }

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

        public void MoveUp()
        {
            TurnUp();
            if (move_through_walls || (on_tile != null && up_tile != null &&
                (up_event == null || up_event.IsPassable()) && on_tile.up_passage &&
                up_tile.allow_passage && up_tile.down_passage))
            {
                target_pos += Vector3.up;
                moving = true;
                tile_activated = false;
            }
        }
        public void MoveLeft()
        {
            TurnLeft();
            if (move_through_walls || (on_tile != null && left_tile != null &&
                (left_event == null || left_event.IsPassable()) && on_tile.left_passage &&
                left_tile.allow_passage && left_tile.right_passage))
            {
                target_pos += Vector3.left;
                moving = true;
                tile_activated = false;
            }
        }
        public void MoveRight()
        {
            TurnRight();
            if (move_through_walls || on_tile.terrain_tag == TerrainTags.StairLeft ||
                (on_tile != null && right_tile != null &&
                (right_event == null || right_event.IsPassable()) && on_tile.right_passage &&
                right_tile.allow_passage && right_tile.left_passage))
            {
                target_pos += Vector3.right;
                moving = true;
                tile_activated = false;
            }
        }
        public void MoveDown()
        {
            TurnDown();
            if (move_through_walls || (on_tile != null && down_tile != null &&
                (down_event == null || down_event.IsPassable()) && on_tile.down_passage &&
                down_tile.allow_passage && down_tile.up_passage))
            {
                target_pos += Vector3.down;
                moving = true;
                tile_activated = false;
            }
        }

        public void MoveUpLeft()
        {
            TurnLeft();
            target_pos += Vector3.up + Vector3.left;
            moving = true;
            tile_activated = false;
        }
        public void MoveDownRight()
        {
            TurnRight();
            target_pos += Vector3.down + Vector3.right;
            moving = true;
            tile_activated = false;
        }

        public void StepForward()
        {
            switch (direction)
            {
                case Directions.Up:
                    MoveUp();
                    break;
                case Directions.Left:
                    MoveLeft();
                    break;
                case Directions.Right:
                    MoveRight();
                    break;
                case Directions.Down:
                    MoveDown();
                    break;
                default:
                    break;
            }
            moving = true;
        }
        public void StepBackward()
        {
            bool prev_fix_direction = fix_direction;
            switch (direction)
            {
                case Directions.Up:
                    FixDirectionOn();
                    MoveDown();
                    fix_direction = prev_fix_direction;
                    break;
                case Directions.Left:
                    FixDirectionOn();
                    MoveRight();
                    fix_direction = prev_fix_direction;
                    break;
                case Directions.Right:
                    FixDirectionOn();
                    MoveLeft();
                    fix_direction = prev_fix_direction;
                    break;
                case Directions.Down:
                    FixDirectionOn();
                    MoveUp();
                    fix_direction = prev_fix_direction;
                    break;
                default:
                    break;
            }
            moving = true;
        }

        public void MoveAtRandom()
        {
            System.Random random = Utilities.Random.GetRandom();
            Directions new_direction = (Directions)random.Next(4);
            switch (new_direction)
            {
                case Directions.Up:
                    MoveUp();
                    break;
                case Directions.Left:
                    MoveLeft();
                    break;
                case Directions.Right:
                    MoveRight();
                    break;
                case Directions.Down:
                    MoveDown();
                    break;
                default:
                    break;
            }
            moving = true;
        }

        public void JumpInPlace() { }
        public void JumpForward(int num_tiles) { }
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