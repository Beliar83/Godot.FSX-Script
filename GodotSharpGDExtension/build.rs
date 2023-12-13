use std::fs::File;
use std::io::Write;
use std::path::Path;
use std::fs;

fn main() {
    let gdextension_interface_rs_content = godot4_prebuilt::load_gdextension_header_rs();
    let mut extension_rs_file = File::create("gdextension_interface.rs").unwrap();
    extension_rs_file.write_all(gdextension_interface_rs_content.as_bytes()).unwrap();

    let mut builder = csbindgen::Builder::default()
        .input_extern_file("gdextension_interface.rs")
        .input_extern_file("src/godot_string.rs")
        .csharp_class_accessibility("public")
        .csharp_dll_name("godot_sharp_gdextension")
        .csharp_namespace("GodotSharpGDExtension");

    let builtin_path = Path::new("src/generated/builtin_classes");
    let files = match fs::read_dir(builtin_path) {
        Ok(files) => {
            files
        }
        Err(err) => {
            panic!("Could not list generated builtin classes: {}", err);
        }
    };
    for file in files {
        let file = file.unwrap();
        builder = builder.input_extern_file(file.path());
    }
    builder
        .generate_csharp_file("../GodotSharpGDExtension.CSharp/Generated/native.godot_sharp_gdextension.g.cs")
        .unwrap();

}