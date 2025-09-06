# CORS Configuration Guide

## Overview

The Ticketing Platform API includes comprehensive Cross-Origin Resource Sharing (CORS) configuration to enable secure access from frontend applications while maintaining proper security boundaries.

## Configuration Methods

### 1. Environment Variables (Recommended for Production)

Set the `CORS_ALLOWED_ORIGINS` environment variable with comma-separated origins:

```bash
# Development
CORS_ALLOWED_ORIGINS=http://localhost:3000,http://localhost:3001,https://localhost:3000

# Production
CORS_ALLOWED_ORIGINS=https://yourdomain.com,https://www.yourdomain.com
```

### 2. AppSettings Configuration

Configure CORS in your `appsettings.json` files:

#### Development (`appsettings.Development.json`)
```json
{
  "Cors": {
    "AllowedOrigins": [
      "http://localhost:3000",
      "http://localhost:3001",
      "http://localhost:5173",
      "http://localhost:8080",
      "https://localhost:3000",
      "https://localhost:3001",
      "https://localhost:5173",
      "https://localhost:8080"
    ],
    "AllowedMethods": ["GET", "POST", "PUT", "DELETE", "OPTIONS", "PATCH"],
    "AllowedHeaders": ["*"],
    "AllowCredentials": true,
    "MaxAge": 3600
  }
}
```

#### Production (`appsettings.Production.json`)
```json
{
  "Cors": {
    "AllowedOrigins": [
      "https://ticketingplatform-frontend.onrender.com",
      "https://your-production-frontend.com"
    ],
    "AllowedMethods": ["GET", "POST", "PUT", "DELETE", "OPTIONS"],
    "AllowedHeaders": ["Content-Type", "Authorization", "X-Requested-With"],
    "AllowCredentials": true,
    "MaxAge": 86400
  }
}
```

## Security Features

### Environment-Based Configuration
- **Development**: Allows common localhost ports with relaxed settings
- **Production**: Strict origin validation, no localhost access allowed
- **Automatic validation**: Prevents localhost origins in production environment

### Configurable Security
- **AllowCredentials**: Controls whether cookies and authorization headers are allowed
- **MaxAge**: Controls preflight request caching duration
- **Specific Methods/Headers**: Restrictive allowlists for production security

## Usage in Frontend Applications

### React/Next.js Example
```javascript
// API call from localhost:3000 (development)
const response = await fetch('https://ticketingplatform-m9on.onrender.com/api/event', {
  method: 'GET',
  headers: {
    'Content-Type': 'application/json',
    'Authorization': `Bearer ${token}`
  },
  credentials: 'include' // Important for CORS with credentials
});
```

### Angular Example
```typescript
// HTTP client configuration
this.http.get('https://ticketingplatform-m9on.onrender.com/api/event', {
  headers: new HttpHeaders({
    'Content-Type': 'application/json',
    'Authorization': `Bearer ${token}`
  }),
  withCredentials: true
});
```

## Environment Variable Priority

The CORS configuration follows this priority order:
1. `CORS_ALLOWED_ORIGINS` environment variable (highest priority)
2. `appsettings.{Environment}.json` configuration
3. Default environment-specific configuration (fallback)

## Troubleshooting

### Common Issues

1. **"Access-Control-Allow-Origin" header is missing**
   - Ensure your frontend origin is included in `AllowedOrigins`
   - Check that CORS middleware is registered before authentication middleware

2. **Preflight request failures**
   - Verify that `OPTIONS` method is included in `AllowedMethods`
   - Check that required headers are in `AllowedHeaders`

3. **Credentials not being sent**
   - Ensure `AllowCredentials` is set to `true`
   - Frontend must set `credentials: 'include'` or `withCredentials: true`

### Debug Logging

The application logs CORS configuration on startup:
```
=== CORS CONFIGURATION (Development) ===
Allowed Origins: http://localhost:3000, http://localhost:3001
Allowed Methods: GET, POST, PUT, DELETE, OPTIONS, PATCH
Allowed Headers: *
Allow Credentials: True
Max Age: 3600 seconds
=== END CORS CONFIGURATION ===
```

## Deployment Examples

### Render.com
```bash
# Environment variable in Render dashboard
CORS_ALLOWED_ORIGINS=https://yourapp.vercel.app,https://yourapp.netlify.app
```

### Vercel/Netlify Frontend
```bash
# In your frontend environment variables
REACT_APP_API_URL=https://ticketingplatform-m9on.onrender.com
```

### Docker
```dockerfile
ENV CORS_ALLOWED_ORIGINS=https://yourdomain.com,https://www.yourdomain.com
```

## Security Best Practices

1. **Never use wildcards in production**: Avoid `"*"` in `AllowedOrigins` for production
2. **Specify exact origins**: Use complete URLs including protocol and port
3. **Minimal permissions**: Only allow required methods and headers
4. **Environment separation**: Different configurations for dev/staging/production
5. **Regular auditing**: Review and update allowed origins periodically

## Testing CORS Configuration

### Browser Developer Tools
1. Open Network tab in browser developer tools
2. Make a request to the API from your frontend
3. Check for CORS-related errors in console
4. Verify `Access-Control-Allow-*` headers in response

### curl Testing
```bash
# Preflight request test
curl -X OPTIONS https://ticketingplatform-m9on.onrender.com/api/event \
  -H "Origin: http://localhost:3000" \
  -H "Access-Control-Request-Method: GET" \
  -H "Access-Control-Request-Headers: Authorization" \
  -v

# Actual request test
curl https://ticketingplatform-m9on.onrender.com/api/event \
  -H "Origin: http://localhost:3000" \
  -v
```
