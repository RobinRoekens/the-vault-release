using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;
using static UnityEngine.AddressableAssets.Addressables;

namespace Universe
{
    public static class Spawn
    {
        #region Public API

        public static void USpawn(this UBehaviour source, AssetReference assetReference, int maxPoolSize = 0) =>
            TrySpawn(source, assetReference, Vector3.zero, Quaternion.identity, null, EmptyCallback, maxPoolSize);
        
        public static void USpawn(this UBehaviour source, AssetReference assetReference, Vector3 position, Quaternion rotation, int maxPoolSize = 0) =>
            TrySpawn(source, assetReference, position, rotation, null, EmptyCallback, maxPoolSize);

        public static void USpawn(this UBehaviour source, AssetReference assetReference, Vector3 position, Quaternion rotation, Transform parent, int maxPoolSize = 0) => 
            TrySpawn(source, assetReference, position, rotation, parent, EmptyCallback, maxPoolSize);
        
        public static void USpawn(this UBehaviour source, AssetReference assetReference, Vector3 position, Quaternion rotation, Transform parent, Action<GameObject> callback, int maxPoolSize = 0) =>
            TrySpawn(source, assetReference, position, rotation, parent, callback, maxPoolSize);

        public static void USpawn(this UBehaviour source, AssetReference assetReference, Transform parent, Action<GameObject> callback = null, int maxPoolSize = 0) =>
            TrySpawn(source, assetReference, Vector3.zero, Quaternion.identity, parent, callback, maxPoolSize);

        #endregion
        
        
        #region Main

        private static void TrySpawn(UBehaviour source, AssetReference assetReference, Vector3 position, Quaternion rotation, Transform parent, Action<GameObject> callback, int maxPoolSize = 0)
        {
            if (!_isAdressableReady)
            {
                var delayedSpawn = new DelayedSpawn
                {
                    m_source = source,
                    m_assetReference = assetReference,
                    m_position = position,
                    m_quaternion = rotation,
                    m_transform = parent,
                    callback = callback,
                    maxPoolSize = maxPoolSize
                };

                _delayedSpawn.Add(delayedSpawn);
                return;
            }

            assetReference.InstantiateAsync().Completed += go =>
            {
                var tr = go.Result.transform;
                
                tr.SetParent(parent);
                tr.localPosition = position;
                tr.localRotation = rotation;

                if(callback != null) callback(go.Result);
            };
        }

        private static void EmptyCallback(GameObject gameObject){}
        
        #endregion
        
        
        #region Constructor and Wrapper

        static Spawn()
        {
            InitializeAsync().Completed += AddressableInitializeAsyncCompleted;
        }
        
        private static void AddressableInitializeAsyncCompleted(AsyncOperationHandle<IResourceLocator> obj)
        {
            _isAdressableReady = true;

            foreach (var d in _delayedSpawn)
            {
                USpawn(d.m_source, d.m_assetReference, d.m_position, d.m_quaternion, d.m_transform, d.callback,
                    d.maxPoolSize);
            }
        }
        
        #endregion
        
        
        #region Private
        
        private static bool _isAdressableReady;
        private static List<DelayedSpawn> _delayedSpawn = new();

        #endregion
    }

    public struct DelayedSpawn
    {
        public UBehaviour m_source;
        public AssetReference m_assetReference;
        public Vector3 m_position;
        public Quaternion m_quaternion;
        public Transform m_transform;
        public Action<GameObject> callback;
        public int maxPoolSize;
    }
}