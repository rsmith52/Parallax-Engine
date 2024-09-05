using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utilities;

namespace Eventing
{
    public class PlayerController : MonoBehaviour
    {
        #region Fields

        private MoveableObject player_mover;

        private Vector3 current_pos;
        private Vector3 target_pos;
        private Directions direction;

        private float key_held_time;

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
        }

        private void OnDisable()
        {
            StopRunning();
        }

        private void OnEnable()
        {
            if (Input.GetKey(KeyCode.X))
                StartRunning();
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

            // Handle Running Input
            if (Input.GetKeyDown(Controls.RUN_BUTTON))
                StartRunning();
            else if (Input.GetKeyUp(Controls.RUN_BUTTON))
                StopRunning();

            // Handle Movement Input
            if (Input.GetKey(Controls.MOVE_UP) && current_pos == target_pos)
            {
                if (!player_mover.fix_direction && direction != Directions.Up)
                {
                    player_mover.TurnUp();
                }
                else if (Time.time - key_held_time > Constants.TAP_VS_HOLD_TIME)
                {
                    player_mover.MoveUp();
                }
            }
            else if (Input.GetKey(Controls.MOVE_LEFT) && current_pos == target_pos)
            {
                if (!player_mover.fix_direction && direction != Directions.Left)
                {
                    player_mover.TurnLeft();
                }
                else if (Time.time - key_held_time > Constants.TAP_VS_HOLD_TIME)
                {
                    player_mover.MoveLeft();
                }
            }
            else if (Input.GetKey(Controls.MOVE_RIGHT) && current_pos == target_pos)
            {
                if (!player_mover.fix_direction && direction != Directions.Right)
                {
                    player_mover.TurnRight();
                }
                else if (Time.time - key_held_time > Constants.TAP_VS_HOLD_TIME)
                {
                    player_mover.MoveRight();
                }
            }
            else if (Input.GetKey(Controls.MOVE_DOWN) && current_pos == target_pos)
            {
                if (!player_mover.fix_direction && direction != Directions.Down)
                {
                    player_mover.TurnDown();
                }
                else if (Time.time - key_held_time > Constants.TAP_VS_HOLD_TIME)
                {
                    player_mover.MoveDown();
                }
            }

            // Handle Debug Inputs
            // if (Input.GetKeyDown(KeyCode.Space))
            // {
            //     if (player_mover.move_through_walls)
            //         player_mover.MoveThroughWallsOn();
            //     else
            //         player_mover.MoveThroughWallsOff();
            // }
            // if (Input.GetKeyDown(KeyCode.W))
            //     player_mover.MoveLayerUp();
            // else if (Input.GetKeyDown(KeyCode.S))
            //     player_mover.MoveLayerDown();

        }

        private void StartRunning()
        {
            player_mover.ChangeSpeed(MovementSpeeds.VeryFast);
            // player_mover.animator.SetBool(Constants.RUN_ANIMATION, true);
        }

        private void StopRunning()
        {
            player_mover.ChangeSpeed(MovementSpeeds.Moderate);
            // player_mover.animator.SetBool(Constants.RUN_ANIMATION, false);
        }

        #endregion
    }
}
