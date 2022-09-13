using UnityEngine;

namespace CharacterCore.BoxPhysics
{
    public static class CollisionResolver
    {
        private const int MaxCollisions = 128;

        public struct ResolveCompute
        {
            public Vector3 ResolvePosition;
            public Vector3 ComputePenetration;
            
            public ResolveCompute(Vector3 resolvePosition, Vector3 penetration)
            {
                ResolvePosition = resolvePosition;
                ComputePenetration = penetration;
            }
        }

        private static Collider[] _colliders = new Collider [MaxCollisions];
        
        public static ResolveCompute Resolve(BoxCollider collider, Vector3 origin, LayerMask groundLayerMask)
        {
            Vector3 computePenetration = Vector3.zero;
            
            int numOverlaps = Physics.OverlapBoxNonAlloc (origin, collider.bounds.extents, _colliders,
                Quaternion.identity, groundLayerMask, QueryTriggerInteraction.Ignore);
            
            for (int i = 0; i < numOverlaps; i++) {

                Vector3 direction;
                float distance;

                var otherCollider = _colliders[i];
                if (otherCollider != collider)
                {
                    if (Physics.ComputePenetration(collider, origin,
                            Quaternion.identity, otherCollider, otherCollider.transform.position,
                            otherCollider.transform.rotation, out direction, out distance))
                    {

                        direction.Normalize();
                        Vector3 penetrationVector = direction * distance;
                        origin += penetrationVector;

                        computePenetration = penetrationVector;
                    }
                }
            }

            return new ResolveCompute(origin, computePenetration);
        }
        
        public static ResolveCompute StepResolver(BoxCollider collider, Vector3 origin, Vector3 direction, LayerMask groundLayerMask, float step = 0.1f)
        {
            Vector3 computePenetration = Vector3.zero;
            Vector3 destination = origin + direction;
            
            float distance = Vector3.Distance(origin, destination);
            Vector3 calculateOrigin = origin;
            
            if (distance > step)
            {
                int stepsNumber = (int) (distance / step);
                float stepPower = 1f / stepsNumber;

                for (int i = 0; i < stepsNumber; i++)
                {
                    calculateOrigin += direction * stepPower;

                    var preValues = Resolve(collider, calculateOrigin, groundLayerMask);
                    computePenetration += preValues.ComputePenetration;
                    
                    calculateOrigin = preValues.ResolvePosition;
                }
            }
            else
            {
                var preValues = Resolve(collider, destination, groundLayerMask);
                computePenetration += preValues.ComputePenetration;
                
                calculateOrigin = preValues.ResolvePosition;
            }

            return new ResolveCompute(calculateOrigin, computePenetration);
        }
    }
}