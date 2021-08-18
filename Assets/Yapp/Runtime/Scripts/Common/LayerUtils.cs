using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rowlan.Yapp
{
    /// <summary>
    /// Common utils for layer handling
    /// </summary>
    public class LayerUtils
    {
        /// <summary>
        /// Specific layer values
        /// </summary>
        public enum LayerValue
        {
            Nothing = 0,
            Everything = int.MaxValue,
            IgnoreRaycast = 2,
        }
    }
}
