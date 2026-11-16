using UnityEngine;

namespace Ecos
{
    /// <summary>
    /// Centralized Observer / Delegate Event Manager following the course pattern.
    /// Other components subscribe in OnEnable (+=) and unsubscribe in OnDisable (-=).
    /// </summary>
    public class EventManager : GenericSingleton<EventManager>
    {
        // Delegate declarations
        public delegate void GameStateDelegate(GameStates newState);
        public delegate void ZoneChangedDelegate(int zoneIndex, string zoneName);
        public delegate void DocumentCollectedDelegate(string documentId);
        public delegate void MemoryUnlockedDelegate(int memoryIndex);
        public delegate void ParticleBurstDelegate(Vector3 position, Color color);

        // Events for delegates
        public event GameStateDelegate OnGameStateChanged;
        public event ZoneChangedDelegate OnZoneChanged;
        public event DocumentCollectedDelegate OnDocumentCollected;
        public event MemoryUnlockedDelegate OnMemoryUnlocked;
        public event ParticleBurstDelegate OnParticleBurstRequested;

        public void TriggerGameStateChanged(GameStates newState)
        {
            OnGameStateChanged?.Invoke(newState);
        }

        public void TriggerZoneChanged(int zoneIndex, string zoneName)
        {
            OnZoneChanged?.Invoke(zoneIndex, zoneName);
        }

        public void TriggerDocumentCollected(string documentId)
        {
            OnDocumentCollected?.Invoke(documentId);
        }

        public void TriggerMemoryUnlocked(int memoryIndex)
        {
            OnMemoryUnlocked?.Invoke(memoryIndex);
        }

        public void TriggerParticleBurst(Vector3 position, Color color)
        {
            OnParticleBurstRequested?.Invoke(position, color);
        }
    }
}
