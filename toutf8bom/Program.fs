module Main

open System
open FileSystem
open EncodingConverter

[<EntryPoint>]
let main argv =
    let directory = 
        if argv.Length > 0 then argv.[0]
        else Environment.CurrentDirectory

    let extensions = FileExtensions.allExtensions

    getAllFiles directory
    |> filterFilesByExtensions extensions
    |> Seq.iter convertToUtf8Bom

    printfn "\nConversion complete."
    0 // Return success