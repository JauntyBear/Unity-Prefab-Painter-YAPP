using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rowlan.Yapp
{
    [System.Serializable]
    public class BrushSettings
    {
        public enum Distribution
        {
            [InspectorName("Fluent (with Preview)")]
            Fluent,

            [InspectorName("Center")]
            Center,

            [InspectorName("Poisson (Any Collider)")]
            Poisson_Any,

            [InspectorName("Poisson (Terrain Only)")]
            Poisson_Terrain,

            FallOff,
            FallOff2d
        }

        public float brushSize = 2.0f;

        /// <summary>
        /// Brush rotation for placement
        /// In single placement mode this acts as global prefab angle (but overwritten eg by random rotation). Set to 0 if you don't want it
        /// </summary>
        [Range(0, 360)]
        public int brushRotation = 0;

        /// <summary>
        /// Brush size
        /// </summary>
        public bool sizeGuide = true;

        /// <summary>
        /// Brush normal
        /// </summary>
        public bool normalGuide = true;

        /// <summary>
        /// Brush rotation
        /// </summary>
        public bool rotationGuide = false;

        public bool alignToTerrain = false;

        public Distribution distribution = Distribution.Fluent;

        /// <summary>
        /// The size of a disc in the poisson distribution.
        /// The smaller, the more discs will be inside the brush
        /// </summary>
        public float poissonDiscSize = 1.0f;

        /// <summary>
        /// If any collider (not only terrain) is used for the raycast, then this will used as offset from which the ray will be cast against the collider
        /// </summary>
        public float poissonDiscRaycastOffset = 100f;

        /// <summary>
        /// Falloff curve
        /// </summary>
        public AnimationCurve fallOffCurve = AnimationCurve.Linear(1, 1, 1, 1);

        public AnimationCurve fallOff2dCurveX = AnimationCurve.Linear(1, 1, 1, 1);
        public AnimationCurve fallOff2dCurveZ = AnimationCurve.Linear(1, 1, 1, 1);

        [Range(1,50)]
        public int curveSamplePoints = 10;

        // slope
        public float slopeMin = 0;
        public float slopeMinLimit = 0;
        public float slopeMax = 90;
        public float slopeMaxLimit = 90;

        /// <summary>
        /// Allow prefab overlaps or not.
        /// </summary>
        public bool allowOverlap = false;

        /// <summary>
        /// Layer mask for the brush raycast
        /// </summary>
        public LayerMask layerMask = (int) LayerUtils.LayerIndex.Everything;

        /// <summary>
        /// Optionally spawn into the Persistent Storage of Vegetation Studio Pro
        /// </summary>
        public bool spawnToVSPro = false;

    }
}
