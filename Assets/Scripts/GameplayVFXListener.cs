using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GameplayVFXListener : MonoBehaviour
{
    [SerializeField] private Image screenFlashOverlay;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float shakeMagnitude = 0.2f;
    [SerializeField] private float shakeDuration = 0.2f;

    private Vector3 originalCamPos;
    private Coroutine flashRoutine;
    private Coroutine shakeRoutine;

    private void Start()
    {
        if (cameraTransform != null) originalCamPos = cameraTransform.localPosition;
        if (screenFlashOverlay != null) screenFlashOverlay.color = new Color(1, 0, 0, 0);
    }

    private void OnEnable()
    {
        if (SurvivalGameEvents.Instance == null) return;
        SurvivalGameEvents.Instance.OnDamaged += HandleDamaged;
        SurvivalGameEvents.Instance.OnHealed += HandleHealed;
    }

    private void OnDisable()
    {
        if (SurvivalGameEvents.Instance == null) return;
        SurvivalGameEvents.Instance.OnDamaged -= HandleDamaged;
        SurvivalGameEvents.Instance.OnHealed -= HandleHealed;
    }

    private void HandleDamaged(int cur, int max)
    {
        TriggerFlash(new Color(0.8f, 0f, 0f, 0.45f));
        if (cameraTransform != null)
        {
            if (shakeRoutine != null) StopCoroutine(shakeRoutine);
            shakeRoutine = StartCoroutine(CameraShakeRoutine());
        }
    }

    private void HandleHealed(int cur, int max)
    {
        TriggerFlash(new Color(0f, 0.7f, 0.2f, 0.35f));
    }

    private void TriggerFlash(Color flashColor)
    {
        if (screenFlashOverlay == null) return;
        if (flashRoutine != null) StopCoroutine(flashRoutine);
        flashRoutine = StartCoroutine(FlashRoutine(flashColor));
    }

    private IEnumerator FlashRoutine(Color startColor)
    {
        screenFlashOverlay.color = startColor;
        float elapsed = 0f;
        float duration = 0.3f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float a = Mathf.Lerp(startColor.a, 0f, elapsed / duration);
            screenFlashOverlay.color = new Color(startColor.r, startColor.g, startColor.b, a);
            yield return null;
        }
        screenFlashOverlay.color = new Color(startColor.r, startColor.g, startColor.b, 0f);
    }

    private IEnumerator CameraShakeRoutine()
    {
        float elapsed = 0f;
        while (elapsed < shakeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            cameraTransform.localPosition = originalCamPos + (Vector3)Random.insideUnitCircle * shakeMagnitude;
            yield return null;
        }
        cameraTransform.localPosition = originalCamPos;
    }
}