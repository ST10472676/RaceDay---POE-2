# RaceDay API Endpoint Plan
## Comrades Marathon Event Management System

This document describes the planned API endpoints for the RaceDay Comrades Marathon Event Management System.


# 1. Authentication Endpoints
## 1.1 Register User

**HTTP Method:** POST

**Route:** `/api/auth/register`

**Description:** Registers a new user in the RaceDay system as either a Participant or Organiser.

**Role Required:** None

**Request Body:**

* FirstName
* LastName
* Email
* Password
* PhoneNumber
* Role

**Expected Response:** `201 Created` – User account successfully created.


## 1.2 Login User

**HTTP Method:** POST

**Route:** `/api/auth/login`

**Description:** Authenticates a registered user and allows them to access the RaceDay system.

**Role Required:** None

**Request Body:**

* Email
* Password

**Expected Response:** `200 OK` – Authentication successful.


## 1.3 Logout User

**HTTP Method:** POST

**Route:** `/api/auth/logout`

**Description:** Logs the currently authenticated user out of the system.

**Role Required:** Authenticated User

**Request Body:** None

**Expected Response:** `200 OK` – User successfully logged out.


# 2. User Profile Endpoints
## 2.1 Get User Profile

**HTTP Method:** GET

**Route:** `/api/users/profile`

**Description:** Retrieves the profile information of the currently logged-in user.

**Role Required:** Organiser or Participant

**Request Body:** None

**Expected Response:** `200 OK` – User profile returned successfully.


## 2.2 Update User Profile

**HTTP Method:** PUT

**Route:** `/api/users/profile`

**Description:** Updates the profile information of the currently logged-in user.

**Role Required:** Organiser or Participant

**Request Body:**

* FirstName
* LastName
* Email
* PhoneNumber

**Expected Response:** `200 OK` – Profile successfully updated.


# 3. Event Endpoints
## 3.1 Get All Events

**HTTP Method:** GET

**Route:** `/api/events`

**Description:** Retrieves all available race events in the RaceDay system.

**Role Required:** None

**Request Body:** None

**Expected Response:** `200 OK` – List of events returned successfully.


## 3.2 Get Event by ID

**HTTP Method:** GET

**Route:** `/api/events/{id}`

**Description:** Retrieves detailed information about a specific race event.

**Role Required:** None

**Request Body:** None

**Expected Response:** `200 OK` – Event information returned successfully.


## 3.3 Create Event

**HTTP Method:** POST

**Route:** `/api/events`

**Description:** Creates a new race event.

**Role Required:** Organiser

**Request Body:**

* EventName
* Description
* EventDate
* Location
* Distance
* EventType
* RouteID
* BannerImageUrl

**Expected Response:** `201 Created` – Event successfully created.


## 3.4 Update Event

**HTTP Method:** PUT

**Route:** `/api/events/{id}`

**Description:** Updates an existing race event.

**Role Required:** Organiser

**Request Body:**

* EventName
* Description
* EventDate
* Location
* Distance
* EventType
* RouteID
* BannerImageUrl

**Expected Response:** `200 OK` – Event successfully updated.


## 3.5 Delete Event

**HTTP Method:** DELETE

**Route:** `/api/events/{id}`

**Description:** Deletes an existing race event.

**Role Required:** Organiser

**Request Body:** None

**Expected Response:** `200 OK` – Event successfully deleted.


# 4. Category Endpoints
## 4.1 Get Event Categories

**HTTP Method:** GET

**Route:** `/api/events/{eventId}/categories`

**Description:** Retrieves all categories available for a specific race event.

**Role Required:** None

**Request Body:** None

**Expected Response:** `200 OK` – Categories successfully returned.


## 4.2 Create Category

**HTTP Method:** POST

**Route:** `/api/events/{eventId}/categories`

**Description:** Creates a new category for a specific event.

**Role Required:** Organiser

**Request Body:**

* CategoryName
* MinAge
* MaxAge

**Expected Response:** `201 Created` – Category successfully created.


## 4.3 Update Category

**HTTP Method:** PUT

**Route:** `/api/categories/{id}`

**Description:** Updates an existing category.

**Role Required:** Organiser

**Request Body:**

* CategoryName
* MinAge
* MaxAge

**Expected Response:** `200 OK` – Category successfully updated.


## 4.4 Delete Category

**HTTP Method:** DELETE

**Route:** `/api/categories/{id}`

**Description:** Deletes an existing category.

**Role Required:** Organiser

**Request Body:** None

**Expected Response:** `200 OK` – Category successfully deleted.


# 5. Enrolment Endpoints
## 5.1 Create Enrolment

**HTTP Method:** POST

**Route:** `/api/enrolments`

**Description:** Allows a participant to enrol for a race event.

**Role Required:** Participant

**Request Body:**

* EventID
* CategoryID

**Expected Response:** `201 Created` – Enrolment successfully created.


## 5.2 Get My Enrolments

**HTTP Method:** GET

**Route:** `/api/enrolments/my-enrolments`

**Description:** Retrieves all enrolments belonging to the currently logged-in participant.

**Role Required:** Participant

**Request Body:** None

**Expected Response:** `200 OK` – Participant enrolments returned successfully.


## 5.3 Get Event Enrolments

**HTTP Method:** GET

**Route:** `/api/events/{eventId}/enrolments`

**Description:** Retrieves all participant enrolments for a specific event.

**Role Required:** Organiser

**Request Body:** None

**Expected Response:** `200 OK` – Event enrolments returned successfully.


## 5.4 Update Enrolment Status

**HTTP Method:** PUT

**Route:** `/api/enrolments/{id}/status`

**Description:** Updates the status of an enrolment.

**Role Required:** Organiser

**Request Body:**

* Status

**Expected Response:** `200 OK` – Enrolment status successfully updated.


## 5.5 Cancel Enrolment

**HTTP Method:** DELETE

**Route:** `/api/enrolments/{id}`

**Description:** Cancels an existing enrolment.

**Role Required:** Participant

**Request Body:** None

**Expected Response:** `200 OK` – Enrolment successfully cancelled.


# 6. Results Endpoints
## 6.1 Create Result

**HTTP Method:** POST

**Route:** `/api/results`

**Description:** Records a race result for a participant's enrolment.

**Role Required:** Organiser

**Request Body:**

* EnrolmentID
* FinishTime
* FinishingPosition
* ResultStatus

**Expected Response:** `201 Created` – Race result successfully recorded.


## 6.2 Get My Results

**HTTP Method:** GET

**Route:** `/api/results/my-results`

**Description:** Retrieves race results belonging to the currently logged-in participant.

**Role Required:** Participant

**Request Body:** None

**Expected Response:** `200 OK` – Participant results successfully returned.


## 6.3 Get Event Results

**HTTP Method:** GET

**Route:** `/api/events/{eventId}/results`

**Description:** Retrieves all race results for a specific event.

**Role Required:** Organiser

**Request Body:** None

**Expected Response:** `200 OK` – Event results successfully returned.


## 6.4 Update Result

**HTTP Method:** PUT

**Route:** `/api/results/{id}`

**Description:** Updates an existing race result.

**Role Required:** Organiser

**Request Body:**

* FinishTime
* FinishingPosition
* ResultStatus

**Expected Response:** `200 OK` – Race result successfully updated.


# 7. Event Weather Endpoints
## 7.1 Get Event Weather

**HTTP Method:** GET

**Route:** `/api/events/{eventId}/weather`

**Description:** Retrieves weather information associated with a specific race event.

**Role Required:** None

**Request Body:** None

**Expected Response:** `200 OK` – Weather information successfully returned.

## 7.2 Add Event Weather

**HTTP Method:** POST

**Route:** `/api/events/{eventId}/weather`

**Description:** Adds weather information for a specific event.

**Role Required:** Organiser

**Request Body:**

* WeatherDate
* Temperature
* WeatherCondition
* Humidity

**Expected Response:** `201 Created` – Weather information successfully added.


# 8. Summary

The RaceDay API Endpoint Plan supports the main functionality of the Comrades Marathon Event Management System. Participants can register, manage their profiles, view events, enrol for races and view their results. Organisers can create and manage events, categories, enrolments, results and weather information.

The planned endpoints are designed to work with the RaceDayDB database and remain consistent with the RaceDay Entity Relationship Diagram and SQL database structure.
