# Google OAuth Setup Guide

## Environment Variables Setup

The application now uses environment variables for Google OAuth credentials instead of hardcoded values. This is a security best practice.

### For Development

1. **Option 1: Using launchSettings.json (Recommended for Visual Studio)**
   - Open `Properties/launchSettings.json`
   - Replace `SET_YOUR_GOOGLE_CLIENT_ID_HERE` with your actual Google Client ID
   - Replace `SET_YOUR_GOOGLE_CLIENT_SECRET_HERE` with your actual Google Client Secret
   - The redirect URIs are already configured for localhost development

2. **Option 2: Using system environment variables**
   - Set the following environment variables on your system:
     ```
     GOOGLE_CLIENT_ID=your_actual_client_id
     GOOGLE_CLIENT_SECRET=your_actual_client_secret
     ```

3. **Option 3: Using .env file (requires additional setup)**
   - Copy `.env.example` to `.env`
   - Fill in your actual Google OAuth credentials
   - Install a NuGet package like `DotNetEnv` to load .env files

### For Production

Set the following environment variables in your hosting environment:
- `GOOGLE_CLIENT_ID`
- `GOOGLE_CLIENT_SECRET`
- `GOOGLE_REDIRECT_URI` (your production callback URL)
- `GOOGLE_CALLBACK_PATH` (optional, defaults to `/auth/google/middleware-callback`)

### Getting Google OAuth Credentials

1. Go to the [Google Cloud Console](https://console.cloud.google.com/)
2. Create a new project or select an existing one
3. Enable the Google+ API
4. Go to "Credentials" in the left sidebar
5. Click "Create Credentials" → "OAuth 2.0 Client IDs"
6. Configure the consent screen if prompted
7. Set the application type to "Web application"
8. Add authorized redirect URIs:
   - `http://localhost:5260/auth/google/callback` (for development)
   - Your production callback URL
9. Copy the Client ID and Client Secret

### Security Notes

- Never commit actual credentials to version control
- Use different credentials for development and production
- Regularly rotate your OAuth credentials
- The `.env.example` file shows the format but contains placeholder values only

### Configurable Callback URLs

The application now supports configurable callback URLs through environment variables:

- `GOOGLE_REDIRECT_URI`: The full redirect URI used during token exchange with Google
  - Development default: `http://localhost:5260/auth/google/callback`
  - HTTPS development: `https://localhost:7097/auth/google/callback`
  - Production: Set to your production callback URL (e.g., `https://yourdomain.com/auth/google/callback`)

- `GOOGLE_CALLBACK_PATH`: The relative path used by ASP.NET Core middleware (if enabled)
  - Default: `/auth/google/middleware-callback`
  - Only used if you uncomment the Google middleware in `Program.cs`

**Important**: Make sure to add your actual callback URLs to the "Authorized redirect URIs" in your Google Cloud Console OAuth client configuration.

### Troubleshooting

If you get errors about missing environment variables:
1. Ensure the environment variables are properly set
2. Restart your development server/IDE after setting environment variables
3. Check that the variable names match exactly (case-sensitive)
4. Verify that your callback URLs in Google Cloud Console match your environment variables
