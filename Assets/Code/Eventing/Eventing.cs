using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Eventing
{
    #region Enums

    public enum EventTriggers
    {
        IsPlayer,
        ActionButton,
        Touch,
        Autorun,
        Background
        // TODO
    }

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
        Up = 0,
        Left = 1,
        Right = 2,
        Down = 3
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
        SetOutline,
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
        public Directions dir;
        public bool source_reflective;

        public JumpData(float height, Vector3 direction, int num_tiles, Directions dir, bool source_reflective)
        {
            this.height = height;
            this.direction = direction;
            this.num_tiles = num_tiles;
            this.dir = dir;
            this.source_reflective = source_reflective;
        }
    }

    #endregion
}
