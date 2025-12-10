# Firebase Storage Setup Guide

This guide explains how to set up Firebase Storage for the Document Management application.

## Prerequisites

1. A Google Cloud Platform (GCP) project
2. Firebase project associated with your GCP project
3. Firebase Storage enabled

## Step 1: Create Firebase Project

1. Go to [Firebase Console](https://console.firebase.google.com/)
2. Click "Create a project" or select an existing one
3. Enable Firebase Storage in your project

## Step 2: Create Service Account

1. Go to [GCP Console](https://console.cloud.google.com/)
2. Navigate to "IAM & Admin" > "Service Accounts"
3. Click "Create Service Account"
4. Give it a name like "document-storage-service"
5. Grant the following roles:
   - `Storage Object Admin` (for full control over storage objects)
6. Create a key (JSON format) and download it

## Step 3: Configure Storage Bucket

1. In Firebase Console, go to Storage
2. Note your default bucket name (usually `your-project-id.appspot.com`)
3. Configure security rules as needed (default rules allow authenticated access)

## Step 4: Application Configuration

### For Local Development (.env.local)

```bash
# Firebase Configuration
Firebase__StorageBucket=your-project-id.appspot.com
Firebase__CredentialsPath=/path/to/service-account-key.json
Firebase__CredentialsJson=
```

### For Production (Jenkins/Docker)

Set these environment variables in your Jenkins pipeline:

```groovy
environment {
    Firebase__StorageBucket = 'your-project-id.appspot.com'
    Firebase__CredentialsJson = credentials('firebase-service-account-json')
    // Other variables...
}
```

## Step 5: Firebase Security Rules

Update your Firebase Storage rules in the Firebase Console:

```javascript
rules_version = '2';
service firebase.storage {
  match /b/{bucket}/o {
    // Allow authenticated users to read/write their own documents
    match /documents/{userId}/{allPaths=**} {
      allow read, write: if request.auth != null && request.auth.uid == userId;
    }

    // Allow admin users full access
    match /{allPaths=**} {
      allow read, write: if request.auth != null && request.auth.token.admin == true;
    }
  }
}
```

## Step 6: Testing

1. Start your application
2. Try uploading a document
3. Verify the file appears in Firebase Storage console
4. Test downloading the file

## Troubleshooting

- **Authentication errors**: Check your service account key and permissions
- **Bucket not found**: Verify the `StorageBucket` name matches your Firebase project
- **Permission denied**: Ensure your service account has the correct roles
- **CORS issues**: Configure CORS in Firebase Storage if needed

## Security Notes

- Never commit service account keys to version control
- Use Jenkins credentials or environment variables for production
- Regularly rotate service account keys
- Monitor Firebase Storage usage and costs
