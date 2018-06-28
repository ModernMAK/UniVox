using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace ProceduralMesh
{
    /// <summary>
    /// Behaviour for rendering multiple Meshes returned from Dynamic Meshes
    /// Uses MeshFilter, MeshRenderer, and MeshCollider to achieve this
    /// </summary>
    public class DynamicMeshCollider : MonoBehaviour
    {
        /// <summary>
        /// The Name of the Container Object
        /// </summary>
        public string ContainerName;

        /// <summary>
        /// The container object grouping the submeshes together
        /// </summary>
        private GameObject _container;

        private List<GameObject> _submeshList;

        protected virtual void Awake()
        {
            _submeshList = new List<GameObject>();
            _container = new GameObject(ContainerName + " (Mesh Container)");
            SetParent(_container.transform, transform);
        }

        protected virtual void Start()
        {
            _container.name = ContainerName + " (Mesh Container)";
            SetParent(_container.transform, transform);
        }

        /// <summary>
        /// Sets the meshes of this renderer, which then sets the mesh of the submesh gameObjects.
        /// </summary>
        /// <param name="submeshes">The submeshes to render.</param>
        public void SetMeshes(Mesh[] submeshes)
        {
            //Iterate over submeshes, and "create/fetch" respective submeshes
            for (var i = 0; i < submeshes.Length; i++)
                CreateOrFetchSubmesh(submeshes[i], i).SetActive(true);
            //Iterate over excess submesh gameObjects, and disable them
            for (var i = submeshes.Length; i < _submeshList.Count; i++)
                CreateOrFetchSubmesh(null, i).SetActive(false);
        }

        /// <summary>
        /// Sets the child's parent
        /// Also sets the rotation, position, and scale to default values
        /// </summary>
        /// <param name="child">The Child.</param>
        /// <param name="parent">The new Parent of the Child.</param>
        public static void SetParent(Transform child, Transform parent)
        {
            child.parent = parent;
            child.localScale = Vector3.one;
            child.localPosition = Vector3.zero;
            child.localRotation = Quaternion.identity;
        }

        private string NameHelper(Mesh submesh, int submeshIndex)
        {
            if (submeshIndex == -1)
                submeshIndex = (_submeshList.Count + 1);

//            Debug.Log(submesh != null);

            return string.Format("{0} : {1} (Submesh {2})", ContainerName, (submesh != null ? submesh.name : "NULL"),
                submeshIndex);
        }

        /// <summary>
        /// Creates or Fetches a Submesh Gameobject
        /// </summary>
        /// <param name="submesh">The submesh to use for the gameObject.</param>
        /// <param name="submeshIndex">The index of the gameObject, -1 to force a new gameObject.</param>
        /// <returns></returns>
        private GameObject CreateOrFetchSubmesh(Mesh submesh, int submeshIndex = -1)
        {
            GameObject submeshGameObject;
            //If -1 or we dont have enough submesh gameObjects...
            if (submeshIndex == -1 || _submeshList.Count <= submeshIndex)
            {
                //Create a new submesh gameObject
                //The name of the submesh
                submeshGameObject = new GameObject();
                _submeshList.Add(submeshGameObject);
            }
            else
            {
                //Otherwise, fetch the gameObject from a list
                submeshGameObject = _submeshList[submeshIndex];
            }

            submeshGameObject.name = NameHelper(submesh, submeshIndex);
//            //Mesh Visualizer offers some debug utilities for the mesh
//            var meshVisualizer = submeshGameObject.GetOrAddComponent<MeshVisualizer>();
//            //Mesh Filter IS the mesh according to the renderer
//            var meshFilter = submeshGameObject.GetOrAddComponent<MeshFilter>();

//            if (UseCollider)
//            {
            //Mesh Collider IS the mesh according to physics
            var meshCollider = submeshGameObject.GetOrAddComponent<MeshCollider>();
            meshCollider.sharedMesh = submesh;
//            }
//            //Set the mesh 
//            meshVisualizer.Mesh = meshFilter.mesh = submesh;

//            //Mesh RendererUtil IS the actual renderer,
//            var meshRenderer = submeshGameObject.GetOrAddComponent<MeshRenderer>();
//
//            //Set the material
//            meshRenderer.material = Material;

            //Set the parent, makes sure our scale doesn't get wacky
            SetParent(submeshGameObject.transform, _container.transform);

            return submeshGameObject;
        }
    }
}