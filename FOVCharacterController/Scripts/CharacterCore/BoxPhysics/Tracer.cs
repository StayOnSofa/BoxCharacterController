using UnityEngine;

namespace CharacterCore.BoxPhysics {
    public static class Tracer {
        
        public static Trace TraceBox (Vector3 start, Vector3 destination, Vector3 extents, int layerMask, float contactOffset = 0.01f, float colliderScale = 1f) {

            var result = new Trace () {
                startPos = start,
                endPos = destination
            };

            var longSide = Mathf.Sqrt (contactOffset * contactOffset + contactOffset * contactOffset);
            var direction = (destination - start).normalized;
            var maxDistance = Vector3.Distance (start, destination) + longSide;
            extents *= (1f - contactOffset);

            RaycastHit hit;
            if (Physics.BoxCast (center: start,
                halfExtents: extents * colliderScale,
                direction: direction,
                orientation: Quaternion.identity,
                maxDistance: maxDistance,
                hitInfo: out hit,
                layerMask: layerMask,
                queryTriggerInteraction: QueryTriggerInteraction.Ignore)) {

                result.fraction = hit.distance / maxDistance;
                result.hitCollider = hit.collider;
                result.hitPoint = hit.point;
                result.planeNormal = hit.normal;
                result.distance = hit.distance;
                
                RaycastHit normalHit;
                Ray normalRay = new Ray (hit.point - direction * 0.001f, direction);
                if (hit.collider.Raycast (normalRay, out normalHit, 0.002f)) {
                    result.planeNormal = normalHit.normal;
                }
            } else
                result.fraction = 1;

            return result;
        }
    }
}
