namespace LayeredSelectionDisplay.Systems
{
    using Colossal.Mathematics;
    using Game;
    using Game.Rendering;
    using LayeredSelectionDisplay.Selection;
    using System;
    using Unity.Entities;
    using Unity.Jobs;
    using Unity.Mathematics;
    using UnityEngine;

    [UpdateAfter(typeof(LSDMarqueeSelectionSystem))]
    public partial class LSDMarqueeOverlaySystem : GameSystemBase
    {
        private OverlayRenderSystem m_OverlayRenderSystem;

        private LSDMarqueeSelectionSystem m_SelectionSystem;

        private bool m_LoggedBufferSynchronization;

        protected override void OnCreate()
        {
            base.OnCreate();

            m_OverlayRenderSystem =
                World.GetOrCreateSystemManaged<
                    OverlayRenderSystem>();

            m_SelectionSystem =
                World.GetOrCreateSystemManaged<
                    LSDMarqueeSelectionSystem>();
        }

        protected override void OnUpdate()
        {
            if (m_OverlayRenderSystem == null ||
                m_SelectionSystem == null)
            {
                return;
            }

            if (!m_SelectionSystem.TryGetMarquee(
                    out Quad2 quad,
                    out float height))
            {
                return;
            }

            try
            {
                /*
                 * IMPORTANT:
                 *
                 * OverlayRenderSystem.GetBuffer() returns a JobHandle
                 * representing work which is currently using the shared
                 * overlay buffer.
                 *
                 * We MUST wait for that work before writing to the buffer.
                 *
                 * Your previous implementation discarded this handle:
                 *
                 *     GetBuffer(out _)
                 *
                 * which can cause synchronization stalls/races with the
                 * game's own overlay rendering.
                 */
                OverlayRenderSystem.Buffer buffer =
                    m_OverlayRenderSystem.GetBuffer(
                        out JobHandle bufferJobHandle);

                bufferJobHandle.Complete();

                DrawMarquee(
                    buffer,
                    quad,
                    height);
            }
            catch (Exception ex)
            {
                LayeredSelectionDisplayMod.Instance?
                    .Logger?
                    .Error(
                        ex,
                        $"{nameof(LSDMarqueeOverlaySystem)} rendering failed.");
            }
        }

        private static void DrawMarquee(
            OverlayRenderSystem.Buffer buffer,
            Quad2 quad,
            float height)
        {
            const float lineWidth = 1f;

            Color color =
                Color.magenta;

            buffer.DrawLine(
                color,
                CreateLine(
                    quad.a,
                    quad.b,
                    height),
                lineWidth,
                cameraFacing: true);

            buffer.DrawLine(
                color,
                CreateLine(
                    quad.b,
                    quad.c,
                    height),
                lineWidth,
                cameraFacing: true);

            buffer.DrawLine(
                color,
                CreateLine(
                    quad.c,
                    quad.d,
                    height),
                lineWidth,
                cameraFacing: true);

            buffer.DrawLine(
                color,
                CreateLine(
                    quad.d,
                    quad.a,
                    height),
                lineWidth,
                cameraFacing: true);
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