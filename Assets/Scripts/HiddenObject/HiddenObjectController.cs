using System.Collections.Generic;
using System.Linq;
using Sherlock.Core;
using Sherlock.Data;
using Sherlock.UI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Sherlock.HiddenObject
{
    /// <summary>
    /// HiddenObjectController — manages one Hidden Object scene.
    ///
    /// Responsibilities:
    ///   • Registers all HiddenObjectItem children on scene load
    ///   • Drives pan and pinch-to-zoom via touch input (no Update polling — uses
    ///     Unity's new Input System callbacks when available, falls back to legacy)
    ///   • Tracks found/remaining counts and fires the scene-complete event
    ///   • Owns the HintSystem for this scene
    ///
    /// Attach to the root GameObject of each HO scene prefab.
    /// </summary>
    public class HiddenObjectController : MonoBehaviour
    {
        // ── Inspector ─────────────────────────────────────────────────────────
        [Header("Scene Definition")]
        [SerializeField] private string sceneId = "library_01";
        [SerializeField] private string displayName = "Baker Street Library";

        [Header("Camera / Pan / Zoom")]
        [SerializeField] private Camera   hoCamera;
        [SerializeField] private float    minZoom       = 2f;    // orthographic size
        [SerializeField] private float    maxZoom       = 6f;
        [SerializeField] private float    zoomSpeed     = 0.05f;
        [SerializeField] private float    panSpeed      = 1f;
        [SerializeField] private Bounds   panBounds;             // set in inspector to image bounds

        [Header("UI")]
        [SerializeField] private HintSystem hintSystem;

        // ── Public Properties ─────────────────────────────────────────────────
        public string SceneId => sceneId;

        // ── Events ────────────────────────────────────────────────────────────
        public System.Action<HiddenObjectItem> OnItemFound;
        public System.Action                   OnSceneComplete;

        // ── Private ───────────────────────────────────────────────────────────
        private List<HiddenObjectItem> _allItems       = new();
        private int                    _foundCount;
        private bool                   _sceneComplete;

        // Touch gesture state
        private Vector2 _lastPanPos;
        private float   _pinchStartDist;
        private float   _pinchStartSize;
        private bool    _isPanning;
        private bool    _isPinching;

        // ═════════════════════════════════════════════════════════════════════
        // Lifecycle
        // ═════════════════════════════════════════════════════════════════════

        void Awake()
        {
            if (hoCamera == null) hoCamera = Camera.main;
            DiscoverItems();
        }

        void Start()
        {
            hintSystem?.Init(this);
            RestoreFoundState();
        }

        void Update()
        {
            HandleTouchInput();
        }

        // ═════════════════════════════════════════════════════════════════════
        // Item registration
        // ═════════════════════════════════════════════════════════════════════

        void DiscoverItems()
        {
            _allItems = GetComponentsInChildren<HiddenObjectItem>(includeInactive: true).ToList();
            foreach (var item in _allItems) item.Init(this);
            Debug.Log($"[HO:{sceneId}] Registered {_allItems.Count} items.");
        }

        void RestoreFoundState()
        {
            _foundCount = _allItems.Count(i => i.IsCollected);
            CheckSceneComplete(silent: true);
        }

        // ═════════════════════════════════════════════════════════════════════
        // Called by HiddenObjectItem
        // ═════════════════════════════════════════════════════════════════════

        public void OnObjectFound(HiddenObjectItem item)
        {
            _foundCount++;
            OnItemFound?.Invoke(item);
            GameUIManager.Instance?.ShowItemFoundPopup(item.Data);
            CheckSceneComplete(silent: false);
        }

        void CheckSceneComplete(bool silent)
        {
            if (_sceneComplete) return;
            if (_foundCount < _allItems.Count) return;

            _sceneComplete = true;
            if (!silent)
            {
                OnSceneComplete?.Invoke();
                GameUIManager.Instance?.ShowSceneCompleteScreen(sceneId);
            }
        }

        // ═════════════════════════════════════════════════════════════════════
        // Hint system helper
        // ═════════════════════════════════════════════════════════════════════

        public List<HiddenObjectItem> GetUncollectedItems() =>
            _allItems.Where(i => !i.IsCollected && i.gameObject.activeSelf).ToList();

        // ═════════════════════════════════════════════════════════════════════
        // Touch / Mouse Input — Pan & Pinch-to-Zoom
        // ═════════════════════════════════════════════════════════════════════

        void HandleTouchInput()
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            HandleMouseInput();
#else
            HandleMobileInput();
#endif
        }

        // Hinweis Unity 6: Edit > Project Settings > Player > Active Input Handling
        // auf "Both" stellen, damit Input.GetTouch() funktioniert.

        // ── Editor / PC fallback ──────────────────────────────────────────────

        void HandleMouseInput()
        {
            // Scroll wheel zoom
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scroll) > 0.001f)
                ApplyZoom(hoCamera.orthographicSize - scroll * 5f);

            // Right-click pan
            if (Input.GetMouseButtonDown(1))
            {
                _lastPanPos = Input.mousePosition;
                _isPanning  = true;
            }
            if (Input.GetMouseButton(1) && _isPanning)
                ApplyPan(Input.mousePosition);
            if (Input.GetMouseButtonUp(1))
                _isPanning = false;
        }

        // ── Mobile touch ──────────────────────────────────────────────────────

        void HandleMobileInput()
        {
            if (Input.touchCount == 0) { _isPanning = false; _isPinching = false; return; }

            if (Input.touchCount == 1)
            {
                _isPinching = false;
                var t = Input.GetTouch(0);

                if (t.phase == TouchPhase.Began)
                {
                    // Only pan if touch is NOT over a UI element
                    if (!EventSystem.current.IsPointerOverGameObject(t.fingerId))
                    {
                        _lastPanPos = t.position;
                        _isPanning  = true;
                    }
                }
                else if (t.phase == TouchPhase.Moved && _isPanning)
                {
                    ApplyPan(t.position);
                }
                else if (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled)
                {
                    _isPanning = false;
                }
            }
            else if (Input.touchCount == 2)
            {
                _isPanning = false;
                var t0 = Input.GetTouch(0);
                var t1 = Input.GetTouch(1);

                if (!_isPinching)
                {
                    _pinchStartDist = Vector2.Distance(t0.position, t1.position);
                    _pinchStartSize = hoCamera.orthographicSize;
                    _isPinching     = true;
                }
                else
                {
                    float currentDist = Vector2.Distance(t0.position, t1.position);
                    if (_pinchStartDist > 0f)
                    {
                        float ratio   = _pinchStartDist / currentDist;
                        float newSize = _pinchStartSize * ratio;
                        ApplyZoom(newSize);
                    }
                }
            }
        }

        void ApplyZoom(float targetSize)
        {
            hoCamera.orthographicSize = Mathf.Clamp(targetSize, minZoom, maxZoom);
            ClampCameraToScene();
        }

        void ApplyPan(Vector2 newScreenPos)
        {
            float unitsPerPixel = hoCamera.orthographicSize * 2f / Screen.height;
            var   delta         = (Vector3)(_lastPanPos - newScreenPos) * (unitsPerPixel * panSpeed);
            hoCamera.transform.position += delta;
            _lastPanPos = newScreenPos;
            ClampCameraToScene();
        }

        void ClampCameraToScene()
        {
            if (panBounds == default) return;
            var cam    = hoCamera.transform.position;
            float halfH = hoCamera.orthographicSize;
            float halfW = hoCamera.orthographicSize * hoCamera.aspect;
            cam.x = Mathf.Clamp(cam.x, panBounds.min.x + halfW, panBounds.max.x - halfW);
            cam.y = Mathf.Clamp(cam.y, panBounds.min.y + halfH, panBounds.max.y - halfH);
            hoCamera.transform.position = cam;
        }

        // ═════════════════════════════════════════════════════════════════════
        // Progress queries (used by UI)
        // ═════════════════════════════════════════════════════════════════════

        public int TotalItems  => _allItems.Count;
        public int FoundCount  => _foundCount;
        public bool IsComplete => _sceneComplete;

#if UNITY_EDITOR
        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(panBounds.center, panBounds.size);
        }
#endif
    }
}
