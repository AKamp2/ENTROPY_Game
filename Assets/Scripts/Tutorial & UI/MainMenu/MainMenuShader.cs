using UnityEngine;

public class MainMenuShader : MonoBehaviour
{
    private float _unscaledTime;

    /// <summary>
    /// Called every frame. Updates shader
    /// </summary>
    private void Update()
    {
        _unscaledTime += Time.unscaledTime / 4800;
        if (_unscaledTime >= 10)
        {
            _unscaledTime = 0;
        }
        Shader.SetGlobalFloat("_UnscaledTime", _unscaledTime);
    }
}
