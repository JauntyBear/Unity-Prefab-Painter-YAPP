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
        public enum LayerIndex
        {
            Nothing = 0,
            Everything = int.MaxValue,
            IgnoreRaycast = 2,
        }

        /// <summary>
        /// The layer index for the preview prefab
        /// </summary>
        public static LayerIndex PreviewLayerIndex = LayerIndex.IgnoreRaycast;

        /// <summary>
        /// Get layer mask without the preview layer index
        /// </summary>
        /// <param name="layerMask"></param>
        /// <returns></returns>
        public static LayerMask GetPreviewLayerMask( LayerMask layerMask)
        {
            return layerMask & (int) ~(1 << (int) PreviewLayerIndex);
        }
    }
}
