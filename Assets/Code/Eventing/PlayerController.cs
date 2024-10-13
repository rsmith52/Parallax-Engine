using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using Utilities;

namespace Eventing
{
    public class PlayerController : SerializedMonoBehaviour
    {
        #region Fields

        private MoveableObject player_mover;

        private Vector3 current_pos;
        private Vector3 target_pos;
        private Directions direction;

        private float key_held_time;
        private bool jump_queued;
        private Directions jump_direction;

        #endregion


        #region Mono Behavior

        private void Awake()
        {
            // Set Player game object to not destroy on load
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            player_mover = GetComponent<MoveableObject>();
            jump_queued = false;
        }

        private void OnDisable()
        {
            StopRunning();
            StopSneaking();
        }

        private void OnEnable()
        {
            if (Input.GetKey(Controls.RUN_BUTTON))
                StartRunning();
            else if (Input.GetKey(Controls.SNEAK_BUTTON))
                StartSneaking();
            jump_queued = false;
        }

        private void Update()
        {   
            // Get Positioning
            current_pos = player_mover.GetCurrentPos();
            target_pos = player_mover.GetTargetPos();
            direction = player_mover.direction;

            // Check Key Held Time - allows tapping to turn
            if (Input.GetKeyDown(Controls.MOVE_UP) || Input.GetKeyDown(Controls.MOVE_LEFT) ||
                Input.GetKeyDown(Controls.MOVE_RIGHT) || Input.GetKeyDown(Controls.MOVE_DOWN))
                key_held_time = Time.time;

            // Handle Pause Menu Input
            if (Input.GetKeyDown(Controls.OPEN_MENU)) { }

            // Handle Action Input
            if (Input.GetKeyDown(Controls.ACTION_BUTTON))
                player_mover.ActivateEvent();

            // Handle Running/Sneaking Input
            if (Input.GetKeyDown(Controls.RUN_BUTTON))
                StartRunning();
            else if (Input.GetKeyDown(Controls.SNEAK_BUTTON))
                StartSneaking();
            else if (Input.GetKeyUp(Controls.RUN_BUTTON))
                StopRunning();
            else if (Input.GetKeyUp(Controls.SNEAK_BUTTON))
                StopSneaking();

            // Handle Jump/Dive Input
            if (Input.GetKeyDown(Controls.JUMP))
            {
                if (current_pos == target_pos)
                {
                    if (Settings.ALLOW_DIVING && (player_mover.on_water || player_mover.underwater))
                    {
                        bool sink_success = false;
                        if (player_mover.on_water)
                        {
                            sink_success = player_mover.SinkDown();
                        }
                        if (player_mover.underwater && !sink_success)
                        {
                            player_mover.RiseUp();
                        }
                    }
                    else if (Settings.ALLOW_JUMPING && !player_mover.on_water)
                    {
                        jump_queued = false;
                        bool jump_success = false;
                        if (Input.GetKey(Controls.MOVE_UP) || Input.GetKey(Controls.MOVE_LEFT) ||
                            Input.GetKey(Controls.MOVE_RIGHT) || Input.GetKey(Controls.MOVE_DOWN))
                        {
                            if (Input.GetKey(Controls.MOVE_UP)) player_mover.TurnUp();
                            if (Input.GetKey(Controls.MOVE_LEFT)) player_mover.TurnLeft();
                            if (Input.GetKey(Controls.MOVE_RIGHT)) player_mover.TurnRight();
                            if (Input.GetKey(Controls.MOVE_DOWN)) player_mover.TurnDown();

                            if (player_mover.movement_speed == MovementSpeeds.Moderate)
                                jump_success = player_mover.JumpForward(1);
                            else if (player_mover.movement_speed == MovementSpeeds.Fast || player_mover.movement_speed == MovementSpeeds.VeryFast)
                                jump_success = player_mover.JumpForward(2);
                        }
                        if (!jump_success)
                            player_mover.JumpInPlace();
                    }
                }
                else if (Settings.ALLOW_JUMPING && !player_mover.on_water && !player_mover.underwater && !player_mover.jumping && !player_mover.falling)
                {
                    jump_queued = true;
                    jump_direction = direction;
                }
            }
            if (jump_queued && (Time.time - key_held_time < Constants.TAP_VS_HOLD_TIME))
                jump_queued = false;

            // Handle Movement Input
            if (current_pos == target_pos)
            {
                if (Input.GetKey(Controls.MOVE_UP))
                {
                    if (!player_mover.lock_direction && direction != Directions.Up)
                    {
                        player_mover.TurnUp();
                    }
                    else if (Time.time - key_held_time > Constants.TAP_VS_HOLD_TIME)
                    {
                        bool jump_success = false;
                        if (jump_queued && jump_direction == Directions.Up)
                        {
                            if (player_mover.movement_speed == MovementSpeeds.Moderate)
                                jump_success = player_mover.JumpForward(1);
                                
                            else if (player_mover.movement_speed == MovementSpeeds.Fast || player_mover.movement_speed == MovementSpeeds.VeryFast)
                                jump_success = player_mover.JumpForward(2);
                            
                            jump_queued = false;
                        }
                        else if (jump_queued)
                            jump_queued = false;
                        if (!jump_success)
                        {
                            player_mover.MoveUp();
                        }
                    }
                }
                else if (Input.GetKey(Controls.MOVE_LEFT))
                {
                    if (!player_mover.lock_direction && direction != Directions.Left)
                    {
                        player_mover.TurnLeft();
                    }
                    else if (Time.time - key_held_time > Constants.TAP_VS_HOLD_TIME)
                    {
                        bool jump_success = false;
                        if (jump_queued && jump_direction == Directions.Left)
                        {
                            if (player_mover.movement_speed == MovementSpeeds.Moderate)
                                jump_success = player_mover.JumpForward(1);
                                
                            else if (player_mover.movement_speed == MovementSpeeds.Fast || player_mover.movement_speed == MovementSpeeds.VeryFast)
                                jump_success = player_mover.JumpForward(2);
                            
                            jump_queued = false;
                        }
                        else if (jump_queued)
                            jump_queued = false;
                        if (!jump_success)
                        {
                            player_mover.MoveLeft();
                        }
                    }
                }
                else if (Input.GetKey(Controls.MOVE_RIGHT))
                {
                    if (!player_mover.lock_direction && direction != Directions.Right)
                    {
                        player_mover.TurnRight();
                    }
                    else if (Time.time - key_held_time > Constants.TAP_VS_HOLD_TIME)
                    {
                        bool jump_success = false;
                        if (jump_queued && jump_direction == Directions.Right)
                        {
                            if (player_mover.movement_speed == MovementSpeeds.Moderate)
                                jump_success = player_mover.JumpForward(1);
                                
                            else if (player_mover.movement_speed == MovementSpeeds.Fast || player_mover.movement_speed == MovementSpeeds.VeryFast)
                                jump_success = player_mover.JumpForward(2);
                            
                            jump_queued = false;
                        }
                        else if (jump_queued)
                            jump_queued = false;
                        if (!jump_success)
                        {
                            player_mover.MoveRight();
                        }
                    }
                }
                else if (Input.GetKey(Controls.MOVE_DOWN))
                {
                    if (!player_mover.lock_direction && direction != Directions.Down)
                    {
                        player_mover.TurnDown();
                    }
                    else if (Time.time - key_held_time > Constants.TAP_VS_HOLD_TIME)
                    {
                        bool jump_success = false;
                        if (jump_queued && jump_direction == Directions.Down)
                        {
                            if (player_mover.movement_speed == MovementSpeeds.Moderate)
                                jump_success = player_mover.JumpForward(1);
                                
                            else if (player_mover.movement_speed == MovementSpeeds.Fast || player_mover.movement_speed == MovementSpeeds.VeryFast)
                                jump_success = player_mover.JumpForward(2);
                            
                            jump_queued = false;
                        }
                        else if (jump_queued)
                            jump_queued = false;
                        if (!jump_success)
                        {
                            player_mover.MoveDown();
                        }
                    }
                }
            }

            // Handle Debug Inputs
            if (Application.isEditor)
            {
                // Move through walls
                if (Input.GetKeyDown(Controls.MOVE_THROUGH_WALLS))
                {
                    if (!player_mover.move_through_walls)
                        player_mover.MoveThroughWallsOn();
                    else
                        player_mover.MoveThroughWallsOff();
                }

                // Shift layers
                if (Input.GetKeyDown(Controls.SHIFT_LAYER_UP) && current_pos == target_pos)
                {
                    player_mover.RiseUp();
                }
                else if (Input.GetKeyDown(Controls.SHIFT_LAYER_DOWN) && current_pos == target_pos)
                {
                    player_mover.SinkDown();
                }
            }
        }

        private void StartRunning(bool recursive = false)
        {
            if (!recursive) StopSneaking();
            player_mover.ChangeSpeed(MovementSpeeds.VeryFast);
            player_mover.animator.SetBool(Constants.ANIM_RUN, true);
        }

        private void StopRunning()
        {
            player_mover.animator.SetBool(Constants.ANIM_RUN, false);

            if (Input.GetKey(Controls.SNEAK_BUTTON))
                StartSneaking(true);
            else
                player_mover.ChangeSpeed(MovementSpeeds.Moderate);
            
        }

        private void StartSneaking(bool recursive = false)
        {
            if (!recursive) StopRunning();
            player_mover.ChangeSpeed(MovementSpeeds.VerySlow);
            player_mover.animator.SetBool(Constants.ANIM_SNEAK, true);
        }

        private void StopSneaking()
        {
            player_mover.animator.SetBool(Constants.ANIM_SNEAK, false);

            if (Input.GetKey(Controls.RUN_BUTTON))
                StartRunning(true);
            else
                player_mover.ChangeSpeed(MovementSpeeds.Moderate);
        }

        #endregion
    }
}
