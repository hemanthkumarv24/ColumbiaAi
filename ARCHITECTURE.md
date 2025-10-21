# Architecture Overview

## System Architecture

```
┌─────────────────────────────────────────────────────────────────────┐
│                           User Browser                               │
│                     React + TypeScript Frontend                      │
│  ┌──────────────┐  ┌──────────────┐  ┌─────────────────────────┐  │
│  │ Login/       │  │ Chat         │  │ Session History         │  │
│  │ Register     │  │ Interface    │  │ & File Upload           │  │
│  └──────────────┘  └──────────────┘  └─────────────────────────┘  │
└──────────────────────────┬──────────────────────────────────────────┘
                           │ HTTP/HTTPS + JWT
                           │
┌──────────────────────────▼──────────────────────────────────────────┐
│                    .NET 9 Web API Backend                            │
│  ┌──────────────────────────────────────────────────────────────┐  │
│  │                      Controllers                              │  │
│  │  ┌──────────┐ ┌──────────┐ ┌──────────┐ ┌───────────────┐  │  │
│  │  │  Auth    │ │  Chat    │ │  File    │ │  User Profile │  │  │
│  │  └──────────┘ └──────────┘ └──────────┘ └───────────────┘  │  │
│  └──────────────────────────────────────────────────────────────┘  │
│  ┌──────────────────────────────────────────────────────────────┐  │
│  │                       Services                                │  │
│  │  ┌──────────┐ ┌──────────┐ ┌──────────┐ ┌──────────┐       │  │
│  │  │   JWT    │ │ Cosmos   │ │  Blob    │ │ Cognitive│       │  │
│  │  │ Service  │ │    DB    │ │ Storage  │ │  Search  │       │  │
│  │  └──────────┘ └──────────┘ └──────────┘ └──────────┘       │  │
│  └──────────────────────────────────────────────────────────────┘  │
└─────────────────────────┬───────────────────────────────────────────┘
                          │
        ┌─────────────────┼─────────────────┐
        │                 │                 │
┌───────▼──────┐  ┌──────▼──────┐  ┌──────▼──────┐
│ Azure OpenAI │  │   Cosmos    │  │    Blob     │
│   (GPT-4o)   │  │     DB      │  │   Storage   │
└──────────────┘  └─────────────┘  └─────────────┘
        │                 │                 │
        │         ┌───────▼──────┐  ┌──────▼──────────┐
        │         │  Cognitive   │  │  Application    │
        │         │    Search    │  │    Insights     │
        │         └──────────────┘  └─────────────────┘
        │
        └──────────────────────────────────────┐
                                               │
                                        Monitoring
```

## Data Flow

### 1. User Authentication Flow
```
User → Register/Login → Backend Auth Controller
                           ↓
                    Password Service (Hash)
                           ↓
                    Cosmos DB (Users)
                           ↓
                    JWT Service (Generate Token)
                           ↓
                    Return Token to Frontend
```

### 2. Chat Message Flow
```
User → Send Message → Chat Controller
                         ↓
                   Load Session Context
                         ↓
                   Cosmos DB (Sessions, ChatHistory)
                         ↓
                   Build Conversation Context
                         ↓
                   Azure OpenAI Service (GPT-4o)
                         ↓
                   Get AI Response
                         ↓
                   Save Message & Response
                         ↓
                   Cosmos DB (ChatHistory)
                         ↓
                   Return Response to Frontend
```

### 3. File Upload Flow
```
User → Upload File → File Controller
                         ↓
                   Blob Storage Service
                         ↓
                   Azure Blob Storage
                         ↓
                   Return File URL
                         ↓
                   Attach to Message (optional)
```

### 4. Document Search Flow
```
User → Search Query → Chat Controller
                         ↓
                   Cognitive Search Service
                         ↓
                   Azure Cognitive Search
                         ↓
                   Return Relevant Documents
                         ↓
                   Enhance AI Context
```

## Key Components

### Frontend (React)
- **AuthContext**: Manages user authentication state
- **API Service**: Handles all HTTP requests with JWT
- **Pages**: Login, Register, Chat
- **Components**: Message display, session list, file upload

### Backend (.NET 9)
- **Controllers**: REST API endpoints
- **Services**: Business logic and Azure service integrations
- **Models**: Data models and DTOs
- **Configuration**: Azure service configurations

### Azure Services
- **OpenAI**: AI-powered chat responses
- **Cosmos DB**: NoSQL database for users, sessions, and messages
- **Blob Storage**: File storage
- **Cognitive Search**: Document indexing and search
- **Application Insights**: Monitoring and telemetry

## Security Features

1. **JWT Authentication**: Token-based auth with expiration
2. **Password Hashing**: SHA-256 hashing for passwords
3. **CORS**: Configured for frontend access
4. **HTTPS**: Recommended for production
5. **Input Validation**: API validates all inputs

## Scalability Features

1. **Stateless API**: Can scale horizontally
2. **Cosmos DB**: Auto-scales with throughput
3. **Blob Storage**: Infinite scale for files
4. **Azure OpenAI**: Managed scaling
5. **Session Management**: Persistent across restarts

## Monitoring

- **Application Insights**: Tracks all requests, errors, and performance
- **Structured Logging**: Detailed logs for debugging
- **Telemetry**: Custom metrics and events
