using UnityEngine;

namespace CharacterCore.BoxPhysics {
    public struct Trace {

        public Vector3 startPos;
        public Vector3 endPos;
        public float fraction;
        public bool startSolid;
        public Collider hitCollider;
        public Vector3 hitPoint;
        public Vector3 planeNormal;
        public float distance;

        public bool IsValid => hitCollider != null;
    }
}
