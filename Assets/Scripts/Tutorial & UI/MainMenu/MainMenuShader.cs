using UnityEngine;

public class MainMenuShader : MonoBehaviour
{
    private float _unscaledTime;

    /// <summary>
    /// Called every frame. Updates shader
    /// </summary>
    private void Update()
    {
        _unscaledTime += .001f;
        Shader.SetGlobalFloat("_UnscaledTime", _unscaledTime);
    }
}
