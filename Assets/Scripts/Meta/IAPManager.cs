using System;
using System.Collections.Generic;
using Sherlock.Core;
using UnityEngine;

namespace Sherlock.Meta
{
    /// <summary>
    /// IAPManager — placeholder for Apple/Google in-app purchases.
    ///
    /// Implementation path:
    ///   1. Import Unity IAP (com.unity.purchasing) via Package Manager
    ///   2. Replace stub bodies with IStoreListener / IAppleExtensions calls
    ///   3. Register product IDs in the App Store Connect / Google Play console
    /// </summary>
    public class IAPManager : MonoBehaviour
    {
        public static IAPManager Instance { get; private set; }

        // Product IDs — must match App Store Connect entries
        public static class Products
        {
            public const string RookieKit      = "com.sherlockgame.rookiekit";      // consumable
            public const string CoinPack500    = "com.sherlockgame.coins500";       // consumable
            public const string CoinPack2500   = "com.sherlockgame.coins2500";      // consumable
            public const string RemoveAds      = "com.sherlockgame.removeads";      // non-consumable
            public const string SeasonPass     = "com.sherlockgame.seasonpass";     // auto-renewable
        }

        [Serializable]
        public class ProductDefinition
        {
            public string productId;
            public string displayName;
            public string localizedPrice;     // filled in at runtime from store
            public int    coinValue;          // for consumable coin packs
        }

        [SerializeField] private List<ProductDefinition> catalogue;

        public event Action<string> OnPurchaseSuccess;
        public event Action<string> OnPurchaseFailed;

        // ── Lifecycle ─────────────────────────────────────────────────────────

        void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        void Start() => InitializeStore();

        // ── Store init (stub) ─────────────────────────────────────────────────

        void InitializeStore()
        {
            // TODO: UnityPurchasing.Initialize(this, builder);
            Debug.Log("[IAPManager] Store initialisation stub — integrate Unity IAP.");
        }

        // ── Purchase flow ─────────────────────────────────────────────────────

        public void Purchase(string productId)
        {
            Debug.Log($"[IAPManager] Purchase requested: {productId}");
            // TODO: _storeController.InitiatePurchase(productId);

            // ── Stub for editor / prototype testing ──────────────────────────
#if UNITY_EDITOR
            SimulatePurchase(productId);
#endif
        }

        void SimulatePurchase(string productId)
        {
            Debug.Log($"[IAPManager] Editor simulation: purchase succeeded for {productId}");
            DeliverProduct(productId);
            OnPurchaseSuccess?.Invoke(productId);
        }

        void DeliverProduct(string productId)
        {
            switch (productId)
            {
                case Products.RookieKit:
                    GameState.Instance.AddToPendingInventory("magnifying_glass", 1);
                    GameState.Instance.AddToPendingInventory("notebook", 1);
                    GameState.Instance.AddCoins(100);
                    break;
                case Products.CoinPack500:
                    GameState.Instance.AddCoins(500);
                    break;
                case Products.CoinPack2500:
                    GameState.Instance.AddCoins(2500);
                    break;
                case Products.RemoveAds:
                    PlayerPrefs.SetInt("ads_removed", 1);
                    break;
                case Products.SeasonPass:
                    PlayerPrefs.SetInt("season_pass_active", 1);
                    break;
            }
            SaveSystem.Save();
        }

        public bool IsAdFree    => PlayerPrefs.GetInt("ads_removed", 0) == 1;
        public bool HasSeasonPass => PlayerPrefs.GetInt("season_pass_active", 0) == 1;

        // Restore (required for Apple) — stub
        public void RestorePurchases()
        {
            Debug.Log("[IAPManager] RestorePurchases called — implement with Apple extensions.");
            // TODO: _appleExtensions.RestoreTransactions(result => { ... });
        }
    }
}
