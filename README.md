# Compact Connectors

Portable geometry-rule regressions run with .NET 10 on Windows or Linux without
Designer dependencies. See [tests/README.md](tests/README.md) for the command and
the separate Windows/application acceptance requirements.

Independent geometry for 10-unit Container and Channel Group connectors. It centers their pins and label ink at 5 units, preserves existing positions and widths, and uses 10-unit spacing for newly generated connectors. Component Auto Width is optional.

Connector dragging reuses Designer's **Wiring** snap domain, used by Signal Snakes and wire breakpoints. Version 0.1.1 also retains native corner snapping during connector-only object drags: two native snap results are compared per axis, using the smaller correction and preferring pins on ties. Pin-to-pin and corner-to-corner point sets stay separate, so a corner cannot produce a false alignment to a pin. Native tolerance, guide rendering and Ctrl snap bypass remain in use. Mixed selections retain the native domain rule; ordinary wire/Snake snap instances are unchanged.

Version 0.1.1 passed fixture/native geometry-solver checks and physical wired drags in isolated Designer. Control, AV Stream and MediaCast connectors reached both corner and adjacent pin rows; all 19 connections survived fresh-process save/reopen. Held-modifier Alt/copy and Ctrl-bypass gestures were not physically checked in this follow-up.

The owner tested Windows Display Scale, Application Zoom and Design Zoom and reported no effect. Exact values were not recorded; this is owner-observed evidence for those tests. An exhaustive display-scale sweep is not required without a concrete regression.

For ordinary object snapping, an open component drawer adds the two bottom corners of the main body as targets. Full drawer corners remain available. Wiring targets remain native pin centers. Closed drawers have no extra body targets.

[Better Packing](https://github.com/joshua-small/qsys-better-packing) supplies pin-aligned packing. Version 0.3.0 exposes separate **Top Main**, **Bottom Main**, **Top Drawer** and **Bottom Drawer** actions. Traditional boundary packing retains its meaning, so it can still leave a 10-unit connector one unit from a component pin.

Connector labels remain native: the mod does not add Input/Output prefixes or replace custom/empty labels. Input x / Output x labels in local acceptance designs were created only by the disposable fixture driver.

Removing the module restores native calculations on restart. Connector heights saved in a design may remain 10 units; removal is not a bulk geometry migration. Preserve originals when testing.

Requires Mods Core 0.1.7 or later within 0.1.x and the exact approved Designer 10.5 host. Replace Automatic Geometry in the same Mod Manager transaction; the catalog prevents enabling both. [Component Auto Width](https://github.com/joshua-small/qsys-component-auto-width) is optional. Initialization details are at File > Preferences > Mod Diagnostics.

Clone Mods Core alongside this repository and build with .NET 10.0.112 and local `DesignerDir` references. Run `dotnet run --project tests/GeometryTests.csproj` on Windows for the synthetic WPF harness, or pass the Designer directory for native metadata checks. No vendor binaries are included. See release notes for separate native acceptance.

Release builds can pass `CoreRuntimePath` pointing to the approved Core 0.1.7 runtime DLL instead of compiling a newer sibling checkout. This keeps the assembly reference consistent with the unchanged minimum dependency. Passing `--snap-behavior` after the Designer directory runs native geometry-only snap-solver checks without starting Designer, opening a design or controlling input. Version 0.1.1 passed 36 fixture checks and 17 metadata/native-solver checks. The physical results below separately cover the actual drag path.

Source extracted from qsys-automatic-geometry PR #7 (source checkpoint c5309a2). Source ownership now resides here; no new source license is granted. Third-party license notices and artwork provenance are retained.

## Geometry update acceptance (2026-10-16)

A disposable exact-build Designer 10.5 copy with redirected preferences exercised Containers, Channel Groups and a second Container nesting level. Twelve top/bottom pin operations covered main and exposed drawer pins; each verified equal native CanvasCenter coordinates and one-step undo/redo. Six registered pin commands and twelve existing routed boundary commands ran with native nested-canvas focus. Three checks confirmed six body/drawer snap corners and the Wiring snap domain on compact connectors. The fixture was saved to a separate output and the original hash stayed unchanged.

Follow-up acceptance passed 17 physical drags at 100%/200% Designer zoom and 100% Windows display scaling: wired input/output connectors in all three nested scopes, unwired control connectors to exposed pins, root/nested body-bottom targets, open/closed drawers, retained outer/top targets, near/outside tolerance and physical undo/redo. Better Packing also passed 24 attached-audio-wire packing cases. A fresh-process save/reopen preserved all nine wire endpoints and their rendered attachments. Personal settings and the source fixture retained their hashes. No product fix or replacement release was needed. [Detailed results and guarded driver](https://github.com/joshua-small/qsys-better-packing/blob/main/docs/PHYSICAL-WIRE-ACCEPTANCE.md).

Further native checks passed 40 control/AV Stream/MediaCast packing operations, with all 19 connections preserved through fresh-process save/reopen. Native MediaCast Router-to-Router rejection remained intact. These wire-type checks were programmatic. The 0.1.1 follow-up adds physical drags with those non-audio wires, including adjacent corner Y=180 and pin-aligned connector Y=181 targets. All 19 wire identities, coordinates and rendered endpoints survived fresh-process reopens. Held-modifier Alt/copy and Ctrl-bypass, mixed-selection drags, broader component/pin counts and the wider editor/load-order matrix remain outside this physical check. The owner reported no effect from Display Scale, Application Zoom or Design Zoom; no exhaustive scale matrix is claimed or required. These checks do not establish DSP/hardware behavior. Source and test fixtures contain no vendor code or binaries.
