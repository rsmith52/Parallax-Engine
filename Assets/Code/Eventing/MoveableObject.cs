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
        None,
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

        private float speed;
        private Vector3 pos;
        private SpriteRenderer[] sprites;
        private SpriteMask bush_mask;

        [Title("Flags")]
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
        public bool moved;
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
            return pos;
        }

        #endregion


        #region Mono Behavior

        private void Start()
        {
            // Basic Setup
            pos = transform.position;
            speed = Constants.SPEEDS[(int)movement_speed];
            //animator = GetComponentInChildren<Animator>();
            sprites = GetComponentsInChildren<SpriteRenderer>();
            bush_mask = GetComponentInChildren<SpriteMask>();
            moved = true;
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
            //     animator.SetBool(Constants.WALK_ANIMATION, moved);
            // else if (stepping_animation)
            //     animator.SetBool(Constants.WALK_ANIMATION, true);
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
            if (moved)
            {
                // Activate the tile being moved onto
                if (!tile_activated)
                {
                    Vector3 move_dir = pos - transform.position;
                    tile_activated = ActivateTile(move_dir);
                }

                // Move in that direction
                transform.position = Vector3.MoveTowards(transform.position, pos, Time.deltaTime * speed);
            }

            // Tilemap and Event awareness check
            if ((other_moved || moved) && transform.position == pos)
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

                moved = false;
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
            // No tile, must be moving through air/walls
            if (tile == null)
                return true;

            // Bush Flag
            if (tile.is_bush)
            {
                in_bush = true;
                // StartCoroutine(map_manager.GrassRustle(pos));
            }
            else
                in_bush = false;

            return true;
        }
        private bool ActivateTile(Vector3 move_dir)
        {
            // TODO
            return false;
        }

        #endregion


        #region Move Routing
        #endregion


        #region Direction Setting
        #endregion


        #region Movement Execution
        #endregion


        #region Movement Between Layers
        #endregion


        #region Set Flags
        #endregion


        #region Misc. Effects

        public IEnumerator Wait(float seconds)
        {
            yield return new WaitForSeconds(seconds);
        }

        public void ChangeSpeed(MovementSpeeds new_speed)
        {
            movement_speed = new_speed;
            speed = Constants.SPEEDS[(int)movement_speed];
        }

        #endregion
    }
}