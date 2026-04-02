using System.Collections;
using UnityEngine;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
#endif

/// <summary>
/// High-level speech-to-text: microphone → WAV → Wit.ai → transcript events.
/// Wire XR UI buttons or input actions to <see cref="BeginRecording"/> / <see cref="EndRecordingAndTranscribe"/>.
/// </summary>
public class SpeechToTextController : MonoBehaviour
{
    private static SpeechToTextController _instance;

    /// <summary>
    /// When a <see cref="SpeechToTextUiButtons"/> exists in the scene, it sets this so the generated Editor Game-view panel is not created (avoids duplicate UI).
    /// </summary>
    internal static bool SuppressEditorDebugUi;

    [SerializeField] private SpeechToTextSettings settings;

    [Tooltip("Optional: load from Resources if settings is null.")]
    [SerializeField] private string resourcesSettingsName = "SpeechToTextSettings";

    [Header("Debug (Editor)")]
    [SerializeField] private bool enableEditorSpaceToggle = true;

    [Tooltip("Log transcripts and errors to the Unity Console (no UI wiring needed).")]
    [SerializeField] private bool logToConsole = true;

    [Header("Events")]
    public UnityEvent<string> OnTranscriptionComplete;
    public UnityEvent<string> OnError;
    public UnityEvent OnRecordingStarted;
    public UnityEvent OnRecordingStopped;

    private readonly MicrophoneRecorder _recorder = new MicrophoneRecorder();
    private Coroutine _transcribeRoutine;
    private bool _busy;

#if UNITY_EDITOR
    /// <summary>Set when you click Start in the Editor panel; cleared when mic is live or on error.</summary>
    private bool _editorUiWantsRecording;

    private GameObject _editorCanvasRoot;
    private Text _editorStatusLabel;
    private Text _editorButtonLabel;
    private Button _editorButton;
    private RectTransform _editorButtonRectTransform;
#endif

    private SpeechToTextSettings EffectiveSettings =>
        settings != null ? settings : Resources.Load<SpeechToTextSettings>(resourcesSettingsName);

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;

        EnsureUnityEventsExist();
        if (!logToConsole)
            return;
        OnTranscriptionComplete.AddListener(t => Debug.Log("[SpeechToText] " + t));
        OnError.AddListener(e => Debug.LogWarning("[SpeechToText] " + e));
    }

    /// <summary>
    /// UnityEvents are null when this component is added at runtime via <see cref="SpeechToTextBootstrap"/> before serialization runs.
    /// </summary>
    private void EnsureUnityEventsExist()
    {
        if (OnTranscriptionComplete == null)
            OnTranscriptionComplete = new UnityEvent<string>();
        if (OnError == null)
            OnError = new UnityEvent<string>();
        if (OnRecordingStarted == null)
            OnRecordingStarted = new UnityEvent();
        if (OnRecordingStopped == null)
            OnRecordingStopped = new UnityEvent();
    }

    private void Start()
    {
#if UNITY_EDITOR
        if (enableEditorSpaceToggle && !SuppressEditorDebugUi)
            CreateEditorUguiPanel();
#endif
        StartCoroutine(StartSpeechRoutine());
    }

    private IEnumerator StartSpeechRoutine()
    {
#if UNITY_EDITOR_OSX
        if (!Application.HasUserAuthorization(UserAuthorization.Microphone))
            yield return Application.RequestUserAuthorization(UserAuthorization.Microphone);
#endif
        yield return MicrophonePermissionHelper.EnsureMicrophonePermissionCoroutine();
#if UNITY_EDITOR
        if (logToConsole)
            Debug.Log("[SpeechToText] Use the top-right buttons in the Game view (Space also works if input reaches the game).");
#endif
    }

    private void OnDestroy()
    {
#if UNITY_EDITOR
        if (_editorCanvasRoot != null)
            Destroy(_editorCanvasRoot);
#endif
        if (_instance == this)
            _instance = null;
        CancelRecording();
    }

#if UNITY_EDITOR
    private bool _loggedNoEditorInput;

    private void Update()
    {
        if (!enableEditorSpaceToggle)
            return;
        if (_busy)
            return;

        // EventSystem + InputSystemUIInputModule often fail to send mouse to overlay UI in Game view (XR / Play Focused).
        // Manual hit-test using the same math as uGUI raycasts.
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame
            && _editorButtonRectTransform != null && _editorButton != null && _editorButton.interactable)
        {
            Vector2 screenPos = Mouse.current.position.ReadValue();
            if (RectTransformUtility.RectangleContainsScreenPoint(_editorButtonRectTransform, screenPos, null))
            {
                OnEditorPrimaryButtonClicked();
                return;
            }
        }

        var kb = Keyboard.current ?? InputSystem.GetDevice<Keyboard>();
        if (kb == null || !kb.spaceKey.wasPressedThisFrame)
        {
            if (!_loggedNoEditorInput && kb == null && Mouse.current == null)
            {
                _loggedNoEditorInput = true;
                Debug.LogWarning("[SpeechToText] Input System has no Keyboard or Mouse device — use the on-screen buttons, or on-device UI on Quest.");
            }

            return;
        }

        if (!_recorder.IsRecording)
            BeginRecording();
        else
            EndRecordingAndTranscribe();
    }

    private void LateUpdate()
    {
        if (_editorButton == null || _editorStatusLabel == null || _editorButtonLabel == null)
            return;
        if (!enableEditorSpaceToggle)
            return;

        if (_busy)
        {
            _editorStatusLabel.text = "Sending to Wit.ai…";
            _editorButton.interactable = false;
            return;
        }

        _editorButton.interactable = true;

        if (_recorder.IsRecording)
        {
            _editorStatusLabel.text = "Recording… click Stop when done.";
            _editorButtonLabel.text = "Stop & send";
        }
        else if (_editorUiWantsRecording)
        {
            _editorStatusLabel.text = "Starting microphone…";
            _editorButtonLabel.text = "Stop & send";
        }
        else
        {
            _editorStatusLabel.text = "Click Start, speak, then Stop & send.";
            _editorButtonLabel.text = "Start recording";
        }
    }

    /// <summary>
    /// Legacy uGUI Text draws nothing if font is unset (common in Unity 6); builtin/OS fonts cover Editor + players.
    /// </summary>
    private static Font GetDefaultSpeechUiFont()
    {
        Font f = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (f == null)
            f = Resources.GetBuiltinResource<Font>("Arial.ttf");
        if (f == null)
        {
            try
            {
                f = Font.CreateDynamicFontFromOSFont(
                    new[] { "Arial", "Helvetica Neue", "Segoe UI", "Helvetica" },
                    16);
            }
            catch
            {
                // ignore
            }
        }

        return f;
    }

    private static void ApplySpeechUiFont(Text text, Font font)
    {
        if (text == null || font == null)
            return;
        text.font = font;
    }

    private void CreateEditorUguiPanel()
    {
        if (_instance != this)
            return;

        Font uiFont = GetDefaultSpeechUiFont();
        if (uiFont == null && logToConsole)
            Debug.LogWarning("[SpeechToText] No UI font resolved — labels may be invisible. Install a default font or use TextMeshPro.");

        if (Object.FindFirstObjectByType<EventSystem>() == null)
        {
            var es = new GameObject("SpeechToText_EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<InputSystemUIInputModule>();
            DontDestroyOnLoad(es);
        }

        _editorCanvasRoot = new GameObject("SpeechToText_EditorUI");
        _editorCanvasRoot.transform.SetParent(transform, false);

        var canvas = _editorCanvasRoot.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 32000;
        var scaler = _editorCanvasRoot.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        _editorCanvasRoot.AddComponent<GraphicRaycaster>();

        var panelGo = new GameObject("Panel");
        panelGo.transform.SetParent(_editorCanvasRoot.transform, false);
        var panelRect = panelGo.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(1f, 1f);
        panelRect.anchorMax = new Vector2(1f, 1f);
        panelRect.pivot = new Vector2(1f, 1f);
        panelRect.anchoredPosition = new Vector2(-12f, -12f);
        panelRect.sizeDelta = new Vector2(300f, 170f);
        var panelImg = panelGo.AddComponent<Image>();
        panelImg.color = new Color(0.12f, 0.12f, 0.14f, 0.96f);

        var titleGo = new GameObject("Title");
        titleGo.transform.SetParent(panelGo.transform, false);
        var titleRect = titleGo.AddComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0f, 1f);
        titleRect.anchorMax = new Vector2(1f, 1f);
        titleRect.pivot = new Vector2(0.5f, 1f);
        titleRect.anchoredPosition = new Vector2(0f, -10f);
        titleRect.sizeDelta = new Vector2(-20f, 26f);
        var titleTxt = titleGo.AddComponent<Text>();
        titleTxt.text = "Speech-to-text (Wit.ai)";
        titleTxt.fontStyle = FontStyle.Bold;
        titleTxt.fontSize = 16;
        titleTxt.alignment = TextAnchor.MiddleLeft;
        titleTxt.color = Color.white;
        ApplySpeechUiFont(titleTxt, uiFont);

        var statusGo = new GameObject("Status");
        statusGo.transform.SetParent(panelGo.transform, false);
        var statusRect = statusGo.AddComponent<RectTransform>();
        statusRect.anchorMin = new Vector2(0f, 1f);
        statusRect.anchorMax = new Vector2(1f, 1f);
        statusRect.pivot = new Vector2(0.5f, 1f);
        statusRect.anchoredPosition = new Vector2(0f, -44f);
        statusRect.sizeDelta = new Vector2(-20f, 70f);
        _editorStatusLabel = statusGo.AddComponent<Text>();
        _editorStatusLabel.fontSize = 14;
        _editorStatusLabel.alignment = TextAnchor.UpperLeft;
        _editorStatusLabel.color = new Color(0.9f, 0.9f, 0.9f);
        _editorStatusLabel.text = "Click Start, speak, then Stop & send.";
        _editorStatusLabel.horizontalOverflow = HorizontalWrapMode.Wrap;
        _editorStatusLabel.verticalOverflow = VerticalWrapMode.Overflow;
        ApplySpeechUiFont(_editorStatusLabel, uiFont);

        var btnGo = new GameObject("ActionButton");
        btnGo.transform.SetParent(panelGo.transform, false);
        var btnRt = btnGo.AddComponent<RectTransform>();
        btnRt.anchorMin = new Vector2(0f, 0f);
        btnRt.anchorMax = new Vector2(1f, 0f);
        btnRt.pivot = new Vector2(0.5f, 0f);
        btnRt.anchoredPosition = new Vector2(0f, 12f);
        btnRt.sizeDelta = new Vector2(-20f, 42f);
        var btnImg = btnGo.AddComponent<Image>();
        btnImg.color = new Color(0.28f, 0.4f, 0.65f, 1f);
        _editorButton = btnGo.AddComponent<Button>();
        _editorButton.targetGraphic = btnImg;
        _editorButtonRectTransform = btnRt;

        var btnTextGo = new GameObject("Label");
        btnTextGo.transform.SetParent(btnGo.transform, false);
        var btnTextRt = btnTextGo.AddComponent<RectTransform>();
        btnTextRt.anchorMin = Vector2.zero;
        btnTextRt.anchorMax = Vector2.one;
        btnTextRt.offsetMin = Vector2.zero;
        btnTextRt.offsetMax = Vector2.zero;
        _editorButtonLabel = btnTextGo.AddComponent<Text>();
        _editorButtonLabel.fontSize = 17;
        _editorButtonLabel.alignment = TextAnchor.MiddleCenter;
        _editorButtonLabel.color = Color.white;
        _editorButtonLabel.text = "Start recording";
        _editorButtonLabel.raycastTarget = false;
        ApplySpeechUiFont(_editorButtonLabel, uiFont);

        if (logToConsole)
            Debug.Log("[SpeechToText] Editor UI ready — click uses manual screen hit-test (EventSystem mouse is unreliable in Game view + XR).");
    }

    private void OnEditorPrimaryButtonClicked()
    {
        if (!enableEditorSpaceToggle || _busy)
            return;

        if (!_recorder.IsRecording && !_editorUiWantsRecording)
        {
            _editorUiWantsRecording = true;
            if (logToConsole)
                Debug.Log("[SpeechToText] UI: Start — opening mic next frame.");
            StartCoroutine(BeginRecordingAfterEndOfFrame());
            return;
        }

        EndRecordingAndTranscribe();
    }

    private IEnumerator BeginRecordingAfterEndOfFrame()
    {
        yield return new WaitForEndOfFrame();
        BeginRecording();
    }
#endif

    /// <summary>Starts capturing microphone audio.</summary>
    public void BeginRecording()
    {
        if (_busy)
        {
            OnError?.Invoke("Speech To Text: Busy.");
            return;
        }

        if (!MicrophonePermissionHelper.HasMicrophonePermission())
        {
            StartCoroutine(BeginRecordingAfterPermission());
            return;
        }

        BeginRecordingImpl();
    }

    private IEnumerator BeginRecordingAfterPermission()
    {
        yield return MicrophonePermissionHelper.EnsureMicrophonePermissionCoroutine();
        if (!MicrophonePermissionHelper.HasMicrophonePermission())
        {
            OnError?.Invoke("Speech To Text: Microphone permission denied.");
#if UNITY_EDITOR
            _editorUiWantsRecording = false;
#endif
            yield break;
        }

        BeginRecordingImpl();
    }

    private void BeginRecordingImpl()
    {
        if (_busy)
            return;

        var s = EffectiveSettings;
        if (s == null)
        {
#if UNITY_EDITOR
            _editorUiWantsRecording = false;
#endif
            OnError?.Invoke("Speech To Text: Assign SpeechToTextSettings or add Resources SpeechToTextSettings asset.");
            return;
        }

        if (Microphone.devices.Length == 0)
        {
#if UNITY_EDITOR
            _editorUiWantsRecording = false;
#endif
            OnError?.Invoke("Speech To Text: No microphone.");
            return;
        }

        if (_recorder.IsRecording)
            return;

        try
        {
            _recorder.StartRecording(s.MaxRecordingSeconds, s.SampleRate);
#if UNITY_EDITOR
            _editorUiWantsRecording = false;
#endif
            OnRecordingStarted?.Invoke();
            if (logToConsole)
                Debug.Log("[SpeechToText] Recording started — speak, then press Stop & send.");
        }
        catch (System.Exception e)
        {
#if UNITY_EDITOR
            _editorUiWantsRecording = false;
#endif
            OnError?.Invoke("Speech To Text: " + e.Message);
        }
    }

    /// <summary>Stops capture and sends audio to Wit.ai.</summary>
    public void EndRecordingAndTranscribe()
    {
        if (!_recorder.IsRecording)
        {
#if UNITY_EDITOR
            if (_editorUiWantsRecording)
            {
                _editorUiWantsRecording = false;
                if (logToConsole)
                    Debug.LogWarning("[SpeechToText] Microphone never started. On macOS: System Settings → Privacy & Security → Microphone → enable Unity. Then exit Play and try again.");
            }
#endif
            return;
        }

        _busy = true;
#if UNITY_EDITOR
        _editorUiWantsRecording = false;
#endif
        _recorder.StopRecording();
        OnRecordingStopped?.Invoke();

        AudioClip clip = _recorder.FinalizeClip();
        if (clip == null)
        {
            _busy = false;
            OnError?.Invoke("Speech To Text: No audio captured.");
            return;
        }

        byte[] wav;
        try
        {
            wav = WavUtility.FromAudioClip(clip);
        }
        finally
        {
            Destroy(clip);
        }

        var s = EffectiveSettings;
        if (s == null)
        {
            _busy = false;
            OnError?.Invoke("Speech To Text: Missing settings.");
            return;
        }

        if (_transcribeRoutine != null)
            StopCoroutine(_transcribeRoutine);

        if (logToConsole)
            Debug.Log("[SpeechToText] Sending audio to Wit.ai…");

        _transcribeRoutine = StartCoroutine(
            WitSpeechTranscriptionClient.Transcribe(
                wav,
                s.ServerAccessToken,
                s.WitApiVersion,
                s.RequestTimeoutSeconds,
                text =>
                {
                    _busy = false;
                    _transcribeRoutine = null;
                    OnTranscriptionComplete?.Invoke(text);
                },
                err =>
                {
                    _busy = false;
                    _transcribeRoutine = null;
                    OnError?.Invoke(err);
                }));
    }

    /// <summary>Cancels recording without uploading.</summary>
    public void CancelRecording()
    {
        if (_transcribeRoutine != null)
        {
            StopCoroutine(_transcribeRoutine);
            _transcribeRoutine = null;
        }

        _busy = false;
#if UNITY_EDITOR
        _editorUiWantsRecording = false;
#endif
        if (_recorder.IsRecording)
        {
            _recorder.CancelRecording();
            OnRecordingStopped?.Invoke();
        }
    }

    public bool IsRecording => _recorder.IsRecording;
    public bool IsBusy => _busy;
}
