namespace FSXScriptLanguage

type IScriptSession =
    abstract member GetClassName: unit -> string
    abstract member ParseScript: string -> unit
