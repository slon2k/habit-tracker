# Developer Habit Tracking API Design Document

## 1. Introduction

### 1.1 Purpose

A RESTful API service designed to help developers track their professional development habits, set goals, and monitor progress.
The system focuses on developer-specific activities such as code reviews, documentation writing, and testing practices.

### 1.2 Scope

- User management and authentication
- Habit tracking and management
- Goal setting and progress monitoring
- Entry logging and streak tracking
- Tag-based organization
- Batch operations support

## 2. API Design

### 2.1 Authentication

Authentication is handled through JWT tokens. All authenticated requests must include the JWT token in the `Authorization` header.

#### 2.1.1 Authentication Endpoints

```
/auth
  POST /register      - Create new account
  POST /login         - Login and receive tokens
  POST /refresh       - Refresh access token
```

#### 2.1.2 Authentication Flow

1. User registers or logs in
2. Server validates credentials and returns JWT tokens
3. Client includes access token in subsequent requests
4. Refresh token used to obtain new access token when expired

### 2.2 Core Resources and Endpoints

#### 2.2.1 Users

```
/users
  GET /userId - Get user by ID (admin only)
  GET /me - Get current user's profile
  PUT /me - Update current user's profile
```

#### 2.2.2 Habits

```
/habits
  GET    - List habits for user (returns summary: HabitDto, no tags)
  POST   - Create habit for user (returns summary: HabitDto)
  /{habitId}
    GET    - Get habit details (returns detail: HabitDetailsDto, includes tags[])
    PUT    - Update habit (returns summary: HabitDto)
    DELETE - Delete habit
```

Implicitly filtered by `userId` for all requests.

> **Response shape note:** `GET /habits` and write endpoints (`POST`, `PUT`, `PATCH`) return the summary `HabitDto` which omits tags for payload efficiency. `GET /habits/{habitId}` returns `HabitDetailsDto` which includes the full `tags` array scoped to the current user.

#### 2.2.3 Goals

```
/goals
  GET    - List goals for user
  POST   - Create a goal
  /{goalId}
    GET    - Get goal details
    PUT    - Update goal
    DELETE - Delete goal
```

Implicitly filtered by `userId` for all requests.

#### 2.2.4 Entries

```
/entries
  POST   - Create entry
  GET    - List entries (with filtering)
  /{entryId}
    GET    - Get entry details
    PUT    - Update entry
    DELETE - Delete entry

/entries/batch
  POST   - Create multiple entries
```

Implicitly filtered by `userId` for all requests.

#### 2.2.5 Tags

```
/tags
  GET    - List user's tags
  POST   - Create a new tag
  /{tagId}
    PUT    - Update tag
    DELETE - Delete tag

habits/{habitId}/tags
  PUT   - Upsert tags to habit
  DELETE /{tagId} - Remove tag from habit
```

Implicitly filtered by `userId` for all requests.

## 3. Data Models

### 3.1 Core Models

#### 3.1.1 User

```typescript
interface User {
  id: string;
  email: string;
  name: string;
  passwordHash: string;
  timezone: string;
  preferences: {
    reminderTime?: string;
    emailNotifications: boolean;
    pushNotifications: boolean;
  };
  createdAt: Date;
  updatedAt: Date;
}
```

#### 3.1.2 Habit

```typescript
interface Habit {
  id: string;
  userId: string;
  name: string;
  description?: string;
  type: "boolean" | "count" | "duration";
  frequency: "daily" | "weekly" | "monthly";
  targetValue: number;
  reminderTime?: string;
  activeStreak: number;
  longestStreak: number;
  createdAt: Date;
  updatedAt: Date;
  isArchived: boolean;
}
```

#### 3.1.3 Entry

```typescript
interface Entry {
  id: string;
  habitId: string;
  userId: string;
  value: number;
  notes?: string;
  timestamp: Date;
  createdAt: Date;
  updatedAt: Date;
}
```

#### 3.1.4 Goal

```typescript
interface Goal {
  id: string;
  userId: string;
  habitId: string;
  type: "streak" | "total_count" | "completion_rate";
  target: number;
  startDate: Date;
  endDate?: Date;
  status: "active" | "completed" | "failed";
  progress: number;
  createdAt: Date;
  updatedAt: Date;
}
```

#### 3.1.5 Tag

```typescript
interface Tag {
  id: string;
  userId: string; // Tags are scoped to a user
  name: string;
  description?: string;
  createdAt: Date;
  updatedAt: Date;
}

interface HabitTag {
  habitId: string;
  tagId: string;
  createdAt: Date;
}
```

### 3.2 Database Schema

```sql
-- Core tables
CREATE TABLE users (
  id VARCHAR PRIMARY KEY,
  email VARCHAR UNIQUE NOT NULL,
  name VARCHAR NOT NULL,
  password_hash VARCHAR NOT NULL,
  timezone VARCHAR NOT NULL DEFAULT 'UTC',
  preferences JSONB NOT NULL DEFAULT '{}',
  created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
  updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE habits (
  id VARCHAR PRIMARY KEY,
  user_id VARCHAR REFERENCES users(id) ON DELETE CASCADE,
  name VARCHAR NOT NULL,
  description TEXT,
  type VARCHAR NOT NULL CHECK (type IN ('boolean', 'count', 'duration')),
  frequency VARCHAR NOT NULL CHECK (frequency IN ('daily', 'weekly', 'monthly')),
  target_value INTEGER NOT NULL,
  reminder_time TIME,
  active_streak INTEGER NOT NULL DEFAULT 0,
  longest_streak INTEGER NOT NULL DEFAULT 0,
  is_archived BOOLEAN NOT NULL DEFAULT false,
  created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
  updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE entries (
  id VARCHAR PRIMARY KEY,
  habit_id VARCHAR REFERENCES habits(id) ON DELETE CASCADE,
  user_id VARCHAR REFERENCES users(id) ON DELETE CASCADE,
  value NUMERIC NOT NULL,
  notes TEXT,
  timestamp TIMESTAMP NOT NULL,
  created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
  updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE goals (
  id VARCHAR PRIMARY KEY,
  user_id VARCHAR REFERENCES users(id) ON DELETE CASCADE,
  habit_id VARCHAR REFERENCES habits(id) ON DELETE CASCADE,
  type VARCHAR NOT NULL CHECK (type IN ('streak', 'total_count', 'completion_rate')),
  target INTEGER NOT NULL,
  start_date TIMESTAMP NOT NULL,
  end_date TIMESTAMP,
  status VARCHAR NOT NULL DEFAULT 'active' CHECK (status IN ('active', 'completed', 'failed')),
  progress NUMERIC NOT NULL DEFAULT 0,
  created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
  updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE tags (
  id VARCHAR PRIMARY KEY,
  user_id VARCHAR REFERENCES users(id) ON DELETE CASCADE,
  name VARCHAR NOT NULL,
  description TEXT,
  created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
  updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
  UNIQUE(user_id, name)  -- Names must be unique per user
);

CREATE TABLE habit_tags (
  habit_id VARCHAR REFERENCES habits(id) ON DELETE CASCADE,
  tag_id VARCHAR REFERENCES tags(id) ON DELETE CASCADE,
  created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (habit_id, tag_id)
);

-- Indexes
CREATE INDEX idx_habit_tags_habit_id ON habit_tags(habit_id);
CREATE INDEX idx_habit_tags_tag_id ON habit_tags(tag_id);
CREATE INDEX idx_tags_user_id ON tags(user_id);
CREATE INDEX idx_tags_name ON tags(name, user_id);
CREATE INDEX idx_habits_user_id ON habits(user_id);
CREATE INDEX idx_entries_habit_id ON entries(habit_id);
CREATE INDEX idx_entries_user_id ON entries(user_id);
CREATE INDEX idx_entries_timestamp ON entries(timestamp);
CREATE INDEX idx_goals_habit_id ON goals(habit_id);
CREATE INDEX idx_goals_user_id ON goals(user_id);
```

## 4. Feature Implementation Details

### 4.1 Batch Operations

#### 4.1.1 Batch Entry Creation

```
POST /entries/batch
Request:
{
  "entries": [
    {
      "habitId": "123",
      "value": 1,
      "timestamp": "2025-01-13T10:00:00Z",
      "notes": "Morning code review"
    },
    {
      "habitId": "123",
      "value": 2,
      "timestamp": "2025-01-13T15:30:00Z",
      "notes": "Afternoon code reviews"
    }
  ]
}

Response:
{
  "entries": [
    {
      "id": "entry_1",
      "habitId": "123",
      "value": 1,
      "timestamp": "2025-01-13T10:00:00Z",
      "notes": "Morning code review",
      "createdAt": "2025-01-13T10:01:00Z"
    },
    {
      "id": "entry_2",
      "habitId": "123",
      "value": 2,
      "timestamp": "2025-01-13T15:30:00Z",
      "notes": "Afternoon code reviews",
      "createdAt": "2025-01-13T15:31:00Z"
    }
  ],
  "failed": []
}
```

Implementation considerations:

1. Transaction handling for all-or-nothing creation
2. Validation of all entries before processing
3. Partial success handling with failed entries list
4. Rate limiting based on batch size
5. Streak calculation updates for affected habits

### 4.2 Pagination

The following endpoints support cursor-based pagination:

#### 4.2.1 GET /users

```
Query Parameters:
- cursor: string (optional)
- limit: number (default: 20, max: 100)
- sortBy: string (default: "createdAt")
- order: "asc" | "desc" (default: "desc")

Response:
{
  "data": User[],
  "pagination": {
    "nextCursor": string | null,
    "hasMore": boolean
  }
}
```

#### 4.2.2 GET /users/{userId}/habits

```
Additional Query Parameters:
- tagId?: string
- isArchived?: boolean
```

#### 4.2.3 GET /entries

```
Additional Query Parameters:
- habitId?: string
- fromDate?: string (ISO date)
- toDate?: string (ISO date)
```

#### 4.2.4 GET /users/{userId}/goals

```
Additional Query Parameters:
- status?: "active" | "completed" | "failed"
- habitId?: string
```

### 4.3 Streak Calculation

Streaks are calculated based on habit frequency and completion status:

```typescript
function calculateStreak(habit: Habit, entries: Entry[]): number {
  // Sort entries by date, newest first
  const sortedEntries = entries.sort(
    (a, b) => b.timestamp.getTime() - a.timestamp.getTime()
  );

  if (habit.frequency === "daily") {
    let streak = 0;
    let currentDate = new Date();

    for (let i = 0; i < sortedEntries.length; i++) {
      const entryDate = new Date(sortedEntries[i].timestamp);

      // Break streak if a day is missed
      if (differenceInDays(currentDate, entryDate) > 1) {
        break;
      }

      // Check if target was met for the day
      const dailyTotal = sortedEntries
        .filter((e) => isSameDay(e.timestamp, entryDate))
        .reduce((sum, e) => sum + e.value, 0);

      if (dailyTotal >= habit.targetValue) {
        streak++;
        currentDate = entryDate;
      } else {
        break;
      }
    }

    return streak;
  }

  // Similar logic for weekly/monthly frequencies
  // ...
}
```

### 4.4 Progress Tracking

Progress is tracked differently based on habit type:

1. Boolean Habits:

   - Simple completed/not completed for the period
   - Progress = completion rate over time

2. Count-based Habits:

   - Sum of values towards target
   - Progress = (sum / target) \* 100

3. Duration-based Habits:
   - Accumulated time towards target
   - Progress = (minutes / target) \* 100

## 5. Error Handling

### 5.1 Standard Error Response

```json
{
  "error": {
    "code": "ERROR_CODE",
    "message": "Human readable message",
    "details": {}
  }
}
```

### 5.2 Common Error Scenarios

1. Authentication failures (401)
2. Authorization failures (403)
3. Resource not found (404)
4. Validation errors (400)
5. Rate limiting (429)
6. Server errors (500)

### 5.3 Validation Errors

```json
{
  "error": {
    "code": "VALIDATION_ERROR",
    "message": "Invalid input data",
    "details": {
      "fields": {
        "targetValue": "Must be greater than 0",
        "frequency": "Must be one of: daily, weekly, monthly"
      }
    }
  }
}
```

## 6. Security Considerations

### 6.1 Authentication

1. JWT token management

   - Short-lived access tokens (15-60 minutes)
   - Longer-lived refresh tokens (7-30 days)
   - Secure token storage guidelines for clients

2. Password security
   - Minimum length and complexity requirements
   - Secure password reset flow
   - Rate limiting on authentication endpoints

### 6.2 Authorization

1. Resource access control

   - Owner-only access by default
   - Role-based access for admin functions
   - Proper validation of user ID in paths

2. API security
   - CORS configuration
   - Rate limiting
   - Input validation and sanitization

### 6.3 Data Protection

1. Encryption at rest
2. Secure communication (HTTPS)
3. Audit logging
4. Data retention policies

## 7. Performance Optimization

### 7.1 Caching Strategy

1. Cache frequently accessed data:

   - User profiles
   - Current streak calculations
   - Tag lists
   - Active goals

2. Cache invalidation:
   - On entry creation/updates
   - On habit modifications
   - On goal updates

### 7.2 Database Optimization

1. Proper indexing (as shown in schema)
2. Efficient queries for streak calculation
3. Batch processing for multiple updates
4. Regular maintenance tasks

### 7.3 API Optimization

1. Pagination for list endpoints
2. Efficient batch operations
3. Compression for large responses
4. Request/response filtering
