## Messenger App

**Messenger** is a real-time chat web application supporting file uploads, online presence, message editing and deletion.

### Main Features

* **Authentication/Registration**: user signup and login with cookie-based sessions.
* **Chats**:

  * Create private chats between two users
  * View chat list with last message preview and timestamp
  * Open chat: display message history (text/files) and online status
* **Messages**:

  * Send text and file messages (type and size restrictions)
  * Edit and delete own messages via context menu
  * Render images, videos, and other file types inline
* **User Profile**:

  * View and edit profile (name, bio, avatar)
  * Online/offline status indicator

### Local Setup

1. In project root, install dependencies: `dotnet restore` and `npm install`

2. Add an `appsettings.json` file in the root with example configuration:

   ```json
   {
   /*Tip: use local connection string if your database is on your PC*/
     "ConnectionStrings": {
       "DefaultConnection": "Server=192.168.43.177,1433;Database=Messenger;User Id=[YOUR_NAME];Password=[YOUR_PASSWORD];TrustServerCertificate=True;"
     },
     "JwtSettings": {
       "Key": "<YOUR_SECRET_KEY>",
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

3. Run a project

4. Open in browser on yout local address
   
## Programming Principles

1. **Single Responsibility (SRP)** — classes have one responsibility, e.g., `ChatService.cs` handles chat logic only; `FileStorageService.cs` handles file storage only.
2. **Open/Closed (OCP)** — extend without modifying, e.g., new validators like `ProfanityValidator.cs` can be added with DI in `Program.cs` without touching `TextMessageHandler`.
3. **Dependency Inversion (DIP)** — higher-level modules depend on abstractions, e.g., `MessageService` constructor uses `IMessageRepository`, `IUserRepository` interfaces.
4. **Interface Segregation (ISP)** — fine-grained interfaces, e.g., `IMessageHandler` has only `SetNext` and `ProcessAsync`; `IMessageValidator` only `ValidateAsync`.
5. **Liskov Substitution (LSP)** — subclasses (`FileMessageHandler.cs`, `TextMessageHandler.cs`) behave consistently when used as `MessageHandlerTemplate`.
6. **Don’t Repeat Yourself (DRY)** — shared mapping logic centralized in `MappingProfile.cs` (AutoMapper), HTTP utilities in `api.js`.
7. **KISS** - most of the methods follow pretty simple logic.

## Design Patterns

**Primary Patterns**

1. **Factory Method** — `ChatFactory` encapsulates chat entity creation with proper initialization.
2. **Template Method** — `MessageHandlerTemplate` defines the skeleton workflow for message handling.
3. **Chain of Responsibility** — sequential message processing via `FileMessageHandler` → `TextMessageHandler`, enabling modular validation and handling.
4. **Strategy** — dynamic validation rules provided by `IMessageValidator` implementations (`LengthValidator`, `ProfanityValidator`, `SpamValidator`).

**Additional Patterns**

5. **Repository** — data access abstraction in `ChatRepository`, `MessageRepository`, `UserRepository`.
6. **Singleton** — `OnlineUserTracker` registered as a single instance in DI for centralized user status tracking.
   
## Refactoring Techniques

* **Extract Method** — refactored complex logic into private methods (controllers, services).
* **Rename Variable** — improved clarity with descriptive names.
* **Introduce Interface** — decoupled implementations from contracts (repositories, services, validators).
* **Move Class** — organized classes into appropriate namespaces and folders.
* **Consolidate Conditional** — simplified nested if else structures.
* **Remove Duplicate Code** — eliminated repeated fragments like `AllowedExtensions` definitions.
* **Encapsulate Field** — accessed fields via properties to maintain invariants.
* **Introduce Parameter Object** — grouped multiple parameters into DTOs for cleaner method signatures.
* **Remove Dead Code** — pruned unused methods and commented fragments after refactoring.
