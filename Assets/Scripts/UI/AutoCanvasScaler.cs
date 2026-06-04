using UnityEngine;
using UnityEngine.UI;

namespace Pie.UI
{
    [RequireComponent(typeof(CanvasScaler))]
    public class AutoCanvasScaler : MonoBehaviour
    {
        [SerializeField] private bool inverted = false;
        private Vector2 referenceResolution = new Vector2(720, 1280);
        private CanvasScaler scaler;

        private void Awake()
        {
            scaler = GetComponent<CanvasScaler>();
            referenceResolution = scaler.referenceResolution;
            UpdateMatch();
        }

        private void OnRectTransformDimensionsChange() => UpdateMatch();

        private void UpdateMatch()
        {
            float screenRatio = (float)Screen.width / Screen.height;
            float referenceRatio = referenceResolution.x / referenceResolution.y;
            if (scaler != null)
            {
                if (screenRatio > referenceRatio) scaler.matchWidthOrHeight = inverted ? 0 : 1;
                else scaler.matchWidthOrHeight = inverted ? 1 : 0;
            }
        }
    }
}