using CharacterCore.BoxPhysics;
using CharacterCore.Utils;
using UnityEngine;

namespace CharacterCore.Controller
{
    [RequireComponent(typeof(BoxCollider))]
    public class BoxCharacterController : MonoBehaviour
    {
        private const float WallEpsilon = 0.1f;
        
        private struct Wall
        {
            public Wall(float block, Vector3 normal)
            {
                Block = block;
                Normal = normal;
            }

            public float Block;
            public Vector3 Normal;
        }

        [SerializeField] private LayerMask _layerMask;
        
        private BoxCollider _cacheCollider;

        private BoxCollider _boxCollider
        {
            get
            {
                if (_cacheCollider == null)
                    _cacheCollider = GetComponent<BoxCollider>();
                return _cacheCollider;
            }
        }

        public Vector3 Scale => _boxCollider.size;

        public float GroundAngle { get; private set; }
        
        public Vector3 Normal { get; private set; }
        public Vector3 Velocity { get; private set; }
        public Vector3 ComputePenetration { get; private set; }
        
        public bool IsGrounded { get; private set; }
        public bool IsTouchesCeiling{ get; private set; }
        
        private void OnDrawGizmos()
        {
            var scale = _boxCollider.size;
            var origin = transform.position;
            
            BoxGizmos.DrawBox(Color.red, origin, scale);
            BoxGizmos.Arrow(Color.blue, origin - new Vector3(0, scale.y /2f, 0), Normal);


            Gizmos.color = Color.yellow;
            
            if (IsTouchesCeiling)
                Gizmos.DrawSphere(origin + new Vector3(0, scale.y /2f, 0), 0.1f);
            
            if (IsGrounded)
                Gizmos.DrawSphere(origin - new Vector3(0, scale.y /2f, 0), 0.1f);
        }
        
        private Wall DownTopRayCast(float power)
        {
            var scale = _boxCollider.size;
            var origin = transform.position;
            
            var trace = Tracer.TraceBox(origin, origin + new Vector3(0, power, 0), scale / 2f, _layerMask);
            if (trace.IsValid)
            {
                var position = origin;
                
                position.y = trace.hitPoint.y - scale.y /2f;
                
                if (power < 0)
                    position.y = trace.hitPoint.y + scale.y /2f;
                
                return new Wall(position.y, trace.planeNormal);
            }

            if (power < 0) 
                return new Wall(Mathf.NegativeInfinity, Vector3.up);
            
            return new Wall(Mathf.Infinity, Vector3.up);
        }

        public void Move(Vector3 direction)
        {
            ComputePenetration = Vector3.zero;
            Normal = Vector3.up;
            GroundAngle = Vector3.Angle(Normal, Vector3.up);

            IsGrounded = false;
            IsTouchesCeiling = false;
            
            var directionXZ = new Vector3(direction.x, 0, direction.z);
            var resultXZ = CollisionResolver.StepResolver(_boxCollider, transform.position, directionXZ, _layerMask);
            ComputePenetration = resultXZ.ComputePenetration;
            
            transform.position = resultXZ.ResolvePosition;
            
            var directionY = new Vector3(0, direction.y, 0);
            var resultY = CollisionResolver.StepResolver(_boxCollider, transform.position, directionY, _layerMask);

            transform.position = resultY.ResolvePosition;
            
            var downWall = DownTopRayCast(-(Mathf.Abs(direction.y) + WallEpsilon));
            if (Mathf.Abs(downWall.Block - transform.position.y) < WallEpsilon)
            {
                Normal = downWall.Normal;
                GroundAngle = Vector3.Angle(Normal, Vector3.up);
                
                IsGrounded = true;
            }
            
            var topWall = DownTopRayCast(Mathf.Abs(direction.y) + WallEpsilon);
            if (Mathf.Abs(topWall.Block - transform.position.y) < WallEpsilon)
            {
                IsTouchesCeiling = true;
            }
        }

        private Vector3 _prevPosition;

        private void Update()
        {
            Velocity = (transform.position - _prevPosition) / Time.deltaTime;
            _prevPosition = transform.position;
        }
    }
}