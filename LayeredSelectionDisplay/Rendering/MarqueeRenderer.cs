using UnityEngine;
using Unity.Entities;
using Colossal.Mathematics;
using LayeredSelectionDisplay.Systems;

namespace LayeredSelectionDisplay.Rendering
{
    public class MarqueeRenderer : MonoBehaviour
    {
        private Material m_Material;

        private void Awake()
        {
            // Simple built-in shader used for GL lines
            Shader shader = Shader.Find("Hidden/Internal-Colored");
            if (shader == null)
            {
                Debug.LogWarning("MarqueeRenderer: shader Hidden/Internal-Colored not found");
            }

            m_Material = new Material(shader ?? Shader.Find("Sprites/Default"))
            {
                hideFlags = HideFlags.HideAndDontSave
            };
            // alpha blending, no z write
            m_Material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            m_Material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            m_Material.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
            m_Material.SetInt("_ZWrite", 0);
        }

        private void OnDestroy()
        {
            if (m_Material != null)
            {
                Destroy(m_Material);
            }
        }

        private void OnPostRender()
        {
            // Try to obtain the ECS system from the default World
            var world = World.DefaultGameObjectInjectionWorld;
            if (world == null) return;

            var sys = world.GetExistingSystemManaged<LSDMarqueeSelectionSystem>();
            if (sys == null) return;

            if (!sys.TryGetCurrentQuad(out Quad2 quad)) return;

            if (m_Material == null) return;
            m_Material.SetPass(0);

            GL.PushMatrix();
            // Use current camera projection so lines match screen
            GL.LoadProjectionMatrix(Camera.current ? Camera.current.projectionMatrix : Camera.main.projectionMatrix);

            GL.Begin(GL.LINES);
            GL.Color(new Color(0f, 1f, 0f, 0.9f)); // vivid green

            // Convert Quad2 (float2.x = world x, float2.y = world z) to Vector3; draw at small Y above ground
            float y = 0.05f;
            Vector3 a = new Vector3(quad.a.x, y, quad.a.y);
            Vector3 b = new Vector3(quad.b.x, y, quad.b.y);
            Vector3 c = new Vector3(quad.c.x, y, quad.c.y);
            Vector3 d = new Vector3(quad.d.x, y, quad.d.y);

            DrawLine(a, b);
            DrawLine(b, c);
            DrawLine(c, d);
            DrawLine(d, a);

            GL.End();
            GL.PopMatrix();
        }

        private static void DrawLine(Vector3 from, Vector3 to)
        {
            GL.Vertex(from);
            GL.Vertex(to);
        }
    }
}