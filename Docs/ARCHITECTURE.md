# Sherlock: Hidden Evidence & Mystery Merge — Architecture

## Class Diagram (logical)

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                              DATA LAYER                                     │
│                                                                             │
│  ┌──────────────┐   ┌──────────────┐   ┌──────────────────────────────┐   │
│  │  ItemData    │   │  QuestData   │   │  ItemDatabase (singleton)    │   │
│  │ (ScriptObj.) │   │ (ScriptObj.) │   │  • _items: Dict<id, ItemData>│   │
│  │──────────────│   │──────────────│   │  • Get(id): ItemData         │   │
│  │ itemId       │   │ questId      │   │  • GetMergeResult(id)        │   │
│  │ displayName  │   │ title        │   └─────────────┬────────────────┘   │
│  │ icon         │   │ narrative    │                 │ loads from          │
│  │ tier         │   │ questType    │                 │ Resources/Items/    │
│  │ mergeResult──┼──▶│ targetItemId │                 │                     │
│  │ sellValue    │   │ targetCount  │                 │                     │
│  │ isQuestReward│   │ unlocksScene │                 │                     │
│  └──────────────┘   └──────────────┘                 │                     │
└─────────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────────┐
│                              CORE LAYER                                     │
│                                                                             │
│  ┌──────────────────────────────────────────────────────────────────────┐  │
│  │  GameState (singleton, DontDestroyOnLoad)                            │  │
│  │  • Coins: int                                                         │  │
│  │  • PendingInventory: Dict<itemId, count>                              │  │
│  │  • FoundObjects: Dict<sceneId, Set<objectId>>                        │  │
│  │  • CompletedQuests: Set<questId>                                     │  │
│  │  • UnlockedScenes: Set<sceneId>                                      │  │
│  │  Events: OnCoinsChanged, OnInventoryChanged                           │  │
│  └──────────────────────────┬───────────────────────────────────────────┘  │
│                             │ read/write                                    │
│  ┌──────────────────────────▼──────────────────────────────────────────┐   │
│  │  SaveSystem (static)                                                 │   │
│  │  • Save() → gamestate.json  (JsonUtility)                            │   │
│  │  • Load() ← gamestate.json                                           │   │
│  │  MergeManager also writes → mergeboard.json (separate)               │   │
│  └──────────────────────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────────┐
│                      HIDDEN OBJECT MODULE                                   │
│                                                                             │
│  ┌──────────────────────────────────────────────────────────────────────┐  │
│  │  HiddenObjectController (per-scene prefab root)                      │  │
│  │  • _allItems: List<HiddenObjectItem>                                  │  │
│  │  • Pan / Pinch-to-Zoom (Touch + Mouse fallback)                      │  │
│  │  • OnObjectFound(item) → GameState.AddToPendingInventory             │  │
│  │  Events: OnItemFound, OnSceneComplete                                 │  │
│  └────────────────────────────┬─────────────────────────────────────────┘  │
│                               │ owns N                                      │
│  ┌────────────────────────────▼─────────────────────────────────────────┐  │
│  │  HiddenObjectItem (per collectable object in scene)                  │  │
│  │  • Data: ItemData                                                     │  │
│  │  • IPointerClickHandler.OnPointerClick → Collect()                   │  │
│  │  • RevealAndCollect() (called by HintSystem)                         │  │
│  └──────────────────────────────────────────────────────────────────────┘  │
│                                                                             │
│  ┌──────────────────────────────────────────────────────────────────────┐  │
│  │  HintSystem                                                           │  │
│  │  • CurrentHints: int (recharges over time)                           │  │
│  │  • UseHint() → reveal random uncollected item                        │  │
│  │  • UseCoinHint(gs) → spend coins for instant hint                    │  │
│  └──────────────────────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────────────────┘

         HO finds item → GameState.AddToPendingInventory(itemId)
                                        │
                        GameState.OnInventoryChanged fires
                                        │
                                        ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                         MERGE BOARD MODULE                                  │
│                                                                             │
│  ┌──────────────────────────────────────────────────────────────────────┐  │
│  │  MergeManager (singleton)                                             │  │
│  │  • _grid: MergeCell[columns, rows]                                   │  │
│  │  • SpawnItem(itemId/data) → place on first free cell                 │  │
│  │  • SellItem(item) → award coins, destroy item                        │  │
│  │  • OnItemPickUp / OnItemDrag / OnItemDrop → merge or place           │  │
│  │  • ExecuteMerge() → destroy 2 items, spawn merged result             │  │
│  │  • SaveBoard() / LoadBoard() → mergeboard.json                        │  │
│  │  Events: OnMergeCompleted, OnItemSold, OnBoardFull                   │  │
│  └─────────────────────────┬────────────────────────────────────────────┘  │
│                            │ owns N×M                                       │
│  ┌─────────────────────────▼────────────────────────────────────────────┐  │
│  │  MergeCell                                                            │  │
│  │  • GridPosition: Vector2Int                                           │  │
│  │  • CurrentItem: MergeItem                                             │  │
│  │  • SetHighlight(state)  — None / Hover / Blocked                     │  │
│  └──────────────────────────────────────────────────────────────────────┘  │
│                                                                             │
│  ┌──────────────────────────────────────────────────────────────────────┐  │
│  │  MergeItem (drag-and-drop, per board item)                            │  │
│  │  • Data: ItemData                                                     │  │
│  │  • IPointerDownHandler / IDragHandler / IPointerUpHandler            │  │
│  │  • Forwards events to MergeManager                                   │  │
│  └──────────────────────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────────────────┘

         MergeManager.OnMergeCompleted(itemId) ──▶ QuestManager.OnItemCrafted()

┌─────────────────────────────────────────────────────────────────────────────┐
│                        QUEST / STORY MODULE                                 │
│                                                                             │
│  ┌──────────────────────────────────────────────────────────────────────┐  │
│  │  QuestManager (singleton)                                             │  │
│  │  • _activeQuest: QuestData                                            │  │
│  │  • OnItemCrafted(itemId) → check quest conditions                    │  │
│  │  • OnSceneCompleted(sceneId) → check quest conditions                │  │
│  │  • CompleteQuest() → unlock scene, reward coins/items, chain next    │  │
│  │  Events: OnQuestCompleted, OnQuestActivated                           │  │
│  └──────────────────────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────────┐
│                            UI LAYER                                         │
│                                                                             │
│  ┌──────────────────────────────────────────────────────────────────────┐  │
│  │  GameUIManager (singleton, persistent canvas)                         │  │
│  │  • SwitchToView(Investigation | Analysis)                             │  │
│  │  • ShowItemFoundPopup(data)                                           │  │
│  │  • ShowQuestBanner(quest) / ShowQuestCompleteScreen(quest)            │  │
│  │  • UpdateCoinLabel / UpdateItemCount / UpdateHintUI                  │  │
│  └──────────────────────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────────┐
│                          META SYSTEMS                                       │
│                                                                             │
│  ┌────────────────────────────┐   ┌───────────────────────────────────┐   │
│  │  IAPManager (stub)         │   │  LeaderboardService (stub)        │   │
│  │  • Products: const IDs     │   │  • FetchTopScores(count)          │   │
│  │  • Purchase(productId)     │   │  • SubmitScore(score)             │   │
│  │  • DeliverProduct(id)      │   │  • REST + stub editor data        │   │
│  │  • RestorePurchases()      │   └───────────────────────────────────┘   │
│  └────────────────────────────┘                                            │
└─────────────────────────────────────────────────────────────────────────────┘
```

## Merge Chain Example — Baker Street Chapter 1

```
Stufe 1: letter_fragment  (Briefschnipsel)  — found in HO scene
             +
         letter_fragment
             ↓  merge
Stufe 2: sealed_letter    (Versiegelter Brief)
             +
         sealed_letter
             ↓  merge
Stufe 3: encrypted_doc    (Verschlüsseltes Dokument) ← QuestGate triggers chapter unlock
             +
         encrypted_doc
             ↓  merge
Stufe 4: decoded_message  (Entschlüsselte Nachricht)
             +
         decoded_message
             ↓  merge
Stufe 5: forensics_kit    (Forensik-Kit) ← isQuestReward = true → QuestManager unlocks Chapter 2
```

## Scene Flow

```
Bootstrap Scene (DontDestroyOnLoad singletons)
  └─ GameState, ItemDatabase, MergeManager, QuestManager,
     GameUIManager, IAPManager, LeaderboardService, SaveSystem

Investigation Scene (loaded additively or by scene manager)
  └─ HiddenObjectController
       └─ N × HiddenObjectItem
       └─ HintSystem

 ── player finds all items → scene complete → SwitchToView(Analysis) ──▶

Analysis Scene (Merge Board)
  └─ MergeManager (already persistent)
       └─ MergeCell[6×8]
       └─ MergeItem instances

 ── player crafts forensics_kit → QuestManager.CompleteQuest()
    → UnlockedScenes.Add("chapter2_scene")
    → next Investigation Scene is now available ──▶ loop
```

## Key Design Decisions

| Decision | Rationale |
|---|---|
| Two JSON save files | `gamestate.json` (currency, quest, inventory) and `mergeboard.json` (board layout) load/save independently, reducing write frequency during gameplay |
| ScriptableObject for ItemData | Designer-friendly, asset-based; no code change for new merge chains |
| MergeItem drives its own drag input | Encapsulates per-item touch logic; MergeManager only makes decisions |
| Pending inventory queue | Decouples HO discovery speed from board-placement animation; items queue up cleanly when board is full |
| HintSystem charges over time + coin purchase | Two monetisation levers; recharge keeps F2P players engaged; coin purchase is optional |

## iOS/iPadOS-specific notes

- Pinch-to-zoom uses `Input.touchCount == 2` + `Vector2.Distance` ratio — no additional plugin required
- `EventSystem.current.IsPointerOverGameObject(fingerId)` prevents pan from firing through UI buttons
- `Application.persistentDataPath` resolves to the iOS app sandbox Documents folder — safe for App Store review
- `PlayerPrefs` stores ad-removal and season-pass flags — backed by iOS NSUserDefaults
- Target minimum deployment: **iOS 16.0** (set in Player Settings > Other Settings > Target minimum iOS Version)
