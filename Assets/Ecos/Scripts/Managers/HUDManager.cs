using UnityEngine;
using UnityEngine.Events;

namespace Ecos
{
    /// <summary>
    /// HUDManager using GenericSingleton and Observer pattern (subscribing to EventManager in OnEnable/OnDisable).
    /// </summary>
    public class HUDManager : GenericSingleton<HUDManager>
    {
        public GameUI gameUI;

        [Header("Observed State")]
        public GameStates observedState = GameStates.GameStart;
        public string currentZoneName = "";
        public int collectedDocumentsCount = 0;
        public int unlockedMemoriesCount = 0;

        [Header("Unity Events")]
        public UnityEvent OnZoneBannerEvent = new UnityEvent();
        public UnityEvent OnDocumentAddedEvent = new UnityEvent();
        public UnityEvent OnMemoryUnlockedEvent = new UnityEvent();

        private void OnEnable()
        {
            if (EventManager.Instance == null) return;
            EventManager.Instance.OnGameStateChanged += HandleGameStateChanged;
            EventManager.Instance.OnZoneChanged += HandleZoneChanged;
            EventManager.Instance.OnDocumentCollected += HandleDocumentCollected;
            EventManager.Instance.OnMemoryUnlocked += HandleMemoryUnlocked;
        }

        private void OnDisable()
        {
            if (EventManager.Instance == null) return;
            EventManager.Instance.OnGameStateChanged -= HandleGameStateChanged;
            EventManager.Instance.OnZoneChanged -= HandleZoneChanged;
            EventManager.Instance.OnDocumentCollected -= HandleDocumentCollected;
            EventManager.Instance.OnMemoryUnlocked -= HandleMemoryUnlocked;
        }

        private void HandleGameStateChanged(GameStates newState)
        {
            observedState = newState;
        }

        private void HandleZoneChanged(int zoneIndex, string zoneName)
        {
            currentZoneName = zoneName;
            OnZoneBannerEvent?.Invoke();
        }

        private void HandleDocumentCollected(string documentId)
        {
            collectedDocumentsCount++;
            OnDocumentAddedEvent?.Invoke();
        }

        private void HandleMemoryUnlocked(int memoryIndex)
        {
            unlockedMemoriesCount++;
            OnMemoryUnlockedEvent?.Invoke();
        }
    }
}
