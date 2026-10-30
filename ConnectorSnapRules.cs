namespace Designer.Modules.CompactConnectors;

// Geometry decisions shared by the host adapters and the portable regression suite.
// The host still supplies domains, points, tolerance and guide endpoints.
internal static class ConnectorSnapRules
{
    internal static bool PreferPins(bool pinFound, double pinDelta, bool cornerFound, double cornerDelta) =>
        pinFound && (!cornerFound || Math.Abs(pinDelta) <= Math.Abs(cornerDelta));

    internal static bool HasMainBodyBottom(double height, double drawnHeight, double drawnWidth)
    {
        if (!double.IsFinite(height) || height <= 0 || height >= drawnHeight || !double.IsFinite(drawnWidth))
            return false;
        return true;
    }
}
