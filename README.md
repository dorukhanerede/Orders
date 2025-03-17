# Orders
 
 A .NET 9 solution that interacts with the ChannelEngine API to fetch and process orders. The application retrieves IN_PROGRESS orders, identifies the top 5 most sold products, and updates product stock.

## Solution Overview

The solution consists of multiple projects to maintain an architecture:

- **Orders.Service** – API service handling HTTP requests for order processing.
- **Orders.BusinessLogic** – Contains business logic like identifying top-sold products.
- **Orders.ConsoleApp** – CLI application that interacts with the service.
- **Orders.WebApp** – MVC web application to display orders.
- **Orders.Shared** – Shared contracts and models used across the projects.
- **Orders.Service.Tests** – Unit tests for business logic and service layer.

## Key Features
- Rate-limited & retry API calls using Polly
    - Implements rate limiting
    - Implements exponential backoff retries for transient failures
- Standardized Result Handling (`Result<T>` & `ApiResult`)
    - Introduces a unified error-handling mechanism with `Result<T>`, ensuring consistent responses across the application.
    - Separates `ApiResult` from `Result<T>`, which helps distinguish between business logic results and HTTP API responses.
- Clean Architecture & Separation of Concerns
    - Business logic (`OrderProcessor`) is fully independent and only processes data.
    - API calls are handled separately (`OrderService`), keeping the logic modular.
    - Loose coupling is maintained via dependency injection.

## Future Improvements

1) Swagger documentation
2) Handle Pagination Efficiently
3) Use AWS or Azure for secret management instead of appsettings.json
4) Use Automapper to simplfy DTO and entity mappings as the project scales
5) Implement Authorization
6) ApiResult and Result can be extended even more to cover more scenarios where additional information is needed
7) Validate request bodies using FluentValidation instead of manual checks
8) Implement a RestClient factory for multiple Clients
9) Improve unit test Coverage

## How to Run

Clone the repo and create Orders.Service/appsettings.Development.json file with:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  },
  "ChannelEngine": {
    "ChannelEngineApiBaseUrl": "https://api-dev.channelengine.net/api/v2/",
    "ChannelEngineApiKey": "541b989ef78ccb1bad630ea5b85c6ebff9ca3322"
  }
}
```