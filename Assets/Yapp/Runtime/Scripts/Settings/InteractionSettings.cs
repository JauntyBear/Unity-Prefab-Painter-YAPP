using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rowlan.Yapp
{
    [System.Serializable]
    public class InteractionSettings
    {

        public enum InteractionType
        {
            AntiGravity,
            Magnet,
            ChangeScale,
            SetScale
        }

        #region Public Editor Fields

        public InteractionType interactionType;

        /// <summary>
        /// Anti Gravity strength from 0..100
        /// </summary>
        [Range(0, 100)]
        public int antiGravityStrength = 30;

        /// <summary>
        /// Some arbitrary magnet strength from 0..100
        /// </summary>
        [Range(0,100)]
        public int magnetStrength = 10;

        /// <summary>
        /// Some arbitrary strength from 0..100
        /// </summary>
        [Range(0,100)]
        public float changeScaleStrength = 10;

        /// <summary>
        /// Some arbitrary strength from 0..10
        /// </summary>
        [Range(0, 10)]
        public float setScaleValue = 1;

        #endregion Public Editor Fields



    }
}
