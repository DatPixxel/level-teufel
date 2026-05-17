using UnityEngine;
using UnityEngine.EventSystems;

namespace Sherlock.Merge
{
    /// <summary>
    /// Represents one cell in the merge grid. Holds a reference to the MergeItem sitting on it.
    /// Accepts drop events forwarded by MergeManager.
    /// </summary>
    public class MergeCell : MonoBehaviour
    {
        public Vector2Int GridPosition { get; private set; }
        public MergeItem CurrentItem   { get; private set; }
        public bool IsEmpty => CurrentItem == null;

        // Visual feedback sprites configured in inspector
        [SerializeField] private SpriteRenderer highlightRenderer;
        [SerializeField] private Color normalColor  = Color.white;
        [SerializeField] private Color hoverColor   = new Color(0.8f, 0.95f, 1f);
        [SerializeField] private Color blockedColor = new Color(1f, 0.6f, 0.6f);

        public void Init(Vector2Int gridPos)
        {
            GridPosition = gridPos;
            SetHighlight(HighlightState.None);
        }

        public void PlaceItem(MergeItem item)
        {
            CurrentItem = item;
            if (item != null)
            {
                item.transform.position = transform.position;
                item.CurrentCell        = this;
            }
        }

        public MergeItem RemoveItem()
        {
            var item   = CurrentItem;
            CurrentItem = null;
            return item;
        }

        public enum HighlightState { None, Hover, Blocked }

        public void SetHighlight(HighlightState state)
        {
            if (highlightRenderer == null) return;
            highlightRenderer.color = state switch
            {
                HighlightState.Hover   => hoverColor,
                HighlightState.Blocked => blockedColor,
                _                      => normalColor,
            };
        }
    }
}
