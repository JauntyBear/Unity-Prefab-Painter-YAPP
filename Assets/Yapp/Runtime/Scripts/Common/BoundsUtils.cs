using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rowlan.Yapp
{
    public class BoundsUtils
    {

        private static Bounds zeroBounds = new Bounds(Vector3.zero, Vector3.zero);

        public static bool GetBounds( Transform transform, out Bounds localBounds, out Bounds worldBounds)
        {
            MeshFilter meshFilter = transform.GetComponent<MeshFilter>();
            MeshRenderer meshRenderer = transform.GetComponent<MeshRenderer>();

            if (meshFilter && meshRenderer)
            {
                localBounds = meshFilter.sharedMesh.bounds;
                worldBounds = meshRenderer.bounds;

                return true;
            }
            else
            {

                SkinnedMeshRenderer skinnedMeshRenderer = transform.GetComponent<SkinnedMeshRenderer>();

                if (skinnedMeshRenderer)
                {
                    localBounds = skinnedMeshRenderer.sharedMesh.bounds;
                    worldBounds = skinnedMeshRenderer.bounds;

                    return true;
                }

            }

            localBounds = zeroBounds;
            worldBounds = zeroBounds;

            return false;
        }
    }
}
