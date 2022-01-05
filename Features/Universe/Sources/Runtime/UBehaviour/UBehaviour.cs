using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Universe.SceneTask.Runtime;

namespace Universe
{
    public abstract class UBehaviour : MonoBehaviour
    {
        #region Public

        public static bool MasterDebug;
        public bool IsMasterDebug => MasterDebug;
        
        [Header("Debug")] 
        public bool IsDebug;
        public bool IsVerbose;
        public bool GizmosVisible;
        
        #endregion
        
        
        #region Universe Core Loop

        public virtual void Awake()
        {
            Task.RegisterUpdate(this);
            Task.RegisterFixedUpdate(this);
            Task.RegisterLateUpdate(this);
        }

        public virtual void OnDestroy()
        {
            Task.UnregisterUpdate(this);
            Task.UnregisterFixedUpdate(this);
            Task.UnRegisterLateUpdate(this);
        }

        public virtual void OnUpdate(float deltatime) {}
        public virtual void OnFixedUpdate(float fixedDeltaTime) {}
        public virtual void OnLateUpdate(float deltaTime) {}

        #endregion
        
        
        #region Spawn

        protected void Spawn(AssetReference assetReference, int poolSize = 0) => 
            this.USpawn(assetReference, poolSize);
        
        protected void Spawn(AssetReference assetReference, Vector3 pos, Quaternion rotation, int poolSize = 0 ) =>
            this.USpawn(assetReference, pos, rotation, poolSize);

        protected void Spawn(AssetReference assetReference, Vector3 pos, Quaternion rotation, Transform parent, int poolSize = 0) =>
            this.USpawn(assetReference, pos, rotation, parent, poolSize);

        protected void Spawn(AssetReference assetReference, Vector3 pos, Quaternion rotation, Transform parent, Action<GameObject> callback, int poolSize = 0) => 
            this.USpawn(assetReference, pos, rotation, parent, callback, poolSize);

        protected void Spawn(AssetReference assetReference, Transform parent, Action<GameObject> callback) =>
            this.USpawn(assetReference, parent, callback);
        
        #endregion
        
        
        #region Verbose

        [Conditional("DEBUG")]
        protected void Verbose(string message) => 
            this.ULog(message);

        [Conditional("DEBUG")]
        protected void Verbose<T>(string message, T val) => 
            this.ULog(message, val);
        
        [Conditional("DEBUG")]
        protected void Verbose<T>(string message, List<T> values) => 
            this.ULog(message, values);
        
        [Conditional("DEBUG")]
        protected void Verbose<T>(string message, IEnumerable<T> values) => 
            this.ULog(message, values);

        [Conditional("DEBUG")]
        protected void Verbose<T>(string message, Dictionary<T, T> values) => 
            this.ULog(message, values);

        #endregion
        
        
        #region Inputs

        protected float GetAxis(string axis) => 
            this.UGetAxis(axis);

        protected bool GetKey(UKeyCode keyCode) => 
            this.UGetKey(keyCode);

        protected bool GetKeyDown(UKeyCode keyCode) => 
            this.UGetKeyDown(keyCode);
        
        protected bool GetKeyUp(UKeyCode keyCode) => 
            this.UGetKeyUp(keyCode);
        
        #endregion
        
        
        #region Tasks

        protected void Load(TaskData task) =>
            this.ULoad(task);

        protected void UnloadLastTaskAndLoad(TaskData task) =>
            this.UUnloadLastTaskAndLoad(task);

        protected void Unload(TaskData task) =>
            this.UUnload(task);
        
        #endregion


        #region Cached Members

        [NonSerialized]
        private Transform _transform;
        public new Transform transform => _transform ? _transform : _transform = GetComponent<Transform>();
      
        [NonSerialized]
        private Animation _animation;
        public new Animation animation => 
            _animation ? _animation : _animation = GetComponent<Animation>();
        
        [NonSerialized]
        private Camera _camera;
        public new Camera camera => 
            _camera ? _camera : _camera = GetComponent<Camera>();

        [NonSerialized]
        private Collider _collider;
        public new Collider collider => 
            _collider ? _collider : _collider = GetComponent<Collider>();

        [NonSerialized]
        private Collider2D _collider2D;
        public new Collider2D collider2D => 
            _collider2D ? _collider2D : _collider2D = GetComponent<Collider2D>();

        [NonSerialized]
        private ConstantForce _constantForce;
        public new ConstantForce constantForce => 
            _constantForce ? _constantForce : _constantForce = GetComponent<ConstantForce>();

        [NonSerialized]
        private HingeJoint _hingeJoint;
        public new HingeJoint hingeJoint => 
            _hingeJoint ? _hingeJoint : _hingeJoint = GetComponent<HingeJoint>();

        [NonSerialized]
        private Light _light;
        public new Light light => 
            _light ? _light : _light = GetComponent<Light>();

        [NonSerialized]
        private ParticleSystem _particleSystem;
        public new ParticleSystem particleSystem => 
            _particleSystem ? _particleSystem : _particleSystem = GetComponent<ParticleSystem>();

        [NonSerialized]
        private Renderer _renderer;
        public new Renderer renderer => 
            _renderer ? _renderer : _renderer = GetComponent<Renderer>();

        [NonSerialized]
        private Rigidbody _rigidbody;
        public new Rigidbody rigidbody => 
            _rigidbody ? _rigidbody : _rigidbody = GetComponent<Rigidbody>();

        [NonSerialized]
        private Rigidbody2D _rigidbody2D;
        public new Rigidbody2D rigidbody2D => 
            _rigidbody2D ? _rigidbody2D : _rigidbody2D = GetComponent<Rigidbody2D>();

        #endregion
    }
}