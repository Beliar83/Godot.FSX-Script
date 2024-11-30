namespace LspService
open System.Threading.Tasks

type IScriptSession =
    abstract member SetPath : string -> unit
    abstract member SetCode : string -> unit
    abstract member GetClassName : unit -> string
    abstract member NotifyScriptCloseAsync : unit -> Task
    abstract member NotifyScriptChangeAsync : unit -> Task
