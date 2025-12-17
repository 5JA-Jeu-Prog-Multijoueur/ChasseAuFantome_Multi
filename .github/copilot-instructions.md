# Copilot instructions for ChasseAuFantome_Multi

Short, actionable guidance for AI editing this Unity (Netcode) project.

1. Big picture
- Unity project (see `ProjectSettings/ProjectVersion.txt`: m_EditorVersion: 6000.2.11f1).
- Multiplayer built with Unity Netcode for GameObjects (see `Packages/manifest.json` for `com.unity.netcode.gameobjects`) and Unity Transport/Relay (see `Assets/Scripts/Reseau/RelayManager.cs`).
- Server-authoritative model: server writes NetworkVariables; clients send intent via `ServerRpc`.

2. Key components & boundaries
- Network orchestration: `Assets/Scripts/Reseau/GameManager.cs` (singleton, spawns players, scene loads).
- Relay & services: `Assets/Scripts/Reseau/RelayManager.cs` (uses Unity Services Authentication + Relay). Requires Unity Services configured to run Relay.
- UI & flow: `Assets/Scripts/Reseau/NavigationManager.cs` controls host/client selection and panels.
- Player role & visuals: `Assets/Scripts/CtrlJoueurMulti/JoueurData.cs` and `Assets/Scripts/CtrlJoueurMulti/PlayerRole.cs` — pattern: disable visuals/controllers on spawn, subscribe to `NetworkVariable.OnValueChanged`, then apply current value.
- Player controllers: `Assets/Scripts/ControleJoueur/*` (e.g., `JoueurChasseur.cs`, `JoueurFantome.cs`) — input guarded with `IsOwner`, server-only actions guarded with `IsServer`.

3. Network conventions to preserve
- NetworkVariables: usually `NetworkVariableReadPermission.Everyone` + `NetworkVariableWritePermission.Server`. Do not change write authority unless intentionally switching to owner-writes.
- Always subscribe to `OnValueChanged` BEFORE applying state, and apply the current value right away (see `PlayerData.OnNetworkSpawn`).
- Owner vs server checks:
  - Use `if (!IsOwner) return;` inside `Update()` for local input handlers (see `JoueurChasseur.Update`).
  - Use `if (!IsServer) return;` for server-side physics, raycasts, damage application (see `JoueurChasseur.DoRaycast`).
- RPC usage:
  - Client->Server: `[ServerRpc]` (e.g., `ToggleLampeServerRpc`).
  - Server spawns and assigns role values (see `GameManager.SpawnJoueursDansScene` where `pdata.role.Value = ...`).

4. Build / run notes (developer workflow)
- Open project with Unity Editor matching `ProjectVersion.txt` (6000.x series) to avoid package conflicts.
- Packages: ensure packages in `Packages/manifest.json` are installed (Netcode, Unity Transport, Services). If tests fail, reimport packages via Package Manager.
- Relay testing: enable Unity Services in the Editor, sign-in (RelayManager calls `UnityServices.InitializeAsync()`), and create a Relay allocation or use local IP via `NetworkManager` (`GameManager.LancementHote/LancementClient`).
- Typical playflow in editor: run host via UI (NavigationManager -> RelayManager.ConfigureTransportAndStartNgoAsHost) or set IP and use `GameManager.LancementClient`.

5. Project-specific patterns and pitfalls
- French naming & comments: many symbols and comments are French — preserve names to avoid breaking prefab bindings (`JoueurChasseur`, `chasseurRoot`, etc.).
- Prefab bindings: many serialized fields are wired in prefabs (player prefab references `PlayerData`, controllers, visuals). Avoid renaming serialized fields unless you update prefabs.
- Singleton usage: `GameManager.singleton`, `NavigationManager.singleton`, `Spawner.singleton`, `RelayManager.instance` — follow the singleton lifetime assumptions (DontDestroyOnLoad for GameManager).
- Spawning/ownership: `netObj.SpawnWithOwnership(OwnerClientId)` pattern is used; changing spawn ownership semantics can break role assignment.

6. Files to inspect when modifying networking code
- `Assets/Scripts/Reseau/GameManager.cs` — scene/load/spawn logic
- `Assets/Scripts/CtrlJoueurMulti/JoueurData.cs` — role application pattern
- `Assets/Scripts/Reseau/RelayManager.cs` — Relay + Services integration
- `Assets/Scripts/ControleJoueur/JoueurChasseur.cs` — input, raycast, ServerRpc example
- `Packages/manifest.json` and `ProjectSettings/ProjectVersion.txt`

7. If you need to change behavior
- Always keep authority model explicit: decide whether action is owner-driven or server-driven and update checks and NetworkVariable write permissions accordingly.
- When changing serialized field names, update prefab references in `Assets/Prefabs` (open prefab in Editor).

8. Tests & debugging tips
- Use Unity Console logs (many scripts already use `Debug.Log`) and run host/client in Editor/Editor or Editor/Standalone to reproduce networking behavior.
- For Relay failures, check Unity Services logs and ensure `AuthenticationService.Instance.SignInAnonymouslyAsync()` succeeds (see `RelayManager.AuthenticatePlayer`).

If any section is unclear or you'd like more examples (prefab wiring, a short checklist to run a local host + client), say which part and I'll expand or adjust the instructions.
