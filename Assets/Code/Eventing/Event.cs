using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace Eventing
{
    #region Enums

    public enum EventTriggers
    {
        IsPlayer,
        ActionButton
        // TODO
    }

    #endregion

    [Serializable]
    public class Event : MonoBehaviour
    {
        #region Fields

        // private EventManager event_manager;

        [Title("Event Settings")]
        public EventTriggers event_type = EventTriggers.ActionButton;
        public bool passable = false;

        [Title("Event Status")]
        [ReadOnly]
        public bool event_playing;
        [ReadOnly]
        public bool effect_playing;

        #endregion


        #region Accessors

        public bool IsPlayer()
        {
            return event_type == EventTriggers.IsPlayer;
        }
        public bool IsPassable()
        {
            return passable;
        }

        #endregion


        #region Mono Behavior

        private void Start()
        {
            // event_manager = FindObjectOfType<EventManager>();

            event_playing = false;
            effect_playing = false;
        }

        #endregion


        #region Event Definitions
        #endregion
        
    }
}