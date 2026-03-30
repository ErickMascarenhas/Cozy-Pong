using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum RacketPart { Blade, Handle }

public class ColorPickerControl : MonoBehaviour
{
    [Header("Referencias")]
    public RacketColorManager racketManager;
    public SVImageControl svControl;
    [SerializeField] private RawImage hueImage, satValImage, outputImage;
    [SerializeField] private Slider hueSlider;
    [SerializeField] private TMP_InputField hexInputField;
    public float currentHue, currentSaturation, currentValue;
    private RacketPart currentPart = RacketPart.Blade;
    private Color pendingBladeColor;
    private Color pendingHandleColor;
    private Texture2D hueTexture, svTexture, outputTexture;
    private bool isUpdatingUI = false;
    //[SerializeField] MeshRenderer changeThisColor;

    private void Awake()
    {
        CreateHueImage();
        CreateSVImage();
        CreateOutputImage();
    }

    private void OnEnable()
    {
        if (racketManager != null)
        {
            pendingBladeColor = racketManager.bladeColor;
            pendingHandleColor = racketManager.handleColor;
            SelectBlade();
        }
    }

    public void SelectBlade()
    {
        currentPart = RacketPart.Blade;
        LoadColorIntoPicker(pendingBladeColor);
    }

    public void SelectHandle()
    {
        currentPart = RacketPart.Handle;
        LoadColorIntoPicker(pendingHandleColor);
    }

    private void LoadColorIntoPicker(Color c)
    {
        isUpdatingUI = true;
        Color.RGBToHSV(c, out currentHue, out currentSaturation, out currentValue);
        if (hueSlider != null) hueSlider.value = currentHue;
        if (svControl != null) svControl.SetCursorPosition(currentSaturation, currentValue);
        UpdateSVImageInternal();
        UpdateOutputImage();
        isUpdatingUI = false;
    }

    public void UpdateSVImage()
    {
        if (isUpdatingUI) return;
        currentHue = hueSlider.value;
        UpdateSVImageInternal();
        UpdateOutputImage();
    }

    private void CreateHueImage()
    {
        hueTexture = new Texture2D(1, 16);
        hueTexture.wrapMode = TextureWrapMode.Clamp;
        //hueTexture.name = "HueTexture";
        for (int i = 0; i < hueTexture.height; i++) hueTexture.SetPixel(0, i, Color.HSVToRGB((float)i / hueTexture.height, 1f, 1f));
        hueTexture.Apply();
        //currentHue = 0;
        hueImage.texture = hueTexture;
    }

    private void UpdateSVImageInternal() // CreateSVImage?
    {
        //svTexture = new Texture2D(16, 16);
        //svTexture.wrapMode = TextureWrapMode.Clamp;
        //svTexture.name = "SatValTexture";
        for (int y = 0; y < svTexture.height; y++)
        {
            for (int x = 0; x < svTexture.width; x++)
            {
                svTexture.SetPixel(x, y, Color.HSVToRGB(currentHue, (float)x / svTexture.width, (float)y / svTexture.height));
            }
        }
        svTexture.Apply();
        //currentSaturation = 0;
        //currentValue = 0;
        satValImage.texture = svTexture;
    }

    private void CreateOutputImage()
    {
        outputTexture = new Texture2D(1, 16);
        outputTexture.wrapMode = TextureWrapMode.Clamp;
        //outputTexture.name = "OutputTexture";
        //Color currentColor = Color.HSVToRGB(currentHue, currentSaturation, currentValue);
        //for (int i = 0; i < outputTexture.height; i++) outputTexture.SetPixel(0, i, currentColor);
        //outputTexture.Apply();
        //outputImage.texture = outputTexture;
    }

    private void UpdateOutputImage()
    {
        Color currentColor = Color.HSVToRGB(currentHue, currentSaturation, currentValue);
        for (int i = 0; i < outputTexture.height; i++) outputTexture.SetPixel(0, i, currentColor);
        outputTexture.Apply();
        //changeThisColor.material.SetColor("_BaseColor",  currentColor);
        //changeThis.GetComponent<MeshRenderer>().material.color = currentColor;
        outputImage.texture = outputTexture;
        if (currentPart == RacketPart.Blade) pendingBladeColor = currentColor;
        else pendingHandleColor = currentColor;
        if (!isUpdatingUI && hexInputField != null) hexInputField.text = "#" + ColorUtility.ToHtmlStringRGB(currentColor);
    }

    private void CreateSVImage()
    {
        svTexture = new Texture2D(16, 16);
        svTexture.wrapMode = TextureWrapMode.Clamp;
    }

    public void SetSV(float s, float v)
    {
        currentSaturation = s;
        currentValue = v;
        UpdateOutputImage();
    }

    public void OnHexInputChanged(string hex)
    {
        if (isUpdatingUI) return;
        if (!hex.StartsWith("#")) hex = "#" + hex;
        if (ColorUtility.TryParseHtmlString(hex, out Color newColor)) LoadColorIntoPicker(newColor);
    }

    public void ResetToDefault()
    {
        if (currentPart == RacketPart.Blade) LoadColorIntoPicker(racketManager.defaultBladeColor);
        else LoadColorIntoPicker(racketManager.defaultHandleColor);
    }

    public void ApplyAndClose()
    {
        PlayerPrefs.SetString("RacketBlade", "#" + ColorUtility.ToHtmlStringRGB(pendingBladeColor));
        PlayerPrefs.SetString("RacketHandle", "#" + ColorUtility.ToHtmlStringRGB(pendingHandleColor));
        PlayerPrefs.Save();
        if (racketManager != null)
        {
            racketManager.bladeColor = pendingBladeColor;
            racketManager.handleColor = pendingHandleColor;
            RacketColorManager[] allRacketsInScene = FindObjectsByType<RacketColorManager>(FindObjectsSortMode.None);
            foreach (RacketColorManager racket in allRacketsInScene)
            {
                racket.LoadColors();
                racket.ApplyColors();
            }
        }
        //gameObject.SetActive(false); // fecha
    }

    //public void UpdateSVImage()
    //{
    //    currentHue = hueSlider.value;
    //    for (int y = 0; y < svTexture.height; y++)
    //    {
    //        for (int x = 0; x < svTexture.width; x++)
    //        {
    //            svTexture.SetPixel(x, y, Color.HSVToRGB(currentHue, (float)x / svTexture.width, (float)y / svTexture.height));
    //        }
    //    }
    //    svTexture.Apply();
    //    UpdateOutputImage();
    //}
}