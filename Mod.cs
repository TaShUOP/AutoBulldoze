using Colossal.Logging;
using Game;
using Game.Modding;
using Game.SceneFlow;

namespace AutoBulldoze
{
    public class Mod : IMod
    {
        public static ILog Log = LogManager.GetLogger($"{nameof(AutoBulldoze)}.{nameof(Mod)}").SetShowsErrorsInUI(false);

        public void OnLoad(UpdateSystem updateSystem)
        {
            Log.Info(nameof(OnLoad));

            if (GameManager.instance.modManager.TryGetExecutableAsset(this, out var asset))
                Log.Info($"Current mod asset at {asset.path}");

            // Register our custom system to run during the Game Simulation phase
            updateSystem.UpdateAfter<AutoBulldozeSystem>(SystemUpdatePhase.GameSimulation);
        }

        public void OnDispose()
        {
            Log.Info(nameof(OnDispose));
        }
    }
}
