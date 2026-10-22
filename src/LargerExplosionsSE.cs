using System;
using System.Reflection;
using HarmonyLib;
using Sandbox.Game.Entities.Cube;
using VRage.Game.Components;
using VRageMath;
using System.Collections.Generic;
using VRage.Plugins; // <--- IMPORTANT: Added for IPlugin

namespace LargerExplosionsSE
{
    // Changed from a simple class to implementing IPlugin and IDisposable
    public class Loader : IPlugin, IDisposable
    {
        // This is the method the loader actually looks for
        public void Init(object gameInstance)
        {
            var harmony = new Harmony("com.larger.explosions.patch");
            harmony.PatchAll();
            
            // Pulsar usually captures Console.WriteLine, but Debug is safer for logs
            Console.WriteLine("[LargerExplosionsSE] Harmony patch applied: Blast radius cap removed!");
        }

        // Required by IPlugin interface
        public void Update()
        {
        }

        // Required by IDisposable interface
        public void Dispose()
        {
        }
    }

    [HarmonyPatch(typeof(MyWarhead), "GetExplosionSphere")]
    public class Patch_WarheadRadius
    {
        static void Postfix(ref BoundingSphereD __result, MyWarhead __instance)
        {
            try
            {
                var defField = typeof(MyWarhead).GetField("m_warheadDefinition", BindingFlags.NonPublic | BindingFlags.Instance);
                if (defField == null) return;

                var definition = defField.GetValue(__instance);
                var radiusProp = definition.GetType().GetProperty("ExplosionRadius");
                if (radiusProp == null) return;

                float sbcRadius = (float)radiusProp.GetValue(definition);

                // Overriding the 30m limit with the SBC value
                __result = new BoundingSphereD(__result.Center, (double)sbcRadius);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LargerExplosionsSE] Error in Postfix: {ex.Message}");
            }
        }
    }
}
