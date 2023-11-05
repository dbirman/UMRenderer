using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Urchin.API;
using Urchin.Behaviors;

namespace Urchin.Managers
{
    /// <summary>
    /// Field of View Manager
    /// Handles receiving data messages about FOV objects and passing them to their named object
    /// 
    /// Also handles creating/deleting FOV objects from the FOV prefab
    /// </summary>
    public class FOVManager : MonoBehaviour
    {
        #region Serialized fields
        [SerializeField] private GameObject _fovPrefabGO;
        [SerializeField] private Transform _fovParentT;
        #endregion

        #region Variables
        private Dictionary<string, FOVBehavior> _fovs;
        #endregion

        #region Unity
        void Awake()
        {
            _fovs = new();
        }

        private void Start()
        {

            Client_SocketIO.CreateFOV += Create;
            Client_SocketIO.DeleteFOV += Delete;
            Client_SocketIO.SetFOVPos += SetPosition;
            Client_SocketIO.SetFOVOffset += SetOffset;
            Client_SocketIO.SetFOVTextureDataMetaInit += SetTextureInit;
            Client_SocketIO.SetFOVTextureDataMeta += SetTextureMeta;
            Client_SocketIO.SetFOVTextureData += SetTextureData;
            Client_SocketIO.SetFOVVisibility += SetVisibility;
        }
        #endregion

        #region Public

        /// <summary>
        /// Create new FOV objects
        /// </summary>
        /// <param name="names"></param>
        public void Create(List<string> names)
        {
            foreach (string name in names)
            {
                GameObject newFOV = Instantiate(_fovPrefabGO, _fovParentT);
                newFOV.name = name;

                _fovs.Add(name, newFOV.GetComponent<FOVBehavior>());
            }
        }

        /// <summary>
        /// Delete named FOV objects
        /// </summary>
        /// <param name="names"></param>
        public void Delete(List<string> names)
        {
            foreach (string name in names)
            {
                if (_fovs.ContainsKey(name))
                    Destroy(_fovs[name].gameObject);
            }
        }

        /// <summary>
        /// Change the visibility of an FOV object
        /// </summary>
        /// <param name="data"></param>
        public void SetVisibility(Dictionary<string, bool> data)
        {
            foreach (KeyValuePair<string, bool> kvp in data)
            {
                string name = kvp.Key;
                bool visible = kvp.Value;

                _fovs[name].gameObject.SetActive(visible);
            }
        }

        /// <summary>
        /// Set the corner coordinates for an FOV object
        /// </summary>
        /// <param name="data">List of Vector3 (as a List of List of floats)"</param>
        public void SetPosition(Dictionary<string, List<List<float>>> data)
        {
            foreach (KeyValuePair<string, List<List<float>>> kvp in data)
            {
                string name = kvp.Key;
                var verticesList = kvp.Value;

                List<Vector3> vertices = new();
                for (int i = 0; i < data.Count; i++)
                    vertices.Add(new Vector3(verticesList[i][0], verticesList[i][1], verticesList[i][2]));

                _fovs[name].SetPosition(vertices);
            }
        }

        /// <summary>
        /// Set the offset of a FOV object along DV axis; positive is up.
        /// </summary>
        /// <param name="data"></param>
        public void SetOffset(Dictionary<string, float> data)
        {
            foreach (KeyValuePair<string, float> kvp in data)
            {
                string name = kvp.Key;
                float offset = kvp.Value;
                _fovs[name].SetOffset(offset);
            }
        }

        /// <summary>
        /// Set the total number of texture chunks to receive for a FOV
        /// </summary>
        /// <param name="data"></param>
        public void SetTextureInit(List<object> data)
        {
            string name = (string)data[0];
            int totalChunks = (int)data[1];
            int width = (int)data[2];
            int height = (int)data[3];
            string type = (string)data[4];
            _fovs[name].SetTextureInit(totalChunks, width, height, type);
        }


        string nextFOV;

        /// <summary>
        /// Set the metadata of next incoming texture chunk for a FOV
        /// </summary>
        /// <param name="data"></param>
        public void SetTextureMeta(List<object> data)
        {
            nextFOV = (string)data[0];
            int nextChunk = (int)data[1];
            bool nextApply = (bool)data[2];
            _fovs[nextFOV].SetTextureMeta(nextChunk, nextApply);
        }

        /// <summary>
        /// Set a texture chunk for a FOV
        /// </summary>
        /// <param name="bytes"></param>
        public void SetTextureData(byte[] bytes)
        {
            _fovs[nextFOV].SetTextureData(bytes);
        }


        #endregion
    }


}