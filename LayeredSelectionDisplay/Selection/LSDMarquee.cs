namespace LayeredSelectionDisplay.Selection
{
    using Colossal.Mathematics;
    using Unity.Mathematics;

    internal sealed class LSDMarquee
    {
        private readonly float3 m_StartPosition;

        public Quad2 Quad { get; private set; }

        public float3 StartPosition => m_StartPosition;

        public LSDMarquee(float3 startPosition)
        {
            m_StartPosition = startPosition;

            float2 start = new float2(startPosition.x, startPosition.z);

            Quad = new Quad2(start, start, start, start);
        }

        public Bounds2 Bounds
        {
            get
            {
                float2 min = math.min(math.min(Quad.a, Quad.b), math.min(Quad.c, Quad.d));

                float2 max = math.max(math.max(Quad.a, Quad.b), math.max(Quad.c, Quad.d));

                return new Bounds2(min, max);
            }
        }

        public void Update(float3 currentPosition, float cameraYaw)
        {
            float2 start = new float2(m_StartPosition.x, m_StartPosition.z);

            float2 end = new float2(currentPosition.x, currentPosition.z);

            float2 forward = new float2(math.sin(cameraYaw), math.cos(cameraYaw));

            float2 right = new float2(forward.y, -forward.x);

            float2 delta = end - start;

            float forwardDistance = math.dot(delta, forward);

            float rightDistance = math.dot(delta, right);

            float2 a = start;

            float2 b = start + forward * forwardDistance;

            float2 c = start + forward * forwardDistance + right * rightDistance;

            float2 d = start + right * rightDistance;

            Quad = new Quad2(a, b, c, d);

        }
    }
}