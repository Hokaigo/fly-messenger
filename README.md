## Messenger App

**Messenger** is a real-time chat web application supporting file uploads, online presence, message editing, and deletion.

### Main Features

* **Authentication & Registration**

  * User signup and login with cookie-based sessions

* **Chats**

  * Create private chats between two users
  * View chat list with last message preview and timestamp
  * Open a chat to display message history (text and files) and online status

* **Messages**

  * Send text and file messages (with type and size restrictions)
  * Edit and delete own messages via context menu
  * Render images, videos, and other file types inline

* **User Profile**

  * View and edit profile (name, bio, avatar)
  * Online/offline status indicator

### Local Setup

1. **Prerequisites**

   ```bash
   dotnet --version   # Requires .NET SDK 7.0 or higher
   node -v && npm -v  # Requires Node.js and npm
   ```

2. **Install Dependencies**
   In the project root, run:

   ```bash
   dotnet restore
   npm install
   ```

   *Tip: If you don’t have .NET installed, download it from [https://dotnet.microsoft.com/download](https://dotnet.microsoft.com/download), then verify with `dotnet --version`.*

3. **Configure the Application**
   Create an `appsettings.json` file in the project root with the following content:

   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=192.168.43.177,1433;Database=Messenger;User Id=YOUR_USERNAME;Password=YOUR_PASSWORD;TrustServerCertificate=True;"
     },
     "JwtSettings": {
       "Key": "YOUR_SECRET_KEY",
       "Issuer": "messenger.api",
       "Audience": "messenger.client",
       "ExpiresInMinutes": 1440
     },
     "Logging": {
       "LogLevel": {
         "Default": "Information",
         "Microsoft": "Warning"
       }
     },
     "AllowedHosts": "*"
   }
   ```

4. **Run the Project**

   ```bash
   dotnet run --project Messenger.Web
   ```

5. **Open in Browser**
   Navigate to `http://localhost:5000` (or the URL shown in the console).

### Programming Principles

1. **Single Responsibility (SRP)**
   Each class has one responsibility. E.g., `ChatService.cs` handles chat logic only; `FileStorageService.cs` handles file storage only.

2. **Open/Closed (OCP)**
   The system is open for extension but closed for modification. E.g., add new validators like `ProfanityValidator.cs` via DI in `Program.cs` without changing existing handlers.

3. **Dependency Inversion (DIP)**
   High-level modules depend on abstractions. E.g., `MessageService` receives `IMessageRepository` and `IUserRepository` interfaces in its constructor.

4. **Interface Segregation (ISP)**
   Fine-grained interfaces. E.g., `IMessageHandler` only defines `SetNext` and `ProcessAsync`; `IMessageValidator` only defines `ValidateAsync`.

5. **Liskov Substitution (LSP)**
   Subclasses (`FileMessageHandler`, `TextMessageHandler`) can be used interchangeably with their base (`MessageHandlerTemplate`) without altering correctness.

6. **Don’t Repeat Yourself (DRY)**
   Centralize shared logic, e.g., mapping profiles in `MappingProfile.cs` (AutoMapper) and HTTP utilities in `api.js`.

7. **Keep It Simple & Stupid (KISS)**
   Favor simple and clear implementations over unnecessary complexity.

### Design Patterns

**Primary Patterns**

1. **Factory Method**
   `ChatFactory` encapsulates chat entity creation with proper initialization.

2. **Template Method**
   `MessageHandlerTemplate` defines the skeleton workflow for message handling.

3. **Chain of Responsibility**
   Sequential message processing via `FileMessageHandler` → `TextMessageHandler`, enabling modular validation and handling.

4. **Strategy**
   Dynamic selection of validation rules (`LengthValidator`, `ProfanityValidator`, `SpamValidator`) via `IMessageValidator` implementations.

**Additional Patterns**

5. **Repository**
   Abstract data access in `ChatRepository`, `MessageRepository`, and `UserRepository`.

6. **Singleton**
   `OnlineUserTracker` registered as a single DI instance for centralized user status tracking.

### Refactoring Techniques

* **Extract Method**: Refactor complex logic into smaller private methods within controllers and services.
* **Rename Variable**: Use descriptive names for clarity.
* **Introduce Interface**: Decouple contracts from implementations (repositories, services, validators).
* **Move Class**: Organize classes into proper namespaces and folders.
* **Consolidate Conditional**: Simplify nested `if-else` structures.
* **Remove Duplicate Code**: Eliminate repeated fragments (e.g., `AllowedExtensions` arrays).
* **Encapsulate Field**: Use properties to maintain invariants instead of public fields.
* **Introduce Parameter Object**: Group related parameters into DTOs for cleaner method signatures.
* **Remove Dead Code**: Prune unused methods and commented-out fragments after refactoring.
