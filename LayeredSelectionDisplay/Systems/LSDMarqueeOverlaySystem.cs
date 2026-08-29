namespace LayeredSelectionDisplay.Systems
{
    using Game;
    using Game.Rendering;

    using LayeredSelectionDisplay.Selection;

    using Unity.Entities;
    using UnityEngine;

    [UpdateAfter(typeof(LSDMarqueeSelectionSystem))]
    public partial class LSDMarqueeOverlaySystem :
        GameSystemBase
    {
        private OverlayRenderSystem
            m_OverlayRenderSystem;

        private LSDMarqueeSelectionSystem
            m_SelectionSystem;

        private static readonly Color
            s_Color = Color.magenta;

        private const float
            k_LineWidth = 1f;

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
                    out Colossal.Mathematics.Quad2 quad,
                    out float height))
            {
                return;
            }

            OverlayRenderSystem.Buffer buffer =
                m_OverlayRenderSystem.GetBuffer(
                    out _);

            buffer.DrawLine(
                s_Color,
                CreateLine(
                    quad.a,
                    quad.b,
                    height),
                k_LineWidth,
                cameraFacing: true);

            buffer.DrawLine(
                s_Color,
                CreateLine(
                    quad.b,
                    quad.c,
                    height),
                k_LineWidth,
                cameraFacing: true);

            buffer.DrawLine(
                s_Color,
                CreateLine(
                    quad.c,
                    quad.d,
                    height),
                k_LineWidth,
                cameraFacing: true);

            buffer.DrawLine(
                s_Color,
                CreateLine(
                    quad.d,
                    quad.a,
                    height),
                k_LineWidth,
                cameraFacing: true);
        }

        private static
            Colossal.Mathematics.Line3.Segment CreateLine(
                Unity.Mathematics.float2 start,
                Unity.Mathematics.float2 end,
                float height)
        {
            return new Colossal.Mathematics.Line3.Segment
            {
                a =
                    new Unity.Mathematics.float3(
                        start.x,
                        height,
                        start.y),

                b =
                    new Unity.Mathematics.float3(
                        end.x,
                        height,
                        end.y)
            };
        }
    }
}