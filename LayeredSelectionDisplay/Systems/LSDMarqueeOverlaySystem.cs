using Colossal.Logging;
using Colossal.Mathematics;
using Game;
using Game.Rendering;
using LayeredSelectionDisplay.Selection;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace LayeredSelectionDisplay.Systems
{
    [UpdateAfter(typeof(LSDMarqueeSelectionSystem))]
    /// <summary>
    /// Draws the active marquee rectangle using the game's HDRP overlay system.
    /// </summary>
    public partial class LSDMarqueeOverlaySystem : GameSystemBase
    {
        private OverlayRenderSystem m_OverlayRenderSystem;
        private LSDMarqueeSelectionSystem m_SelectionSystem;
        private ILog m_Log;

        protected override void OnCreate()
        {
            base.OnCreate();

            m_Log =
                LayeredSelectionDisplayMod.Instance?.Logger;

            m_OverlayRenderSystem =
                World.GetOrCreateSystemManaged<OverlayRenderSystem>();

            m_SelectionSystem =
                World.GetOrCreateSystemManaged<LSDMarqueeSelectionSystem>();

            m_Log?.Info(
                $"{nameof(LSDMarqueeOverlaySystem)}.{nameof(OnCreate)}");
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

            JobHandle dependencies;

            OverlayRenderSystem.Buffer buffer =
                m_OverlayRenderSystem.GetBuffer(
                    out dependencies);

            // Ensure the overlay buffers are not being read while writing.
            dependencies.Complete();

            Color color =
                new Color(
                    1f,
                    0f,
                    1f,
                    1f);

            const float lineWidth = 1f;

            buffer.DrawLine(
                color,
                CreateLine(quad.a, quad.b, height),
                lineWidth,
                cameraFacing: true);

            buffer.DrawLine(
                color,
                CreateLine(quad.b, quad.c, height),
                lineWidth,
                cameraFacing: true);

            buffer.DrawLine(
                color,
                CreateLine(quad.c, quad.d, height),
                lineWidth,
                cameraFacing: true);

            buffer.DrawLine(
                color,
                CreateLine(quad.d, quad.a, height),
                lineWidth,
                cameraFacing: true);

            // The lines were written synchronously on the main thread.
            m_OverlayRenderSystem.AddBufferWriter(
                default);
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
                    end.y),
            };
        }
    }
}