using Sherlock.Data;
using UnityEngine;
using UnityEngine.UI;

namespace Sherlock.UI
{
    /// <summary>
    /// ItemIconDisplay — zeigt das Icon eines Items in der UI an.
    ///
    /// Kette: ItemData.icon (Sprite direkt im ScriptableObject) hat Vorrang.
    /// Wenn leer → SpriteManager.GetItemSprite(itemId) → Placeholder.
    ///
    /// Anhängen an: Inventory-Slots, Item-Found-Popup, Shop-Reihen.
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class ItemIconDisplay : MonoBehaviour
    {
        [SerializeField] private Image targetImage;
        [SerializeField] private Text  itemNameLabel;
        [SerializeField] private Text  tierLabel;

        private Image _img;

        void Awake() => _img = targetImage != null ? targetImage : GetComponent<Image>();

        // ── Öffentliche API ───────────────────────────────────────────────────

        public void Show(ItemData data)
        {
            if (data == null) { gameObject.SetActive(false); return; }
            gameObject.SetActive(true);

            // Sprite: zuerst ItemData.icon, dann SpriteManager, dann Placeholder
            Sprite sprite = data.icon;
            if (sprite == null && SpriteManager.Instance != null)
                sprite = SpriteManager.Instance.GetItemSprite(data.itemId);
            if (sprite == null)
                sprite = PlaceholderSpriteGenerator.GenerateItemSprite(data.itemId, data.tier);

            _img.sprite = sprite;

            if (itemNameLabel) itemNameLabel.text = data.displayName;
            if (tierLabel)     tierLabel.text     = $"Stufe {data.tier}";
        }

        public void Show(string itemId)
        {
            if (Data.ItemDatabase.Instance == null) return;
            var data = Data.ItemDatabase.Instance.Get(itemId);
            Show(data);
        }

        public void Clear()
        {
            _img.sprite = null;
            if (itemNameLabel) itemNameLabel.text = "";
            if (tierLabel)     tierLabel.text     = "";
        }
    }
}
