module FileExtensions

/// Common F# file extensions
let fsharpExtensions = [
    ".fs"    // F# source file
    ".fsi"   // F# signature file
    ".fsx"   // F# script file
    ".fsproj" // F# project file
    ".ml"
    ".mli"
]

/// Common C# file extensions
let csharpExtensions = [
    ".cs"    // C# source file
    ".csproj" // C# project file
]

/// Common text file extensions
let textExtensions = [
    ".txt"   // Plain text file
    ".md"    // Markdown file
    ".json"  // JSON file
    ".xml"   // XML file
    ".yml"   // YAML file
    ".yaml"  // YAML file (alternative extension)
    ".csv"   // Comma-separated values file
    ".html"  // HTML file
    ".htm"   // HTML file (alternative extension)
    ".css"   // Cascading Style Sheets file
    ".js"    // JavaScript file
    ".ts"    // TypeScript file
    ".config" // Configuration file
    ".ini"   // INI configuration file
    ".log"   // Log file
]

/// Combined list of all supported extensions
let allExtensions =
    fsharpExtensions @ csharpExtensions @ textExtensions
