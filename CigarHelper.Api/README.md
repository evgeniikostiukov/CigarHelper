# CigarHelper.Api

Backend API for the Cigar Helper application with JWT authentication and PostgreSQL database.

## Requirements

- .NET 9 SDK
- PostgreSQL server (installed locally or accessible remotely)

## Database Configuration

The application uses PostgreSQL as its database. You need to have PostgreSQL installed and running.

### PostgreSQL Database Setup

1. Make sure PostgreSQL is installed and running.
2. Update the connection string in `appsettings.json` and `appsettings.Development.json` if needed:

```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Database=CigarHelperDb;Username=postgres;Password=postgres"
}
```

Replace `postgres` with your PostgreSQL username and password.

### Creating the Database

You can create the database in two ways:

#### Option 1: Using the SQL Script

Run the provided SQL script to create the database and tables:

```bash
# Run the script file directly
psql -U postgres -f create-database.sql

# Or use the shell script
./create-database.sh
```

#### Option 2: Using Entity Framework Migrations

```bash
# Make sure you have the EF Core tools installed
dotnet tool install --global dotnet-ef

# Apply migrations
dotnet ef database update
```

## Authentication

The application uses JWT (JSON Web Token) for authentication. You need to update the JWT configuration in `appsettings.json`:

```json
"Jwt": {
  "Key": "your_super_secret_key_at_least_32_characters_long",
  "Issuer": "CigarHelperApi",
  "Audience": "CigarHelperClient"
}
```

Replace the key with a strong secret key (at least 32 characters).

## Running the Application

```bash
# Build the application
dotnet build

# Run the application
dotnet run
```

The API will be available at `https://localhost:5001` and `http://localhost:5000`.

## API Endpoints

### Authentication

- **POST /api/Auth/register** - Register a new user
- **POST /api/Auth/login** - Login with credentials

### Cigars (Requires Authentication)

- **GET /api/Cigars** - Get all cigars for the authenticated user
- **GET /api/Cigars/{id}** - Get a specific cigar
- **POST /api/Cigars** - Create a new cigar
- **PUT /api/Cigars/{id}** - Update a cigar
- **DELETE /api/Cigars/{id}** - Delete a cigar 