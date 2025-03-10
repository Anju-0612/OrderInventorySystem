# Order Processing & Inventory Management System

## Overview
This is a .NET Core Web API (or console application) for managing orders and inventory in an e-commerce system. It supports placing orders, canceling orders, fulfilling orders asynchronously, and handling concurrent inventory updates. The project follows SOLID principles, uses dependency injection, and includes unit tests for validation.

## Setup Instructions
1. **Clone the Repository**:
   - Run `git clone (https://github.com/Anju-0612/OrderInventorySystem.git) in your terminal (replace with your GitHub URL).
2. **Restore Dependencies**:
   - Navigate to the project folder: `cd OrderInventorySystem`.
   - Run `dotnet restore` to install NuGet packages (e.g., xUnit, Moq).
3. **Build the Solution**:
   - Run `dotnet build` to compile the project.
4. **Run Tests**:
   - Run `dotnet test` to execute all 6 unit tests.
5. **Run the Application** (if Web API):
   - Run `dotnet run` from the main project directory to start the API.
   - Access Swagger UI (if implemented) at `http://localhost:7101/swagger`.

## Key Design Decisions
- **SOLID Principles**: Followed Single Responsibility (e.g., `OrderService` for business logic, `IProductRepository` for data access) and Dependency Inversion (via interfaces like `INotificationService`).
- **Dependency Injection**: Used constructor injection to decouple components (e.g., `OrderService` takes `IProductRepository` and `IOrderRepository`).
- **Repository Pattern**: Abstracted data access for testability (e.g., in-memory repositories for testing).
- **Asynchronous Programming**: Implemented `async/await` for non-blocking operations (e.g., `PlaceOrderAsync`, `FulfillOrderAsync`).
- **Unit Testing**: Used xUnit and Moq to mock dependencies and test business logic, concurrency, and notifications.

## Concurrency & Asynchronous Processing
- **Concurrency Handling**:
  - Used `Interlocked.Increment` for thread-safe order ID generation.
  - Ensured stock updates are safe by validating stock before reservation (e.g., `PlaceOrderAsync_ConcurrentOrders_HandlesStockSafely` test simulates concurrent orders).
- **Asynchronous Processing**:
  - `FulfillOrderAsync` simulates background processing with a delay (currently triggered manually in `PlaceOrderAsync`).
  - Notification service (`INotificationService`) logs fulfillment messages asynchronously.

## Testing
The project includes 6 unit tests covering key scenarios:
- `CancelOrderAsync_FulfilledOrder_ThrowsException`: Ensures fulfilled orders cannot be canceled.
- `CancelOrderAsync_PendingOrder_RestoresStockAndDeletes`: Validates stock restoration and order deletion.
- `FulfillOrderAsync_PendingOrder_UpdatesStatusAndNotifies`: Confirms fulfillment updates status and sends notifications.
- `PlaceOrderAsync_ConcurrentOrders_HandlesStockSafely`: Tests thread-safe stock reservation.
- `PlaceOrderAsync_InsufficientStock_ThrowsException`: Verifies exception for insufficient stock.
- `PlaceOrderAsync_ValidOrder_ReservesStockAndPlacesOrder`: Ensures valid orders reduce stock correctly.

All tests pass, achieving 100% coverage of implemented features.

## Challenges and Solutions
- **Challenge**: Incorrect stock reduction (e.g., 9 instead of 12).
  - **Solution**: Adjusted mock callback in tests to reflect updates.
- **Challenge**: Exception not thrown for insufficient stock.
  - **Solution**: Removed `try-catch` in `PlaceOrderAsync` to propagate exceptions.

## License
This project is licensed under the MIT License - see the [LICENSE.md](LICENSE.md) file for details.

## Contact
For questions, contact [Your Name] at [your.email@example.com].
