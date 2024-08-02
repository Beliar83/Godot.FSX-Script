use std::fs::File;
use std::io::Write;
use std::panic::catch_unwind;
use std::path::Path;

use godot_bindings::{load_gdextension_json, StopWatch, write_gdextension_headers};

fn main() {
    println!("cargo::rerun-if-env-changed=GODOT4_BIN");
    // Generate api and header, so godot-dotnet uses the same version
    let result = load_gdextension_json(&mut StopWatch::start());
    let mut json = File::create(Path::new("../../godot-dotnet/gdextension/extension_api.json")).unwrap();
    json.write(result.as_bytes()).expect("Could not write extension_api.json");
    // This will generate the header without patching it, which would make it not parseable by godot-dotnet
    let _ = catch_unwind(|| {
        write_gdextension_headers(Path::new("../../godot-dotnet/gdextension/_"), Path::new("../godot-dotnet/gdextension/_"), &mut StopWatch::start())
    });
}