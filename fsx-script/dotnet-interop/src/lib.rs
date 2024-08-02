use std::ffi::c_void;
use netcorehost::{nethost, pdcstr};
use netcorehost::hostfxr::ManagedFunction;

#[no_mangle]
extern "C" fn dotnet_interop_init(get_proc_address: *const c_void, library: *const c_void, initialization: *const c_void) -> bool {
    println!("{}", std::env::current_dir().unwrap().to_string_lossy());
    let hostfxr = nethost::load_hostfxr().unwrap();
    let context = hostfxr
        .initialize_for_runtime_config(pdcstr!("addons/fsx-script/bin/FSXScript.Editor.runtimeconfig.json"))
        .unwrap();
    let fn_loader = context
        .get_delegate_loader_for_assembly(pdcstr!("addons/fsx-script/bin/FSXScript.Editor.dll"))
        .unwrap();
    let result: Result<ManagedFunction<extern "system" fn(*const c_void, *const c_void, *const c_void) -> bool>, _> = fn_loader.get_function_with_unmanaged_callers_only::<fn(*const c_void, *const c_void, *const c_void) -> bool>(
        pdcstr!("FSXScript.Editor.Main, FSXScript.Editor"),
        pdcstr!("Init"),
    );

    let init = result.unwrap();
    init(get_proc_address, library, initialization)
}