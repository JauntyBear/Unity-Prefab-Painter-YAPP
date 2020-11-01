using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Yapp
{
    [System.Serializable]
    public class InteractionSettings
    {

        #region Public Editor Fields

        /// <summary>
        /// Some arbitrary magnet strength from 0..100
        /// </summary>
        [Range(0,100)]
        public int magnetStrength = 10;

        #endregion Public Editor Fields



    }
}
