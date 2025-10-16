# Azure AD Two-App Authentication Setup

## Overview
This API uses a **two-app Azure AD setup** with separate registrations for frontend and backend:

- **Frontend App (MyReatFrontend)**: `f6c2a5e9-3bd5-4223-ad2c-618846a668c5`
- **Backend App (MyDotnetBackend)**: `59aef810-e681-4b84-bc17-2561fe854c0e`  
- **Tenant ID**: `7b967b11-c0b9-402b-b483-d694f50dfb82`

## How It Works

### 1. Frontend Authentication Flow
1. User signs in via **MyReatFrontend** app
2. Frontend requests tokens for **MyDotnetBackend** API audience
3. Token has `aud` claim = `api://59aef810-e681-4b84-bc17-2561fe854c0e`
4. Token has `appid` claim = `f6c2a5e9-3bd5-4223-ad2c-618846a668c5` (frontend)

### 2. Backend Validation
1. Backend validates token issued to **frontend** for **backend audience**
2. Accepts multiple audience formats:
   - `59aef810-e681-4b84-bc17-2561fe854c0e` (backend client ID)
   - `api://59aef810-e681-4b84-bc17-2561fe854c0e` (backend API URI)
   - `f6c2a5e9-3bd5-4223-ad2c-618846a668c5` (frontend client ID)
   - `api://f6c2a5e9-3bd5-4223-ad2c-618846a668c5` (frontend API URI)

## Azure AD Configuration Required

### Frontend App Registration (MyReatFrontend)
- **Application ID**: `f6c2a5e9-3bd5-4223-ad2c-618846a668c5`
- **API Permissions**: 
  - MyDotnetBackend API (delegated)
  - Microsoft Graph User.Read (delegated)
- **Redirect URIs**: Your frontend URLs
- **Application ID URI**: `api://f6c2a5e9-3bd5-4223-ad2c-618846a668c5`

### Backend App Registration (MyDotnetBackend)  
- **Application ID**: `59aef810-e681-4b84-bc17-2561fe854c0e`
- **Client Secret**: Required for Microsoft Graph calls
- **API Permissions**:
  - Microsoft Graph TeamsActivity.Send (application)
  - Microsoft Graph User.Read (application)
- **Expose an API**: `api://59aef810-e681-4b84-bc17-2561fe854c0e`
- **Application ID URI**: `api://59aef810-e681-4b84-bc17-2561fe854c0e`

### Required Configuration Steps

1. **In MyReatFrontend app**:
   - Add API permission for MyDotnetBackend
   - Grant admin consent
   - Configure redirect URIs

2. **In MyDotnetBackend app**:
   - Create client secret
   - Expose API with scopes
   - Grant Microsoft Graph permissions
   - Grant admin consent

3. **Frontend Code**:
   ```javascript
   // Request token for backend API
   const tokenRequest = {
     scopes: ["api://59aef810-e681-4b84-bc17-2561fe854c0e/.default"],
     account: account
   };
   ```

4. **Backend Configuration** (appsettings.json):
   ```json
   {
     "AzureAd": {
       "TenantId": "7b967b11-c0b9-402b-b483-d694f50dfb82",
       "ClientId": "59aef810-e681-4b84-bc17-2561fe854c0e", 
       "ClientSecret": "your-secret",
       "Audience": "api://59aef810-e681-4b84-bc17-2561fe854c0e"
     },
     "FrontendApp": {
       "ClientId": "f6c2a5e9-3bd5-4223-ad2c-618846a668c5"
     }
   }
   ```

## Troubleshooting

### Common Issues

1. **401 Unauthorized - Audience Mismatch**
   - Frontend requesting wrong audience
   - Backend not accepting frontend tokens
   - **Fix**: Ensure frontend requests `api://59aef810-e681-4b84-bc17-2561fe854c0e`

2. **401 Unauthorized - Missing Permissions**
   - Frontend app doesn't have permission to backend API
   - **Fix**: Add API permission in Azure Portal and grant consent

3. **MSAL Client Credentials Error**
   - Missing or invalid client secret
   - **Fix**: Create valid client secret in MyDotnetBackend app

### Debug Endpoints

- `GET /debug/config` - Shows configuration status
- `POST /api/notification/debug-token` - Inspects token claims
- `GET /api/notification/test-logging` - Tests error logging
- `GET /api/notification/error-logs` - Views recent errors

### Expected Token Claims

A valid token should contain:
```json
{
  "aud": "api://59aef810-e681-4b84-bc17-2561fe854c0e", // Backend API
  "appid": "f6c2a5e9-3bd5-4223-ad2c-618846a668c5",     // Frontend app
  "iss": "https://login.microsoftonline.com/7b967b11-c0b9-402b-b483-d694f50dfb82/v2.0",
  "oid": "user-object-id",
  "preferred_username": "user@domain.com"
}
```

## Security Considerations

1. **Principle of Least Privilege**: Only grant necessary permissions
2. **Client Secret Protection**: Never expose backend client secret
3. **Token Validation**: Always validate audience and issuer
4. **HTTPS Only**: All communication must use HTTPS
5. **Token Lifetime**: Use appropriate token expiration times

## Testing the Setup

1. **Test authentication**: `POST /api/notification/debug-token`
2. **Test permissions**: `POST /api/notification/send-test`  
3. **Test Graph API**: Check console logs for Microsoft Graph calls
4. **Test notifications**: Verify Teams notifications are sent