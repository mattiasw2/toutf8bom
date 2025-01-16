module FileSystem

open System
open System.IO
open System.Text // Add this to use Encoding

/// Recursively gets all files in a directory and its subdirectories
let rec getAllFiles (dir: string) =
    seq {
        yield! Directory.GetFiles(dir)
        for subDir in Directory.GetDirectories(dir) do
            yield! getAllFiles subDir
    }

/// Filters files by their extensions
let filterFilesByExtensions (extensions: string list) (files: string seq) =
    files
    |> Seq.filter (fun file ->
        extensions |> List.exists (fun ext -> file.EndsWith(ext, StringComparison.OrdinalIgnoreCase))
    )

/// Checks if a file is already in UTF-8 BOM encoding
let isUtf8Bom (filePath: string) =
    use reader = new StreamReader(filePath, detectEncodingFromByteOrderMarks = true)
    reader.Peek() |> ignore // Force encoding detection
    reader.CurrentEncoding = Encoding.UTF8 && reader.BaseStream.Position = 3L