#pragma once
#include <coreclr_delegates.h>

typedef void (CORECLR_DELEGATE_CALLTYPE *de_init)(GDExtensionInitializationLevel p_level);

typedef struct {
    de_init initialize;
    de_init uninitialize;
} DotnetInitialization;

DotnetInitialization* bind();