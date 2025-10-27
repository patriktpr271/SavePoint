# CI/CD Workflows - SavePoint

## 🔄 Workflow Overview

This project uses GitHub Actions for Continuous Integration and Continuous Deployment.

### Workflows

1. **`ci.yml`** - Continuous Integration (Build & Test)
2. **`cd-azure.yml`** - Continuous Deployment to Azure Production

---

## 📋 CI Workflow (`ci.yml`)

**Triggers:**
- Push to: `main`, `develop`, `feature/**` branches
- Pull requests to: `main`, `develop`
- Manual dispatch

### Jobs

#### 1. Build Backend
- ✅ Setup .NET 8.0
- ✅ Restore dependencies
- ✅ Build solution
- ✅ **Run ALL backend tests** (Unit + Integration)

#### 2. Build Frontend
- ✅ Setup Node.js 20
- ✅ Install dependencies
- ✅ Run ESLint
- ✅ **Run component tests** (Vitest)
- ✅ **Generate test coverage**
- ✅ Upload coverage reports
- ✅ Build production bundle
- ✅ Upload build artifact

#### 3. E2E Tests (Optional)
- 🔄 Only runs on pull requests or manual dispatch
- ✅ Starts backend server
- ✅ Installs Playwright
- ✅ **Runs E2E tests**
- ✅ Uploads test reports
- ℹ️ Continues on error (won't block PR)

---

## 🚀 CD Workflow (`cd-azure.yml`)

**Triggers:**
- Push to: `test` branch
- Manual dispatch

### Jobs

#### 1. Build Application
- ✅ Setup .NET 8.0 + Node.js 20
- ✅ Clean build cache
- ✅ Install frontend dependencies
- ✅ **Run frontend tests**
- ✅ Upload frontend test results
- ✅ Restore .NET dependencies
- ✅ Build .NET solution
- ✅ **Run backend tests**
- ✅ Upload backend test results
- ✅ Publish application
- ✅ Verify published files
- ✅ Upload deployment artifact

#### 2. Deploy to Production
- ✅ Download artifact
- ✅ Deploy to Azure Web App
- ✅ Display deployment summary

---

## 🧪 Test Execution in CI/CD

### Backend Tests (C# / .NET)

**CI Workflow:**
```bash
dotnet test SavePointBackend/SavePointBackend.sln \
  --configuration Release \
  --no-build \
  --verbosity normal
```

**CD Workflow:**
```bash
dotnet test SavePointBackend/SavePointBackend.sln \
  --configuration Release \
  --no-build \
  --verbosity normal \
  --logger "trx;LogFileName=test-results.trx"
```

**What's Tested:**
- ✅ SavePoint.Tests (Unit tests)
- ✅ SavePoint.BusinessLogic.Tests (Business logic)
- ✅ SavePoint.DAL.Tests (Data access)
- ✅ SavePoint.IntegrationTests (API endpoints)

**Artifacts:**
- `backend-test-results` (TRX format, 7 days retention)

---

### Frontend Tests (React / TypeScript)

**CI Workflow:**
```bash
# Component tests
npm test -- --run

# With coverage
npm run test:coverage -- --run
```

**CD Workflow:**
```bash
npm test -- --run
```

**What's Tested:**
- ✅ Component tests (src/test/components/)
- ✅ Service tests (src/test/services/)
- ✅ Test coverage reporting

**Artifacts:**
- `frontend-coverage` (Coverage report, 7 days retention)
- `frontend-test-results` (Coverage results, 7 days retention)

---

### E2E Tests (Playwright)

**CI Workflow (Optional):**
```bash
# Only runs on PRs or manual dispatch
npx playwright install chromium
npm run test:e2e
```

**What's Tested:**
- ✅ Homepage flows (e2e/homepage.spec.ts)
- ✅ Authentication (e2e/authentication.spec.ts)
- ✅ Game browsing (e2e/game-browsing.spec.ts)

**Artifacts:**
- `e2e-test-results` (Playwright HTML report, 7 days retention)

---

## 📊 Test Results & Artifacts

### Viewing Test Results

1. **Go to GitHub Actions tab**
2. **Click on workflow run**
3. **Scroll to "Artifacts" section**
4. **Download:**
   - `backend-test-results` - Backend test results (TRX)
   - `frontend-coverage` - Frontend coverage report (HTML)
   - `frontend-test-results` - Frontend test results
   - `e2e-test-results` - E2E test report (HTML)

### Coverage Reports

Frontend coverage reports include:
- Line coverage
- Branch coverage
- Function coverage
- Statement coverage

View by opening `coverage/index.html` from the artifact.

---

## 🎯 Pipeline Flow

### CI Pipeline (Feature Development)

```
Push to feature branch
  ↓
Build Backend
  ├─ Restore & Build
  └─ Run Unit + Integration Tests ✅
  
Build Frontend
  ├─ Install Dependencies
  ├─ Run Linter
  ├─ Run Component Tests ✅
  ├─ Generate Coverage ✅
  └─ Build Production Bundle

E2E Tests (Optional on PR)
  ├─ Start Backend
  ├─ Install Playwright
  └─ Run E2E Tests ✅
```

### CD Pipeline (Deployment)

```
Push to test branch
  ↓
Build Application
  ├─ Install Frontend Dependencies
  ├─ Run Frontend Tests ✅
  ├─ Build .NET Solution
  ├─ Run Backend Tests ✅
  ├─ Publish Application
  └─ Upload Artifact
  ↓
Deploy to Production
  ├─ Download Artifact
  └─ Deploy to Azure ☁️
```

---

## 🔧 Configuration

### Environment Variables

**CI Workflow:**
- None required

**CD Workflow:**
- `DOTNET_VERSION`: '8.0.x'
- `NODE_VERSION`: '20'
- `AZURE_WEBAPP_NAME`: 'SavePoint'
- `AZURE_WEBAPP_PACKAGE_PATH`: './publish'

### Secrets Required

- `AZURE_WEBAPP_PUBLISH_PROFILE` - Azure Web App publish profile

---

## ✅ Quality Gates

### Required Checks (Blocking)

- ✅ Backend build succeeds
- ✅ Backend tests pass
- ✅ Frontend build succeeds
- ✅ Frontend tests pass

### Optional Checks (Non-blocking)

- ℹ️ ESLint warnings
- ℹ️ E2E tests (only on PRs)
- ℹ️ Test coverage generation

---

## 🚨 Failure Handling

### If Tests Fail

**CI Workflow:**
- ❌ Workflow fails
- ❌ PR cannot be merged (if branch protection enabled)
- 📧 Notification sent to commit author

**CD Workflow:**
- ❌ Workflow fails
- ❌ Deployment does not proceed
- 💾 Test results uploaded for debugging

### Troubleshooting

1. **Check test results artifact**
2. **Review workflow logs**
3. **Run tests locally:**
   ```bash
   # Backend
   dotnet test
   
   # Frontend
   npm test -- --run
   ```

---

## 📈 Best Practices

### Before Committing

```bash
# Run all tests locally
cd SavePointBackend
dotnet test

cd ../SavePointFrontend
npm test -- --run
npm run lint
npm run build
```

### Before Creating PR

1. ✅ All tests pass locally
2. ✅ No linting errors
3. ✅ Build succeeds
4. ✅ Code coverage maintained

### Before Merging

1. ✅ CI workflow passes
2. ✅ Code reviewed
3. ✅ Tests added for new features
4. ✅ Documentation updated

---

## 🔄 Future Enhancements

### Potential Additions

- [ ] Code coverage thresholds
- [ ] SonarQube integration
- [ ] Performance testing
- [ ] Visual regression testing
- [ ] Automated changelog generation
- [ ] Semantic versioning
- [ ] Multiple environment deployments (dev, staging, prod)

---

## 📚 Related Documentation

- [Complete Testing Overview](../../TESTING-COMPLETE-OVERVIEW.md)
- [Frontend Testing Guide](../../SavePointFrontend/TESTING.md)
- [Frontend Quick Start](../../SavePointFrontend/QUICKSTART-TESTING.md)
- [Integration Tests README](../../SavePointBackend/SavePoint.IntegrationTests/README.md)

---

## 💡 Tips

### Speed Up CI Runs

- Use `npm ci` instead of `npm install` (already configured)
- Cache dependencies (already configured)
- Run tests in parallel when possible

### Reduce False Failures

- Make tests deterministic
- Mock external dependencies
- Use proper test isolation
- Avoid timing-dependent tests

### Debug Workflow Issues

```bash
# Run workflow locally with act
act -j build-backend
act -j build-frontend

# Or use workflow dispatch for testing
# Go to Actions → Select workflow → Run workflow
```

---

## 🎉 Summary

Your CI/CD pipeline now includes:

- ✅ **Automated Testing** on every push and PR
- ✅ **Backend Tests** (Unit + Integration)
- ✅ **Frontend Tests** (Component + E2E)
- ✅ **Test Coverage Reports**
- ✅ **Quality Gates** before deployment
- ✅ **Automated Deployment** to Azure
- ✅ **Test Artifacts** for debugging

**Quality is now built into every step of your development process!** 🚀
