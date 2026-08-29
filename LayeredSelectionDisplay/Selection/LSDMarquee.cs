namespace LayeredSelectionDisplay.Selection
{
    using Colossal.Mathematics;

    using Unity.Mathematics;

    /// <summary>
    /// Represents the currently active marquee selection.
    ///
    /// The marquee is defined on the XZ ground plane and oriented
    /// relative to the camera yaw.
    /// </summary>
    internal sealed class LSDMarquee
    {
        private readonly float3 m_StartPosition;

        private Quad2 m_Quad;

        public float3 StartPosition =>
            m_StartPosition;

        public Quad2 Quad =>
            m_Quad;

        public LSDMarquee(
            float3 startPosition)
        {
            m_StartPosition =
                startPosition;

            float2 start =
                new float2(
                    startPosition.x,
                    startPosition.z);

            m_Quad =
                new Quad2(
                    start,
                    start,
                    start,
                    start);
        }

        public Bounds2 Bounds
        {
            get
            {
                float2 min =
                    math.min(
                        math.min(
                            m_Quad.a,
                            m_Quad.b),
                        math.min(
                            m_Quad.c,
                            m_Quad.d));

                float2 max =
                    math.max(
                        math.max(
                            m_Quad.a,
                            m_Quad.b),
                        math.max(
                            m_Quad.c,
                            m_Quad.d));

                return new Bounds2(
                    min,
                    max);
            }
        }

        public void Update(
            float3 currentPosition,
            float cameraYaw)
        {
            float2 start =
                new float2(
                    m_StartPosition.x,
                    m_StartPosition.z);

            float2 end =
                new float2(
                    currentPosition.x,
                    currentPosition.z);

            /*
             * Camera-relative axes.
             *
             * CS2's camera yaw is converted to a ground-plane
             * forward vector.
             */
            float2 forward =
                new float2(
                    math.sin(cameraYaw),
                    math.cos(cameraYaw));

            float2 right =
                new float2(
                    forward.y,
                    -forward.x);

            float2 delta =
                end - start;

            float forwardDistance =
                math.dot(
                    delta,
                    forward);

            float rightDistance =
                math.dot(
                    delta,
                    right);

            float2 a =
                start;

            float2 b =
                start +
                forward *
                forwardDistance;

            float2 c =
                start +
                forward *
                forwardDistance +
                right *
                rightDistance;

            float2 d =
                start +
                right *
                rightDistance;

            m_Quad =
                new Quad2(
                    a,
                    b,
                    c,
                    d);
        }
    }
}