using System;
using System.Collections.Generic;
using Sherlock.Core;
using Sherlock.Data;
using Sherlock.Quest;
using UnityEngine;

namespace Sherlock.Merge
{
    /// <summary>
    /// MergeManager — owns the merge grid, drives merge logic, and bridges with
    /// GameState (inventory) and QuestManager (progress checks).
    ///
    /// Grid layout: a flat 2-D array of MergeCell MonoBehaviours instantiated
    /// at startup.  Items sit on cells and are moved via drag-and-drop events
    /// forwarded from MergeItem.
    /// </summary>
    public class MergeManager : MonoBehaviour
    {
        // ── Singleton ─────────────────────────────────────────────────────────
        public static MergeManager Instance { get; private set; }

        // ── Inspector ─────────────────────────────────────────────────────────
        [Header("Grid Configuration")]
        [SerializeField] private int   columns     = 6;
        [SerializeField] private int   rows        = 8;
        [SerializeField] private float cellSize    = 1.2f;   // world units
        [SerializeField] private Vector2 boardOrigin = Vector2.zero;

        [Header("Prefabs")]
        [SerializeField] private MergeCell mergeCellPrefab;
        [SerializeField] private MergeItem mergeItemPrefab;

        [Header("VFX / Audio")]
        [SerializeField] private GameObject mergeParticlePrefab;
        [SerializeField] private AudioClip  mergeSfx;
        [SerializeField] private AudioClip  placeSfx;
        [SerializeField] private AudioClip  sellSfx;

        // ── Private state ─────────────────────────────────────────────────────
        private MergeCell[,] _grid;
        private MergeCell    _hoveredCell;   // cell under the dragged item
        private MergeItem    _draggedItem;
        private MergeCell    _originCell;    // cell the drag started from
        private AudioSource  _audio;

        // ── Events ────────────────────────────────────────────────────────────
        public event Action<ItemData> OnMergeCompleted;   // fires after a successful merge
        public event Action<ItemData> OnItemSold;
        public event Action           OnBoardFull;        // all cells occupied

        // ── Serialisable board save ───────────────────────────────────────────
        [Serializable] private class BoardSave
        {
            public List<CellSave> cells = new();
        }
        [Serializable] private class CellSave
        {
            public int col, row;
            public string itemId;
        }

        // ═════════════════════════════════════════════════════════════════════
        // Lifecycle
        // ═════════════════════════════════════════════════════════════════════

        void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            _audio   = GetComponent<AudioSource>();
        }

        void Start()
        {
            BuildGrid();
            LoadBoard();

            // Listen for newly found HO items
            GameState.Instance.OnInventoryChanged += OnPendingInventoryChanged;
        }

        void OnDestroy()
        {
            if (GameState.Instance != null)
                GameState.Instance.OnInventoryChanged -= OnPendingInventoryChanged;
        }

        // ═════════════════════════════════════════════════════════════════════
        // Grid construction
        // ═════════════════════════════════════════════════════════════════════

        void BuildGrid()
        {
            _grid = new MergeCell[columns, rows];
            for (int c = 0; c < columns; c++)
            {
                for (int r = 0; r < rows; r++)
                {
                    var worldPos = GridToWorld(c, r);
                    var cell     = Instantiate(mergeCellPrefab,
                                               new Vector3(worldPos.x, worldPos.y, 0f),
                                               Quaternion.identity,
                                               transform);
                    cell.Init(new Vector2Int(c, r));
                    _grid[c, r] = cell;
                }
            }
        }

        Vector2 GridToWorld(int col, int row) =>
            boardOrigin + new Vector2(col * cellSize, row * cellSize);

        MergeCell WorldToCell(Vector3 worldPos)
        {
            int c = Mathf.RoundToInt((worldPos.x - boardOrigin.x) / cellSize);
            int r = Mathf.RoundToInt((worldPos.y - boardOrigin.y) / cellSize);
            if (c < 0 || c >= columns || r < 0 || r >= rows) return null;
            return _grid[c, r];
        }

        // ═════════════════════════════════════════════════════════════════════
        // Public API — called by HiddenObjectController / QuestManager / UI
        // ═════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Places an item by itemId onto the first free cell.
        /// Returns false if the board is full.
        /// </summary>
        public bool SpawnItem(string itemId)
        {
            var data = ItemDatabase.Instance.Get(itemId);
            if (data == null) return false;
            return SpawnItem(data);
        }

        public bool SpawnItem(ItemData data)
        {
            var cell = FindFreeCell();
            if (cell == null) { OnBoardFull?.Invoke(); return false; }

            var item = Instantiate(mergeItemPrefab, cell.transform.position, Quaternion.identity, transform);
            item.Init(data);
            cell.PlaceItem(item);
            PlaySfx(placeSfx);
            return true;
        }

        /// <summary>
        /// Removes an item from its current cell and awards coins (sell flow).
        /// </summary>
        public void SellItem(MergeItem item)
        {
            if (item == null) return;
            int coins = item.Data.sellValue;
            item.CurrentCell?.RemoveItem();
            Destroy(item.gameObject);
            GameState.Instance.AddCoins(coins);
            PlaySfx(sellSfx);
            OnItemSold?.Invoke(item.Data);
        }

        // ═════════════════════════════════════════════════════════════════════
        // Drag-and-drop event handlers (called by MergeItem)
        // ═════════════════════════════════════════════════════════════════════

        public void OnItemPickUp(MergeItem item)
        {
            _draggedItem = item;
            _originCell  = item.CurrentCell;
            _originCell?.RemoveItem();
        }

        public void OnItemDrag(MergeItem item, Vector3 worldPos)
        {
            var cell = WorldToCell(worldPos);

            if (_hoveredCell != null && _hoveredCell != cell)
                _hoveredCell.SetHighlight(MergeCell.HighlightState.None);

            if (cell == null) { _hoveredCell = null; return; }

            bool canMerge = !cell.IsEmpty && cell.CurrentItem?.Data.itemId == item.Data.itemId
                            && ItemDatabase.Instance.GetMergeResult(item.Data.itemId) != null;
            bool canPlace = cell.IsEmpty;

            cell.SetHighlight(canMerge || canPlace
                ? MergeCell.HighlightState.Hover
                : MergeCell.HighlightState.Blocked);
            _hoveredCell = cell;
        }

        /// <summary>
        /// Returns true if the drop was handled (merge or placement succeeded).
        /// </summary>
        public bool OnItemDrop(MergeItem item, Vector3 worldPos)
        {
            if (_hoveredCell != null)
                _hoveredCell.SetHighlight(MergeCell.HighlightState.None);

            var targetCell = WorldToCell(worldPos);

            if (targetCell == null) return false;

            // ── Merge: same itemId + result exists ──────────────────────────
            if (!targetCell.IsEmpty && targetCell.CurrentItem.Data.itemId == item.Data.itemId)
            {
                var resultData = ItemDatabase.Instance.GetMergeResult(item.Data.itemId);
                if (resultData != null)
                {
                    ExecuteMerge(item, targetCell, resultData);
                    return true;
                }
                // Max-tier: cannot merge — refuse drop
                return false;
            }

            // ── Place on empty cell ─────────────────────────────────────────
            if (targetCell.IsEmpty)
            {
                targetCell.PlaceItem(item);
                _draggedItem = null;
                return true;
            }

            return false;
        }

        public void OnItemSnapBack(MergeItem item)
        {
            // Restore the item to its origin cell
            _originCell?.PlaceItem(item);
            _draggedItem = null;
        }

        // ═════════════════════════════════════════════════════════════════════
        // Merge execution
        // ═════════════════════════════════════════════════════════════════════

        void ExecuteMerge(MergeItem dragged, MergeCell targetCell, ItemData resultData)
        {
            // Remove both participants
            var resident = targetCell.RemoveItem();
            Destroy(resident.gameObject);
            Destroy(dragged.gameObject);

            // Spawn merged result on the target cell
            var resultItem = Instantiate(mergeItemPrefab,
                                         targetCell.transform.position,
                                         Quaternion.identity,
                                         transform);
            resultItem.Init(resultData);
            targetCell.PlaceItem(resultItem);

            PlaySfx(mergeSfx);
            SpawnMergeParticles(targetCell.transform.position);
            Core.MobileSetup.VibrateShort();

            _draggedItem = null;
            OnMergeCompleted?.Invoke(resultData);

            // Notify quest system
            QuestManager.Instance?.OnItemCrafted(resultData.itemId);

            SaveBoard();
        }

        // ═════════════════════════════════════════════════════════════════════
        // Helpers
        // ═════════════════════════════════════════════════════════════════════

        MergeCell FindFreeCell()
        {
            // Search left-to-right, bottom-to-top so new items appear at lower-left
            for (int r = 0; r < rows; r++)
                for (int c = 0; c < columns; c++)
                    if (_grid[c, r].IsEmpty) return _grid[c, r];
            return null;
        }

        void OnPendingInventoryChanged(string itemId, int count)
        {
            // Drain the pending queue and place items on the board
            while (GameState.Instance.ConsumeFromPendingInventory(itemId))
            {
                if (!SpawnItem(itemId)) break; // board full — stop draining
            }
        }

        void PlaySfx(AudioClip clip)
        {
            if (_audio != null && clip != null) _audio.PlayOneShot(clip);
        }

        void SpawnMergeParticles(Vector3 pos)
        {
            if (mergeParticlePrefab == null) return;
            var go = Instantiate(mergeParticlePrefab, pos, Quaternion.identity);
            Destroy(go, 2f);
        }

        // ═════════════════════════════════════════════════════════════════════
        // Board persistence (separate from GameState save)
        // ═════════════════════════════════════════════════════════════════════

        void SaveBoard()
        {
            var save = new BoardSave();
            for (int c = 0; c < columns; c++)
                for (int r = 0; r < rows; r++)
                    if (!_grid[c, r].IsEmpty)
                        save.cells.Add(new CellSave
                        {
                            col    = c,
                            row    = r,
                            itemId = _grid[c, r].CurrentItem.Data.itemId,
                        });

            var path = System.IO.Path.Combine(Application.persistentDataPath, "mergeboard.json");
            System.IO.File.WriteAllText(path, JsonUtility.ToJson(save, true));
        }

        void LoadBoard()
        {
            var path = System.IO.Path.Combine(Application.persistentDataPath, "mergeboard.json");
            if (!System.IO.File.Exists(path)) return;
            try
            {
                var save = JsonUtility.FromJson<BoardSave>(System.IO.File.ReadAllText(path));
                foreach (var cs in save.cells)
                {
                    if (cs.col < 0 || cs.col >= columns || cs.row < 0 || cs.row >= rows) continue;
                    var data = ItemDatabase.Instance.Get(cs.itemId);
                    if (data == null) continue;
                    var item = Instantiate(mergeItemPrefab,
                                           _grid[cs.col, cs.row].transform.position,
                                           Quaternion.identity,
                                           transform);
                    item.Init(data);
                    _grid[cs.col, cs.row].PlaceItem(item);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[MergeManager] Board load failed: {e.Message}");
            }
        }

#if UNITY_EDITOR
        // Debug visualisation in Scene view
        void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.4f, 0.8f, 1f, 0.3f);
            for (int c = 0; c < columns; c++)
                for (int r = 0; r < rows; r++)
                {
                    var wp = (Vector3)(boardOrigin + new Vector2(c * cellSize, r * cellSize));
                    Gizmos.DrawWireCube(wp, Vector3.one * (cellSize * 0.9f));
                }
        }
#endif
    }
}
