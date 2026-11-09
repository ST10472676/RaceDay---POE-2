# RaceDay – Part 1, Part 2 and Part 3

RaceDay is a South African road running, walking and cycling event management platform. This repository preserves the Part 1 planning artefacts and Part 2 ASP.NET Core REST API, and adds a Part 3 ASP.NET Core MVC front end.

## Architecture

- **RaceDay.Api**: ASP.NET Core Web API, EF Core and SQL Server. It remains the only application component that accesses the database.
- **RaceDay.Mvc**: ASP.NET Core MVC application. All event, account, category, enrolment, result and profile operations are sent to the API using `HttpClient`.
- **Azure Blob Storage**: Images are uploaded from MVC to API upload endpoints; the API stores them in Blob containers and saves image URLs.
- **Docker**: The MVC application is built and run in a Linux container.

The MVC project must not connect directly to SQL Server and must not contain duplicated database business logic.

## Roles and features

### Organiser
- Role-aware dashboard and navigation.
- Create, edit and delete events.
- Upload event banner images.
- Add and view event categories.
- View event enrolments and update their statuses.
- Capture race finish times, finishing positions and result statuses.

### Participant
- Browse and view events and available categories.
- Enrol in an event and cancel an enrolment.
- View personal event entries and race history.
- View and update profile information and upload a profile picture.

## Requirements

- .NET 8 SDK
- Visual Studio 2022 or later
- SQL Server / SQL Server Express and SSMS
- Azure Storage account with Blob service enabled (for real cloud image uploads)
- Docker Desktop (for container execution)

## 1. Database setup

1. Back up the existing database.
2. Run `docs/RaceDayDatabase.sql` only if creating a fresh database.
3. For an existing Part 2 database, run `docs/Part3-Database-Update.sql` once to add `Users.ProfilePictureUrl`.
4. Confirm the API connection string in `src/RaceDay.Api/appsettings.json` or use an environment variable. Do not commit production credentials.

## 2. Configure and run the API

Set the SQL Server connection string using a local user-secrets store or environment variable. Example PowerShell environment variable:

```powershell
$env:ConnectionStrings__RaceDayConnection="Server=localhost\SQLEXPRESS01;Database=RaceDayDB;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
```

Run the API from the repository root:

```bash
dotnet restore RaceDay.sln
dotnet run --project src/RaceDay.Api/RaceDay.Api.csproj --urls http://localhost:5080
```

Swagger is available in Development at `http://localhost:5080/swagger`.

## 3. Configure Azure Blob Storage

Create an Azure Storage account and obtain its connection string from the Azure Portal. Configure it as a secret; do not commit it to Git.

PowerShell:

```powershell
$env:AzureBlobStorage__ConnectionString="<your Azure Storage connection string>"
```

Linux/macOS:

```bash
export AzureBlobStorage__ConnectionString="<your Azure Storage connection string>"
```

The API creates these containers when needed:

- `event-banners`
- `profile-pictures`

Accepted image formats: JPEG, PNG and WebP. Maximum upload size: 5 MB. The API uploads the image and returns its URL; the MVC application never talks to Azure directly.

## 4. Run the MVC application locally

Start the API first on `http://localhost:5080`. In another terminal:

```bash
dotnet run --project src/RaceDay.Mvc/RaceDay.Mvc.csproj
```

Open `http://localhost:5081` (or the URL shown in the terminal). If the API uses another address, set `ApiSettings:BaseUrl` in `src/RaceDay.Mvc/appsettings.json` or the `ApiSettings__BaseUrl` environment variable.

**Session flow:** MVC stores the user's ID and role in its own session. The API's `.RaceDay.Session` cookie is also retained in the MVC session and forwarded with subsequent `HttpClient` requests. The API remains responsible for authorising each protected operation.

## 5. Docker

Build the MVC container:

```bash
docker build -t raceday-mvc:part3 .
```

Run it:

```bash
docker run --rm -p 8080:8080 -e ApiSettings__BaseUrl="http://host.docker.internal:5080/" raceday-mvc:part3
```

Or use Compose:

```bash
docker compose up --build
```

Then open `http://localhost:8080`. The API must be running and reachable at the configured `ApiSettings__BaseUrl`. On Docker Desktop, `host.docker.internal` points from the container to the host machine. For a fully containerised deployment, configure the API as a separate service and use its Compose service name as the base URL. The current Dockerfile intentionally builds the MVC web application, as required by Part 3.

## 6. Tests and CI/CD

Run the existing Part 2 API tests:

```bash
dotnet test tests/RaceDay.Api.Tests/RaceDay.Api.Tests.csproj
```

The `.github/workflows/part3-ci.yml` workflow restores and builds the solution, runs the API tests and builds the MVC Docker image on pushes and pull requests to `main`. Check the Actions tab and include a genuine screenshot of a successful green run in your final evidence.

## 7. Important verification before submission

- Test registration and login for both roles.
- Confirm an unauthenticated user is redirected from protected MVC pages.
- Confirm an Organiser can manage only their own events and a Participant cannot access Organiser actions.
- Create an event with a valid existing `RouteID`.
- Upload an event banner and verify that the URL is stored and the image displays.
- Enrol a Participant, update the enrolment status, and capture a result.
- Update a Participant profile and upload a profile image.
- Build and run the MVC container while the API is reachable.
- Verify the GitHub Actions workflow is green.

## 8. Part 3 video and evidence

Add your own unlisted YouTube demonstration link here after recording your voiceover:

`PART3_YOUTUBE_VIDEO_LINK_TO_BE_ADDED`

Add a genuine screenshot of the green GitHub Actions run under `docs/CI-Part3-Success.png` and link it here once available. Do not use a fabricated screenshot or claim a build passed before it has run.

## 9. GitHub submission checklist

- Push the source code and Docker files to the prescribed Part 3 branch/folder in the lecturer-provided repository.
- Make at least 20 meaningful commits for Part 3. Commits must represent actual development work; do not manufacture commits just to meet the number.
- Include the README, Docker run command, real green CI screenshot and unlisted YouTube link.
- Submit the GitHub repository link through ARC. The question paper states that ZIP files are not accepted as the submission.

## AI-use disclosure

If AI tools were used for planning, code assistance or proofreading, disclose that use in the manner required by the module instructions. Review, understand, test and adapt every generated change before submitting it as your own work.
