# Part 3 Architecture and Verification Plan

## Request flow
1. The browser submits a form to the RaceDay MVC application.
2. An MVC controller calls `RaceDayApiClient`, which sends an HTTP request to the existing RaceDay API.
3. The API's session cookie is retained inside the MVC session and forwarded on later API calls.
4. The API checks the session and role, performs the operation through its existing services/controllers and EF Core context, and returns JSON.
5. MVC renders the returned data. MVC has no database connection.

## Image upload flow
1. An Organiser selects a banner in the MVC event form, or a user selects a profile picture.
2. MVC sends multipart form data to `POST /api/uploads/events/{eventId}/banner` or `POST /api/uploads/profile-picture`.
3. The API validates the image type and 5 MB limit, uploads it to Azure Blob Storage and saves the resulting URL in SQL Server.
4. MVC displays the returned/stored URL.

## New API configuration
Set `AzureBlobStorage__ConnectionString` as a secret/environment variable on the API host. Do not commit the connection string. The storage account must permit the intended image access policy. The implementation creates public-blob containers to allow the image URLs to be rendered directly; if the storage account blocks anonymous blob access, configure a private-container/SAS strategy before deployment.

## Database change
The Part 2 `Users` table did not contain a profile image URL. Run `docs/Part3-Database-Update.sql` once against the existing database. This is an additive nullable column; it does not remove or rename existing data.

## Manual test matrix
| Scenario | Expected outcome |
|---|---|
| Browse events without signing in | Public event list and details display |
| Register as Participant | API creates account; MVC returns user to login |
| Register as Organiser | API creates account; MVC returns user to login |
| Login as Organiser | MVC session created; organiser menu shown |
| Participant opens organiser page | Redirected to public home |
| Unauthenticated user opens protected page | Redirected to login |
| Organiser creates event | API creates event for authenticated organiser |
| Organiser uploads banner | API uploads to Azure and persists banner URL |
| Organiser manages categories/enrolments/results | API authorises role and event ownership |
| Participant enrols in event | API creates pending enrolment |
| Participant opens My Results | Only that participant's results returned |
| Profile picture upload | API uploads to Blob and persists profile URL |
| Logout | API and MVC session cleared |
| Docker run | MVC container serves UI and reaches configured API URL |

Automated tests currently cover the API project. Add MVC integration tests and end-to-end Azure tests as a further improvement; Azure credentials should never be placed in the test repository.
