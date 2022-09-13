using UnityEngine;

namespace CharacterCore.Utils
{
    public static class BoxGizmos
    {
        public static void Arrow(Color color, Vector3 pos, Vector3 direction, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f)
        {
            Color prevColor = Gizmos.color;
            Gizmos.color = color;
            
            if (direction != Vector3.zero)
            {
                Gizmos.DrawRay(pos, direction);

                Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) *
                                new Vector3(0, 0, 1);
                Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) *
                               new Vector3(0, 0, 1);
                Gizmos.DrawRay(pos + direction, right * arrowHeadLength);
                Gizmos.DrawRay(pos + direction, left * arrowHeadLength);
            }
            
            Gizmos.color = prevColor;
        }

        public static void DrawBox(Color color, Vector3 position, Vector3 scale)
        {
            Color prevColor = Gizmos.color;
            
            Gizmos.color = color;
            Gizmos.DrawWireCube(position, scale);
            Gizmos.color = prevColor;
        }
    }
}