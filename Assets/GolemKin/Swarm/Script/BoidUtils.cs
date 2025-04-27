using Unity.Collections;
using Unity.Mathematics;

namespace GolemKin.Swarm
{
    public static class BoidUtils
    {
        public static float3 ComputeCenterOfMass(NativeArray<float3> positions, int excludeIndex)
        {
            float3 center = float3.zero;
            int count = 0;

            for (int i = 0; i < positions.Length; i++)
            {
                if (i == excludeIndex) continue;
                center += positions[i];
                count++;
            }

            return count > 0 ? center / count : center;
        }

        public static float3 GetBoundarySteeringForce(float3 position, float3 boundarySize)
        {
            float3 steer = float3.zero;

            if (position.x > boundarySize.x / 2) steer.x = -1;
            else if (position.x < -boundarySize.x / 2) steer.x = 1;

            if (position.y > boundarySize.y / 2) steer.y = -1;
            else if (position.y < -boundarySize.y / 2) steer.y = 1;

            if (position.z > boundarySize.z / 2) steer.z = -1;
            else if (position.z < -boundarySize.z / 2) steer.z = 1;

            return steer;
        }
    }
}