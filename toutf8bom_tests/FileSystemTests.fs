module FileSystemTests

open Xunit
open FileSystem
open System.IO
open System

[<Fact>]
let ``getAllFiles should return all files in directory`` () =
    // Create a temporary directory
    let tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"))
    Directory.CreateDirectory(tempDir) |> ignore

    try
        // Create some test files
        let file1 = Path.Combine(tempDir, "test1.fs")
        let file2 = Path.Combine(tempDir, "test2.cs")
        File.WriteAllText(file1, "test content") |> ignore
        File.WriteAllText(file2, "test content") |> ignore

        // Get all files in the directory
        let files = getAllFiles tempDir |> Seq.toList

        // Verify the results
        Assert.Equal(2, files.Length)
        Assert.Contains(file1, files)
        Assert.Contains(file2, files)
    finally
        // Clean up the temporary directory
        Directory.Delete(tempDir, recursive = true)

[<Fact>]
let ``filterFilesByExtensions should filter files by extensions`` () =
    // Create a temporary directory
    let tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"))
    Directory.CreateDirectory(tempDir) |> ignore

    try
        // Create some test files
        let file1 = Path.Combine(tempDir, "test1.fs")
        let file2 = Path.Combine(tempDir, "test2.cs")
        let file3 = Path.Combine(tempDir, "test3.txt")
        File.WriteAllText(file1, "test content") |> ignore
        File.WriteAllText(file2, "test content") |> ignore
        File.WriteAllText(file3, "test content") |> ignore

        // Filter files by extensions
        let files = getAllFiles tempDir
        let filtered = filterFilesByExtensions [ ".fs"; ".cs" ] files |> Seq.toList

        // Verify the results
        Assert.Equal(2, filtered.Length)
        Assert.Contains(file1, filtered)
        Assert.Contains(file2, filtered)
        Assert.DoesNotContain(file3, filtered)
    finally
        // Clean up the temporary directory
        Directory.Delete(tempDir, recursive = true)

[<Fact>]
let ``getAllFiles should skip directories starting with dot`` () =
    // Create a temporary directory structure
    let tempDir = Path.Combine(Path.GetTempPath(), "test_skip_dot_dirs")
    Directory.CreateDirectory(tempDir) |> ignore
    
    try
        // Create normal directory with a file
        let normalDir = Path.Combine(tempDir, "normal")
        Directory.CreateDirectory(normalDir) |> ignore
        let normalFile = Path.Combine(normalDir, "test.txt")
        File.WriteAllText(normalFile, "test") |> ignore
        
        // Create .git directory with a file
        let dotDir = Path.Combine(tempDir, ".git")
        Directory.CreateDirectory(dotDir) |> ignore
        let dotFile = Path.Combine(dotDir, "config")
        File.WriteAllText(dotFile, "test") |> ignore
        
        // Get all files
        let files = getAllFiles tempDir |> Seq.toList
        
        // Verify
        Assert.Single(files)
        Assert.Contains(normalFile, files)
        Assert.DoesNotContain(dotFile, files)
    finally
        // Cleanup
        Directory.Delete(tempDir, true)

[<Fact>]
let ``getAllFiles should skip build and publish directories`` () =
    // Create a temporary directory structure
    let tempDir = Path.Combine(Path.GetTempPath(), "test_skip_build_dirs")
    Directory.CreateDirectory(tempDir) |> ignore
    
    try
        // Create normal directory with a file
        let normalDir = Path.Combine(tempDir, "src")
        Directory.CreateDirectory(normalDir) |> ignore
        let normalFile = Path.Combine(normalDir, "test.txt")
        File.WriteAllText(normalFile, "test") |> ignore
        
        // Create build directories with files
        let objDir = Path.Combine(tempDir, "obj")
        let binDir = Path.Combine(tempDir, "bin")
        let publishDir = Path.Combine(tempDir, "publish")
        
        Directory.CreateDirectory(objDir) |> ignore
        Directory.CreateDirectory(binDir) |> ignore
        Directory.CreateDirectory(publishDir) |> ignore
        
        let objFile = Path.Combine(objDir, "debug.txt")
        let binFile = Path.Combine(binDir, "app.exe")
        let publishFile = Path.Combine(publishDir, "app.dll")
        
        File.WriteAllText(objFile, "test") |> ignore
        File.WriteAllText(binFile, "test") |> ignore
        File.WriteAllText(publishFile, "test") |> ignore
        
        // Get all files
        let files = getAllFiles tempDir |> Seq.toList
        
        // Verify
        Assert.Single(files)
        Assert.Contains(normalFile, files)
        Assert.DoesNotContain(objFile, files)
        Assert.DoesNotContain(binFile, files)
        Assert.DoesNotContain(publishFile, files)
    finally
        // Cleanup
        Directory.Delete(tempDir, true)

[<Fact>]
let ``getAllFiles should throw when path is not a directory`` () =
    // Create a temporary file
    let tempFile = Path.GetTempFileName()
    
    try
        // Verify that exception is thrown
        let ex = Assert.Throws<ArgumentException>(fun () -> 
            getAllFiles tempFile |> Seq.toList |> ignore
        )
        Assert.Contains("is not a directory", ex.Message)
    finally
        // Cleanup
        File.Delete(tempFile)

[<Fact>]
let ``getAllFiles should handle locked files`` () =
    // Create a temporary directory structure
    let tempDir = Path.Combine(Path.GetTempPath(), "test_locked_files")
    Directory.CreateDirectory(tempDir) |> ignore
    
    try
        // Create a normal file
        let normalFile = Path.Combine(tempDir, "normal.txt")
        File.WriteAllText(normalFile, "test") |> ignore
        
        // Create a locked file
        let lockedFile = Path.Combine(tempDir, "locked.txt")
        File.WriteAllText(lockedFile, "test") |> ignore
        use stream = File.Open(lockedFile, FileMode.Open, FileAccess.ReadWrite, FileShare.None)
        
        // Get all files and their access status
        let files = getAllFiles tempDir |> Seq.toList
        
        // Verify
        Assert.Contains(normalFile, files)
        Assert.Contains(lockedFile, files)
        
        // Try to get access status
        let accessResults = files |> List.map getFileAccessStatus
        Assert.Contains(FileAccessStatus.Accessible normalFile, accessResults)
        let lockedStatus = accessResults |> List.find (function 
            | FileAccessStatus.Inaccessible(path, _) -> path = lockedFile
            | _ -> false)
        match lockedStatus with
        | FileAccessStatus.Inaccessible(_, error) ->
            Assert.Contains("locked", error)
        | _ -> Assert.True(false, "Expected locked file to be inaccessible")
        
    finally
        // Cleanup
        Directory.Delete(tempDir, true)