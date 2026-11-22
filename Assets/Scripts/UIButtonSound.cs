using UnityEngine;
using UnityEngine.EventSystems;

public class UIButtonSound : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    public enum ButtonSoundType { Normal, Back, Success, Error }
    [SerializeField] private ButtonSoundType soundType = ButtonSoundType.Normal;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayHover();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (AudioManager.Instance == null) return;

        switch (soundType)
        {
            case ButtonSoundType.Back: AudioManager.Instance.PlayBack(); break;
            case ButtonSoundType.Success: AudioManager.Instance.PlaySuccess(); break;
            case ButtonSoundType.Error: AudioManager.Instance.PlayError(); break;
            default: AudioManager.Instance.PlayClick(); break;
        }
    }
}