using System.Collections;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;
using HarmonyLib;

namespace Designer.Modules.CompactConnectors;

// Compare two native snap results. Keeping their point sets separate prevents
// a connector corner from snapping to a pin (or its pin to an object corner).
internal sealed class ConnectorCornerSnapping
{
    private const BindingFlags Instance = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
    private readonly Type connector, moveDrag;
    private readonly ConstructorInfo createTool, createPoint;
    private readonly MethodInfo build, calculate, getPoints, addDrag, calcAxis;
    private readonly FieldInfo sources, snapTool, snap, pointX, pointY;
    private readonly FieldInfo[] endpoints, sorted;
    private readonly object defaultDomain;
    private readonly ConditionalWeakTable<object, object> corners = new();
    private static ConnectorCornerSnapping? active;
    private static int failures;

    internal ConnectorCornerSnapping(Assembly host)
    {
        Type T(string name) => host.GetType("QSC.QSys." + name, true)!;
        FieldInfo F(Type t, string name) => t.GetField(name, Instance) ?? throw new MissingFieldException(t.FullName, name);
        MethodInfo M(Type t, string name, params Type[] parameters) => t.GetMethod(name, Instance, null, parameters, null)
            ?? throw new MissingMethodException(t.FullName, name);
        connector = T("Graphic.ContainerPin"); moveDrag = T("Tools.MoveToolDrag");
        var canvas = T("Graphic.GraphicCanvas"); var tool = T("Tools.SnapTool"); var domain = T("Tools.SnapTool+Domain");
        var point = T("Tools.ToolPoint"); var points = typeof(List<>).MakeGenericType(point); var snapped = T("Drag.SnappedAdornerDrag");
        createTool = tool.GetConstructor([canvas, typeof(bool), domain]) ?? throw new MissingMethodException("SnapTool constructor");
        createPoint = point.GetConstructor([typeof(Point)]) ?? throw new MissingMethodException("ToolPoint constructor");
        defaultDomain = Enum.Parse(domain, "Default");
        build = M(snapped, "CalculateSnap", canvas, typeof(bool));
        calculate = M(tool, "CalculateSnap", typeof(Point)); addDrag = M(tool, "AddToDrag", typeof(Point));
        getPoints = M(T("Graphic.Object"), "GetSnapPoints", points.MakeByRefType(), domain);
        calcAxis = M(tool, "_calc", points, points, point, point, point, point.MakeByRefType(), point.MakeByRefType(), point.MakeByRefType());
        sources = F(moveDrag, "_SourceObjects"); snapTool = F(snapped, "_Snap"); snap = F(tool, "_Snap");
        pointX = F(point, "X"); pointY = F(point, "Y");
        endpoints = new[] { "_XSnapEndpointA", "_XSnapEndpointB", "_YSnapEndpointA", "_YSnapEndpointB" }.Select(n => F(tool, n)).ToArray();
        sorted = new[] { "_dragPtsSortedX", "_snapPtsSortedX", "_dragPtsSortedY", "_snapPtsSortedY" }.Select(n => F(tool, n)).ToArray();
        foreach (var target in new[] { build, calculate })
            if (Harmony.GetPatchInfo(target)?.Owners.Any() == true) throw new InvalidOperationException("Drag snapping is already patched: " + target.Name);
    }

    internal void Install(Harmony harmony)
    {
        active = this;
        harmony.Patch(build, postfix: new HarmonyMethod(typeof(ConnectorCornerSnapping), nameof(PrepareCorners)));
        harmony.Patch(calculate, postfix: new HarmonyMethod(typeof(ConnectorCornerSnapping), nameof(Combine)));
    }

    private static void PrepareCorners(object __instance, object[] __args)
    {
        try
        {
            var h = active!;
            if (!h.moveDrag.IsInstanceOfType(__instance)) return;
            var moving = ((IEnumerable)h.sources.GetValue(__instance)!).Cast<object>().ToArray();
            if (moving.Length == 0 || moving.Any(o => !h.connector.IsInstanceOfType(o))) return;
            var native = h.snapTool.GetValue(__instance)!;
            var corner = h.createTool.Invoke([__args[0], __args[1], h.defaultDomain]);
            // Match the native drag lifecycle, including Alt/copy and canvas changes.
            foreach (var graphic in moving)
            {
                object[] args = [Activator.CreateInstance(h.getPoints.GetParameters()[0].ParameterType.GetElementType()!)!, h.defaultDomain];
                h.getPoints.Invoke(graphic, args);
                foreach (var p in (IEnumerable)args[0]) h.addDrag.Invoke(corner, [h.Point(p)]);
            }
            h.corners.Remove(native); h.corners.Add(native, corner);
        }
        catch (Exception error) { Report(error); }
    }

    private readonly record struct Axis(bool Found, double Delta, Point Target, Point Moving);
    internal static bool PreferPins(bool pinFound, double pinDelta, bool cornerFound, double cornerDelta) =>
        ConnectorSnapRules.PreferPins(pinFound, pinDelta, cornerFound, cornerDelta);

    private static void Combine(object __instance, Point __0, ref bool __result)
    {
        try
        {
            var h = active!;
            if (!h.corners.TryGetValue(__instance, out var corner)) return;
            _ = h.calculate.Invoke(corner, [__0]); // Unpaired native instance; no recursive combination.
            var pinX = h.ReadAxis(__instance, __0, false); var pinY = h.ReadAxis(__instance, __0, true);
            var cornerX = h.ReadAxis(corner, __0, false); var cornerY = h.ReadAxis(corner, __0, true);
            var x = PreferPins(pinX.Found, pinX.Delta, cornerX.Found, cornerX.Delta) ? pinX : cornerX;
            var y = PreferPins(pinY.Found, pinY.Delta, cornerY.Found, cornerY.Delta) ? pinY : cornerY;
            var adjustment = new Vector(x.Found ? x.Delta : 0, y.Found ? y.Delta : 0);
            // The native renderer consumes these same fields. Keep guide endpoints
            // consistent with the combined X/Y displacement, including exact matches.
            var values = new[] { x.Found ? x.Target : new Point(), x.Found ? x.Moving + adjustment : new Point(),
                y.Found ? y.Target : new Point(), y.Found ? y.Moving + adjustment : new Point() };
            var boxed = values.Select(h.ToolPoint).ToArray();
            var combined = h.ToolPoint(new Point(adjustment.X, adjustment.Y));
            for (int i = 0; i < endpointsCount; i++) h.endpoints[i].SetValue(__instance, boxed[i]);
            h.snap.SetValue(__instance, combined); __result = x.Found || y.Found;
        }
        catch (Exception error) { Report(error); }
    }
    private const int endpointsCount = 4;

    private Axis ReadAxis(object tool, Point delta, bool vertical)
    {
        int i = vertical ? 2 : 0;
        var a = Point(endpoints[i].GetValue(tool)!); var b = Point(endpoints[i + 1].GetValue(tool)!);
        var adjustment = Point(snap.GetValue(tool)!);
        // Native CalculateSnap adds the full displacement to both B endpoints,
        // even when one axis missed. Remove it before checking the miss sentinel.
        var moving = b - (Vector)adjustment;
        if (a != new Point() || moving != new Point())
            return new(true, vertical ? adjustment.Y : adjustment.X, a, moving);
        // A legitimate match at the origin has zero-valued endpoints, just like
        // a miss. Ask the native axis solver in that ambiguous case.
        object[] args = [sorted[i].GetValue(tool)!, sorted[i + 1].GetValue(tool)!,
            ToolPoint(vertical ? new(0, 1) : new(1, 0)), ToolPoint(vertical ? new(1, 0) : new(0, 1)), ToolPoint(delta),
            ToolPoint(new()), ToolPoint(new()), ToolPoint(new())];
        bool found = (bool)calcAxis.Invoke(tool, args)!;
        var correction = Point(args[5]);
        return new(found, vertical ? correction.Y : correction.X, Point(args[6]), Point(args[7]));
    }

    private Point Point(object value) => new((double)pointX.GetValue(value)!, (double)pointY.GetValue(value)!);
    private object ToolPoint(Point value) => createPoint.Invoke([value]);
    private static void Report(Exception error)
    {
        if (failures++ < 5) ConnectorHooks.Report?.Invoke("connector-corner-snap-error", new { error = error.GetBaseException().Message });
    }
}
