# ScreenshotApp

A WPF-based screenshot capture application with annotation capabilities.

## Project Structure

```
ScreenshotApp/
├── ScreenshotApp/              # Main WPF Application
│   ├── Infrastructure/         # ServiceLocator, ViewModelLocator, Win32 interop
│   ├── Services/               # Service implementations
│   ├── ViewModels/             # MVVM ViewModels
│   └── Views/                  # XAML Views
├── ScreenshotApp.Core/         # Business Logic (testable, no WPF dependencies)
│   ├── Models/                 # Domain models
│   └── Services/Interfaces/    # Service contracts
└── ScreenshotApp.Tests/        # Unit Tests (xUnit + FluentAssertions)
```

## Build Requirements

- .NET 6.0 or higher
- Windows 10/11
- Visual Studio 2022 or VS Code with C# extension

## Build Instructions

```bash
# Restore packages
dotnet restore

# Build solution
dotnet build

# Run tests
dotnet test

# Run application
dotnet run --project ScreenshotApp
```

## Architecture

### MVVM Pattern
- **Models**: Domain entities (CaptureRegion, CaptureResult, etc.)
- **ViewModels**: Observable objects using CommunityToolkit.Mvvm
- **Views**: XAML with data binding via ViewModelLocator

### Service Locator Pattern
Services are registered in `App.xaml.cs` and accessed via `ServiceLocator`:

```csharp
var hotkeyService = ServiceLocator.Get<IHotkeyService>();
```

### Win32 Interop
All native API calls are wrapped with defensive error handling:
- `Try-pattern` methods return bool with error messages
- No exceptions thrown from Win32 operations
- Automatic resource cleanup in finally blocks

## Features (In Development)

- [x] Solution structure and MVVM infrastructure
- [ ] System tray integration
- [ ] Global hotkey registration
- [ ] Screen capture with region selection
- [ ] Annotation tools
- [ ] Save and copy to clipboard
- [ ] Settings and personalization

## License

MIT
