using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Wires uGUI <see cref="Button"/>s to <see cref="SpeechToTextController.BeginRecording"/> and
/// <see cref="SpeechToTextController.EndRecordingAndTranscribe"/>. Assign buttons in the Inspector, or enable
/// <see cref="createDefaultScreenSpaceUiIfNeeded"/> to build a small overlay at runtime.
/// </summary>
[DefaultExecutionOrder(-50)]
public class SpeechToTextUiButtons : MonoBehaviour
{
    [SerializeField] private SpeechToTextController speechToTextController;

    [Tooltip("If null at runtime, a screen-space canvas with two buttons is created under this object.")]
    [SerializeField] private bool createDefaultScreenSpaceUiIfNeeded = true;

    [Tooltip("Matches the built-in Editor panel: wait one frame before opening the mic after a UI click.")]
    [SerializeField] private bool beginRecordingAfterEndOfFrame = true;

    [Tooltip("Log every button click and state changes so you can tell clicks vs. missing controller vs. mic.")]
    [SerializeField] private bool logUiToConsole = true;

    [SerializeField] private Button startRecordingButton;
    [SerializeField] private Button stopAndTranscribeButton;

    private Text _statusLabel;

    private void Awake()
    {
        SpeechToTextController.SuppressEditorDebugUi = true;

        if (createDefaultScreenSpaceUiIfNeeded && (startRecordingButton == null || stopAndTranscribeButton == null))
            BuildDefaultScreenSpaceUi();
    }

    private void Start()
    {
        if (speechToTextController == null)
            speechToTextController = FindFirstObjectByType<SpeechToTextController>();

        if (speechToTextController == null)
            Debug.LogWarning("[SpeechToTextUiButtons] No SpeechToTextController found. Add one to the scene or rely on SpeechToTextBootstrap.");

        if (startRecordingButton == null || stopAndTranscribeButton == null)
            Debug.LogWarning("[SpeechToTextUiButtons] Assign Start and Stop buttons, or enable create default UI.");

        if (speechToTextController != null)
        {
            speechToTextController.OnRecordingStarted.AddListener(OnControllerRecordingStarted);
            speechToTextController.OnRecordingStopped.AddListener(OnControllerRecordingStopped);
            speechToTextController.OnTranscriptionComplete.AddListener(OnControllerTranscript);
            speechToTextController.OnError.AddListener(OnControllerError);
        }

        if (Object.FindFirstObjectByType<EventSystem>() == null && logUiToConsole)
            Debug.LogError("[SpeechToTextUiButtons] No EventSystem in the scene — uGUI buttons will not receive clicks. Add: GameObject → UI → Event System.");

        SetStatusLine(speechToTextController != null
            ? "Ready. Click Start, speak, then Stop & send."
            : "No SpeechToTextController — check Console.");
    }

    private void OnDestroy()
    {
        if (speechToTextController == null)
            return;
        speechToTextController.OnRecordingStarted.RemoveListener(OnControllerRecordingStarted);
        speechToTextController.OnRecordingStopped.RemoveListener(OnControllerRecordingStopped);
        speechToTextController.OnTranscriptionComplete.RemoveListener(OnControllerTranscript);
        speechToTextController.OnError.RemoveListener(OnControllerError);
    }

    private void OnControllerRecordingStarted()
    {
        SetStatusLine("Recording… speak, then tap Stop & send.");
    }

    private void OnControllerRecordingStopped()
    {
        SetStatusLine("Processing…");
    }

    private void OnControllerTranscript(string _)
    {
        SetStatusLine("Done. Tap Start to record again.");
    }

    private void OnControllerError(string _)
    {
        SetStatusLine("Error — see Console ([SpeechToText]).");
    }

    private void SetStatusLine(string message)
    {
        if (_statusLabel != null)
            _statusLabel.text = message;
    }

    private void OnEnable()
    {
        if (startRecordingButton != null)
            startRecordingButton.onClick.AddListener(OnStartClicked);
        if (stopAndTranscribeButton != null)
            stopAndTranscribeButton.onClick.AddListener(OnStopClicked);
    }

    private void OnDisable()
    {
        if (startRecordingButton != null)
            startRecordingButton.onClick.RemoveListener(OnStartClicked);
        if (stopAndTranscribeButton != null)
            stopAndTranscribeButton.onClick.RemoveListener(OnStopClicked);
    }

    private void OnStartClicked()
    {
        if (logUiToConsole)
            Debug.Log("[SpeechToTextUiButtons] Start clicked — if nothing follows, the click reached this button.");

        if (speechToTextController == null)
            speechToTextController = FindFirstObjectByType<SpeechToTextController>();
        if (speechToTextController == null)
        {
            Debug.LogError("[SpeechToTextUiButtons] No SpeechToTextController in the scene. Play a scene that runs SpeechToTextBootstrap or add the component.");
            SetStatusLine("Error: no SpeechToTextController.");
            return;
        }

        SetStatusLine("Starting microphone…");

        if (beginRecordingAfterEndOfFrame)
            StartCoroutine(BeginRecordingNextFrame());
        else
            speechToTextController.BeginRecording();
    }

    private IEnumerator BeginRecordingNextFrame()
    {
        yield return new WaitForEndOfFrame();
        if (speechToTextController != null)
            speechToTextController.BeginRecording();
    }

    private void OnStopClicked()
    {
        if (logUiToConsole)
            Debug.Log("[SpeechToTextUiButtons] Stop & send clicked.");

        if (speechToTextController == null)
            speechToTextController = FindFirstObjectByType<SpeechToTextController>();
        speechToTextController?.EndRecordingAndTranscribe();
    }

    private void BuildDefaultScreenSpaceUi()
    {
        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (font == null)
            font = Resources.GetBuiltinResource<Font>("Arial.ttf");

        var canvasGo = new GameObject("SpeechToText_DefaultCanvas");
        canvasGo.transform.SetParent(transform, false);
        canvasGo.layer = 5;

        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        // Draw above most game/XR overlays so clicks and labels stay visible.
        canvas.sortingOrder = 32760;

        var scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasGo.AddComponent<GraphicRaycaster>();

        var panel = new GameObject("Panel");
        panel.transform.SetParent(canvasGo.transform, false);
        panel.layer = 5;
        var panelRt = panel.AddComponent<RectTransform>();
        panelRt.anchorMin = new Vector2(0f, 0f);
        panelRt.anchorMax = new Vector2(0f, 0f);
        panelRt.pivot = new Vector2(0f, 0f);
        panelRt.anchoredPosition = new Vector2(16f, 16f);
        panelRt.sizeDelta = new Vector2(440f, 132f);
        var panelImg = panel.AddComponent<Image>();
        panelImg.color = new Color(0.1f, 0.1f, 0.12f, 0.92f);

        var statusGo = new GameObject("Status");
        statusGo.transform.SetParent(panel.transform, false);
        statusGo.layer = 5;
        var statusRt = statusGo.AddComponent<RectTransform>();
        statusRt.anchorMin = new Vector2(0f, 1f);
        statusRt.anchorMax = new Vector2(1f, 1f);
        statusRt.pivot = new Vector2(0f, 1f);
        statusRt.anchoredPosition = new Vector2(8f, -8f);
        statusRt.sizeDelta = new Vector2(-16f, 40f);
        _statusLabel = statusGo.AddComponent<Text>();
        _statusLabel.font = font;
        _statusLabel.fontSize = 14;
        _statusLabel.color = new Color(0.92f, 0.92f, 0.92f);
        _statusLabel.text = "…";
        _statusLabel.horizontalOverflow = HorizontalWrapMode.Wrap;

        startRecordingButton = CreateButton(panel.transform, "StartRecording", "Start recording", new Vector2(0f, 0f), new Vector2(200f, 56f), font);
        stopAndTranscribeButton = CreateButton(panel.transform, "StopAndTranscribe", "Stop & send", new Vector2(212f, 0f), new Vector2(200f, 56f), font);
    }

    private static Button CreateButton(Transform parent, string name, string label, Vector2 anchoredPos, Vector2 size, Font font)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.layer = 5;

        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0f, 0f);
        rt.anchorMax = new Vector2(0f, 0f);
        rt.pivot = new Vector2(0f, 0f);
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = size;

        var img = go.AddComponent<Image>();
        img.color = new Color(0.28f, 0.42f, 0.62f, 1f);

        var btn = go.AddComponent<Button>();
        btn.targetGraphic = img;

        var textGo = new GameObject("Label");
        textGo.transform.SetParent(go.transform, false);
        textGo.layer = 5;
        var textRt = textGo.AddComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.offsetMin = Vector2.zero;
        textRt.offsetMax = Vector2.zero;

        var text = textGo.AddComponent<Text>();
        text.font = font;
        text.text = label;
        text.fontSize = 18;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        text.raycastTarget = false;

        return btn;
    }
}
