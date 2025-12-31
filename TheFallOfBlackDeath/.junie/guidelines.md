Project: The Fall Of The Black Death (Unity 2022.3.62f1)

This document records project-specific guidance for advanced contributors. It focuses on what is unique or easy to miss in this repository.

1) Build and configuration
- Unity version: 2022.3.62f1 LTS (as used in Rider). Open the folder TheFallOfBlackDeath in Unity with the exact editor version to avoid asset reimports and package churn.
- Target frameworks:
  - Runtime scripts compile into Assembly-CSharp targeting .NET Framework 4.x (Unity equivalent; Rider shows net471 target for editor context).
  - Avoid adding standalone .NET projects unless there is a strong reason; most code should live under Assets and be compiled by Unity.
- Packages: Packages/manifest.json and Packages/packages-lock.json are part of source control. Do not manually upgrade packages from Package Manager without testing the game scenes (combat, menu, and tesis scenes) as shaders, VFX, and input bindings are sensitive.
- Scenes and indices (observed from code):
  - SceneManager.LoadScene(1) is used after combat victory (see Assets/Scripts/CombatSystem/CombatManager.cs). Scene index mapping matters; keep Build Settings consistent when adding/removing scenes, or replace magic numbers with constants.
  - SceneManager.LoadSceneAsync(6) is used on defeat. Keep those entries present in Build Settings to prevent runtime errors.
- PlayerPrefs keys used by gameplay:
  - "GrupoEnemigo" is written after victory; if you modify enemy progression or persistence, keep this key stable or migrate it carefully.
- Audio and animation assumptions:
  - CombatManager triggers an AudioSource.Play() on victory and expects child Animators on the player to have a "Victory" state. Avoid renaming this animation state without updating the animator controllers.
- Save system hooks:
  - GameManager.Instance.SavePlayerState(GameManager.Instance.character1) is called at win/defeat. If SavePlayerState or character persistence is modified, verify combat win/lose flows.

2) Testing guidance (Unity Test Framework)
Use the Unity Test Framework (UTF) for EditMode and PlayMode tests. Avoid external dotnet test runners inside this repo; they are not wired to the Unity compilation pipeline and won’t see Unity types.

A. Configure an assembly for tests
- Create a folder Assets/Tests/EditMode or Assets/Tests/PlayMode.
- Add an Assembly Definition file (.asmdef) inside each test folder:
  - Name: TheFallOfBlackDeath.Tests.EditMode (or .PlayMode).
  - References: Assembly-CSharp (and any other local asmdefs your tests need).
  - Platforms: Editor only for EditMode; Any Platform for PlayMode.
  - Test assemblies: enable the “Test Assemblies” flag.

B. Minimal example tests
EditMode example (does not require scenes):

using NUnit.Framework;

namespace TheFallOfBlackDeath.Tests.EditMode
{
    public class MathSmokeTests
    {
        [Test]
        public void Addition_Works()
        {
            Assert.AreEqual(4, 2 + 2);
        }
    }
}

PlayMode example (scene-related, uses coroutines):

using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace TheFallOfBlackDeath.Tests.PlayMode
{
    public class CoroutineSmokeTests
    {
        [UnityTest]
        public IEnumerator WaitForFrames_Works()
        {
            yield return null; // 1 frame
            yield return null; // 2 frames
            Assert.Pass();
        }
    }
}

C. Running tests in Rider/Unity
- In Unity: Window > General > Test Runner. Choose EditMode or PlayMode and Run All.
- In Rider: open the Unity solution from the project root; Rider’s Unit Tests window will show Unity test categories when the asmdef has “Test Assemblies” enabled. Use the Run/Debug icons to execute.
- CI: If you later add CI, prefer Unity -runTests with -testPlatform editmode/playmode rather than dotnet test.

D. Adding new tests for this project
- Gameplay systems to target:
  - Combat system: test experience calculation and turn ordering using isolated fighter instances. Consider extracting pure logic (e.g., experience formula or speed sort) into static helpers to make EditMode tests deterministic.
  - Scene/state transitions: model scene indices behind constants and test mapping using EditMode tests to ensure indices exist in Build Settings via EditorSceneManager if needed.
  - Data persistence: wrap PlayerPrefs access behind an interface so EditMode tests can use a mock in-memory implementation.
- Guidelines:
  - Prefer EditMode tests for pure logic; they are faster and don’t need scene loads.
  - Use [UnityTest] coroutines for time/frames dependent logic.
  - Avoid relying on specific scene indices in tests; stub or load additive test scenes created under Assets/Tests/Scenes.

E. Troubleshooting test discovery
- If tests don’t appear, confirm the asmdef has “Test Assemblies” toggled and references Assembly-CSharp.
- Clear the Library folder only as a last resort; it will trigger a full reimport.

3) Additional development information
- Code style
  - C# 8/9 features are fine when compatible with Unity 2022.3 (nullable reference types supported in editor but be cautious for runtime allocations on IL2CPP).
  - Use PascalCase for public fields/properties, camelCase for private fields; prefer [SerializeField] private over public fields in MonoBehaviours.
- Scene management
  - Replace magic scene indices (1, 6) with a central enum or ScriptableObject that maps human-readable names to indices to reduce coupling. Update CombatManager accordingly.
- Coroutines and timing
  - CombatManager uses StartCoroutine and WaitForSeconds; keep gameplay-affecting waits short and configurable. For deterministic tests, abstract delays behind an interface.
- Animators and state names
  - The string literal "Victory" is used. Consider centralizing animator state names to avoid silent breakage when controllers change.
- Serialization and PlayerPrefs
  - Keys such as "GrupoEnemigo" and lists like ListEnemyDefeat.enemiesDefeat are part of progression. If changing, implement versioned migrations to retain saves.
- Project structure tips
  - Assets/Scripts/CombatSystem contains core battle flow; StatsManager and related classes are sensitive to balancing changes.
  - Movement system folders are named Movent_Sistem/Invet; avoid renaming without updating references/prefabs to prevent missing script issues.
- Packages and shaders
  - Visual effects and shader packages (e.g., Toony Colors, CFXR, KinoBloom) are present. Always test target scenes after upgrading as material/shader GUIDs and URP/HDRP compatibility can shift.

Appendix: Verified test process
- A minimal EditMode and PlayMode test structure is provided above and is compatible with Unity Test Framework. External dotnet test was intentionally not added because this environment is Unity-centric and dotnet CLI may not be available; use Unity’s Test Runner or Rider’s Unity tests integration.
