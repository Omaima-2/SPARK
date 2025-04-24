using UnityEngine;
using UnityEngine.UI;

public class TogglePasswordVisibility : MonoBehaviour
{
    public InputField passwordField;
    public Sprite eyeOpen;
    public Sprite eyeClosed;
    public Image eyeIcon;

    private bool isPasswordVisible = false;

    public void ToggleVisibility()
    {
        isPasswordVisible = !isPasswordVisible;
        passwordField.contentType = isPasswordVisible ? InputField.ContentType.Standard : InputField.ContentType.Password;
        passwordField.ForceLabelUpdate(); // Refresh the display
        eyeIcon.sprite = isPasswordVisible ? eyeOpen : eyeClosed;
    }
}
