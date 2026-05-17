using Sherlock.Core;
using Sherlock.Data;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Sherlock.HiddenObject
{
    /// <summary>
    /// Placed on every clickable object in a Hidden Object scene.
    /// Handles tap detection, already-found guard, and transfer to GameState inventory.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class HiddenObjectItem : MonoBehaviour, IPointerClickHandler
    {
        [Header("Item Definition")]
        [SerializeField] private ItemData itemData;
        [SerializeField] private string   objectId;   // unique within the scene, e.g. "candle_01"

        [Header("Visual Feedback")]
        [SerializeField] private Animator         animator;
        [SerializeField] private ParticleSystem   collectFx;
        [SerializeField] private AudioClip        collectSfx;
        [SerializeField] private SpriteRenderer   glowRenderer;

        private HiddenObjectController _controller;
        private bool                   _collected;
        private static AudioSource     _sharedAudio;

        // ── Init ──────────────────────────────────────────────────────────────

        public void Init(HiddenObjectController controller)
        {
            _controller = controller;

            // Restore already-found state on scene re-load
            if (GameState.Instance.HasFound(_controller.SceneId, objectId))
            {
                MarkAsCollected(playFx: false);
            }
        }

        // ── Input ─────────────────────────────────────────────────────────────

        public void OnPointerClick(PointerEventData eventData)
        {
            if (_collected) return;
            Collect();
        }

        // ── Public helpers ────────────────────────────────────────────────────

        public bool IsCollected => _collected;
        public string ObjectId  => objectId;
        public ItemData Data    => itemData;

        /// <summary>Externally triggers collection (e.g. from hint system).</summary>
        public void RevealAndCollect()
        {
            if (_collected) return;
            if (glowRenderer) glowRenderer.enabled = true;
            // Give the player a moment to see the highlight before auto-collecting
            Invoke(nameof(Collect), 0.6f);
        }

        // ── Private ───────────────────────────────────────────────────────────

        void Collect()
        {
            if (_collected) return;
            MarkAsCollected(playFx: true);
            GameState.Instance.MarkFound(_controller.SceneId, objectId);

            // Push to merge board inventory
            if (itemData != null)
                GameState.Instance.AddToPendingInventory(itemData.itemId);

            _controller.OnObjectFound(this);
        }

        void MarkAsCollected(bool playFx)
        {
            _collected = true;
            GetComponent<Collider2D>().enabled = false;

            if (playFx)
            {
                collectFx?.Play();
                PlaySfx(collectSfx);
                animator?.SetTrigger("Collect");
            }

            // Fade out after particle burst
            Invoke(nameof(HideSelf), playFx ? 0.8f : 0f);
        }

        void HideSelf() => gameObject.SetActive(false);

        void PlaySfx(AudioClip clip)
        {
            if (clip == null) return;
            if (_sharedAudio == null)
            {
                var go = new GameObject("HO_AudioSource");
                _sharedAudio = go.AddComponent<AudioSource>();
                DontDestroyOnLoad(go);
            }
            _sharedAudio.PlayOneShot(clip);
        }
    }
}
