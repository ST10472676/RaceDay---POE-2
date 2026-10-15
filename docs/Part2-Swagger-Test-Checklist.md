# Part 2 Swagger Test Checklist
Checklist for testing the API in Swagger UI.

## 1. Public endpoints
- [ ] `GET /api/events` returns the seeded events.
- [ ] `GET /api/events/{id}` returns event details.
- [ ] `GET /api/events/{eventId}/categories` returns categories.
- [ ] `GET /api/events/{eventId}/weather` returns weather records.

## 2. Participant account
1. `POST /api/auth/register` with role `Participant`.
2. `POST /api/auth/login`.
3. `GET /api/users/profile`.
4. `PUT /api/users/profile`.
5. `GET /api/enrolments/my-enrolments`.
6. `POST /api/enrolments` with an event and matching category.
7. `GET /api/enrolments/my-enrolments` again and show the new record.
8. `GET /api/results/my-results`.
9. `DELETE /api/enrolments/{id}` to demonstrate cancellation.
10. Try `POST /api/events` and show `403 Forbidden`.

## 3. Organiser account
1. `POST /api/auth/logout` for the Participant.
2. Register a second account with role `Organiser`.
3. Login as the Organiser.
4. `GET /api/users/profile`.
5. `POST /api/events`.
6. `PUT /api/events/{id}`.
7. `GET /api/events/{eventId}/categories`.
8. `POST /api/events/{eventId}/categories`.
9. `GET /api/events/{eventId}/enrolments`.
10. `PUT /api/enrolments/{id}/status`.
11. `POST /api/results`.
12. `GET /api/events/{eventId}/results`.
13. `PUT /api/results/{id}`.
14. `POST /api/events/{eventId}/weather`.
15. `GET /api/events/{eventId}/weather`.
16. Try `POST /api/enrolments` and show `403 Forbidden`.

## 4. Authentication failure
- [ ] Try login with an incorrect password and show `401 Unauthorized`.
- [ ] Log out.
- [ ] Call `GET /api/users/profile` and show `401 Unauthorized`.

## 5. Test project
Run:

```bash
dotnet test RaceDay.sln
```

The test output should show all tests passing before the GitHub Actions workflow is pushed.
