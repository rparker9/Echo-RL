using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Pixel-perfect camera controller that supports click-drag panning and zooming
/// using scroll wheel by adjusting the assetsPixelsPerUnit property in powers of two.
/// </summary>
public class CameraController : MonoBehaviour
{
    [Header("Panning")]
    [Tooltip("Speed multiplier for drag-based camera panning.")]
    public float dragSpeed = 1f;

    [Header("Zoom (Pixel Perfect)")]
    [Tooltip("Minimum zoom level (assetsPixelsPerUnit = 1).")]
    public int minZoomLevel = 1;

    [Tooltip("Maximum zoom level (assetsPixelsPerUnit = 16).")]
    public int maxZoomLevel = 16;

    [Tooltip("Current zoom level (must be power of 2 between min and max).")]
    public int zoomLevel = 4;

    private PixelPerfectCamera ppc;
    private Camera cam;
    private Vector3 lastMouseWorld;

    private void Awake()
    {
        cam = Camera.main;
        ppc = cam.GetComponent<PixelPerfectCamera>();

        if (ppc == null)
        {
            Debug.LogError("PixelPerfectCamera component not found on Main Camera.");
            enabled = false;
            return;
        }

        // Initialize zoom level
        ApplyZoomLevel();
    }

    private void Update()
    {
        HandlePanning();
        HandleZooming();
    }

    /// <summary>
    /// Handles dragging the camera using the left mouse button.
    /// </summary>
    private void HandlePanning()
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        if (Input.GetMouseButtonDown(0))
        {
            lastMouseWorld = cam.ScreenToWorldPoint(Input.mousePosition);
        }

        if (Input.GetMouseButton(0))
        {
            Vector3 currentMouseWorld = cam.ScreenToWorldPoint(Input.mousePosition);
            Vector3 delta = lastMouseWorld - currentMouseWorld;
            transform.position += delta * dragSpeed;
        }
    }

    /// <summary>
    /// Handles scroll-wheel zooming by adjusting assetsPixelsPerUnit in power-of-two steps.
    /// </summary>
    private void HandleZooming()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (scroll != 0f)
        {
            // Store current mouse world position
            Vector3 mouseWorldBefore = cam.ScreenToWorldPoint(Input.mousePosition);

            // Adjust zoom level by scroll direction
            if (scroll > 0f && zoomLevel < maxZoomLevel)
                zoomLevel *= 2;
            else if (scroll < 0f && zoomLevel > minZoomLevel)
                zoomLevel /= 2;

            ApplyZoomLevel();

            // Adjust camera position so zoom happens relative to cursor
            Vector3 mouseWorldAfter = cam.ScreenToWorldPoint(Input.mousePosition);
            Vector3 zoomOffset = mouseWorldBefore - mouseWorldAfter;
            transform.position += zoomOffset;
        }
    }

    /// <summary>
    /// Applies the current zoomLevel to the PixelPerfectCamera.
    /// </summary>
    private void ApplyZoomLevel()
    {
        zoomLevel = Mathf.Clamp(zoomLevel, minZoomLevel, maxZoomLevel);
        ppc.assetsPPU = zoomLevel;
    }
}
