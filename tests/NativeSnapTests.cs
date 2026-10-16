using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;
using Designer.Modules.CompactConnectors;
using HarmonyLib;

internal static class NativeSnapTests
{
    // Executes the installed host's geometry-only snap solver with explicit
    // points. No Designer App, window, design or input device is initialized.
    internal static void Run(Assembly assembly, Action<bool, string> check)
    {
        var bridge = new ConnectorCornerSnapping(assembly);
        var harmony = new Harmony("qsys.compact-connectors.tests.dual-snap");
        var type = assembly.GetType("QSC.QSys.Tools.SnapTool", true)!;
        var calculate = type.GetMethod("CalculateSnap")!;
        var pairs = (ConditionalWeakTable<object, object>)typeof(ConnectorCornerSnapping)
            .GetField("corners", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(bridge)!;
        object Tool(Point moving, Point target)
        {
            var tool = Activator.CreateInstance(type)!;
            type.GetMethod("AddToDrag")!.Invoke(tool, [moving]); type.GetMethod("AddToSnap")!.Invoke(tool, [target]); return tool;
        }
        Point Snap(object tool) => (Point)type.GetProperty("Snap")!.GetValue(tool)!;
        bool Calculate(object tool, Point delta) => (bool)calculate.Invoke(tool, [delta])!;
        bridge.Install(harmony);
        try
        {
            var pin = Tool(new(6, 5), new(106, 106)); var corner = Tool(new(0, 0), new(100, 100)); pairs.Add(pin, corner);
            check(Calculate(pin, new(100, 100)) && Snap(pin).Y == 0, "native solver retains exact corner alignment");
            check(Calculate(pin, new(100, 101)) && Snap(pin).Y == 0, "native solver retains adjacent exact pin alignment");
            check(Calculate(pin, new(100, 103)) && Snap(pin).Y == -2, "nearer pin row wins above the component");
            check(Calculate(pin, new(100, 98)) && Snap(pin).Y == 2, "nearer corner row wins below the component");
            check(Calculate(pin, new(100, 106)) && Snap(pin).Y == -5, "corner-to-pin cross-pairing cannot create a false exact snap");
            Point Endpoint(string name)
            {
                var value = type.GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(pin)!;
                return new((double)value.GetType().GetField("X")!.GetValue(value)!, (double)value.GetType().GetField("Y")!.GetValue(value)!);
            }
            check(Endpoint("_XSnapEndpointA").X == Endpoint("_XSnapEndpointB").X
                && Endpoint("_YSnapEndpointA").Y == Endpoint("_YSnapEndpointB").Y, "native guide endpoints use the final combined displacement");
            check(Calculate(pin, new(100, 100)) && Endpoint("_XSnapEndpointA") == new Point(106, 106)
                && Endpoint("_YSnapEndpointA") == new Point(100, 100), "X and Y can independently choose pin and corner targets");
            check(!Calculate(pin, new(1000, 1000)) && Snap(pin) == new Point(), "both domains retain native outside-tolerance behavior");
            var origin = Tool(new(0, 0), new(0, 0)); var far = Tool(new(100, 100), new(200, 200)); pairs.Add(origin, far);
            check(Calculate(origin, new()) && Snap(origin) == new Point(), "exact origin match is distinct from no target");
            var verticalOnly = Tool(new(6, 5), new(200, 106)); var horizontalOnly = Tool(new(0, 0), new(101, 200)); pairs.Add(verticalOnly, horizontalOnly);
            check(Calculate(verticalOnly, new(100, 100)) && Snap(verticalOnly) == new Point(1, 1),
                "missing pin X and missing corner Y cannot masquerade as exact matches after endpoint adjustment");
            var ordinary = Tool(new(6, 5), new(106, 106));
            check(Calculate(ordinary, new(100, 100)) && Snap(ordinary).Y == 1, "unpaired wire and Snake snap instances retain native pin-only behavior");
            harmony.UnpatchAll(harmony.Id);
            check(Calculate(pin, new(100, 100)) && Snap(pin).Y == 1, "removing hooks restores native pin-only calculation");
        }
        finally { harmony.UnpatchAll(harmony.Id); }
    }
}
