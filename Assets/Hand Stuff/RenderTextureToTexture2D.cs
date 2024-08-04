using UnityEngine;
using Klak.TestTools;
using MediaPipe.HandPose;

public class RenderTextureToTexture2D : MonoBehaviour
{
    public RenderTexture renderTexture;
    public ImageSourceEdited imageSource;

    // Call this method to convert RenderTexture to Texture2D
    public Texture2D ConvertRenderTextureToTexture2D(RenderTexture rt)
    {
        // Create a new Texture2D with the same dimensions as the RenderTexture
        Texture2D texture = new Texture2D(rt.width, rt.height, TextureFormat.RGBA32, false);

        // Remember the currently active RenderTexture
        RenderTexture currentActiveRT = RenderTexture.active;

        // Set the RenderTexture active
        RenderTexture.active = rt;

        // Read pixels from the RenderTexture to the Texture2D
        texture.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        texture.Apply();

        // Restore the active RenderTexture
        RenderTexture.active = currentActiveRT;

        return texture;
    }

    // Example usage
    void Start()
    {
        if (renderTexture != null)
        {
            Texture2D texture2D = ConvertRenderTextureToTexture2D(renderTexture);
            imageSource._texture = texture2D;
        }
    }
}
