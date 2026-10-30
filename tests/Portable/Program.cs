using Designer.Modules.CompactConnectors;

int checks = 0;
void Check(bool condition, string name)
{
    if (!condition) throw new InvalidOperationException(name);
    checks++;
    Console.WriteLine("PASS " + name);
}

// Expected choices are explicit examples, not a second copy of the selection rule.
(bool PinFound, double PinDelta, bool CornerFound, double CornerDelta, bool PinWins, string Name)[] choices =
[
    (false, 0, false, 0, false, "neither solver found a target"),
    (false, 0, true, 5, false, "a missing pin cannot suppress a corner"),
    (false, -100, true, 1, false, "an unused pin correction cannot win"),
    (true, 5, false, 0, true, "a missing corner cannot suppress a pin"),
    (true, 1, false, -100, true, "an unused corner correction cannot win"),
    (true, 0, true, 1, true, "an exact pin beats a nearby corner"),
    (true, 1, true, 0, false, "an exact corner beats a nearby pin"),
    (true, 0, true, 0, true, "two exact targets prefer pins"),
    (true, 1, true, 2, true, "a closer positive pin wins"),
    (true, 2, true, 1, false, "a closer positive corner stays reachable"),
    (true, -1, true, -2, true, "a closer negative pin wins"),
    (true, -2, true, -1, false, "a closer negative corner stays reachable"),
    (true, -2, true, 1, false, "direction does not make a farther pin win"),
    (true, 1, true, -2, true, "direction does not make a farther corner win"),
    (true, -1, true, 1, true, "opposite equal corrections prefer pins"),
    (true, 1, true, -1, true, "reversed equal corrections prefer pins"),
    (true, 0.25, true, 0.5, true, "sub-unit pin correction remains precise"),
    (true, 0.5, true, 0.25, false, "sub-unit corner correction remains precise"),
    (true, double.Epsilon, true, 0, false, "a nonzero pin is not rounded to exact"),
    (true, 0, true, double.Epsilon, true, "an exact pin wins at the smallest difference"),
    (true, double.MaxValue, true, -double.MaxValue, true, "large opposite corrections tie without squaring"),
    (true, double.MaxValue, true, double.MaxValue / 2, false, "large corrections keep their ordering")
];
foreach (var test in choices)
    Check(ConnectorSnapRules.PreferPins(test.PinFound, test.PinDelta, test.CornerFound, test.CornerDelta) == test.PinWins, test.Name);

Check(ConnectorSnapRules.PreferPins(true, 1, true, 2)
    && !ConnectorSnapRules.PreferPins(true, 2, true, 1), "X and Y may choose different solvers");

(double Body, double Drawn, double Width, bool Expected, string Name)[] drawers =
[
    (24, 56, 82, true, "open drawer adds a main-body bottom"),
    (24, 24, 82, false, "closed drawer adds no duplicate bottom"),
    (24, 20, 82, false, "a body extending below the drawing adds no target"),
    (0, 56, 82, false, "zero body height is rejected"),
    (-1, 56, 82, false, "negative body height is rejected"),
    (double.NaN, 56, 82, false, "NaN body height is rejected"),
    (double.PositiveInfinity, 56, 82, false, "positive infinite body height is rejected"),
    (double.NegativeInfinity, 56, 82, false, "negative infinite body height is rejected"),
    (24, 56, double.NaN, false, "NaN width is rejected"),
    (24, 56, double.PositiveInfinity, false, "positive infinite width is rejected"),
    (24, 56, double.NegativeInfinity, false, "negative infinite width is rejected"),
    (24, double.BitIncrement(24), 82, true, "a positive drawer extent is not rounded away"),
    (24, double.BitDecrement(24), 82, false, "a drawing just above the body bottom is rejected"),
    (double.Epsilon, 1, 82, true, "small positive body height is retained"),
    (24, 56, 0, true, "zero finite width retains the existing eligibility rule"),
    (24, 56, double.MaxValue, true, "large finite width retains the existing eligibility rule"),
    (10, 20, 30, true, "ten-unit body geometry remains eligible"),
    (12, 24, 30, true, "twelve-unit body geometry remains eligible")
];
foreach (var test in drawers)
    Check(ConnectorSnapRules.HasMainBodyBottom(test.Body, test.Drawn, test.Width) == test.Expected, test.Name);

Console.WriteLine($"{checks} portable production-rule checks passed; native hooks and physical gestures were not exercised.");
