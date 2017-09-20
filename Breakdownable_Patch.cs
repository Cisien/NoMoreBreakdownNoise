using Harmony;
using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI.Group;

namespace JustFixIt
{
    [StaticConstructorOnStartup]
    public static class Breakdownable_Patch
    {
        static Breakdownable_Patch()
        {
            var harmony = HarmonyInstance.Create("com.cisien.nomorebreakdownnoise");
            var breakdownMethod = AccessTools.Method(typeof(CompBreakdownable), nameof(CompBreakdownable.DoBreakdown));
            var prefixMethod = new HarmonyMethod(typeof(Breakdownable_Patch).GetMethod(nameof(DoBreakdownPrefix)));
            var noOpMethod = new HarmonyMethod(typeof(Breakdownable_Patch).GetMethod(nameof(DoBreakdownNoOp)));
            harmony.Patch(breakdownMethod, prefixMethod, null, noOpMethod);
        }

        public static void DoBreakdownPrefix(CompBreakdownable __instance)
        {
            var _this = Traverse.Create(__instance);
            //orignal implementation
            _this.Field("brokenDownInt").SetValue(true);
            var parent = _this.Field("parent").GetValue<ThingWithComps>();
            parent.BroadcastCompSignal("Breakdown");
            parent.Map.GetComponent<BreakdownManager>().Notify_BrokenDown(parent);
            
            if (parent.Faction == Faction.OfPlayer)
            {
                //end original implentation
                DisplayMessage(parent);
            }
        }

        public static void DisplayMessage(ThingWithComps parent)
        {
            var message = new TransitionAction_Message("LetterLabelBuildingBrokenDown".Translate(new object[] { parent.LabelShort }), MessageSound.Standard, parent);
            message.DoAction(new Transition(null, null));
        }

        public static IEnumerable<CodeInstruction> DoBreakdownNoOp(IEnumerable<CodeInstruction> instructions)
        {
            yield break;
        }
    }
}