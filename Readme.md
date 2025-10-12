# NMKR Studio

NMKR Studio is a comprehensive platform for Cardano NFT management, built with C# and .NET 8.0. The project provides a complete solution for minting, burning, and managing NFTs on the Cardano blockchain.

## Project Structure

The NMKR Studio project consists of the following components:

### Core Components

- **NMKR.Pro**  
  The main NMKR Studio user interface built with Blazor, providing an intuitive web-based interface for managing NFT operations.

- **NMKR.Api**  
  Contains all API functions and endpoints for interacting with the NMKR Studio platform.

- **NMKR.BackgroundService**  
  Handles all background processing tasks including minting, burning, and other long-running operations.

- **NMKR.CardanoCliApi**  
  A separate API that encapsulates the Cardano CLI functionality, providing a clean interface for blockchain interactions.

### Shared Libraries

- **NMKR.Shared**  
  Contains shared classes, utilities, and the database structure used across all projects. This is the central location for common functionality and data models.

- **NMKR.RazorSharedClassLibrary**  
  A shared library specifically for Blazor components, currently used by NMKR.Pro.

## Prerequisites

Before running NMKR Studio, ensure you have the following services and accounts set up:

### Required Services

- **MySQL Server** - For database operations
- **RabbitMQ Server** - For message queuing
- **Redis Server** - For caching and session management
- **Blockfrost Account** - Cardano blockchain data provider
- **Koios Account** - Cardano blockchain API service
- **Maestro Account** - Cardano blockchain infrastructure
- **AWS Account** - For email services (AWS SES or similar)

### Development Requirements

- .NET 8.0 SDK
- MySQL client
- Access to the required external services listed above

## Getting Started

### 1. Database Setup

The database structure is provided in the root directory:

```bash
mysql -u your_username -p your_database < defaultdb.sql
```

### 2. Configuration

Configuration files are located in the root directory and must be populated with your own credentials and settings:

- `settings.yaml` - Main configuration file for production
- `settings.preprod.yaml` - Configuration file for pre-production environment

**Important:** Update these files with your actual service credentials, API keys, and connection strings before running the application.

### 3. Required Configuration Parameters

Ensure the following settings are configured in your `settings.yaml` file:

- MySQL connection string
- RabbitMQ connection details
- Redis connection details
- Blockfrost API key
- Koios API endpoint and credentials
- Maestro API key
- AWS email service credentials
- Cardano network settings (mainnet/testnet/preprod)

### 4. Running the Application

```bash
# Restore dependencies
dotnet restore

# Build the solution
dotnet build

# Run NMKR.Pro (main interface)
cd NMKR.Pro
dotnet run

# Run background services (in a separate terminal)
cd NMKR.BackgroundService
dotnet run

# Run API services (in a separate terminal)
cd NMKR.Api
dotnet run

# Run Cardano CLI API (in a separate terminal)
cd NMKR.CardanoCliApi
dotnet run
```

## Technology Stack

- **Framework:** .NET 8.0
- **Language:** C#
- **Frontend:** Blazor
- **Database:** MySQL
- **Message Queue:** RabbitMQ
- **Cache:** Redis
- **Blockchain:** Cardano

## Project Dependencies

Each component has specific dependencies on shared libraries:

- NMKR.Pro → NMKR.Shared, NMKR.RazorSharedClassLibrary
- NMKR.Api → NMKR.Shared
- NMKR.BackgroundService → NMKR.Shared
- NMKR.CardanoCliApi → NMKR.Shared

## Security Notes

- Never commit `settings.yaml` or `settings.preprod.yaml` files with actual credentials to version control
- Keep all API keys and secrets secure
- Ensure database credentials are stored securely
- Use environment-specific configuration files for different deployment environments

## Additional Information

For more information about NMKR Studio features and usage, please refer to the official documentation or contact the development team.

## License

Licensed under the MIT License. See `LICENSE` for details.

## Contributing

We welcome contributions. The common approach is:

1. Fork the repository and create a feature branch
2. Make focused changes following the existing code style
3. Ensure builds succeed and add tests where applicable
4. Open a pull request with a clear description of the changes

## Support

For contact and support, visit [NMKR Support](https://nmkr.io/support).
