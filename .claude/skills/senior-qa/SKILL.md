---
name: senior-qa
description: Comprehensive QA and testing skill for quality assurance, test automation, and testing strategies for C#, .NET (xUnit, NUnit, MSTest), ASP.NET Core integration tests (WebApplicationFactory), Playwright for .NET, and for ReactJS, NextJS, NodeJS stacks. Includes test suite generation, coverage analysis, E2E testing setup, and quality metrics. Use when designing test strategies, writing test cases, implementing test automation, performing manual testing, or analyzing test coverage.
---

# Senior Qa

Complete toolkit for senior qa with modern tools and best practices.

## Язык

- При обращении на русском оформляй стратегии тестирования, кейсы и отчёты **на русском**; имена тестов в коде — по конвенции проекта.
- Явный выбор языка пользователем имеет приоритет.

## Quick Start

### Main Capabilities

This skill provides three core capabilities through automated scripts:

```bash
# Script 1: Test Suite Generator
python scripts/test_suite_generator.py [options]

# Script 2: Coverage Analyzer
python scripts/coverage_analyzer.py [options]

# Script 3: E2E Test Scaffolder
python scripts/e2e_test_scaffolder.py [options]
```

## Core Capabilities

### 1. Test Suite Generator

Automated tool for test suite generator tasks.

**Features:**
- Automated scaffolding
- Best practices built-in
- Configurable templates
- Quality checks

**Usage:**
```bash
python scripts/test_suite_generator.py <project-path> [options]
```

### 2. Coverage Analyzer

Comprehensive analysis and optimization tool.

**Features:**
- Deep analysis
- Performance metrics
- Recommendations
- Automated fixes

**Usage:**
```bash
python scripts/coverage_analyzer.py <target-path> [--verbose]
```

### 3. E2E Test Scaffolder

Advanced tooling for specialized tasks.

**Features:**
- Expert-level automation
- Custom configurations
- Integration ready
- Production-grade output

**Usage:**
```bash
python scripts/e2e_test_scaffolder.py [arguments] [options]
```

## Reference Documentation

### Testing Strategies

Comprehensive guide available in `references/testing_strategies.md`:

- Detailed patterns and practices
- Code examples
- Best practices
- Anti-patterns to avoid
- Real-world scenarios

### Test Automation Patterns

Complete workflow documentation in `references/test_automation_patterns.md`:

- Step-by-step processes
- Optimization strategies
- Tool integrations
- Performance tuning
- Troubleshooting guide

### Qa Best Practices

Technical reference guide in `references/qa_best_practices.md`:

- Technology stack details
- Configuration examples
- Integration patterns
- Security considerations
- Scalability guidelines

## Tech Stack

**Languages:** C#, F#, TypeScript, JavaScript, Python, Go, Swift, Kotlin
**Testing (.NET):** xUnit, NUnit, MSTest, FluentAssertions, Bogus, Moq, NSubstitute, Verify; `dotnet test`, coverlet; integration tests with `WebApplicationFactory`; Playwright .NET; SpecFlow / Reqnroll (BDD) when used
**Frontend:** React, Next.js, React Native, Flutter, Blazor
**Backend:** ASP.NET Core, Node.js, Express, GraphQL, REST APIs
**Database:** PostgreSQL, SQL Server, SQLite, Prisma, NeonDB, Supabase
**DevOps:** Docker, Kubernetes, Terraform, GitHub Actions, CircleCI, Azure DevOps
**Cloud:** AWS, GCP, Azure

## Development Workflow

### 1. Setup and Configuration

```bash
# Install dependencies
npm install
# or
pip install -r requirements.txt
# or (.NET)
dotnet restore

# Configure environment
cp .env.example .env
```

### 2. Run Quality Checks

```bash
# Use the analyzer script
python scripts/coverage_analyzer.py .

# Review recommendations
# Apply fixes
```

### 3. Implement Best Practices

Follow the patterns and practices documented in:
- `references/testing_strategies.md`
- `references/test_automation_patterns.md`
- `references/qa_best_practices.md`

## Best Practices Summary

### Code Quality
- Follow established patterns
- Write comprehensive tests
- Document decisions
- Review regularly

### Performance
- Measure before optimizing
- Use appropriate caching
- Optimize critical paths
- Monitor in production

### Security
- Validate all inputs
- Use parameterized queries
- Implement proper authentication
- Keep dependencies updated

### Maintainability
- Write clear code
- Use consistent naming
- Add helpful comments
- Keep it simple

## Common Commands

```bash
# Development
npm run dev
npm run build
npm run test
npm run lint
# .NET
dotnet build
dotnet test

# Analysis
python scripts/coverage_analyzer.py .
python scripts/e2e_test_scaffolder.py --analyze

# Deployment
docker build -t app:latest .
docker-compose up -d
kubectl apply -f k8s/
```

## Troubleshooting

### Common Issues

Check the comprehensive troubleshooting section in `references/qa_best_practices.md`.

### Getting Help

- Review reference documentation
- Check script output messages
- Consult tech stack documentation
- Review error logs

## Resources

- Pattern Reference: `references/testing_strategies.md`
- Workflow Guide: `references/test_automation_patterns.md`
- Technical Guide: `references/qa_best_practices.md`
- Tool Scripts: `scripts/` directory
