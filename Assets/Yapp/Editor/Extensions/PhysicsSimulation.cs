using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Yapp
{
    public class PhysicsSimulation : ScriptableObject
    {
        private PhysicsSettings physicsSettings;

        private SimulatedBody[] simulatedBodies;

        private List<Rigidbody> generatedRigidbodies;
        private List<Collider> generatedColliders;

        private Transform[] simulatedGameObjects = null;

        public void ApplySettings(PhysicsSettings physicsSettings)
        {
            this.physicsSettings = physicsSettings;

        }

        #region Simulate Once
        public void RunSimulationOnce(Transform[] gameObjects)
        {

            this.simulatedGameObjects = gameObjects;

            PreProcessSimulation();

            simulatedBodies = simulatedGameObjects.Select(rb => new SimulatedBody(rb, physicsSettings.forceAngleInDegrees, physicsSettings.randomizeForceAngle)).ToArray();

            SimulateOnce(simulatedBodies);

            PostProcessSimulation();
        }

        private void SimulateOnce(SimulatedBody[] simulatedBodies)
        {
            // apply force if necessary
            if (physicsSettings.forceApplyType == PhysicsSettings.ForceApplyType.Initial)
            {
                ApplyForce();
            }

            // Run simulation for maxIteration frames, or until all child rigidbodies are sleeping
            Physics.autoSimulation = false;
            for (int i = 0; i < physicsSettings.maxIterations; i++)
            {
                // apply force if necessary
                if (physicsSettings.forceApplyType == PhysicsSettings.ForceApplyType.Continuous)
                {
                    ApplyForce();
                }

                Physics.Simulate(Time.fixedDeltaTime);

                if (simulatedBodies.All(body => body.rigidbody.IsSleeping()))
                {
                    break;
                }
            }
            Physics.autoSimulation = true;
        }
        #endregion Simulate Once

        private void ApplyForce()
        {
            // Add force to bodies
            foreach (SimulatedBody body in simulatedBodies)
            {
                float randomForceAmount = Random.Range(physicsSettings.forceMinMax.x, physicsSettings.forceMinMax.y);
                float forceAngle = body.forceAngle;
                Vector3 forceDir = new Vector3(Mathf.Sin(forceAngle), 0, Mathf.Cos(forceAngle));
                body.rigidbody.AddForce(forceDir * randomForceAmount, ForceMode.Impulse);
            }
        }

        #region Simulate Continuously
        private bool simulationStopTriggered = false;

        Dictionary<Collider, HideFlags> colliderFlagsMap = null;

        public void StartSimulation(Transform[] gameObjects)
        {
            if (physicsSettings.simulationRunning)
            {
                Debug.Log("Simulation already running");
                return;
            }
            
            Debug.Log("Simulation started");

            this.simulatedGameObjects = gameObjects;

            // store eg collider flags, generate colliders for gameobjects which don't have them
            PreProcessSimulation();

            simulatedBodies = gameObjects.Select(rb => new SimulatedBody(rb, physicsSettings.forceAngleInDegrees, physicsSettings.randomizeForceAngle)).ToArray();

            physicsSettings.simulationRunning = true;
            physicsSettings.simulationStepCount = 0;
            simulationStopTriggered = false;

            // Run simulation for maxIteration frames, or until all child rigidbodies are sleeping
            Physics.autoSimulation = false;

            // apply force if necessary
            if (physicsSettings.forceApplyType == PhysicsSettings.ForceApplyType.Initial)
            {
                ApplyForce();
            }

            EditorCoroutines.Execute(SimulateContinuously());

        }

        private void PreProcessSimulation()
        {
            // store the hide flags for the colliders
            colliderFlagsMap = new Dictionary<Collider, HideFlags>();

            foreach (Transform transform in simulatedGameObjects)
            {
                Collider collider = transform.GetComponent<Collider>();

                if (!collider)
                    continue;

                colliderFlagsMap.Add(collider, hideFlags);
            }

            // hide the colliders in the hierarchy
            foreach (Transform transform in simulatedGameObjects)
            {
                Collider collider = transform.GetComponent<Collider>();

                if (!collider)
                    continue;

                transform.GetComponent<Collider>().hideFlags = HideFlags.HideInHierarchy;
            }

            // generate eg colliders for gameobjects which don't have them
            AutoGenerateComponents();

        }

        private void PostProcessSimulation()
        {
            // remove auto generated rigidbodies and colliders
            RemoveAutoGeneratedComponents();

            // restore the hide flags
            foreach (Transform transform in simulatedGameObjects)
            {
                Collider collider = transform.GetComponent<Collider>();

                if (!collider)
                    continue;

                HideFlags hideFlags;
                if( colliderFlagsMap.TryGetValue(collider, out hideFlags))
                {
                    collider.hideFlags = hideFlags;
                }
            }

            // clear the hide flags map
            colliderFlagsMap = null;

        }


        public void StopSimulation()
        {
            simulationStopTriggered = true;

            Debug.Log("Simulation stopp triggered");

        }

        private void PerformSimulateStep(SimulatedBody[] simulatedBodies)
        {

            // apply force if necessary
            if (physicsSettings.forceApplyType == PhysicsSettings.ForceApplyType.Continuous)
            {
                ApplyForce();
            }

            Physics.Simulate(Time.fixedDeltaTime);

        }

        IEnumerator SimulateContinuously()
        {

            while (!simulationStopTriggered && physicsSettings.IsStepCountValid())
            {

                for (int i = 0; i < physicsSettings.simulationStepIterations; i++)
                {
                    physicsSettings.simulationStepCount++;

                    PerformSimulateStep(simulatedBodies);

                    // in batch process skip if the max count is already reached
                    if (!physicsSettings.IsStepCountValid())
                        break;
                }

                yield return 0;
            }

            Physics.autoSimulation = true;

            PostProcessSimulation();

            physicsSettings.simulationRunning = false;

            Debug.Log("Simulation stopped");


        }
        #endregion Simulate Continuously

        // Automatically add rigidbody and box collider to object if it doesn't already have
        void AutoGenerateComponents()
        {
            generatedRigidbodies = new List<Rigidbody>();
            generatedColliders = new List<Collider>();

            foreach (Transform child in simulatedGameObjects)
            {
                if (!child.GetComponent<Rigidbody>())
                {

                    Rigidbody rb = child.gameObject.AddComponent<Rigidbody>();

                    rb.useGravity = true;
                    rb.mass = 1;

                    generatedRigidbodies.Add(rb);

                }
                if (!child.GetComponent<Collider>())
                {
                    MeshCollider collider = child.gameObject.AddComponent<MeshCollider>();

                    // hide colliders in the hierarchy, they cost performance
                    collider.hideFlags = HideFlags.HideInHierarchy;

                    collider.convex = true;

                    generatedColliders.Add(collider);
                }
            }
        }

        // Remove the components which were generated at start of simulation
        void RemoveAutoGeneratedComponents()
        {
            foreach (Rigidbody rb in generatedRigidbodies)
            {
                DestroyImmediate(rb);
            }
            foreach (Collider c in generatedColliders)
            {
                DestroyImmediate(c);
            }
        }

        public void UndoSimulation()
        {
            if (simulatedBodies != null)
            {
                foreach (SimulatedBody body in simulatedBodies)
                {
                    body.Undo();
                }
            }
        }

        struct SimulatedBody
        {
            public readonly Rigidbody rigidbody;

            readonly Geometry geometry;
            readonly Transform transform;

            public readonly float forceAngle;

            public SimulatedBody(Transform transform, float forceAngleInDegrees, bool randomizeForceAngle)
            {
                this.transform = transform;
                this.rigidbody = transform.GetComponent<Rigidbody>();

                this.geometry = new Geometry(transform);

                this.forceAngle = ((randomizeForceAngle) ? Random.Range(0, 360f) : forceAngleInDegrees) * Mathf.Deg2Rad;
            }

            public void Undo()
            {
                // check if the transform was removed manually
                if (transform == null)
                    return;

                transform.position = geometry.getPosition();
                transform.rotation = geometry.getRotation();

                if (rigidbody != null)
                {
                    rigidbody.velocity = Vector3.zero;
                    rigidbody.angularVelocity = Vector3.zero;
                }
            }
        }
    }
}