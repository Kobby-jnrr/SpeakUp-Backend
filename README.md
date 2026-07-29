# SpeakUp Backend Documentation

## What this backend does

This project is the backend for the SpeakUp application. It handles user accounts, reports, chat support, notifications, and email sending.

The backend receives requests from the frontend, checks the user, saves data to the database, and returns a response.

---

## Simple architecture

The project uses a simple web API structure.

### Main idea

- Controllers receive HTTP requests.
- Services contain the main business rules.
- DbContext talks to the PostgreSQL database.
- Models describe the data.
- DTOs help move data safely between layers.

### Request flow

1. The frontend sends a request to an API endpoint.
2. The controller receives the request.
3. The controller checks user identity and permissions.
4. The controller calls a service or database layer.
5. The data is saved or read.
6. The backend sends a response back to the frontend.

This is a clean and easy structure for a small to medium project.

---

## Main folders

### Controllers

These files handle API routes.

Examples:

- AuthController: login, registration, password reset, admin setup
- ReportController: create reports, update status, assign admins
- ChatConversationController: create and manage support chats
- ChatMessageController: send and read chat messages
- NotificationController: manage user notifications

### Services

These files contain the main business logic.

Examples:

- Auth logic is mostly in the controller, but services are used for token creation, email sending, audit logging, and notifications.
- ReportService: handles report-related helper actions
- ChatService: creates chat conversations from reports
- NotificationService: saves notifications for users
- EmailService: sends verification and password reset emails
- AuditService: saves important admin actions
- TokenService: creates JWT tokens for authentication

### Data

The database setup lives here.

- ApplicationDbContext connects the app to PostgreSQL.
- It defines the database tables and relationships between them.

### Models

These files represent the real data used by the application.

Examples include:

- User
- Report
- ChatConversation
- ChatMessage
- Notification
- AuditLog
- Resource
- HomePageContent

### DTOs

DTOs are simple data transfer objects.
They help the API accept and return only the fields it needs.

---

## Tools and technologies

### ASP.NET Core

This is the main web framework.
It is used because it is fast, modern, and works very well for building APIs in C#.

### Entity Framework Core

This is used to talk to the database with C# objects instead of writing raw SQL all the time.
It makes database work easier and cleaner.

### PostgreSQL

The database used by the app.
It is a strong choice for structured data such as users, reports, chat messages, and notifications.

### JWT authentication

The app uses JSON Web Tokens for login sessions.
This is a common and secure way to keep users signed in without storing sessions in the server memory.

### Swagger / OpenAPI

Swagger is enabled so developers can test the API endpoints easily in the browser.
This is helpful during development and debugging.

### BCrypt

Passwords are hashed with BCrypt.
This is a good choice because it is safe and widely used for password storage.

### Resend

Resend is used to send emails for verification and password reset.
It keeps the email system simple and reliable.

### CORS

CORS is enabled so the frontend can call the backend from a different domain.
This is important when the frontend and backend are hosted separately.

---

## Why this design was chosen

### Why use controllers and services?

This keeps the code organized.
Controllers handle API requests, while services handle the logic.
This makes the app easier to read and maintain.

### Why use Entity Framework Core?

It saves time.
The team can work with C# objects instead of writing a lot of SQL manually.
It also makes migrations easier when the database changes.

### Why use JWT instead of sessions?

JWT is simple for APIs.
It works well with modern frontend apps and mobile apps.
It also avoids storing session data on the server for every user.

### Why use PostgreSQL?

PostgreSQL is reliable and strong for relational data.
The app has many linked records such as users, reports, chat conversations, and notifications.
A relational database fits this project well.

### Why not use a microservice architecture?

A microservice setup is useful for very large systems.
This project is smaller and focused on one app, so a simple layered backend is easier to build and maintain.
It is faster to develop and easier for one team to manage.

### Why not add repositories everywhere?

The project uses the DbContext directly in many places.
This is fine for a small project because it keeps the code simple.
If the app grows larger, repositories or a cleaner domain layer can be added later.

### Why keep the design simple?

The main goal was to build a working system quickly.
A simple architecture is easier to understand for a student or small team.
It also makes it easier to ship features without too much setup.

---

## Main features of the backend

### User authentication

The backend supports:

- user registration
- email verification
- login
- password reset
- admin setup
- admin account creation

### Report system

Students can submit reports.
Admins can review, claim, reassign, and update the status of reports.

### Support chat

Users and admins can open conversations related to a report.
Messages can be sent and read.

### Notification system

Users receive notifications when important events happen, such as:

- report submitted
- report assigned
- chat assigned
- status updated
- new message received

### Audit logging

Important admin actions are logged.
This helps with accountability and debugging.

---

## Database design overview

The database stores several main entities:

- Users
- Reports
- ChatConversations
- ChatMessages
- Notifications
- AuditLogs
- Resources
- HomePageContent

The relationships are important.
For example:

- a user can create many reports
- a report can have one chat conversation
- a conversation can contain many messages
- a notification belongs to one user

This structure makes the app easy to extend later.

---

## Security notes

The backend uses several security measures:

- password hashing with BCrypt
- JWT authentication
- role-based access for admins
- validation of important actions like admin changes
- protected routes using authorization attributes

These steps help protect the app from common problems.

---

## Development notes

The app is configured to run with:

- ASP.NET Core
- PostgreSQL
- Swagger
- JWT settings
- email settings

The app also runs database migrations automatically on startup.
This helps keep the database in sync with the code.

---

## API endpoint overview

The backend exposes several main API groups.

### Auth endpoints

- POST /api/auth/register: create a new student account
- POST /api/auth/login: sign in and receive a JWT token
- POST /api/auth/verify-email: confirm a user email
- POST /api/auth/resend-verification-code: send a new verification code
- POST /api/auth/forgot-password: start password reset
- POST /api/auth/reset-password: finish password reset
- POST /api/auth/setup-superadmin: create the first super admin
- POST /api/auth/create-junior-admin: create a junior admin
- POST /api/auth/create-super-admin: create a super admin
- DELETE /api/auth/delete-user/{id}: delete a user account

### Report endpoints

- POST /api/report/create: create a full report
- POST /api/report/quick: create a quick report
- GET /api/report/all: get all reports for admins
- POST /api/report/claim/{reportId}: assign an admin to a report
- GET /api/report/assigned-to-me: get reports assigned to the current admin
- PUT /api/report/status/{reportId}: update report status
- PUT /api/report/reassign/{reportId}: reassign a report to another admin
- GET /api/report/my: get reports created by the current user

### Chat endpoints

- POST /api/chatconversation/create: create or open a support chat
- GET /api/chatconversation/my: get the current user’s conversations
- GET /api/chatconversation/admin/all: get all conversations for admins
- GET /api/chatconversation/admin/unassigned: get unassigned conversations
- GET /api/chatconversation/admin/assigned-to-me: get conversations assigned to the current admin
- GET /api/chatconversation/admin/closed: get closed conversations
- PUT /api/chatconversation/assign: assign an admin to a conversation
- GET /api/chatconversation/by-report/{reportId}: get the conversation tied to a report
- PUT /api/chatconversation/close/{conversationId}: close a conversation

### Message endpoints

- POST /api/chatmessage/send: send a chat message
- GET /api/chatmessage/{conversationId}: get messages in a conversation
- PUT /api/chatmessage/read/{messageId}: mark one message as read
- PUT /api/chatmessage/read/conversation/{conversationId}: mark all messages in a conversation as read

### Notification endpoints

- GET /api/notification/my: get notifications for the current user
- PUT /api/notification/read/{id}: mark a notification as read

### Other endpoints

- Home page content, resources, audit logs, and user management also have their own controllers and routes.

---

## Future improvement ideas

If the project grows, these improvements could help:

- add repositories for cleaner data access
- split services into smaller modules
- add unit and integration tests
- add more detailed error handling
- move business rules out of controllers further
- add caching for repeated reads
- add background jobs for long tasks

---

## Summary

This backend is a simple but strong ASP.NET Core API for the SpeakUp app.
It handles authentication, reporting, chat, notifications, and admin tasks.

The design was chosen to be practical, clear, and fast to build while still being secure and easy to extend.
