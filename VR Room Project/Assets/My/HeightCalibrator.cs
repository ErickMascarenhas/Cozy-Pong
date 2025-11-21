using System.Diagnostics;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HeightCalibrator : MonoBehaviour
{
    public TMP_InputField heightInput;
    public Button confirmButton;
    public GameObject cameraTarget; // camera offset

    void Start()
    {
        if (heightInput != null)
        {
            heightInput.onValueChanged.AddListener(ValidateInput);
            ValidateInput(heightInput.text);
        }
    }
    public void ValidateInput(string currentText)
    {
        if (confirmButton == null) return;
        string normalizedText = currentText.Replace(',', '.');
        bool isValid = float.TryParse(normalizedText, NumberStyles.Any, CultureInfo.InvariantCulture, out float result);
        if (isValid && (result <= 0 || result > 300)) isValid = false;
        confirmButton.interactable = isValid;
    }

    public void ApplyHeight()
    {
        if (heightInput == null || cameraTarget == null) return;
        string normalizedText = heightInput.text.Replace(',', '.');
        if (float.TryParse(normalizedText, NumberStyles.Any, CultureInfo.InvariantCulture, out float heightValue))
        {
            if (heightValue > 3.0f) heightValue /= 100f;
            Vector3 newPosition = cameraTarget.transform.localPosition;
            newPosition.y = heightValue;
            cameraTarget.transform.localPosition = newPosition;
        }
    }
}