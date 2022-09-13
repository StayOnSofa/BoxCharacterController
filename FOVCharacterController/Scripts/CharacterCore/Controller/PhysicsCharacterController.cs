using CharacterCore.Utils;
using UnityEngine;

namespace CharacterCore.Controller
{
    [RequireComponent(typeof(BoxCharacterController))]
    public class PhysicsCharacterController : MonoBehaviour
    {
        private enum MoveStates
        {
            Slope,
            Walk,
            Air,
        }

        [SerializeField] private float _slopeLimit = 45;
        
        [SerializeField] private float _walkSpeed = 6f;
        [SerializeField] private float _groundedAcceleration = 8f;
        [SerializeField] private float _midAirSpeed = 6f;
        [SerializeField] private float _midAirAcceleration = 2f;
        [SerializeField] private float _gravityScale = 2.8f;
        [SerializeField] private float _jumpHeight = 3;
        [SerializeField] private float _jumpCooldown = 0.2f;
        [SerializeField] private float _slowDownPenetrationPower = 0.5f;
        
        private Vector3 _walkDirection;
        private Vector3 _velocityToApply;
        
        private MoveStates _moveStates = MoveStates.Air;
        
        private BoxCharacterController _character;

        private Vector3 _groundWalkNormal;

        private void OnDrawGizmos()
        {
            if (_character == null)
                return;

            var origin = transform.position;
            BoxGizmos.Arrow(new Color(0.99f, 0.4f, 0.22f), origin, (_character.ComputePenetration) * 15);
            
            origin.y -= _character.Scale.y / 2;
            BoxGizmos.Arrow(Color.magenta, origin, _groundWalkNormal);
        }

        private void Start()
        {
            _character = GetComponent<BoxCharacterController>();
        }
        
        public Vector3 ScaledGravity
        {
            get
            {
                return Physics.gravity * _gravityScale;
            }
        }
        
        private Vector3 SlopedDirection(Vector3 direction, Vector3 slopeNormal)
        {
            Vector3 desiredDirection = Vector3.Cross(direction, slopeNormal);
            return Vector3.Cross(slopeNormal, desiredDirection);
        }
        
        public void Walk(Vector3 walkDirection)
        {
            _walkDirection = walkDirection;
        }
        
        private float _jumpTimer;
        private bool _jumpLock;
        
        public void Jump()
        {
            if (_character.IsGrounded && _character.GroundAngle < _slopeLimit)
            {
                if (_jumpTimer > _jumpCooldown && !_jumpLock)
                {
                    float jumpForce = Mathf.Sqrt(_gravityScale * -Physics.gravity.y * _jumpHeight);
                    _velocityToApply.y = jumpForce;

                    _jumpTimer = 0;
                    _jumpLock = true;
                }
            }
        }
        
        public void AddForce(Vector3 velocity)
        {
            _velocityToApply += velocity;
        }

        private void OnWalk(float dt)
        {
            _jumpLock = false;
            
            Vector3 normal = _character.Normal;
            Vector3 velocity = _character.Velocity;
	        
            Vector3 normalizedInputDirection = Vector3.Normalize(_walkDirection);
	        
            Vector3 desiredDirection = Vector3.Cross(normalizedInputDirection, normal);
            desiredDirection = Vector3.Cross(normal, desiredDirection);

            _groundWalkNormal = desiredDirection;
            
            Vector3 velocityToAdd = desiredDirection * _walkSpeed - _velocityToApply;

            float currentAcceleration = _groundedAcceleration;
            float accelerationDot = 2f - (Vector3.Dot(velocity.normalized, normalizedInputDirection) + 1f);

            currentAcceleration += accelerationDot * _midAirAcceleration;
            
            _velocityToApply += velocityToAdd * dt * currentAcceleration;
            
            Vector3 penetration = Vector3.Normalize(_character.ComputePenetration);
            penetration.y = 0;
            
            _velocityToApply -= Vector3.Dot(_velocityToApply, penetration) * penetration * _slowDownPenetrationPower;
        }
        
        private void OnAir(float dt)
        {
            Vector3 normalizedInputDirection = Vector3.Normalize(_walkDirection);
            float currentSpeed = Vector3.Dot(_velocityToApply, normalizedInputDirection);
	        
            float speedToAdd = _midAirSpeed - currentSpeed;
            float currentAcceleration = _midAirAcceleration;

            Vector3 velocity = _character.Velocity;
            velocity.y = 0;
	        
            float accelerationDot = 2f - (Vector3.Dot(velocity.normalized, normalizedInputDirection) + 1f);

            currentAcceleration += accelerationDot * _midAirAcceleration;
            speedToAdd = Mathf.Max(Mathf.Min(speedToAdd, currentAcceleration * dt), 0f);
            
          
            _velocityToApply += normalizedInputDirection * speedToAdd;
            _velocityToApply += ScaledGravity * dt;

            Vector3 penetration = Vector3.Normalize(_character.ComputePenetration);
            penetration.y = 0;
            
            _velocityToApply -= Vector3.Dot(_velocityToApply, penetration) * penetration * _slowDownPenetrationPower;
            
            if (_character.IsTouchesCeiling && _velocityToApply.y > 0f)
            {
                _velocityToApply.y = Mathf.Min(0f, -_velocityToApply.y * 0.25f);
            }
        }
        
        private void OnSlope(float dt)
        {
            Vector3 normal = _character.Normal;
            Vector3 normalizedInputDirection = Vector3.Normalize(_walkDirection);
	        
            Vector3 desiredDirection = Vector3.Cross(normalizedInputDirection, normal);
            desiredDirection = Vector3.Cross(normal, desiredDirection);
            Vector3 desiredVelocity = desiredDirection * _walkSpeed;
            if (desiredVelocity.y > 0f)
            {
                desiredVelocity.y = 0f;
            }
            desiredVelocity += SlopedDirection(Vector3.down, normal) * ScaledGravity.magnitude;
            Vector3 velocityToAdd = desiredVelocity - _velocityToApply;
                
            float currentAcceleration = _midAirAcceleration * 0.5f;
            _velocityToApply += velocityToAdd * dt * currentAcceleration;
            
            Vector3 penetration = Vector3.Normalize(_character.ComputePenetration);
            penetration.y = 0;
            
            _velocityToApply -= Vector3.Dot(_velocityToApply, penetration) * penetration * _slowDownPenetrationPower;
        }
        
        private void UpdateMovementState()
        {
            MoveStates state = MoveStates.Air;
	        
            if (_character.IsGrounded && _character.GroundAngle <= 89f)
            {
                if (_character.GroundAngle > _slopeLimit)
                {
                    if (_character.Velocity.y <= 0f)
                    {
                        state = MoveStates.Slope;
                    }
                }
                else
                {
                    state = MoveStates.Walk;
                }
            }

            if (state != _moveStates)
            {
                _moveStates = state;
                ChangeMoveState(state);
            }
        }
        
        private void ChangeMoveState(MoveStates state)
        {
            if (state == MoveStates.Slope)
                _velocityToApply = _character.Velocity;
        }
        
        private void PhysicsUpdate(float dt)
        {
            switch (_moveStates)
            {
                case MoveStates.Walk:
                    OnWalk(dt); 
                    break;
                case MoveStates.Slope:
                    OnSlope(dt); 
                    break;
                case MoveStates.Air:
                    OnAir(dt); 
                    break;
            }
        }

        private void Update()
        {
            var dt = Time.deltaTime;
            _character.Move(_velocityToApply * dt);

            UpdateMovementState();
            PhysicsUpdate(dt);

            _jumpTimer += dt;
        }
    }
}