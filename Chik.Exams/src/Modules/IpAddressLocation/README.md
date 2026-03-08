# IpAddressLocation

## Overview

The **IpAddressLocation** module maps IP addresses to country codes and provides timezone resolution based on geographic location. It is used to determine a user's locale from their login IP address, enabling features such as localized time display. Each IP address location record can be associated with one or more logins, forming a link between user authentication events and their geographic origin.

## Features

- **IP-to-Country Mapping** — Stores and retrieves the relationship between IP addresses and ISO country codes.
- **Timezone Resolution** — Resolves a country code to a representative `TimeZoneInfo` using the IANA/TZDB timezone database (via NodaTime), selecting the median timezone when a country spans multiple zones.
- **User Local Time** — Extension methods on the `Auth` user model to determine a user's timezone and current local time based on their most recent login IP.
- **CRUD Operations** — Full create, read, update, and delete support with filtering capabilities.
- **Upsert on Create** — Creating a location for an already-known IP address updates the existing record rather than duplicating it.
- **Dependency Injection** — Single extension method to register all module services into the DI container.

## Data Model

```
┌──────────────────────────┐         ┌──────────────┐
│   IpAddressLocationDbo   │         │   LoginDbo   │
├──────────────────────────┤         ├──────────────┤
│ Id          : Guid (PK)  │ 1────*  │ ...          │
│ IpAddress   : string     │         │              │
│ CountryCode : string     │         │              │
└──────────────────────────┘         └──────────────┘
```

Each `IpAddressLocation` can be associated with many `Login` records.

## Registration

Register the module's services with a single call in your `IServiceCollection` configuration:

```csharp
services.AddIpAddressLocation();
```

This registers:

| Interface                      | Implementation                | Lifetime |
| ------------------------------ | ----------------------------- | -------- |
| `IIpAddressLocationRepository` | `IpAddressLocationRepository` | Scoped   |
| `IIpAddressLocationService`    | `IpAddressLocationService`    | Scoped   |

## Public API

### IIpAddressLocationService

| Method             | Parameters                            | Return Type               | Description                                                                       |
| ------------------ | ------------------------------------- | ------------------------- | --------------------------------------------------------------------------------- |
| `Create`           | `IpAddressLocation.Create`            | `Task<IpAddressLocation>` | Creates a new record or updates an existing one if the IP address already exists. |
| `Update`           | `Guid id`, `IpAddressLocation.Update` | `Task<IpAddressLocation>` | Partially updates an existing record by ID. Only non-null fields are applied.     |
| `Get`              | `Guid id`                             | `Task<IpAddressLocation>` | Retrieves a single record by its primary key.                                     |
| `GetByIpAddress`   | `string ipAddress`                    | `Task<IpAddressLocation>` | Looks up a record by its IP address.                                              |
| `GetByCountryCode` | `string countryCode`                  | `Task<IpAddressLocation>` | Retrieves the first record matching a country code.                               |

### IIpAddressLocationRepository

The repository exposes the same CRUD operations as the service plus:

| Method   | Parameters                 | Return Type                        | Description                                                                 |
| -------- | -------------------------- | ---------------------------------- | --------------------------------------------------------------------------- |
| `Get`    | `IpAddressLocation.Filter?` | `Task<List<IpAddressLocationDbo>>` | Returns all records, optionally filtered by IP address and/or country code. |
| `Delete` | `Guid id`                  | `Task`                             | Deletes a record by ID. No-ops if the record does not exist.                |

### IpAddressLocation (Domain Model)

#### Records

| Record                     | Fields                                              | Description                                         |
| -------------------------- | --------------------------------------------------- | --------------------------------------------------- |
| `IpAddressLocation`        | `Guid Id`, `string IpAddress`, `string CountryCode` | Core domain record.                                 |
| `IpAddressLocation.Create` | `string IpAddress`, `string CountryCode`            | Input for creating a new location.                  |
| `IpAddressLocation.Update` | `string? IpAddress`, `string? CountryCode`          | Input for partial updates; null fields are skipped. |

#### Methods

| Method                                   | Parameters           | Return Type    | Description                                                                                                                            |
| ---------------------------------------- | -------------------- | -------------- | -------------------------------------------------------------------------------------------------------------------------------------- |
| `GetCountryTimezone()`                   | —                    | `TimeZoneInfo` | Returns the median timezone for the record's country code.                                                                             |
| `GetCountryTimezone(string countryCode)` | `string countryCode` | `TimeZoneInfo` | (Static) Resolves a country code to its median timezone using NodaTime's TZDB source. Returns `TimeZoneInfo.Utc` if no match is found. |

### IpAddressLocation.Filter

```csharp
public record IpAddressLocation.Filter(
    string? IpAddress = null,
    string? CountryCode = null
);
```

Used to filter results when listing IP address locations. Both fields are optional; when set, they apply equality matching.

### User Extension Methods

These extension methods on `Auth` bridge the gap between authentication and localization:

| Method         | Parameters                                                  | Return Type          | Description                                                                                  |
| -------------- | ----------------------------------------------------------- | -------------------- | -------------------------------------------------------------------------------------------- |
| `GetTimezone`  | `ILoginService? loginService`                               | `Task<TimeZoneInfo>` | Determines the user's timezone from their last login IP. Falls back to `TimeZoneInfo.Local`. |
| `GetLocalTime` | `ILoginService? loginService`, `TimeProvider? timeProvider` | `Task<DateTime>`     | Returns the user's current local time by converting UTC to their resolved timezone.          |

## Timezone Resolution Flow

```
User Login
    │
    ▼
Retrieve last Login record
    │
    ▼
Get associated IpAddressLocation
    │
    ▼
Extract CountryCode
    │
    ▼
Query NodaTime TzdbDateTimeZoneSource
for all zones in that country
    │
    ▼
Match against system TimeZoneInfo entries
    │
    ▼
Sort by current UTC offset
    │
    ▼
Select median timezone
    │
    ▼
Return TimeZoneInfo
```

## Usage Examples

### Creating an IP address location

```csharp
var location = await ipAddressLocationService.Create(
    new IpAddressLocation.Create("203.0.113.42", "NG")
);
```

### Resolving a user's local time

```csharp
Auth user = GetCurrentUser();
DateTime localTime = await user.GetLocalTime();
```

### Filtering locations by country

```csharp
var filter = new IpAddressLocation.Filter(CountryCode: "US");
var locations = await repository.Get(filter);
```

### Getting a timezone from a country code

```csharp
TimeZoneInfo timezone = IpAddressLocation.GetCountryTimezone("DE");
```
