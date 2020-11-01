using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Yapp
{
    public class AutoPhysicsSimulation
    {
        public static void ApplyPhysics( GameObject container, BrushSettings.AutoSimulationType autoSimulationType, int autoSimulationStepCountMax, int autoSimulationStepIterations)
        {
            PhysicsSimulation physicsSimulation = ScriptableObject.CreateInstance<PhysicsSimulation>();

            PhysicsSettings physicsSettings = new PhysicsSettings();
            physicsSettings.simulationStepCountMax = autoSimulationStepCountMax;
            physicsSettings.simulationStepIterations = autoSimulationStepIterations;

            physicsSimulation.ApplySettings(physicsSettings);

            // TODO: use only the new added ones?
            Transform[] containerChildren = PrefabUtils.GetContainerChildren( container);

            if( autoSimulationType == BrushSettings.AutoSimulationType.Once)
            {
                physicsSimulation.RunSimulationOnce(containerChildren);
            }
            else if ( autoSimulationType == BrushSettings.AutoSimulationType.Continuous)
            {
                physicsSimulation.StartSimulation(containerChildren);
            }

        }
    }
}
