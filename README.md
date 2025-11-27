# Roommate App (Expense Splitter)

A cross-platform mobile application built with .NET MAUI for tracking and splitting shared expenses among roommates. The application features user management, automatic debt calculation using various strategies, and a notification system.

## Features

* **Group Management**: Create and manage groups of roommates.
* **Expense Tracking**: Log shared expenses with details on who paid and who participated.
* **Debt Calculation**: Automatically calculates debts between members using flexible strategies (e.g., Even Split, Weighted Split).
* **Settlement**: Mark debts as paid with validation logic (only the creditor can confirm).
* **Notifications**: Notifies users about debt settlement via Email, SMS, or In-App alerts using the Observer pattern.
* **Authentication**: Secure user registration and login with BCrypt password hashing.

## Architecture & Design Patterns

The project follows a 4-layer architecture and implements several design patterns:

* **MVVM**: Separation of UI (Views) and logic (ViewModels).
* **Strategy Pattern**: Used for debt calculation algorithms (`IVypocetDluhuStrategy`).
* **Observer Pattern**: Used for the notification system when debt status changes (`ISubject`, `IObserver`).
* **Factory Pattern**: Used to instantiate specific notifiers (`INotifierFactory`).

## Tech Stack

* **Framework**: .NET 8 (MAUI)
* **Database**: SQLite with Entity Framework Core
* **Security**: BCrypt.Net-Next for password hashing
* **Platforms**: Android, iOS, Windows, MacCatalyst

## Setup & Usage

### Prerequisites
* Visual Studio 2022 (with .NET MAUI workload installed)
* .NET 8 SDK

### Installation
1.  Clone the repository.
2.  Open `RoommateApp.sln` in Visual Studio.
3.  Restore NuGet packages.
4.  Run the application on an emulator or physical device.

### First Run
On the first launch, the application uses `DataSeeder` to populate the SQLite database with test users and groups if it is empty.
* **Test Accounts**: Created automatically (e.g., `petr@example.com`, password: `123`).

## Project Structure

* **RoommateApp.Core**: Contains the application logic, domain models, database context, and interfaces.
    * `Models/`: Database entities (User, Group, Expense, Debt).
    * `Services/`: Business logic (Auth, Notifications, Account Management).
    * `Strategies/`: Algorithms for splitting expenses.
    * `Observers/`: Notification implementations.
* **RoommateApp.Maui**: The UI layer.
    * `Views/`: XAML pages.
    * `ViewModels/`: Data binding and command logic.
    * `Resources/`: Assets (images, fonts, styles).
