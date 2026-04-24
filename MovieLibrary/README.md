# Завдання 2: Бібліотека фільмів

## Предметна область
Каталог фільмів з рейтингами та рецензіями.

## Сутності
Movie: Id, Title, Director, Genre, ReleaseYear, DurationMinutes, Rating
Review: Id, MovieId, UserId, Score (1-10), Comment, CreatedAt
User: Id, Username, Email

## Business rules

- оцінка відгуку має бути від 1 до 10
- один користувач може залишити лише один відгук на фільм
- рік випуску не може бути в майбутньому
- відгуки мають містити щонайменше 20 символів

## Як зібрати та запустити

Start PostgreSQL first:

```bash
docker compose up -d postgres
```

Build the solution:

```bash
dotnet build MovieLibrary.slnx
```

Run the API:

```bash
dotnet run --project src/MovieLibrary.Api
```

## Як запустити тести

Run all tests:

```bash
dotnet test MovieLibrary.slnx --verbosity normal
```

Run only unit tests:

```bash
dotnet test tests/MovieLibrary.UnitTests/MovieLibrary.UnitTests.csproj --verbosity normal
```

## Як запустити тести k6

With the API running:

```bash
k6 run k6/smoke-test.js
k6 run k6/load-test.js
k6 run k6/stress-test.js
```

## Як згенерувати звіт покриття

```bash
dotnet test --collect:"XPlat Code Coverage"
```