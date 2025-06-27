# TourPlanner
TourPlanner is an application we developed for the course SWEN2 in the summer semester of 2025. The application is a GUI app running on Windows Presentation Foundation (WPF), implementing the MVVM design pattern along with numerous other best practices, such as the S.O.L.I.D. principles.  
As the name suggests, this application enables users to manage tours. Features include, among others:

- Creating, editing, and deleting tours ✏️
- Automatic calculation of route, distance, and duration for a tour 🧭
- Displaying tours on a map 🗺️
- Storing log entries for tours 📒
- Full-text search across tours and log entries 🔍
- Automatically calculated tour attributes such as child-friendliness and popularity 👶⭐
- AI generated summaries of tours 🤖📝
- Import and export of tours 🔄
- PDF export of tours (individual tours or a summary of all tours) 📄
- Dark mode support 🌙

The frontend can only store tours in-memory. For persistence, a PostgreSQL database, which is accessed via a REST API, is used. This API is also written in C# and utilizes ASP.NET Core.

## Setup

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) (or the runtime)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- Microsoft Windows 11 (might work on Windows 10 or lower, but we haven't tested it)
    - Due to the use of WPF, the application is not compatible with Linux or macOS.

### Rest-API
TourPlanner requires a REST API server to save and retrieve tours from a PostgreSQL database. To get the API up and running, please follow these steps:

1. Start a PostgreSQL database. The easiest method is using Docker:

```powershell
docker run -d --rm --name postgresdb -e POSTGRES_USER=swen2 -e POSTGRES_PASSWORD=passwordswen2 -p 5432:5432 -v pgdata:/var/lib/postgresql/data postgres
```
You can also host the database externally and change the username/password or ports. In that case, adjust the connection string in the `appsettings.json` file of `TourPlanner.RestServer` accordingly.

2. Connect to the database and create a new database named `TourPlanner`:

```sql
psql -U swen2
CREATE DATABASE "TourPlanner";
```

3. Start the REST API server:

```powershell
dotnet run --project .\TourPlanner.RestServer\TourPlanner.RestServer.csproj
```

### Frontend

1. Enter the URL of the REST API server in the `appsettings.json` file of `TourPlanner`, e.g., `http://localhost:5168`
2. Enter your OpenRouteService and OpenRouter API key in the `appsettings.json` file of `TourPlanner`. For OpenRouter, you can use an invalid API key if you want to run the app without AI features.
3. Start the WPF application:

```powershell
dotnet run --project .\TourPlanner\TourPlanner.csproj
```