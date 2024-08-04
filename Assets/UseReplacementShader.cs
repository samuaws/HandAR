using UnityEngine;

public class UseReplacementShader : MonoBehaviour
{
    public Shader replacementShader;
    public Color clearColor = Color.black;

    private void OnEnable()
    {
        if (replacementShader != null)
        {
            Camera.main.SetReplacementShader(replacementShader, "RenderType");
            Camera.main.backgroundColor = clearColor;
            Camera.main.clearFlags = CameraClearFlags.SolidColor;
        }
    }

    private void OnDisable()
    {
        Camera.main.ResetReplacementShader();
    }
}
