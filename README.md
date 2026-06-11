# Nexus Backend 🚀

A production-ready **Project Management REST API** built with **ASP.NET Core (.NET 10)** — think Jira/Trello, but yours.

> 🔗 Frontend → [nexus-frontend](https://github.com/t9amw0rk-sys/nexus-frontend)

---

## ✨ Features

- 🔐 **JWT Authentication** with Refresh Tokens + **Google OAuth**
- 👥 **Project Management** — Create projects, invite members, assign roles
- ✅ **Task Tracking** — Full task lifecycle with priorities, statuses, and progress
- 🤝 **Task Collaboration** — Multiple collaborators per task
- 💬 **Comments** — Threaded comments on tasks
- 📎 **Attachments** — File attachments on tasks
- 🔔 **Notifications** — Real-time activity notifications
- 📊 **Activity Log** — Track all changes per task
- 🌱 **Database Seeder** — Auto-seeds initial data on startup
- 🛡️ **Global Error Handling** — Centralized exception middleware
- 🚀 **Auto Migrations** — Runs on startup automatically

---

## 🛠️ Tech Stack

| Layer | Technology |
|-------|-----------|
| Framework | ASP.NET Core (.NET 10) |
| Database | PostgreSQL |
| ORM | Entity Framework Core 9 |
| Identity | ASP.NET Core Identity |
| Auth | JWT + Refresh Tokens + Google OAuth |
| Mapping | AutoMapper |
| Hosting | Railway / Any PaaS |

---

## 📁 Project Structure

```
NexusBackend/
├── Controllers/
│   ├── AuthController.cs           # Login, Register, Google OAuth, Refresh Token
│   ├── ProjectsController.cs       # Project CRUD & member management
│   ├── TasksController.cs          # Task CRUD, assign, update status
│   ├── CommentsController.cs       # Task comments
│   ├── NotificationsController.cs  # User notifications
│   └── UsersController.cs          # User profile & search
├── Services/
│   ├── AuthService.cs              # Auth business logic
│   ├── ProjectService.cs           # Project business logic
│   ├── TaskService.cs              # Task business logic
│   ├── CommentService.cs           # Comment business logic
│   ├── NotificationService.cs      # Notification business logic
│   ├── UserService.cs              # User business logic
│   └── TokenService.cs             # JWT & refresh token generation
├── Models/
│   └── Models.cs                   # AppUser, Project, Task, Comment, Notification...
├── DTOs/                           # Request/Response objects per feature
├── Data/
│   ├── AppDbContext.cs             # EF Core DB context
│   └── DatabaseSeeder.cs          # Initial data seeder
├── Middleware/
│   └── ExceptionMiddleware.cs      # Global error handling
├── Helpers/
│   └── ApiResponse.cs             # Standardized API response wrapper
├── Migrations/                     # EF Core migrations
└── Program.cs                      # App configuration & startup
```

---

## 🚀 Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [PostgreSQL](https://www.postgresql.org/)
- Google OAuth credentials *(optional)*

### 1. Clone the repository

```bash
git clone https://github.com/t9amw0rk-sys/nexus-backend.git
cd nexus-backend/NexusBackend
```

### 2. Configure `appsettings.json`

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=nexus;Username=postgres;Password=yourpassword"
  },
  "JwtSettings": {
    "Secret": "your-super-secret-key-minimum-32-characters",
    "Issuer": "NexusApp",
    "Audience": "NexusUsers",
    "ExpiryMinutes": 60,
    "RefreshTokenExpiryDays": 7
  },
  "GoogleAuth": {
    "ClientId": "your-google-client-id",
    "ClientSecret": "your-google-client-secret"
  },
  "AllowedOrigins": ["http://localhost:5173"]
}
```

Or use environment variables for production:

```bash
export DATABASE_URL="postgresql://user:password@host:5432/nexus"
export JwtSettings__Secret="your-secret-key"
```

### 3. Run the API

```bash
dotnet run
```

Migrations and seeding run automatically on startup.

---

## 📡 API Endpoints

### Auth
| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| POST | `/api/auth/register` | Register new user | ❌ |
| POST | `/api/auth/login` | Login with email/password | ❌ |
| POST | `/api/auth/google` | Login with Google | ❌ |
| POST | `/api/auth/refresh` | Refresh access token | ❌ |
| POST | `/api/auth/logout` | Logout & revoke refresh token | ✅ |

### Projects
| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| GET | `/api/projects` | Get user's projects | ✅ |
| POST | `/api/projects` | Create new project | ✅ |
| GET | `/api/projects/{id}` | Get project details | ✅ |
| PUT | `/api/projects/{id}` | Update project | ✅ Owner |
| DELETE | `/api/projects/{id}` | Delete project | ✅ Owner |
| POST | `/api/projects/{id}/members` | Add member | ✅ Owner |
| DELETE | `/api/projects/{id}/members/{userId}` | Remove member | ✅ Owner |

### Tasks
| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| GET | `/api/projects/{id}/tasks` | Get project tasks | ✅ |
| POST | `/api/projects/{id}/tasks` | Create task | ✅ |
| PUT | `/api/tasks/{id}` | Update task | ✅ |
| DELETE | `/api/tasks/{id}` | Delete task | ✅ |
| PATCH | `/api/tasks/{id}/status` | Update task status | ✅ |
| POST | `/api/tasks/{id}/collaborators` | Add collaborator | ✅ |

### Comments & Notifications
| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| GET | `/api/tasks/{id}/comments` | Get task comments | ✅ |
| POST | `/api/tasks/{id}/comments` | Add comment | ✅ |
| DELETE | `/api/comments/{id}` | Delete comment | ✅ |
| GET | `/api/notifications` | Get user notifications | ✅ |
| PATCH | `/api/notifications/{id}/read` | Mark as read | ✅ |

---

## 🔒 Authentication

Uses **JWT Bearer Tokens** with **Refresh Token** rotation.

```
Authorization: Bearer <access-token>
```

Access tokens expire after 60 minutes. Use `/api/auth/refresh` with your refresh token to get a new one.

---

## 🌐 Deployment (Railway)

1. Push to GitHub
2. Create new project on [Railway](https://railway.app)
3. Add a PostgreSQL database
4. Set environment variables (`DATABASE_URL`, `JwtSettings__Secret`, etc.)
5. Deploy — migrations run automatically!

---

## 🤝 Contributing

Pull requests are welcome. For major changes, please open an issue first.

---

## 📄 License

This project is licensed under the MIT License.
