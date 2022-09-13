using UnityEngine;

namespace FOVCamera
{
    [RequireComponent(typeof(Camera))]
    public class PitchCamera : MonoBehaviour
    {
        [SerializeField] private Vector2 _minMaxPith = new Vector2(89.9f, -89.9f);
        [SerializeField] private Vector2 _angles;
        
        private Camera _cameraCache;

        private Camera _camera
        {
            get
            {
                if (_cameraCache == null)
                {
                    _cameraCache = GetComponent<Camera>();
                }

                return _cameraCache;
            }
        }

        public Vector2 Angles
        {
            get => GetAngles();
            set => SetAngles(value);
        }
        
        public Vector2 GetAngles()
        {
            return _angles;
        }
        
        public void SetAngles(Vector2 angles)
        {
            _angles = angles;
            _angles.y = ClampAngle(_angles.y, _minMaxPith);
        }

        private float ClampAngle(float angle, Vector2 clampRule)
        {
            if (angle > clampRule.x)
                angle = clampRule.x;

            if (angle < clampRule.y)
                angle = clampRule.y;

            return angle;
        }
        
        private void OnValidate()
        {
            if (Application.isPlaying)
                return;
            
            ApplyTransform();
        }

        private Vector3 _cameraFront;

        public void ApplyTransform()
        {
            float yaw = Angles.x;
            float pitch = Angles.y;
            
            pitch = ClampAngle(pitch, _minMaxPith);

            pitch *= Mathf.Deg2Rad;
            yaw *= Mathf.Deg2Rad;
            
            _cameraFront = Vector3.zero;
            
            _cameraFront.x = Mathf.Cos(pitch) * Mathf.Cos(yaw);
            _cameraFront.y = Mathf.Sin(pitch);
            _cameraFront.z = Mathf.Cos(pitch) * Mathf.Sin(yaw);
            
            _cameraFront = Vector3.Normalize(_cameraFront);

            _camera.transform.forward = _cameraFront;
        }

        public Vector3 GetDirectionByDeg(float angle)
        {
            float yaw = Angles.x + angle;
            float pitch = Angles.y;
            
            pitch = ClampAngle(pitch, _minMaxPith);
            
            pitch *= Mathf.Deg2Rad;
            yaw *= Mathf.Deg2Rad;

            Vector3 front = Vector3.zero;
            
            front.x = Mathf.Cos(pitch) * Mathf.Cos(yaw);
            front.z = Mathf.Cos(pitch) * Mathf.Sin(yaw);

            return front;
        }

        public Vector2 GetCameraFront()
        {
            return new Vector2(_cameraFront.x, _cameraFront.z);
        }
        
        public Vector3 GetViewDirection()
        {
            Vector2 front = GetCameraFront();
            return new Vector3(front.x, 0, front.y);
        }
    }
}