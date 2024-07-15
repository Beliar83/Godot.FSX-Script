@tool
extends FsxScriptLanguage
class_name FsxScriptLanguageExt

#This is to work around gdext currently not generating method signatures than can accept null values for virtual methods

func _complete_code(code: String, path: String, owner: Object) -> Dictionary:
	return self.complete_code(code,path, owner)

func _lookup_code(code: String, symbol: String, path: String, owner: Object) -> Dictionary:
	return self.lookup_code(code, symbol, path, owner)
