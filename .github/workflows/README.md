# GitHub Actions CI/CD Pipeline

## Overview

This directory contains the GitHub Actions workflow configuration for the Enterprise Ticket Management System. The CI/CD pipeline automates building, testing, and deploying both the Angular frontend and ASP.NET Core backend.

## Workflow File

- **File**: `ci-cd.yml`
- **Triggers**: 
  - Push to `main` or `develop` branches
  - Pull requests to `main` or `develop`
  - Manual workflow dispatch

## Pipeline Jobs

### 1. Backend Build & Test (`backend-build-test`)
- **Purpose**: Build and test ASP.NET Core API
- **Steps**:
  - Checkout code
  - Setup .NET 10.0
  - Restore NuGet packages
  - Build in Release configuration
  - Run unit tests
  - Publish API artifacts
- **Artifact**: `api-build`

### 2. Frontend Build & Test (`frontend-build-test`)
- **Purpose**: Build and test Angular application
- **Steps**:
  - Checkout code
  - Setup Node.js 18.x
  - Install npm dependencies
  - Run linting
  - Run unit tests
  - Build production bundle
  - Upload build artifacts
- **Artifact**: `angular-build`

### 3. Code Quality (`code-quality`)
- **Purpose**: Analyze code quality
- **Steps**:
  - Run ESLint on TypeScript/Angular code
  - Check TypeScript compilation
  - Verify code standards

### 4. Security Scan (`security-scan`)
- **Purpose**: Check for vulnerabilities
- **Steps**:
  - Run npm audit for frontend dependencies
  - Check .NET package vulnerabilities
  - Report security issues

### 5. Deploy to DEV (`deploy-dev`)
- **Trigger**: Automatic on push to `develop` branch
- **Environment**: Development
- **Steps**:
  - Download build artifacts
  - Deploy to DEV server
  - Run smoke tests

### 6. Deploy to STAGING (`deploy-staging`)
- **Trigger**: Automatic on push to `main` branch
- **Environment**: Staging (requires approval)
- **Steps**:
  - Download build artifacts
  - Deploy to STAGING server
  - Run integration tests

### 7. Deploy to PRODUCTION (`deploy-production`)
- **Trigger**: Manual workflow dispatch only
- **Environment**: Production (requires approval)
- **Steps**:
  - Download build artifacts
  - Deploy to PRODUCTION server
  - Verify deployment
  - Send notifications

### 8. Build Summary (`build-summary`)
- **Purpose**: Generate build report
- **Runs**: Always (even if previous jobs fail)
- **Output**: Summary in GitHub Actions UI

## Environment Variables

Configure these in your repository settings under **Settings → Secrets and variables → Actions**:

```yaml
DOTNET_VERSION: '10.0.x'          # .NET SDK version
NODE_VERSION: '18.x'               # Node.js version
ANGULAR_PROJECT_PATH: '.'          # Angular project root
API_PROJECT_PATH: './TicketManagementAPI'  # API project path
```

## GitHub Secrets (Required for Deployment)

Add these secrets in **Settings → Secrets and variables → Actions → New repository secret**:

### For Azure Deployment:
```
AZURE_CREDENTIALS          # Azure service principal credentials
AZURE_WEBAPP_NAME         # Azure App Service name
AZURE_PUBLISH_PROFILE     # Publish profile from Azure
```

### For AWS Deployment:
```
AWS_ACCESS_KEY_ID         # AWS access key
AWS_SECRET_ACCESS_KEY     # AWS secret key
AWS_REGION                # AWS region (e.g., us-east-1)
```

### For General Deployment:
```
SSH_PRIVATE_KEY           # SSH key for server access
SERVER_HOST               # Deployment server hostname
SERVER_USERNAME           # Server username
DEPLOY_PATH               # Path on server
```

### For Notifications:
```
SLACK_WEBHOOK_URL         # Slack webhook for notifications
SMTP_SERVER               # Email server for notifications
SMTP_USERNAME             # SMTP username
SMTP_PASSWORD             # SMTP password
```

## Environment Protection Rules

Configure environment protection rules in **Settings → Environments**:

### Development Environment
- No protection rules (auto-deploy)
- URL: Update with your DEV URL

### Staging Environment
- Required reviewers: Add team members who can approve
- Wait timer: 0 minutes
- URL: Update with your STAGING URL

### Production Environment
- Required reviewers: Add senior team members
- Wait timer: 5 minutes (optional)
- Restrict to specific branches: `main` only
- URL: Update with your PRODUCTION URL

## Workflow Execution

### Automatic Triggers

1. **On Push to `develop`**:
   - Builds backend and frontend
   - Runs tests and quality checks
   - Auto-deploys to DEV environment

2. **On Push to `main`**:
   - Builds backend and frontend
   - Runs tests and quality checks
   - Auto-deploys to STAGING (with approval)

3. **On Pull Request**:
   - Builds and tests only
   - No deployment
   - Provides feedback on PR

### Manual Trigger

1. Go to **Actions** tab in GitHub
2. Select **CI/CD Pipeline** workflow
3. Click **Run workflow**
4. Select branch
5. Click **Run workflow** button

This will trigger the full pipeline including production deployment (if approved).

## Customizing Deployment

### Azure App Service Deployment

Replace the deployment step with:

```yaml
- name: Deploy to Azure App Service
  uses: azure/webapps-deploy@v2
  with:
    app-name: ${{ secrets.AZURE_WEBAPP_NAME }}
    publish-profile: ${{ secrets.AZURE_PUBLISH_PROFILE }}
    package: ./api-deploy
```

### AWS Elastic Beanstalk Deployment

Replace the deployment step with:

```yaml
- name: Deploy to AWS Elastic Beanstalk
  uses: einaregilsson/beanstalk-deploy@v21
  with:
    aws_access_key: ${{ secrets.AWS_ACCESS_KEY_ID }}
    aws_secret_key: ${{ secrets.AWS_SECRET_ACCESS_KEY }}
    application_name: ticket-management-api
    environment_name: ticket-management-env
    version_label: ${{ github.sha }}
    region: ${{ secrets.AWS_REGION }}
    deployment_package: ./api-deploy
```

### Docker Deployment

Replace the deployment step with:

```yaml
- name: Build and push Docker image
  uses: docker/build-push-action@v5
  with:
    context: .
    push: true
    tags: |
      yourusername/ticket-api:latest
      yourusername/ticket-api:${{ github.sha }}
```

## Adding Tests

### Backend Tests

Add unit tests in your API project and they will automatically run:

```bash
cd TicketManagementAPI
dotnet test
```

### Frontend Tests

Update `package.json` to add test scripts:

```json
{
  "scripts": {
    "test": "ng test",
    "test:ci": "ng test --watch=false --browsers=ChromeHeadless --code-coverage",
    "lint": "ng lint"
  }
}
```

## Monitoring Build Status

### Badge in README

Add this badge to your `README.md`:

```markdown
[![CI/CD Pipeline](https://github.com/YOUR_USERNAME/YOUR_REPO/actions/workflows/ci-cd.yml/badge.svg)](https://github.com/YOUR_USERNAME/YOUR_REPO/actions/workflows/ci-cd.yml)
```

### Notifications

Configure GitHub notifications in **Settings → Notifications** to receive email alerts on build failures.

## Troubleshooting

### Build Fails on Dependencies

**Problem**: `npm ci` or `dotnet restore` fails

**Solution**: 
- Ensure `package-lock.json` is committed
- Check `.gitignore` isn't excluding necessary files
- Verify dependency versions are compatible

### Tests Fail in CI but Pass Locally

**Problem**: Tests pass on local machine but fail in CI

**Solution**:
- Check timezone differences
- Verify environment variables
- Use headless browser for Angular tests
- Check file path differences (Windows vs Linux)

### Deployment Fails

**Problem**: Deployment step errors out

**Solution**:
- Verify all secrets are configured correctly
- Check deployment target is accessible
- Ensure artifact paths are correct
- Review deployment logs in Actions tab

### Permissions Error

**Problem**: GitHub Actions can't access resources

**Solution**:
- Check repository permissions in Settings → Actions → General
- Verify workflow has necessary permissions
- Enable "Read and write permissions" for workflows

## Best Practices

1. **Branch Protection**: Enable branch protection rules for `main` and `develop`
2. **Required Checks**: Make CI checks required before merging PRs
3. **Secrets Management**: Never commit secrets to repository
4. **Artifact Retention**: Adjust retention days based on storage needs
5. **Caching**: Use caching for npm/NuGet to speed up builds
6. **Parallel Jobs**: Jobs run in parallel when possible for faster execution
7. **Status Checks**: Monitor build status regularly
8. **Rollback Plan**: Keep previous successful artifacts for rollback

## Cost Optimization

GitHub Actions is free for public repositories and includes 2,000 minutes/month for private repos on free plan.

**Tips to reduce minutes**:
- Use caching for dependencies
- Run tests only on changed files
- Skip unnecessary jobs based on file changes
- Use matrix builds efficiently
- Cancel redundant runs on new pushes

## Support

For issues with the CI/CD pipeline:
1. Check workflow run logs in GitHub Actions tab
2. Review this documentation
3. Check GitHub Actions documentation: https://docs.github.com/actions
4. Contact DevOps team

---

**Last Updated**: December 23, 2025  
**Maintained By**: Development Team
