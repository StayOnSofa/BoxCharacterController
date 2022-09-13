using UnityEngine;

namespace FOVCamera
{
    [RequireComponent(typeof(PitchCamera))]
    public class PitchMouseDrag : MonoBehaviour
    {
        private const string c_MouseX = "Mouse X";
        private const string c_MouseY = "Mouse Y";

        [SerializeField] private float _sensetive = 1f;
        
        private PitchCamera _pitchCameraCache;

        private PitchCamera _pitchCamera
        {
            get
            {
                if (_pitchCameraCache == null)
                {
                    _pitchCameraCache = GetComponent<PitchCamera>();
                }

                return _pitchCameraCache;
            }
        }

        public bool IsDrag { private set; get; }

        private void Start()
        {
            Drag();
        }

        public void Drag()
        {
            IsDrag = true;
            Cursor.lockState = CursorLockMode.Locked;
        }

        public void UnDrag()
        {
            IsDrag = false;
            Cursor.lockState = CursorLockMode.None;
        }

        private Vector2 DeltaMouse()
        {
            return new Vector2(Input.GetAxis(c_MouseX), 
                Input.GetAxis(c_MouseY));
        }

        private void Update()
        {
            if (IsDrag)
            {
                Vector2 preDelta = DeltaMouse() * _sensetive;
                var delta = new Vector2(preDelta.x, -preDelta.y);
                
                _pitchCamera.Angles -= delta;
                
                _pitchCamera.ApplyTransform();
            }
        }
    }
}