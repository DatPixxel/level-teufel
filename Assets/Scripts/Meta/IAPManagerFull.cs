// ┌─────────────────────────────────────────────────────────────────────────┐
// │  IAPManagerFull.cs — Production Unity IAP Integration                   │
// │                                                                         │
// │  Requirements:                                                           │
// │    com.unity.purchasing >= 4.9 (Unity Package Manager)                  │
// │    Assembly reference: Unity.Purchasing in .asmdef                      │
// │                                                                         │
// │  To activate: rename this file to IAPManager.cs and delete the stub.   │
// └─────────────────────────────────────────────────────────────────────────┘

using System;
using System.Collections.Generic;
using Sherlock.Core;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;
using UnityEngine.Purchasing.Security;

namespace Sherlock.Meta
{
    /// <summary>
    /// Production IAPManager implementing IDetailedStoreListener.
    /// Handles consumables, non-consumables, and auto-renewable subscriptions.
    /// Includes Apple receipt validation via CrossPlatformValidator.
    /// </summary>
    public class IAPManagerFull : MonoBehaviour, IDetailedStoreListener
    {
        public static IAPManagerFull Instance { get; private set; }

        // ── Product IDs (must match App Store Connect / Google Play exactly) ──
        public static class Products
        {
            public const string RookieKit      = "com.sherlockgame.rookiekit";
            public const string CoinPack500    = "com.sherlockgame.coins500";
            public const string CoinPack2500   = "com.sherlockgame.coins2500";
            public const string RemoveAds      = "com.sherlockgame.removeads";
            public const string SeasonPass     = "com.sherlockgame.seasonpass";
        }

        // ── Events ────────────────────────────────────────────────────────────
        public event Action<string>       OnPurchaseSuccess;
        public event Action<string>       OnPurchaseFailed;
        public event Action               OnInitialised;
        public event Action<string>       OnInitialiseFailed;

        // ── State ─────────────────────────────────────────────────────────────
        private IStoreController   _storeController;
        private IAppleExtensions   _appleExtensions;
        private bool               _isInitialised;

        public bool IsInitialised => _isInitialised;
        public bool IsAdFree      => PlayerPrefs.GetInt("ads_removed", 0) == 1
                                  || IsSubscriptionActive(Products.SeasonPass);

        // ═════════════════════════════════════════════════════════════════════
        // Lifecycle
        // ═════════════════════════════════════════════════════════════════════

        void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        void Start() => InitialisePurchasing();

        // ═════════════════════════════════════════════════════════════════════
        // Initialisation
        // ═════════════════════════════════════════════════════════════════════

        void InitialisePurchasing()
        {
            var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

            // Consumables
            builder.AddProduct(Products.RookieKit,   ProductType.Consumable);
            builder.AddProduct(Products.CoinPack500,  ProductType.Consumable);
            builder.AddProduct(Products.CoinPack2500, ProductType.Consumable);

            // Non-consumable
            builder.AddProduct(Products.RemoveAds, ProductType.NonConsumable);

            // Auto-renewable subscription
            builder.AddProduct(Products.SeasonPass, ProductType.Subscription);

            UnityPurchasing.Initialize(this, builder);
        }

        // ── IStoreListener ────────────────────────────────────────────────────

        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            _storeController  = controller;
            _appleExtensions  = extensions.GetExtension<IAppleExtensions>();
            _isInitialised    = true;

            // Register deferred purchase handler (Ask-to-Buy on iOS)
            _appleExtensions?.RegisterPurchaseDeferredListener(OnDeferredPurchase);

            Debug.Log("[IAP] Initialised successfully.");
            OnInitialised?.Invoke();
        }

        public void OnInitializeFailed(InitializationFailureReason reason)
        {
            Debug.LogWarning($"[IAP] Init failed: {reason}");
            OnInitialiseFailed?.Invoke(reason.ToString());
        }

        public void OnInitializeFailed(InitializationFailureReason reason, string message)
        {
            Debug.LogWarning($"[IAP] Init failed: {reason} — {message}");
            OnInitialiseFailed?.Invoke(message);
        }

        // ═════════════════════════════════════════════════════════════════════
        // Purchase flow
        // ═════════════════════════════════════════════════════════════════════

        public void Purchase(string productId)
        {
            if (!_isInitialised)
            {
                Debug.LogWarning("[IAP] Not initialised — cannot purchase.");
                return;
            }
            var product = _storeController.products.WithID(productId);
            if (product == null || !product.availableToPurchase)
            {
                Debug.LogWarning($"[IAP] Product unavailable: {productId}");
                OnPurchaseFailed?.Invoke(productId);
                return;
            }
            _storeController.InitiatePurchase(product);
        }

        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
        {
            var productId = args.purchasedProduct.definition.id;

            if (!ValidateReceipt(args))
            {
                Debug.LogWarning($"[IAP] Receipt validation failed for {productId}.");
                return PurchaseProcessingResult.Complete;
            }

            DeliverProduct(productId);
            OnPurchaseSuccess?.Invoke(productId);
            SaveSystem.Save();
            return PurchaseProcessingResult.Complete;
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureReason reason)
        {
            Debug.LogWarning($"[IAP] Purchase failed: {product.definition.id} — {reason}");
            OnPurchaseFailed?.Invoke(product.definition.id);
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureDescription description)
        {
            Debug.LogWarning($"[IAP] Purchase failed: {product.definition.id} — {description.message}");
            OnPurchaseFailed?.Invoke(product.definition.id);
        }

        // ═════════════════════════════════════════════════════════════════════
        // Receipt validation
        // ═════════════════════════════════════════════════════════════════════

        bool ValidateReceipt(PurchaseEventArgs args)
        {
#if UNITY_EDITOR
            return true;   // skip validation in editor
#elif UNITY_IOS || UNITY_ANDROID
            try
            {
                // Apple/Google public keys are embedded in your game by Unity IAP.
                // CrossPlatformValidator checks the receipt signature.
                var validator = new CrossPlatformValidator(
                    GooglePlayTangle.Data(),
                    AppleTangle.Data(),
                    Application.identifier);

                var result = validator.Validate(args.purchasedProduct.receipt);
                foreach (var r in result)
                    Debug.Log($"[IAP] Receipt validated — productId: {r.productID}");
                return true;
            }
            catch (IAPSecurityException ex)
            {
                Debug.LogError($"[IAP] Receipt security exception: {ex.Message}");
                return false;
            }
#else
            return true;
#endif
        }

        // ═════════════════════════════════════════════════════════════════════
        // Product delivery
        // ═════════════════════════════════════════════════════════════════════

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
                default:
                    Debug.LogWarning($"[IAP] Unknown product delivered: {productId}");
                    break;
            }
        }

        // ═════════════════════════════════════════════════════════════════════
        // Restore & Subscription helpers
        // ═════════════════════════════════════════════════════════════════════

        public void RestorePurchases()
        {
#if UNITY_IOS
            _appleExtensions?.RestoreTransactions(result =>
            {
                Debug.Log(result
                    ? "[IAP] Restore completed."
                    : "[IAP] Restore failed or no purchases to restore.");
            });
#else
            Debug.Log("[IAP] RestorePurchases is only applicable on iOS.");
#endif
        }

        /// <summary>Returns true if the given subscription product is currently active.</summary>
        public bool IsSubscriptionActive(string productId)
        {
            if (!_isInitialised) return false;
            var product = _storeController?.products.WithID(productId);
            if (product == null || !product.hasReceipt) return false;

            try
            {
                var manager = new SubscriptionManager(product, null);
                return manager.getSubscriptionInfo().isSubscribed() == Result.True;
            }
            catch
            {
                return false;
            }
        }

        // ── Ask-to-Buy deferred purchase (iOS parental controls) ─────────────
        void OnDeferredPurchase(Product product) =>
            Debug.Log($"[IAP] Purchase deferred (Ask-to-Buy): {product.definition.id}");

        // ── Localised price helper (for shop UI) ──────────────────────────────
        public string GetLocalizedPrice(string productId)
        {
            if (!_isInitialised) return "–";
            var product = _storeController?.products.WithID(productId);
            return product?.metadata.localizedPriceString ?? "–";
        }
    }
}
