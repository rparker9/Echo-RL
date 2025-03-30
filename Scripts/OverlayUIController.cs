using TMPro;
using UnityEngine;

namespace EchoRL
{
    public class OverlayUIController : MonoBehaviour
    {
        [Header("UI References")]
        public TMP_Dropdown overlayDropdown;

        [Header("Target Visualizer")]
        public GridVisualizer gridVisualizer;

        private void Start()
        {
            if (gridVisualizer == null)
            {
                Debug.LogError("OverlayUIController: GridVisualizer reference not assigned.");
                return;
            }

            if (overlayDropdown == null)
            {
                Debug.LogError("OverlayUIController: TMP_Dropdown not assigned.");
                return;
            }

            overlayDropdown.onValueChanged.AddListener(OnOverlayModeChanged);
        }

        private void OnOverlayModeChanged(int selectedIndex)
        {
            gridVisualizer.SetOverlayMode((OverlayMode)selectedIndex);
        }
    }
}
