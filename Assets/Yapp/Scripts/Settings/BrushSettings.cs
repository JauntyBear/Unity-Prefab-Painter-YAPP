using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Yapp
{
    [System.Serializable]
    public class BrushSettings
    {
        public enum Distribution
        {
            Center,
            Poisson,
            FallOff,
            FallOff2d
        }

        public enum AutoSimulationType
        {
            None,
            Once,
            Continuous
        }

        public float brushSize = 2.0f;

        [Range(0, 360)]
        public int brushRotation = 0;

        public bool alignToTerrain = false;
        public Distribution distribution = Distribution.Center;

        /// <summary>
        /// The size of a disc in the poisson distribution.
        /// The smaller, the more discs will be inside the brush
        /// </summary>
        public float poissonDiscSize = 1.0f;

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
        /// Automatically apply physics after painting
        /// </summary>
        public AutoSimulationType autoSimulationType = AutoSimulationType.None;

        /// <summary>
        /// When auto physics is enabled, then this value will be added to the y-position of the gameobject.
        /// This way e. g. rocks are placed higher by default and gravity can be applied
        /// </summary>
        public float autoSimulationHeightOffset = 1f;

        /// <summary>
        /// The number of seconds that physics is applied automatically
        /// </summary>
        [Range(0,10000)]
        public int autoSimulationStepCountMax = 1000;

        /// <summary>
        /// The number of physics steps to perform in a single simulation step. lower = smoother, higher = faster
        /// </summary>
        [Range(1, 1000)]
        public int autoSimulationStepIterations = 1;

        /// <summary>
        /// Optionally spawn into the Persistent Storage of Vegetation Studio Pro
        /// </summary>
        public bool spawnToVSPro = false;

    }
}
