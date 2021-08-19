using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rowlan.Yapp
{
    public class MeshUtils
    {

        /// <summary>
        /// Render a game object using DrawMesh.
        /// Thanks Lennart!
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="sourceLODLevel"></param>
        public static void RenderGameObject(GameObject gameObject, int sourceLODLevel)
        {
            GameObject root = gameObject;

            LODGroup lodGroup = gameObject.GetComponent<LODGroup>();
            if (lodGroup && lodGroup.lodCount > 0)
            {
                root = lodGroup.GetLODs()[sourceLODLevel].renderers[0].gameObject;
            }

            MeshRenderer[] renderers = root.GetComponentsInChildren<MeshRenderer>();
            for (int i = 0; i < renderers.Length; i++)
            {
                MeshFilter meshFilter = renderers[i].gameObject.GetComponent<MeshFilter>();
                if (meshFilter)
                {
                    Matrix4x4 matrix = Matrix4x4.TRS(
                        renderers[i].transform.position,
                        renderers[i].transform.rotation,
                        renderers[i].transform.lossyScale);

                    Mesh mesh = meshFilter.sharedMesh;

                    for (int j = 0; j < renderers[i].sharedMaterials.Length; j++)
                    {
                        Material material = renderers[i].sharedMaterials[j];
                        material.SetPass(0);

                        Graphics.DrawMesh(mesh, matrix, material, 0);
                    }
                }
            }
        }
    }
}
