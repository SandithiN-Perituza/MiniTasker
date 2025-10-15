# Microsoft Graph Teams Notifications Implementation

## Overview
This implementation enables sending Teams activity notifications to users when they are assigned tasks in the MiniTasker application.

## Prerequisites

### 1. Azure AD App Registration
Your Azure AD app registration must have the following permissions:
- **Delegated Permissions:**
  - `User.Read` - Basic user profile reading
  - `TeamsActivity.Send` - Send Teams activity notifications

### 2. Grant Admin Consent
In Azure Portal:
1. Go to Azure Active Directory ? App registrations
2. Find your app: `086fdd43-c0b7-4997-a181-dbf938026ae5`
3. Navigate to API permissions
4. Click "Grant admin consent for [Your Organization]"

### 3. Teams App Manifest (Optional for Activity Templates)
If you want custom activity templates, add this to your Teams app manifest:
```json
{
  "webApplicationInfo": {
    "id": "086fdd43-c0b7-4997-a181-dbf938026ae5",
    "resource": "api://086fdd43-c0b7-4997-a181-dbf938026ae5"
  },
  "activities": {
    "activityTypes": [
      {
        "type": "taskAssigned",
        "description": "Task assigned notification",
        "templateText": "{actorName} assigned you a task: {taskId}"
      }
    ]
  }
}
```

## How It Works

### 1. Service Architecture
```
TaskController ? NotificationService ? GraphTokenService ? Microsoft Graph API
                      ?
                  ErrorLogger (for logging)
```

### 2. Authentication Flow
1. **User Authentication**: User authenticates via Azure AD
2. **Token Acquisition**: `GraphTokenService` acquires delegated access token
3. **Graph API Call**: Token is used to call Microsoft Graph TeamsActivity API
4. **Notification Delivery**: Teams receives and displays the notification

### 3. Fallback Mechanism
- **Azure AD Available**: Sends actual Teams notifications
- **Azure AD Not Available**: Logs notification attempts (development mode)
- **Token Failure**: Gracefully falls back to logging without breaking task creation

## Configuration

### appsettings.json
```json
{
  "AzureAd": {
    "Instance": "https://login.microsoftonline.com/",
    "Domain": "perituza.com",
    "TenantId": "7b967b11-c0b9-402b-b483-d694f50dfb82",
    "ClientId": "086fdd43-c0b7-4997-a181-dbf938026ae5",
    "CallbackPath": "/signin-oidc",
    "Audience": "api://086fdd43-c0b7-4997-a181-dbf938026ae5",
    "Scopes": "https://graph.microsoft.com/TeamsActivity.Send https://graph.microsoft.com/User.Read"
  }
}
```

## Usage

### Sending Notifications
```csharp
await _notificationService.SendTaskCreatedNotificationAsync(
    userId: "azure-ad-user-id",           // Azure AD User ID (GUID)
    taskId: "123",                        // Task identifier
    actorName: "John Doe",               // Person who assigned the task
    taskUrl: "https://app.com/task/123"  // Optional: direct link to task
);
```

### Required User Data
For notifications to work, users in your database need:
- `AzureAdId` field populated with their Azure AD User ID
- The user must be present in your organization's Azure AD

## Troubleshooting

### Common Issues

1. **403 Forbidden**: Admin consent not granted for required permissions
2. **404 User Not Found**: User's AzureAdId is incorrect or user not in tenant
3. **401 Unauthorized**: Token acquisition failed or invalid configuration

### Debug Information
The service logs detailed information for troubleshooting:
- Token acquisition attempts
- Graph API responses
- Fallback scenarios

### Testing
1. **Health Check**: `GET /health` - Shows if Graph API is enabled
2. **Service Status**: `GET /` - Shows Azure AD configuration status
3. **Console Logs**: Check application logs for notification attempts

## Security Considerations

1. **Least Privilege**: Only request necessary Graph API permissions
2. **Token Security**: Tokens are acquired on-demand and not stored
3. **Error Handling**: Sensitive information is not exposed in error responses
4. **Fallback Safety**: Notification failures don't break core functionality

## Performance Notes

- Notifications are asynchronous and don't block task creation
- Failed notifications are logged but don't cause 500 errors
- Token acquisition is handled by Microsoft Identity Web with built-in caching