module EncodingConverterTests

open Xunit
open EncodingConverter
open System.IO
open System.Text
open System

[<Fact>]
let ``convertToUtf8Bom should convert file to UTF-8 BOM`` () =
    // Create a temporary directory
    let tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"))
    Directory.CreateDirectory(tempDir) |> ignore

    try
        // Create a test file
        let filePath = Path.Combine(tempDir, "testFile.fs")
        File.WriteAllText(filePath, "test content", Encoding.ASCII)

        // Convert the file to UTF-8 BOM
        convertToUtf8Bom filePath

        // Verify the file is now in UTF-8 BOM encoding
        use reader = new StreamReader(filePath, detectEncodingFromByteOrderMarks = true)
        reader.Peek() |> ignore // Force encoding detection
        Assert.Equal(Encoding.UTF8, reader.CurrentEncoding) // Check encoding

        // Verify the BOM is present by reading the first 3 bytes
        use fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read)
        let bom = Array.zeroCreate<byte> 3
        fileStream.Read(bom, 0, 3) |> ignore
        Assert.Equal(0xEFuy, bom.[0]) // First byte of UTF-8 BOM
        Assert.Equal(0xBBuy, bom.[1]) // Second byte of UTF-8 BOM
        Assert.Equal(0xBFuy, bom.[2]) // Third byte of UTF-8 BOM
    finally
        // Clean up the temporary directory
        Directory.Delete(tempDir, recursive = true)

[<Fact>]
let ``convertToUtf8Bom should not convert already UTF-8 BOM encoded file`` () =
    // Create a temporary directory
    let tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"))
    Directory.CreateDirectory(tempDir) |> ignore

    try
        // Create a test file
        let filePath = Path.Combine(tempDir, "testFile.fs")
        File.WriteAllText(filePath, "test content", Encoding.ASCII)

        // First conversion
        let sw = new StringWriter()
        Console.SetOut(sw)
        convertToUtf8Bom filePath
        let firstOutput = sw.ToString()
        
        // Get file info after first conversion
        let firstConversionTime = File.GetLastWriteTime(filePath)
        
        // Second conversion
        sw.GetStringBuilder().Clear() |> ignore
        convertToUtf8Bom filePath
        let secondOutput = sw.ToString()
        
        // Get file info after second conversion
        let secondConversionTime = File.GetLastWriteTime(filePath)
        
        // Verify the file was only converted once
        Assert.Contains($"Converted {filePath}", firstOutput)
        Assert.Empty(secondOutput)
        Assert.Equal(firstConversionTime, secondConversionTime)
    finally
        // Clean up the temporary directory
        Directory.Delete(tempDir, recursive = true)