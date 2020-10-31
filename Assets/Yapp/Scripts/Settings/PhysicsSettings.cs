using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Yapp
{
    [System.Serializable]
    public class PhysicsSettings
    {
        public enum ForceApplyType
        {
            /// <summary>
            /// Apply the force at the start of a simulation.
            /// Like an explosion when you have a lot of prefabs at the same location.
            /// </summary>
            Initial,

            /// <summary>
            /// Apply the force continuously during the simulation.
            /// Like wind blowing the prefabs away
            /// </summary>
            Continuous
        }

        #region Public Editor Fields

        public ForceApplyType forceApplyType = ForceApplyType.Initial;
        public int maxIterations = 1000;
        public Vector2 forceMinMax = Vector2.zero;
        public float forceAngleInDegrees = 0f;
        public bool randomizeForceAngle = false;

        #endregion Public Editor Fields

        #region Simulate Continuously
        public bool simulationRunning = false;
        public int simulationStepCount = 0;

        [Range(1,1000)]
        public int simulationStepIterations = 1;

        [Range(1,10000)]
        public int simulationStepCountMax = 1000;
        #endregion Simulate Continuously

        public bool IsStepCountValid()
        {
            return simulationStepCount <= simulationStepCountMax;
        }

    }
}
