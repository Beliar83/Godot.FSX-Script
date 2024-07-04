use std::ffi::c_void;
use std::path::Path;
use netcorehost::{nethost, pdcstr};

#[no_mangle]
pub extern "C" fn fsx_script_init(get_proc_address: usize, library : usize, initialization : usize) -> u8 {
    println!("{}", std::env::current_dir().unwrap().to_string_lossy());
    let hostfxr = nethost::load_hostfxr().unwrap();
    let context =
        hostfxr.initialize_for_runtime_config(pdcstr!("addons/fsx-script/bin/FSXScript.Editor.runtimeconfig.json")).unwrap();
    let fn_loader =
        context.get_delegate_loader_for_assembly(pdcstr!("addons/fsx-script/bin/FSXScript.Editor.dll")).unwrap();
    let init = fn_loader.get_function_with_unmanaged_callers_only::<fn(usize, usize, usize) -> u8>(
        pdcstr!("FSXScript.Editor.Main, FSXScript.Editor"),
        pdcstr!("Init"),
    ).unwrap();

    return init(get_proc_address, library, initialization);
}
