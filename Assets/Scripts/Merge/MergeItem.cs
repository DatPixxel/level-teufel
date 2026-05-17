using Sherlock.Data;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Sherlock.Merge
{
    /// <summary>
    /// Visual and logical representation of one item sitting on the merge board.
    /// Handles its own drag-and-drop touch input and reports back to MergeManager.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class MergeItem : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        public ItemData Data      { get; private set; }
        public MergeCell CurrentCell { get; set; }

        private SpriteRenderer _sr;
        private Vector3        _dragOffset;
        private Vector3        _originPos;
        private int            _originalSortOrder;
        private bool           _isDragging;

        // Drag-layer Z so the item renders above the board
        private const int DragSortOrder = 100;

        void Awake() => _sr = GetComponent<SpriteRenderer>();

        public void Init(ItemData data)
        {
            Data = data;

            // Priorität: 1. Icon direkt im ScriptableObject
            //            2. SpriteManager (aus Resources/Sprites/Items/)
            //            3. Prozeduraler Placeholder
            if (data.icon != null)
            {
                _sr.sprite = data.icon;
            }
            else if (UI.SpriteManager.Instance != null)
            {
                _sr.sprite = UI.SpriteManager.Instance.GetItemSprite(data.itemId);
            }
            else
            {
                _sr.sprite = UI.PlaceholderSpriteGenerator.GenerateItemSprite(data.itemId, data.tier);
            }
        }

        // ── IPointer handlers ─────────────────────────────────────────────────

        public void OnPointerDown(PointerEventData e)
        {
            _isDragging       = true;
            _originPos        = transform.position;
            _originalSortOrder = _sr.sortingOrder;
            _sr.sortingOrder  = DragSortOrder;

            // World-space drag offset so the item doesn't snap its centre to the finger
            var worldTouch = Camera.main.ScreenToWorldPoint(e.position);
            worldTouch.z   = transform.position.z;
            _dragOffset    = transform.position - worldTouch;

            MergeManager.Instance.OnItemPickUp(this);
        }

        public void OnDrag(PointerEventData e)
        {
            if (!_isDragging) return;
            var worldPos = Camera.main.ScreenToWorldPoint(e.position);
            worldPos.z   = transform.position.z;
            transform.position = worldPos + _dragOffset;

            MergeManager.Instance.OnItemDrag(this, worldPos);
        }

        public void OnPointerUp(PointerEventData e)
        {
            if (!_isDragging) return;
            _isDragging = false;
            _sr.sortingOrder = _originalSortOrder;

            var worldPos = Camera.main.ScreenToWorldPoint(e.position);
            bool placed  = MergeManager.Instance.OnItemDrop(this, worldPos);

            if (!placed)
            {
                // Snap back to original cell
                transform.position = _originPos;
                MergeManager.Instance.OnItemSnapBack(this);
            }
        }

        public void SnapToCell(MergeCell cell)
        {
            CurrentCell        = cell;
            transform.position = cell.transform.position;
            _sr.sortingOrder   = _originalSortOrder;
        }
    }
}
