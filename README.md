# SneedSmoother - Fixed Version

A Path of Exile asset patcher with comprehensive error handling and logging.

## Recent Fixes Applied

This version has been thoroughly fixed to prevent crashes and provide better error feedback:

### 🔧 Critical Fixes
- **Global Exception Handling**: All unhandled exceptions are now caught and logged
- **Resource Management**: Proper disposal of file handles and streams using `using` statements
- **Null Reference Protection**: Added null checks throughout the codebase
- **File Validation**: All file operations now validate file existence before proceeding
- **Thread Safety**: UI updates are now properly dispatched to the UI thread

### 📋 Error Logging
- **Automatic Logging**: All errors are logged to `error_log.txt` in the application directory
- **Detailed Stack Traces**: Full exception details with timestamps
- **User-Friendly Messages**: Clear error messages shown to users
- **Graceful Degradation**: Application continues running even when individual operations fail

## Building the Application

### Prerequisites
- .NET 8.0 SDK or later
- Windows 10/11 (due to WPF dependency)

### Build Instructions

#### Option 1: Using the Build Script (Recommended)
1. On a Windows machine, double-click `build_windows.bat`
2. The script will check for .NET SDK and build the application
3. Executable will be in `bin\Release\net8.0-windows\`

#### Option 2: Manual Build
```bash
dotnet clean
dotnet build --configuration Release
```

## Usage

1. **Select GGPK/BIN File**: Click "Select GGPK" to choose your Path of Exile game files
2. **Extract Assets**: Extract vanilla assets for modification
3. **Apply Patches**: Use the patch functionality to modify game assets
4. **Restore Assets**: Restore original assets when needed

## Configuration Files

- `paths_to_extract.json`: Defines which paths to extract from game files
- `color_mods.json`: Color modification settings
- `mtx_replacements.json`: MTX replacement configurations
- `paths_to_skip.txt`: Paths to skip during processing

## Debugging Issues

### Check Error Logs
If the application crashes or behaves unexpectedly:
1. Look for `error_log.txt` in the application directory
2. The log contains detailed error information with timestamps
3. Share the relevant log entries when reporting issues

### Common Issues
- **"GGPK is not selected"**: Select a valid .ggpk or .bin file first
- **"File not found"**: Ensure the selected GGPK file exists and is accessible
- **"No patches enabled"**: Check that patch settings are configured correctly
- **"Cache directory not found"**: Run "Extract Assets" before applying patches

### Validation Features
The application now validates:
- File existence before operations
- Directory structure before processing
- JSON file format and content
- Patch availability and configuration

## Technical Details

### Architecture
- **MainWindow**: UI layer with comprehensive exception handling
- **PatchManager**: Core patching logic with resource management
- **FileExtractor**: Asset extraction with validation
- **IPatch Interface**: Modular patch system

### Error Handling Strategy
- **Graceful Degradation**: Individual patch failures don't crash the application
- **Detailed Logging**: All errors are logged with context
- **User Feedback**: Clear messages inform users of issues
- **Resource Cleanup**: Proper disposal prevents resource leaks

## Troubleshooting

### Build Issues
- Ensure .NET 8.0 SDK is installed
- Check that all NuGet packages are restored
- Verify Windows compatibility (WPF requirement)

### Runtime Issues
- Check `error_log.txt` for detailed error information
- Verify all configuration JSON files are present and valid
- Ensure GGPK/BIN files are accessible and not corrupted

## Support

When reporting issues, please include:
1. Contents of `error_log.txt`
2. Steps to reproduce the problem
3. GGPK/BIN file information (size, source)
4. System information (Windows version, .NET version)

This version is significantly more robust and should provide clear feedback about any issues that occur during operation.