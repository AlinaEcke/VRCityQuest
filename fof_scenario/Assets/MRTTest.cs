using UnityEngine;
using System.Collections;

public class MRTTest : MonoBehaviour {
    public float velocityMultiplier = 2.0f;
    public float blurStrength = 12.0f;

    protected bool isSupported = false;
    protected Matrix4x4 prevViewProj;
    protected Matrix4x4 matrix;
    protected Material blurMat;

    protected RenderTexture mrt0;
    protected RenderTexture mrt1;
    protected RenderBuffer[] colors = new RenderBuffer[2];

    protected bool CheckSupport(Camera camera, bool needDepth)
    {
        if (!SystemInfo.supportsImageEffects || !SystemInfo.supportsRenderTextures)
            return false;

        if (needDepth && !SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.Depth))
            return false;

        if (needDepth)
            camera.depthTextureMode = DepthTextureMode.Depth;

        return true;
    }

    protected Material CreateMaterial(Shader shader, Material material)
    {
        if (null == shader)
            return null;

        if (material != null && material.shader == shader && shader.isSupported)
            return material;

        if (!shader.isSupported)
            return null;

        material = new Material(shader);
        material.hideFlags = HideFlags.DontSave;

        return material;
    }

    protected bool CheckResources()
    {
        if (!this.CheckSupport(GetComponent<Camera>(), true))
            return false;

        if (SystemInfo.supportedRenderTargetCount < 2)
            return false;

        blurMat = this.CreateMaterial(Shader.Find("Test/MRT"), blurMat);
        if (null == blurMat)
            return false;

        return true;
    }

    protected void DrawFsQuadRecPos(Camera camera)
    {
        if (camera.orthographic)
            return;

        float t = Mathf.Tan(camera.fieldOfView * 0.5f * Mathf.Deg2Rad),
              x = t * camera.aspect * camera.farClipPlane,
              y = t * camera.farClipPlane,
              z = camera.farClipPlane;

        GL.PushMatrix();
        GL.LoadIdentity();

        //GL.LoadPixelMatrix(0.0f, Screen.width, 0.0f, Screen.height);
        GL.LoadProjectionMatrix(Matrix4x4.identity);
        //GL.LoadPixelMatrix(-1.0f, 1.0f, -1.0f, 1.0f);

        GL.Begin(GL.QUADS);

        GL.TexCoord3(-x, -y, -z);
        GL.Vertex3(-1.0f, -1.0f, 0.0f);
        GL.TexCoord3(x, -y, -z);
        GL.Vertex3(1.0f, -1.0f, 0.0f);
        GL.TexCoord3(x, y, -z);
        GL.Vertex3(1.0f, 1.0f, 0.0f);
        GL.TexCoord3(-x, y, -z);
        GL.Vertex3(-1.0f, 1.0f, 0.0f);

        GL.End();

        GL.PopMatrix();
    }

    void Awake()
    {
        mrt0 = new RenderTexture(Screen.width, Screen.height, 24, RenderTextureFormat.Default, RenderTextureReadWrite.Default);
        mrt1 = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Default);

        colors[0] = mrt0.colorBuffer;
        colors[1] = mrt1.colorBuffer;
    }

    void LateUpdate()
    {
        matrix = prevViewProj * GetComponent<Camera>().cameraToWorldMatrix;
        prevViewProj = GetComponent<Camera>().projectionMatrix * GetComponent<Camera>().worldToCameraMatrix;
    }

    void OnGUI()
    {
        int w = Screen.width >> 2, h = Screen.height >> 2;
        GUI.DrawTexture(new Rect(0, 0, w, h), mrt0);
        GUI.DrawTexture(new Rect(0, h, w, h), mrt1);
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (!isSupported && !this.CheckResources())
        {
            Graphics.Blit(source, destination);
            return;
        }

        blurMat.SetMatrix("_PrevViewProj", matrix);
        blurMat.SetFloat("_BlurStrength", blurStrength);
        blurMat.SetFloat("_VelocityMult", velocityMultiplier);
        blurMat.SetTexture("_MainTex", source);

        Graphics.SetRenderTarget(colors, mrt0.depthBuffer);
        blurMat.SetPass(0);
        this.DrawFsQuadRecPos(GetComponent<Camera>());
        Graphics.SetRenderTarget(null);

        Graphics.Blit(source, destination);
    }
}
