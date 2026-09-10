# vc.Ifx.Generators

`vc.Ifx.Generators` contains source generators that turn explicit framework annotations into compile-time artifacts. The package keeps generated code deterministic and discoverable while avoiding runtime reflection for repetitive framework plumbing.

The implementation is organized by generator under `Generators/`. Public marker attributes live in `vc.Ifx.Generators.Abstractions` so application projects can reference the annotations without taking a dependency on the generator implementation.
