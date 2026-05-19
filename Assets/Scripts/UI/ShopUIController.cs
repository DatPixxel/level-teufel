using Sherlock.Meta;
using UnityEngine;
using UnityEngine.UI;

namespace Sherlock.UI
{
    /// <summary>
    /// In-game shop overlay. Shows available IAP products with localised prices
    /// and handles purchase button callbacks.
    ///
    /// Attach to a Canvas > ShopPanel GameObject.
    /// Each ShopItem row is a child with a ShopItemRow component.
    /// </summary>
    public class ShopUIController : MonoBehaviour
    {
        [Header("Product Rows")]
        [SerializeField] private ShopItemRow rookieKitRow;
        [SerializeField] private ShopItemRow coins500Row;
        [SerializeField] private ShopItemRow coins2500Row;
        [SerializeField] private ShopItemRow removeAdsRow;
        [SerializeField] private ShopItemRow seasonPassRow;

        [Header("Close")]
        [SerializeField] private Button closeBtn;

        void OnEnable()
        {
            closeBtn?.onClick.AddListener(() => gameObject.SetActive(false));
            RefreshPrices();
            BindButtons();
        }

        void RefreshPrices()
        {
            // Prices shown as "–" until Unity IAP is integrated
            SetPrice(rookieKitRow,   IAPManager.Products.RookieKit);
            SetPrice(coins500Row,    IAPManager.Products.CoinPack500);
            SetPrice(coins2500Row,   IAPManager.Products.CoinPack2500);
            SetPrice(removeAdsRow,   IAPManager.Products.RemoveAds);
            SetPrice(seasonPassRow,  IAPManager.Products.SeasonPass);
        }

        static void SetPrice(ShopItemRow row, string productId)
        {
            if (row == null) return;
            row.SetPrice("–");
        }

        void BindButtons()
        {
            Bind(rookieKitRow,  IAPManager.Products.RookieKit);
            Bind(coins500Row,   IAPManager.Products.CoinPack500);
            Bind(coins2500Row,  IAPManager.Products.CoinPack2500);
            Bind(removeAdsRow,  IAPManager.Products.RemoveAds);
            Bind(seasonPassRow, IAPManager.Products.SeasonPass);
        }

        static void Bind(ShopItemRow row, string productId)
        {
            if (row == null) return;
            row.BuyButton.onClick.RemoveAllListeners();
            row.BuyButton.onClick.AddListener(() => IAPManager.Instance?.Purchase(productId));
        }
    }

    // ── Small helper component attached to each row prefab ────────────────────
    public class ShopItemRow : MonoBehaviour
    {
        [SerializeField] private Text   priceLabel;
        [SerializeField] private Button buyButton;

        public Button BuyButton => buyButton;
        public void SetPrice(string price) { if (priceLabel) priceLabel.text = price; }
    }
}
