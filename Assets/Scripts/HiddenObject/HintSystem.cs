using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sherlock.HiddenObject
{
    /// <summary>
    /// Manages hint availability and reveals a random uncollected item.
    /// Hints recharge over time; the player may also spend coins for instant hints.
    /// </summary>
    public class HintSystem : MonoBehaviour
    {
        [SerializeField] private int   maxHints          = 3;
        [SerializeField] private float rechargeSeconds   = 60f;
        [SerializeField] private int   coinCostPerHint   = 20;

        public int  CurrentHints    { get; private set; }
        public float RechargeProgress { get; private set; }  // 0..1

        private float _rechargeTimer;
        private HiddenObjectController _controller;

        public System.Action<int> OnHintsChanged;

        void Awake() => CurrentHints = maxHints;

        public void Init(HiddenObjectController controller) => _controller = controller;

        void Update()
        {
            if (CurrentHints >= maxHints) { RechargeProgress = 1f; return; }

            _rechargeTimer   += Time.deltaTime;
            RechargeProgress  = _rechargeTimer / rechargeSeconds;

            if (_rechargeTimer >= rechargeSeconds)
            {
                _rechargeTimer = 0f;
                AddHint(1);
            }
        }

        public bool UseHint()
        {
            if (CurrentHints <= 0) return false;
            CurrentHints--;
            OnHintsChanged?.Invoke(CurrentHints);
            RevealRandomItem();
            return true;
        }

        public bool UseCoinHint(Sherlock.Core.GameState gs)
        {
            if (!gs.SpendCoins(coinCostPerHint)) return false;
            RevealRandomItem();
            return true;
        }

        void AddHint(int count)
        {
            CurrentHints = Mathf.Min(CurrentHints + count, maxHints);
            OnHintsChanged?.Invoke(CurrentHints);
        }

        void RevealRandomItem()
        {
            if (_controller == null) return;
            var uncollected = _controller.GetUncollectedItems();
            if (uncollected.Count == 0) return;
            int idx = Random.Range(0, uncollected.Count);
            uncollected[idx].RevealAndCollect();
        }
    }
}
