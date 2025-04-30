# TourPlanner

Command for creating the PostgreSQL databse using docker:

```powershell
docker run -d --rm --name postgresdb -e POSTGRES_USER=swen2 -e POSTGRES_PASSWORD=passwordswen2 -p 5432:5432 -v pgdata:/var/lib/postgresql/data postgres
```
