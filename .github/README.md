# GitHub Actions CI/CD Workflows

This repository contains automated CI/CD workflows for various project types in the Incubator.

## 🚀 Active Workflows

### 1. React Landing Page CI (`react-landing-page.yml`)
**Triggers:** Changes to `LandingPage/**`
- **Technology:** React + Vite + Biome
- **Node Versions:** 18.x, 20.x
- **Features:**
  - Dependency installation and caching
  - Biome linting and formatting
  - Production and development builds
  - Security audits
  - Preview deployment preparation
  - Build artifact uploads

### 2. Angular Micro Learning Framework CI (`angular-micro-learning.yml`)
**Triggers:** Changes to `micro-learning-framework/**`
- **Technology:** Angular 20+ with SSR
- **Node Versions:** 18.x, 20.x
- **Features:**
  - Angular CLI tests with ChromeHeadless
  - Production builds
  - Angular linting
  - Prettier formatting checks
  - Security audits
  - Supabase schema validation
  - Migration consistency checks
  - Comprehensive build summaries

### 3. React Projects CI (`react-projects.yml`)
**Triggers:** Changes to `React/**`
- **Technology:** Generic React projects
- **Node Versions:** 18.x, 20.x
- **Features:**
  - Dynamic project detection
  - Multi-project support
  - Flexible script execution (lint, test, build)
  - Auto-discovery of package.json files
  - Helpful messaging when no projects found

### 4. Theme CI (`theme.yml`)
**Triggers:** Changes to `Theme/**`
- **Technology:** Tailwind CSS + HTML
- **Node Versions:** 18.x, 20.x
- **Features:**
  - Tailwind CSS compilation
  - CSS quality metrics
  - HTML validation
  - Server startup testing
  - Documentation completeness checks
  - File size analysis
  - Theme artifact uploads

## 📁 Project Structure Coverage

```
Incubator/
├── LandingPage/          → react-landing-page.yml
├── React/                → react-projects.yml
├── Theme/                → theme.yml
├── micro-learning-framework/ → angular-micro-learning.yml
└── .NET-Template/        → (Existing .NET workflows)
```

## 🔧 Workflow Features

### Security & Quality
- **Security Audits:** All workflows include `npm audit`
- **Dependency Checks:** Outdated package detection
- **Code Quality:** Linting and formatting validation
- **Build Verification:** Multi-environment testing

### Performance
- **Caching:** Intelligent npm cache management
- **Parallel Jobs:** Matrix builds for multiple Node versions
- **Conditional Execution:** Path-based triggering
- **Artifact Management:** 7-day retention for build outputs

### Monitoring & Reporting
- **Build Summaries:** Detailed status reports
- **Step Summaries:** GitHub Actions summary integration
- **Quality Reports:** CSS metrics and documentation status
- **Validation Results:** Schema and configuration checks

## 🎯 Usage Guidelines

### Branch Strategy
- **Main Branch:** Production-ready code
- **Develop Branch:** Integration testing
- **Feature Branches:** Trigger on PR to main/develop

### Path-Based Triggering
Workflows only run when relevant files change:
- Changes to workflow files trigger their own workflow
- Project-specific changes only affect related workflows
- Efficient resource usage and faster feedback

### Node Version Strategy
- **18.x:** LTS support
- **20.x:** Current stable
- **Matrix Testing:** Ensures compatibility across versions

## 🔄 Integration with Existing Workflows

These workflows complement existing .NET workflows:
- Independent execution paths
- No conflicts with existing CI/CD
- Shared artifact storage patterns
- Consistent security and quality standards

## 📋 Maintenance

### Adding New Projects
1. Create project in appropriate folder
2. Workflows auto-detect new projects
3. Update path triggers if needed
4. Test with small commits

### Customizing Workflows
1. Modify relevant `.yml` file
2. Test changes on feature branch
3. Monitor workflow runs
4. Update this documentation

## 🚦 Status Monitoring

Check workflow status at:
- **Actions Tab:** GitHub repository actions
- **Pull Requests:** Status checks on PRs
- **Commit Status:** Individual commit results
- **Artifacts:** Download build outputs

---

*Last Updated: Created during k6 template distribution and CI/CD setup*