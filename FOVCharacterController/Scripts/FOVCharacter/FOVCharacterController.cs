using CharacterCore.Controller;
using FOVCamera;
using UnityEngine;

namespace FOVCharacter
{
    public class FOVCharacterController : MonoBehaviour
    {
        [SerializeField] private PitchCamera _pitchCamera;
        [SerializeField] private PhysicsCharacterController _character;

        public KeyCode Forward = KeyCode.W;
        public KeyCode Back = KeyCode.S;
        public KeyCode StrafeLeft = KeyCode.A;
        public KeyCode StrafeRight = KeyCode.D;
        public KeyCode Jump = KeyCode.Space;
        
        private void Update()
        {
            Vector3 direction = Vector3.zero;

            if (Input.GetKey(StrafeRight))
                direction = _pitchCamera.GetDirectionByDeg(-90);

            if (Input.GetKey(StrafeLeft))
                direction = _pitchCamera.GetDirectionByDeg(90);

            
            if (Input.GetKey(Forward))
            {
                direction = _pitchCamera.GetDirectionByDeg(0);
               
                if (Input.GetKey(StrafeRight))
                    direction = _pitchCamera.GetDirectionByDeg(-45);

                if (Input.GetKey(StrafeLeft))
                    direction = _pitchCamera.GetDirectionByDeg(45);
            }

            if (Input.GetKey(Back))
            {
                direction = _pitchCamera.GetDirectionByDeg(180);
               
                if (Input.GetKey(StrafeRight))
                    direction = _pitchCamera.GetDirectionByDeg(180+45);

                if (Input.GetKey(StrafeLeft))
                    direction = _pitchCamera.GetDirectionByDeg(180-45);
            }

            if (Input.GetKey(Jump))
                _character.Jump();

            direction = Vector3.Normalize(direction);
            _character.Walk(direction);
        }
    }
}