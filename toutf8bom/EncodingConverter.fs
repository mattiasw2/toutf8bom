module EncodingConverter

open System
open System.IO
open System.Text

/// Converts a file to UTF-8 BOM encoding
let convertToUtf8Bom (filePath: string) =
    if not (FileSystem.isUtf8Bom filePath) then
        let content = File.ReadAllText(filePath)
        File.WriteAllText(filePath, content, Encoding.UTF8)