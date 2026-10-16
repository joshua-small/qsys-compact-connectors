# Compact Connectors

Independent geometry for 10-unit Container and Channel Group connectors. It centers their pins and label ink at 5 units, preserves existing positions and widths, and uses 10-unit spacing for newly generated connectors. Component Auto Width is optional.

Connector dragging reuses Designer's **Wiring** snap domain, used by Signal Snakes and wire breakpoints. The native engine supplies actual pin centers, tolerance and gesture handling. Mixed selections follow the native rule: Wiring applies only when every selected object requests that domain.

For ordinary object snapping, an open component drawer adds the two bottom corners of the main body as targets. Full drawer corners remain available. Wiring targets remain native pin centers. Closed drawers have no extra body targets.

[Better Packing](https://github.com/joshua-small/qsys-better-packing) supplies **Pack to Top Pins** and **Pack to Bottom Pins**. Traditional boundary packing retains its meaning, so it can still leave a 10-unit connector one unit from a component pin.

Removing the module restores native calculations on restart. Connector heights saved in a design may remain 10 units; removal is not a bulk geometry migration. Preserve originals when testing.

Requires Mods Core 0.1.7 or later within 0.1.x and the exact approved Designer 10.5 host. Replace Automatic Geometry in the same Mod Manager transaction; the catalog prevents enabling both. [Component Auto Width](https://github.com/joshua-small/qsys-component-auto-width) is optional. Initialization details are at File > Preferences > Mod Diagnostics.

Clone Mods Core alongside this repository and build with .NET 10.0.112 and local `DesignerDir` references. Run `dotnet run --project tests/GeometryTests.csproj` on Windows for the synthetic WPF harness, or pass the Designer directory for native metadata checks. No vendor binaries are included. See release notes for separate native acceptance.

Source extracted from qsys-automatic-geometry PR #7 (source checkpoint c5309a2). Source ownership now resides here; no new source license is granted. Third-party license notices and artwork provenance are retained.

## Geometry update acceptance (2026-10-16)

A disposable exact-build Designer 10.5 copy with redirected preferences exercised Containers, Channel Groups and a second Container nesting level. Twelve top/bottom pin operations covered main and exposed drawer pins; each verified equal native CanvasCenter coordinates and one-step undo/redo. Six registered pin commands and twelve existing routed boundary commands ran with native nested-canvas focus. Three checks confirmed six body/drawer snap corners and the Wiring snap domain on compact connectors. The fixture was saved to a separate output and the original hash stayed unchanged.

Follow-up acceptance passed 17 physical drags at 100%/200% Designer zoom and 100% Windows display scaling: wired input/output connectors in all three nested scopes, unwired control connectors to exposed pins, root/nested body-bottom targets, open/closed drawers, retained outer/top targets, near/outside tolerance and physical undo/redo. Better Packing also passed 24 attached-audio-wire packing cases. A fresh-process save/reopen preserved all nine wire endpoints and their rendered attachments. Personal settings and the source fixture retained their hashes. No product fix or replacement release was needed. [Detailed results and guarded driver](https://github.com/joshua-small/qsys-better-packing/blob/main/docs/PHYSICAL-WIRE-ACCEPTANCE.md).

Other Windows display scales, mixed-selection drag domains, broader component/pin counts, attached non-audio wires and the wider editor/load-order matrix remain unverified. These checks do not establish DSP/hardware behavior. Source and test fixtures contain no vendor code or binaries.
