# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is a C# WPF application called "LLMConfigManager" (Claude Code Model Switch Manager) that provides a graphical interface for managing Claude Code AI model configurations. The application allows users to create, edit, and apply different AI model profiles to Claude Code's settings.json file.

**Key Features:**
- Profile management for different AI models (Gemini, Kimi, etc.)
- PowerShell environment variable configuration
- Direct integration with Claude Code settings
- Launch Claude Code with specific model configurations

## Architecture

The application follows a three-layer architecture pattern:

### UI Layer (`MainWindow.xaml/.cs`)
- Main WPF window with profile list, editing controls, and action buttons
- Handles user interactions and UI events
- Located in `MainWindow.xaml` and `MainWindow.xaml.cs:25-176`

### Business Logic Layer (`BusinessLogic/`)
- `SettingsManager.cs` - Core logic for parsing PowerShell statements, applying settings, and launching Claude Code
- Handles environment variable parsing using regex patterns
- Manages JSON operations for settings.json files

### Data Access Layer (`DataAccess/`)
- `ProfileManager.cs` - Handles persistence of model profiles to/from JSON
- Uses Newtonsoft.Json for serialization

### Model Layer (`Model/`)
- `ModelProfile.cs` - Simple POCO class representing a configuration profile with Name and StatementBlock properties

## Common Development Commands

### Build the Project
```bash
dotnet build LLMConfigManager.csproj
```

### Run the Application
```bash
dotnet run --project LLMConfigManager.csproj
```

### Build for Release
```bash
dotnet build -c Release
```

## Key Technical Details

### Dependencies
- **.NET Framework 4.7.2** - Target framework
- **Newtonsoft.Json 13.0.3** - JSON serialization
- **Costura.Fody 5.7.0** - Resource embedding for deployment
- **WPF** - UI framework

### Configuration Files
- `profiles.json` - Stores user-defined model profiles (created at runtime)
- `settings.json` - Claude Code's configuration file (user-selected location)

### Environment Variable Parsing
The application parses PowerShell environment variable statements using regex pattern:
```csharp
// Pattern: $Env:VARIABLE_NAME="value"
var regex = new Regex("\\$Env:(\\S+)\\s*=\\s*\"?([^\n\r\"]*)\"?", RegexOptions.Multiline);
```

### Launch Integration
Two launch methods available:
1. **With Profile**: Sets environment variables then launches Claude Code
2. **Direct**: Launches Claude Code without configuration changes

## Important File Locations

- Main window logic: `MainWindow.xaml.cs:25-176`
- Settings management: `BusinessLogic/SettingsManager.cs:14-101`
- Profile persistence: `DataAccess/ProfileManager.cs:12-28`
- UI layout: `MainWindow.xaml:1-80`

## Application Flow

1. User creates/edits model profiles with PowerShell environment variables
2. Profiles are saved to `profiles.json`
3. User selects their Claude Code `settings.json` file location
4. Application can:
   - Apply profile environment variables to settings.json
   - Reset settings.json environment configuration
   - Launch Claude Code with specific profile applied