# ColumbiaAi

A full-stack AI chat assistant application with React frontend and .NET 9 Web API backend, powered by Azure services.

## Features

- **Azure OpenAI (GPT-4o)** - AI-powered chat responses
- **Azure Cosmos DB** - Store users, sessions, and chat history
- **Azure Blob Storage** - File uploads and management
- **Azure Cognitive Search** - Document retrieval capabilities
- **JWT Authentication** - Secure user authentication
- **Persistent Sessions** - Chat sessions that persist across visits
- **Context Memory** - Pick up conversations where you left off
- **Toggleable User Profiling** - Optional user profiling for personalized responses
- **Application Insights** - Monitoring and telemetry

## Architecture

```
ColumbiaAi/
├── Backend/                    # .NET 9 Web API
│   ├── Configuration/         # Configuration classes
│   ├── Controllers/           # API Controllers
│   ├── Models/                # Data models and DTOs
│   ├── Services/              # Business logic services
│   └── appsettings.json       # Configuration file
└── frontend/                   # React + TypeScript
    ├── src/
    │   ├── components/        # React components
    │   ├── contexts/          # React contexts (Auth)
    │   ├── pages/             # Page components
    │   ├── services/          # API service layer
    │   └── types/             # TypeScript types
    └── .env                   # Environment variables
```

## Prerequisites

- .NET 9 SDK
- Node.js 18+ and npm
- Azure subscription with the following services:
  - Azure OpenAI Service
  - Azure Cosmos DB
  - Azure Blob Storage
  - Azure Cognitive Search
  - Application Insights (optional)

## Setup Instructions

### Backend Setup

1. Navigate to the Backend directory:
```bash
cd Backend
```

2. Update `appsettings.json` with your Azure service credentials:
```json
{
  "AzureOpenAI": {
    "Endpoint": "https://your-openai-resource.openai.azure.com/",
    "ApiKey": "your-azure-openai-api-key",
    "DeploymentName": "gpt-4o"
  },
  "CosmosDb": {
    "Endpoint": "https://your-cosmos-account.documents.azure.com:443/",
    "Key": "your-cosmos-db-key",
    "DatabaseName": "ColumbiaAiDb",
    "UsersContainer": "Users",
    "SessionsContainer": "Sessions",
    "ChatHistoryContainer": "ChatHistory"
  },
  "BlobStorage": {
    "ConnectionString": "your-blob-storage-connection-string",
    "ContainerName": "uploads"
  },
  "CognitiveSearch": {
    "Endpoint": "https://your-search-service.search.windows.net",
    "ApiKey": "your-search-api-key",
    "IndexName": "documents"
  },
  "JwtSettings": {
    "SecretKey": "your-secret-key-min-32-characters-long",
    "Issuer": "ColumbiaAi",
    "Audience": "ColumbiaAiUsers",
    "ExpirationMinutes": 1440
  },
  "ApplicationInsights": {
    "ConnectionString": "your-application-insights-connection-string"
  },
  "Features": {
    "EnableUserProfiling": true
  }
}
```

3. Create the Cosmos DB database and containers:
   - Database: `ColumbiaAiDb`
   - Containers:
     - `Users` (partition key: `/id`)
     - `Sessions` (partition key: `/userId`)
     - `ChatHistory` (partition key: `/sessionId`)

4. Restore dependencies and run:
```bash
dotnet restore
dotnet run
```

The API will be available at `https://localhost:5001` (or `http://localhost:5000`)

### Frontend Setup

1. Navigate to the frontend directory:
```bash
cd frontend
```

2. Install dependencies:
```bash
npm install
```

3. Update `.env` file with your backend URL:
```
REACT_APP_API_URL=http://localhost:5000/api
```

4. Run the development server:
```bash
npm start
```

The app will be available at `http://localhost:3000`

## API Endpoints

### Authentication
- `POST /api/auth/register` - Register a new user
- `POST /api/auth/login` - Login user

### Chat
- `POST /api/chat/message` - Send a message and get AI response
- `GET /api/chat/sessions` - Get all user sessions
- `GET /api/chat/sessions/{sessionId}/messages` - Get messages for a session
- `POST /api/chat/sessions/{sessionId}/continue` - Continue a previous session
- `POST /api/chat/search` - Search documents

### File Management
- `POST /api/file/upload` - Upload a file
- `DELETE /api/file/{fileName}` - Delete a file

### User Profile
- `GET /api/user/profile` - Get user profile
- `PUT /api/user/profile` - Update user profile

## Features Detail

### JWT Authentication
- Secure token-based authentication
- Tokens expire after 24 hours (configurable)
- Includes user claims for authorization

### Persistent Sessions
- All chat sessions are stored in Cosmos DB
- Users can view and continue previous conversations
- Session titles are auto-generated from first message

### Context Memory
- Last 10 messages are used as context for new responses
- Optional user profiling adds personalized context
- Sessions maintain conversation history

### User Profiling
- Toggle feature on/off in `appsettings.json`
- Store user preferences, interests, and context
- Personalize AI responses based on user profile

## Development

### Building for Production

Backend:
```bash
cd Backend
dotnet publish -c Release
```

Frontend:
```bash
cd frontend
npm run build
```

### Testing

Backend:
```bash
cd Backend
dotnet test
```

Frontend:
```bash
cd frontend
npm test
```

## Security Considerations

1. **Never commit secrets** - Keep `appsettings.json` and `.env` files out of version control
2. **Use strong JWT secret keys** - Minimum 32 characters
3. **Enable HTTPS** - Use SSL certificates in production
4. **Validate all inputs** - API includes validation middleware
5. **Secure Cosmos DB** - Use connection strings with least privilege

## Deployment

1. Deploy the backend to Azure App Service or Azure Container Apps
2. Deploy the frontend to Azure Static Web Apps or Azure Blob Storage with CDN
3. Update frontend `.env` with production API URL
4. Configure CORS in backend for production domains

## Troubleshooting

### Backend Issues
- Check Azure service credentials in `appsettings.json`
- Verify Cosmos DB containers exist
- Check Application Insights for errors

### Frontend Issues
- Verify API URL in `.env`
- Check browser console for errors
- Ensure backend is running and accessible

## License

MIT

## Contributing

Contributions are welcome! Please open an issue or submit a pull request.
