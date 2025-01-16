module FileSystem

open System
open System.IO
open System.Text // Add this to use Encoding

/// List of directory names to skip during file scanning
let private directoriesToSkip = 
    [
        // Build and output directories
        "bin"
        "obj"
        "publish"
        // Hidden directories (starting with dot)
        ".git"
        ".vs"
        ".vscode"
        ".idea"
    ] |> Set.ofList

/// Checks if a directory should be skipped
let private shouldSkipDirectory (dirPath: string) =
    let dirName = Path.GetFileName(dirPath)
    dirName.StartsWith(".") || Set.contains dirName directoriesToSkip

/// Recursively gets all files in a directory and its subdirectories
/// Skips directories that:
/// - Start with a dot (.)
/// - Are build/output directories (bin, obj, publish)
let rec getAllFiles (dir: string) =
    seq {
        yield! Directory.GetFiles(dir)
        for subDir in Directory.GetDirectories(dir) do
            if not (shouldSkipDirectory subDir) then
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
    use fs = new FileStream(filePath, FileMode.Open, FileAccess.Read)
    if fs.Length >= 3L then
        let bom = Array.zeroCreate<byte> 3
        fs.Read(bom, 0, 3) |> ignore
        bom.[0] = 0xEFuy && bom.[1] = 0xBBuy && bom.[2] = 0xBFuy
    else
        false