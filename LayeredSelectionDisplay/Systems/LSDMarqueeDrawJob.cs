namespace LayeredSelectionDisplay.Systems
{
    using Colossal.Mathematics;

    using Game.Rendering;

    using Unity.Jobs;
    using Unity.Mathematics;

    using UnityEngine;

    /// <summary>
    /// Writes the LSD marquee geometry into the game's
    /// OverlayRenderSystem buffer.
    ///
    /// This job intentionally contains no managed state and performs
    /// no camera access. All data required for rendering is copied into
    /// the job before it is scheduled.
    /// </summary>
    internal struct LSDMarqueeDrawJob : IJob
    {
        /*
         * OverlayRenderSystem.Buffer is the same type used by
         * Move It's DrawOverlaysJob.
         */
        public OverlayRenderSystem.Buffer Buffer;

        public Quad2 Quad;

        public float Height;

        public Color Color;

        public float LineWidth;

        public void Execute()
        {
            /*
             * Move It draws its marquee as four LineSimple calls.
             *
             * We use the same basic approach here.
             *
             * The important difference from the original LSD code is
             * that these calls execute as part of the job dependency
             * chain registered with OverlayRenderSystem.
             */

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