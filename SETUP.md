# Quick Setup Guide

## 1. Azure Resources Setup

### Create Azure OpenAI Service
1. Go to Azure Portal
2. Create an Azure OpenAI resource
3. Deploy a GPT-4o model
4. Note the endpoint and API key

### Create Cosmos DB Account
1. Create a Cosmos DB account (Core SQL API)
2. Create a database named `ColumbiaAiDb`
3. Create three containers:
   - `Users` with partition key `/id`
   - `Sessions` with partition key `/userId`
   - `ChatHistory` with partition key `/sessionId`
4. Note the endpoint and key

### Create Blob Storage Account
1. Create a Storage Account
2. Create a container named `uploads`
3. Note the connection string

### Create Cognitive Search Service
1. Create an Azure Cognitive Search service
2. Create an index named `documents`
3. Note the endpoint and API key

### Create Application Insights (Optional)
1. Create an Application Insights resource
2. Note the connection string

## 2. Backend Configuration

1. Copy the sample configuration:
```bash
cd Backend
cp appsettings.sample.json appsettings.json
```

2. Edit `appsettings.json` and fill in your Azure credentials:
   - AzureOpenAI: Endpoint, ApiKey, DeploymentName
   - CosmosDb: Endpoint, Key
   - BlobStorage: ConnectionString
   - CognitiveSearch: Endpoint, ApiKey
   - JwtSettings: SecretKey (generate a strong random key)
   - ApplicationInsights: ConnectionString

3. Run the backend:
```bash
dotnet restore
dotnet run
```

The API will start at `http://localhost:5000`

## 3. Frontend Configuration

1. Copy the sample environment file:
```bash
cd frontend
cp .env.sample .env
```

2. Edit `.env` if needed (default: `http://localhost:5000/api`)

3. Install dependencies and run:
```bash
npm install
npm start
```

The app will open at `http://localhost:3000`

## 4. First Use

1. Navigate to `http://localhost:3000`
2. Click "Register" to create a new account
3. Enter your name, email, and password
4. Start chatting!

## 5. Features to Try

- **New Conversation**: Click "+ New Chat" to start a fresh conversation
- **Chat History**: View and continue previous conversations from the sidebar
- **File Upload**: Click the 📎 button to upload files (stored in Azure Blob Storage)
- **Context Memory**: The AI remembers the last 10 messages in each session
- **User Profile**: Update your profile to enable personalized responses (if feature is enabled)

## 6. Production Deployment

### Backend to Azure App Service
```bash
cd Backend
dotnet publish -c Release
# Deploy the publish folder to Azure App Service
```

### Frontend to Azure Static Web Apps
```bash
cd frontend
npm run build
# Deploy the build folder to Azure Static Web Apps
```

Update the frontend `.env` with your production API URL before building.

## Troubleshooting

### Backend won't start
- Check that all Azure credentials are correct in `appsettings.json`
- Verify Cosmos DB containers exist with correct partition keys
- Check the console for detailed error messages

### Frontend can't connect to backend
- Verify the backend is running on the URL specified in `.env`
- Check browser console for CORS errors
- Ensure CORS is properly configured in the backend

### Authentication fails
- Verify the JWT secret key is at least 32 characters
- Check that the token hasn't expired
- Clear browser storage and try logging in again

### AI responses are slow or fail
- Check Azure OpenAI deployment is active and has quota
- Verify the deployment name matches the one in configuration
- Check Application Insights for detailed error logs

## Cost Optimization

- Use Azure OpenAI's usage limits and quotas
- Set appropriate TTL on Cosmos DB containers
- Use Azure Blob Storage lifecycle management
- Monitor usage with Application Insights

## Security Checklist

- [ ] Use strong JWT secret key (32+ characters)
- [ ] Never commit `appsettings.json` or `.env` to version control
- [ ] Enable HTTPS in production
- [ ] Use Azure Key Vault for secrets in production
- [ ] Set up proper CORS policies
- [ ] Implement rate limiting
- [ ] Enable Application Insights monitoring
- [ ] Regular security updates for dependencies
