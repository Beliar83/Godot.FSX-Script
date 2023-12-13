pub extern crate godot;
pub extern crate once_cell;

pub mod generator {
    #[macro_export]
    macro_rules! variant_constructor {
        (
            $variant_type:ident,
            $constr_id:expr,
            $($arg:ident),*) => {
            {
                const CONSTRUCTOR_ID: i32 = $constr_id;
                let variant_type = stringify!($variant_type);
                let size = *$crate::generator::once_cell::sync::Lazy::new(|| {
                    match $crate::generated::config::VARIANT_SIZES.get(variant_type) {
                        None => panic!("No size found for {}", variant_type),
                        Some(size) => size,
                    }
                });

                let constructor = $crate::generator::once_cell::sync::Lazy::new(|| {
                    match $crate::generator::godot::sys::interface_fn!(variant_get_ptr_constructor)($variant_type, CONSTRUCTOR_ID) {
                        None => panic!("Could not get constructor {} for {}", CONSTRUCTOR_ID, variant_type),
                        Some(function) => function,
                    }
                });
                let mut data = std::mem::ManuallyDrop::new(Vec::with_capacity(*size));
                let mut args = Vec::new();
                $(
                    args.push($arg);
                )*
                constructor(data.as_mut_ptr(), args.as_ptr());
                data.set_len(*size);
                data.as_ptr() as GDExtensionConstTypePtr
            }
        };
        (
            $variant_type:ident,
            $constr_id:expr) => {
            {
                const CONSTRUCTOR_ID: i32 = $constr_id;
                let variant_type = stringify!($variant_type);
                let size = *$crate::generator::once_cell::sync::Lazy::new(|| {
                    match $crate::generated::config::VARIANT_SIZES.get(variant_type) {
                        None => panic!("No size found for {}", variant_type),
                        Some(size) => size,
                    }
                });
                let constructor = $crate::generator::once_cell::sync::Lazy::new(|| {
                    match $crate::generator::godot::sys::interface_fn!(variant_get_ptr_constructor)($variant_type, CONSTRUCTOR_ID) {
                        None => panic!("Could not get constructor {} for {}", CONSTRUCTOR_ID, variant_type),
                        Some(function) => function,
                    }
                });
                let mut data = std::mem::ManuallyDrop::new(Vec::with_capacity(*size));
                let args = Vec::new();
                constructor(data.as_mut_ptr(), args.as_ptr());
                data.set_len(*size);
                data.as_ptr() as GDExtensionConstTypePtr
            }
        };
    }
}