namespace LayeredSelectionDisplay.Systems
{
    using Colossal.Mathematics;
    using Game.Rendering;
    using Unity.Jobs;
    using Unity.Mathematics;
    using UnityEngine;

    internal struct LSDMarqueeDrawJob : IJob
    {
        public OverlayRenderSystem.Buffer Buffer;

        public Quad2 Quad;

        public float Height;

        public Color Color;

        public float LineWidth;

        public void Execute()
        {
            Buffer.DrawLine(
                Color,
                CreateLine(
                    Quad.a,
                    Quad.b,
                    Height),
                LineWidth,
                cameraFacing: false);

            Buffer.DrawLine(
                Color,
                CreateLine(
                    Quad.b,
                    Quad.c,
                    Height),
                LineWidth,
                cameraFacing: false);

            Buffer.DrawLine(
                Color,
                CreateLine(
                    Quad.c,
                    Quad.d,
                    Height),
                LineWidth,
                cameraFacing: false);

            Buffer.DrawLine(
                Color,
                CreateLine(
                    Quad.d,
                    Quad.a,
                    Height),
                LineWidth,
                cameraFacing: false);
        }

        private static Line3.Segment CreateLine(
            float2 start,
            float2 end,
            float height)
        {
            return new Line3.Segment
            {
                a = new float3(
                    start.x,
                    height,
                    start.y),

                b = new float3(
                    end.x,
                    height,
                    end.y)
            };
        }
    }
}