namespace LayeredSelectionDisplay.Systems
{
    using System;
    using Game;
    using Game.Rendering;
    using LayeredSelectionDisplay.Selection;
    using Unity.Entities;
    using Unity.Jobs;
    using UnityEngine;

    [UpdateAfter(typeof(LSDMarqueeSelectionSystem))]
    public partial class LSDMarqueeOverlaySystem :
        GameSystemBase
    {
        private OverlayRenderSystem m_OverlayRenderSystem;

        private LSDMarqueeSelectionSystem m_SelectionSystem;

        private static readonly Color s_MarqueeColor =
            Color.magenta;

        private const float k_LineWidth = 1f;

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

            /*
             * Nothing to draw.
             */
            if (!m_SelectionSystem.TryGetMarquee(
                    out Colossal.Mathematics.Quad2 quad,
                    out float height))
            {
                return;
            }

            try
            {
                /*
                 * IMPORTANT:
                 *
                 * GetBuffer gives us both:
                 *
                 *   1. The overlay buffer.
                 *   2. A JobHandle representing work that must finish
                 *      before our job writes to that buffer.
                 *
                 * We DO NOT Complete this handle here.
                 */
                JobHandle bufferDependency;

                OverlayRenderSystem.Buffer buffer =
                    m_OverlayRenderSystem.GetBuffer(
                        out bufferDependency);

                /*
                 * Create the job using only blittable/simple data.
                 *
                 * No Camera access.
                 * No managed objects.
                 * No ToolSystem.
                 * No EntityManager.
                 */
                LSDMarqueeDrawJob drawJob =
                    new LSDMarqueeDrawJob
                    {
                        Buffer = buffer,

                        Quad = quad,

                        Height = height,

                        Color =
                            s_MarqueeColor,

                        LineWidth =
                            k_LineWidth
                    };

                /*
                 * Combine our system's dependency with the dependency
                 * owned by OverlayRenderSystem.
                 *
                 * This is the key synchronization step.
                 */
                JobHandle dependency =
                    JobHandle.CombineDependencies(
                        Dependency,
                        bufferDependency);

                /*
                 * Schedule the actual overlay write.
                 *
                 * There is no Complete() here.
                 *
                 * OverlayRenderSystem will be informed about the writer
                 * below.
                 */
                JobHandle drawHandle =
                    drawJob.Schedule(
                        dependency);

                /*
                 * Tell OverlayRenderSystem that our job writes into
                 * its buffer.
                 *
                 * This allows the game's rendering pipeline to respect
                 * our job when consuming the buffer.
                 */
                m_OverlayRenderSystem.AddBufferWriter(
                    drawHandle);

                /*
                 * Continue our own dependency chain.
                 */
                Dependency =
                    drawHandle;
            }
            catch (Exception ex)
            {
                LayeredSelectionDisplayMod.Instance?
                    .Logger?
                    .Error(
                        ex,
                        $"{nameof(LSDMarqueeOverlaySystem)} overlay job failed.");
            }
        }
    }
}