//------------------------------------------------------------------------------
// <auto-generated />
//
// This file was automatically generated by SWIG (https://www.swig.org).
// Version 4.1.1
//
// Do not make changes to this file unless you know what you are doing - modify
// the SWIG interface file instead.
//------------------------------------------------------------------------------


public class FSharpMethodInfo : global::System.IDisposable {
  private global::System.Runtime.InteropServices.HandleRef swigCPtr;
  protected bool swigCMemOwn;

  internal FSharpMethodInfo(global::System.IntPtr cPtr, bool cMemoryOwn) {
    swigCMemOwn = cMemoryOwn;
    swigCPtr = new global::System.Runtime.InteropServices.HandleRef(this, cPtr);
  }

  internal static global::System.Runtime.InteropServices.HandleRef getCPtr(FSharpMethodInfo obj) {
    return (obj == null) ? new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero) : obj.swigCPtr;
  }

  internal static global::System.Runtime.InteropServices.HandleRef swigRelease(FSharpMethodInfo obj) {
    if (obj != null) {
      if (!obj.swigCMemOwn)
        throw new global::System.ApplicationException("Cannot release ownership as memory is not owned");
      global::System.Runtime.InteropServices.HandleRef ptr = obj.swigCPtr;
      obj.swigCMemOwn = false;
      obj.Dispose();
      return ptr;
    } else {
      return new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero);
    }
  }

  ~FSharpMethodInfo() {
    Dispose(false);
  }

  public void Dispose() {
    Dispose(true);
    global::System.GC.SuppressFinalize(this);
  }

  protected virtual void Dispose(bool disposing) {
    lock(this) {
      if (swigCPtr.Handle != global::System.IntPtr.Zero) {
        if (swigCMemOwn) {
          swigCMemOwn = false;
          DotnetScriptInteropPINVOKE.delete_FSharpMethodInfo(swigCPtr);
        }
        swigCPtr = new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero);
      }
    }
  }

  public GodotString Name {
    set {
      DotnetScriptInteropPINVOKE.FSharpMethodInfo_Name_set(swigCPtr, GodotString.getCPtr(value));
    } 
    get {
      global::System.IntPtr cPtr = DotnetScriptInteropPINVOKE.FSharpMethodInfo_Name_get(swigCPtr);
      GodotString ret = (cPtr == global::System.IntPtr.Zero) ? null : new GodotString(cPtr, false);
      return ret;
    } 
  }

  public PropertyInfoVector args {
    set {
      DotnetScriptInteropPINVOKE.FSharpMethodInfo_args_set(swigCPtr, PropertyInfoVector.getCPtr(value));
    } 
    get {
      global::System.IntPtr cPtr = DotnetScriptInteropPINVOKE.FSharpMethodInfo_args_get(swigCPtr);
      PropertyInfoVector ret = (cPtr == global::System.IntPtr.Zero) ? null : new PropertyInfoVector(cPtr, false);
      return ret;
    } 
  }

  public FSharpMethodInfo() : this(DotnetScriptInteropPINVOKE.new_FSharpMethodInfo(), true) {
  }

}
