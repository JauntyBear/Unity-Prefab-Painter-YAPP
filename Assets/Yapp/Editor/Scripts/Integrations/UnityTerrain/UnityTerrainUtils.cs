using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Rowlan.Yapp
{
    public class UnityTerrainUtils
    {

        /// <summary>
        /// Internal variable which switches between linq (parallel) usage and serial usage.
        /// Will be removed once parallel linq proves it's way superior.
        /// </summary>
        private static bool useLinq = true;

        /// <summary>
        /// Unfiltered
        /// </summary>
        private const int PROTOTYPE_DEFAULT_FILTER_INDEX = -1;


        /// <summary>
        /// Get the prototype index for the prefab from the terrain data
        /// </summary>
        /// <param name="prefab"></param>
        /// <returns>The prototype index or -1 if the no matching prototype found</returns>
        public static int GetTreePrototypeIndex(TerrainData terrainData, GameObject prefab)
        {
            TreePrototype[] trees = terrainData.treePrototypes;

            for (int i = 0; i < trees.Length; i++)
            {
                TreePrototype prototype = trees[i];

                if (prototype.prefab == prefab)
                    return i;
            }

            return -1;
        }

        public static void PlaceTree(Terrain terrain, int treePrototype, Vector3 position, Color color, float height, float width, float rotation)
        {
            TreeInstance instance = new TreeInstance();

            instance.position = position;
            instance.color = color;
            instance.lightmapColor = Color.white;
            instance.prototypeIndex = treePrototype;
            instance.heightScale = height;
            instance.widthScale = width;
            instance.rotation = rotation;

            terrain.AddTreeInstance(instance);
        }

        /// <summary>
        /// Remove all trees from the terrain
        /// </summary>
        /// <param name="terrainData"></param>
        public static void RemoveAllTreeInstances(TerrainData terrainData)
        {
            Undo.RegisterCompleteObjectUndo(terrainData, "Remove all trees");

            terrainData.treeInstances = new TreeInstance[0];
        }

        /// <summary>
        /// Get tree color using with variation in color
        /// </summary>
        /// <param name="treeColorAdjustment"></param>
        /// <returns></returns>
        public static Color GetTreeColor( float treeColorAdjustment)
        {
            Color color = Color.white * UnityEngine.Random.Range(1.0F, 1.0F - treeColorAdjustment);
            color.a = 1;

            return color;
        }

        public static bool IsOverlapping(TerrainData terrainData, Vector3 position, int prototypeIndexFilter, float minDistanceWorld)
        {
            if (useLinq)
            {
                return IsOverlappingFast(terrainData, position, prototypeIndexFilter, minDistanceWorld);
            }
            else
            {
                return IsOverlappingSlow(terrainData, position, prototypeIndexFilter, minDistanceWorld);
            }
        }

        public static void RemoveOverlapping(Terrain terrain, Vector3 position, float brushSize)
        {
            RemoveOverlapping(terrain, position, PROTOTYPE_DEFAULT_FILTER_INDEX, brushSize);
        }

        public static void RemoveOverlapping(Terrain terrain, Vector3 position, int prototypeIndexFilter, float brushSize)
        {
            Undo.RegisterCompleteObjectUndo(terrain.terrainData, "Remove trees");

            if (useLinq)
            {
                RemoveOverlappingFast(terrain, position, prototypeIndexFilter, brushSize);
            }
            else
            {
                RemoveOverlappingSlow(terrain, position, prototypeIndexFilter, brushSize);
            }
        }

        #region Internal remove methods

        // parallel linq version
        private static void RemoveOverlappingFast(Terrain terrain, Vector3 position, int prototypeIndexFilter, float brushSize)
        {
            TerrainData terrainData = terrain.terrainData;

            // get radius in world space
            float localBrushRadius = brushSize * 0.5f;

            // get radius in terrain local space
            localBrushRadius = localBrushRadius / terrainData.size.x;

            // local position
            Vector3 localPosition = GetLocalPosition(terrain, position);

            // set a new tree instance array without the elements within the brush
            terrainData.treeInstances = terrainData.treeInstances.AsParallel().Where(x => (prototypeIndexFilter == PROTOTYPE_DEFAULT_FILTER_INDEX || prototypeIndexFilter == x.prototypeIndex) && Vector3.Distance(localPosition, x.position) > localBrushRadius).ToArray();

        }

        /// <summary>
        /// Get the position on the terrain in local terrain coordinates and considering the transform position
        /// </summary>
        /// <param name="terrain"></param>
        /// <param name="position"></param>
        /// <returns></returns>
        private static Vector3 GetLocalPosition( Terrain terrain, Vector3 position)
        {
            Vector3 localPosition = new Vector3(
                (position.x - terrain.transform.position.x) / terrain.terrainData.size.x,
                0f,
                (position.z - terrain.transform.position.z) / terrain.terrainData.size.z
                )
            ;

            return localPosition;
        }

        private static void RemoveOverlappingSlow(Terrain terrain, Vector3 position, int prototypeIndexFilter, float brushSize)
        {
            TerrainData terrainData = terrain.terrainData;

            // get all instances within the brush
            TreeInstance[] array = GetOverlappingInstancesSlow(terrain, position, prototypeIndexFilter, brushSize).ToArray();

            // set a new tree instance array without the elements within the brush
            terrainData.treeInstances = terrainData.treeInstances.Except(array).ToArray();
        }

        private static List<TreeInstance> GetOverlappingInstancesSlow(Terrain terrain, Vector3 position, int prototypeIndexFilter, float brushSize)
        {
            TerrainData terrainData = terrain.terrainData;

            // get radius in world space
            float localBrushRadius = brushSize * 0.5f;

            // get radius in terrain local space
            localBrushRadius = localBrushRadius / terrainData.size.x;

            // local position
            Vector3 localPosition = GetLocalPosition(terrain, position);

            List<TreeInstance> list = new List<TreeInstance>();

            foreach (TreeInstance treeInstance in terrainData.treeInstances)
            {
                // filter on prototype index
                if (prototypeIndexFilter != PROTOTYPE_DEFAULT_FILTER_INDEX && prototypeIndexFilter != treeInstance.prototypeIndex)
                    continue;

                // check distance
                float distance = Vector3.Distance(localPosition, treeInstance.position);

                if (distance < localBrushRadius)
                {
                    list.Add(treeInstance);
                }
            }

            return list;

        }

        #endregion Internal remove methods

        #region Internal overlapping methods

        // check for overlaps with other trees. using parallel linq
        private static bool IsOverlappingFast(TerrainData terrainData, Vector3 position, int prototypeIndexFilter, float minDistanceWorld)
        {

            minDistanceWorld = minDistanceWorld / terrainData.size.x;

            // if no item matches, then the return value of FirstOrDefault will be default(<T>), not null!
            TreeInstance defaultReturnValue = default(TreeInstance);

            TreeInstance instance = terrainData.treeInstances.AsParallel().Where(x => (prototypeIndexFilter == PROTOTYPE_DEFAULT_FILTER_INDEX || prototypeIndexFilter == x.prototypeIndex) && Vector3.Distance(position, x.position) < minDistanceWorld).FirstOrDefault();

            // compare against the default value
            return !instance.Equals(defaultReturnValue);

        }


        // check for overlaps with other trees. that's way too slow on a terrain full of trees
        private static bool IsOverlappingSlow(TerrainData terrainData, Vector3 position, int prototypeIndexFilter, float minDistanceWorld)
        {

            foreach (TreeInstance treeInstance in terrainData.treeInstances)
            {
                // filter on prototype index
                if (prototypeIndexFilter != PROTOTYPE_DEFAULT_FILTER_INDEX && prototypeIndexFilter != treeInstance.prototypeIndex)
                    continue;

                // check distance
                float distance = Vector3.Distance(position, treeInstance.position) * terrainData.size.x;

                if (distance < minDistanceWorld)
                {
                    return true;
                }
            }

            return false;

        }

        #endregion Internal overlapping methods
    }
}
