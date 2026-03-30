using UnityEngine;

public class RacketColorManager : MonoBehaviour
{
    [Header("Referencias")]
    public MeshRenderer racketRenderer;
    public Texture2D originalTexture;
    public Color bladeColor = Color.red;
    public Color handleColor = Color.white;
    public Color defaultBladeColor = Color.red;
    public Color defaultHandleColor = Color.white;
    public string shaderTextureName = "";
    public Material trailMaterial;

    private Texture2D _customTexture;
    private Material _customMaterial;
    private Color32[] _originalPixels;

    private void Awake()
    {
        if (racketRenderer == null || originalTexture == null) return;
        _customTexture = new Texture2D(originalTexture.width, originalTexture.height, TextureFormat.RGBA32, false);
        _customTexture.filterMode = originalTexture.filterMode;
        _originalPixels = originalTexture.GetPixels32();
        _customMaterial = new Material(racketRenderer.material);
        _customMaterial.mainTexture = _customTexture;
        if (!string.IsNullOrEmpty(shaderTextureName) && _customMaterial.HasProperty(shaderTextureName)) _customMaterial.SetTexture(shaderTextureName, _customTexture);
        else
        {
            if (_customMaterial.HasProperty("_BaseMap")) _customMaterial.SetTexture("_BaseMap", _customTexture);
            if (_customMaterial.HasProperty("_MainTex")) _customMaterial.SetTexture("_MainTex", _customTexture);
            if (_customMaterial.HasProperty("_AlbedoMap")) _customMaterial.SetTexture("_AlbedoMap", _customTexture);
            if (_customMaterial.HasProperty("_BaseColorMap")) _customMaterial.SetTexture("_BaseColorMap", _customTexture);
        }
        racketRenderer.material = _customMaterial;
        LoadColors();
        ApplyColors();
    }

    public void ApplyColors()
    {
        if (_originalPixels == null || _customTexture == null) return;
        Color32[] newPixels = new Color32[_originalPixels.Length];
        Color32 bColor = bladeColor;
        Color32 hColor = handleColor;
        for (int i = 0; i < _originalPixels.Length; i++)
        {
            float maskValue = _originalPixels[i].g / 255f;
            newPixels[i] = Color32.Lerp(bColor, hColor, maskValue);
        }
        _customTexture.SetPixels32(newPixels);
        _customTexture.Apply();
        if (trailMaterial != null)
        {
            if (trailMaterial.HasProperty("_Color")) trailMaterial.SetColor("_Color", bladeColor);
            else if (trailMaterial.HasProperty("_BaseColor")) trailMaterial.SetColor("_BaseColor", bladeColor);
        }
    }

    public void SaveColors()
    {
        PlayerPrefs.SetString("RacketBlade", "#" + ColorUtility.ToHtmlStringRGB(bladeColor));
        PlayerPrefs.SetString("RacketHandle", "#" + ColorUtility.ToHtmlStringRGB(handleColor));
        PlayerPrefs.Save();
    }

    public void LoadColors()
    {
        string savedBlade = PlayerPrefs.GetString("RacketBlade", "#" + ColorUtility.ToHtmlStringRGB(defaultBladeColor));
        string savedHandle = PlayerPrefs.GetString("RacketHandle", "#" + ColorUtility.ToHtmlStringRGB(defaultHandleColor));
        if (ColorUtility.TryParseHtmlString(savedBlade, out Color bColor)) bladeColor = bColor;
        if (ColorUtility.TryParseHtmlString(savedHandle, out Color hColor)) handleColor = hColor;
    }
}