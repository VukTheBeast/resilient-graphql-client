# Contributing to ResilientGraphQLClient

Thanks for your interest in contributing! This guide explains how to propose changes and what we expect in pull requests.

## Development setup
1. Clone the repo and open the solution in Visual Studio or VS Code.
2. Use .NET 8 SDK.
3. Build and run tests: `dotnet build` and `dotnet test`.

## Branching and workflow
- Create feature branches from `main` using the format: `feat/<short-name>` or `fix/<short-name>`.
- Keep PRs focused and small; separate unrelated changes.

## Commit messages
- Use Conventional Commits: `feat:`, `fix:`, `docs:`, `refactor:`, `test:`, `chore:`.
- Write clear, descriptive messages.

## Coding standards
- C# 12 / .NET 8.
- Prefer clarity over cleverness; name things descriptively.
- Add XML docs for public APIs when helpful.
- Handle errors thoughtfully; avoid swallowing exceptions.

## Testing
- Add or update tests for all changes affecting behavior.
- Ensure `dotnet test` passes locally.

## Pull requests
- Follow the PR template.
- Include a summary, motivation, and any breaking changes.
- Update README or docs if behavior or public API changes.
- Link related issues (e.g., `Fixes #123`).

## Code of Conduct
Be respectful and inclusive. Harassment or discrimination is not tolerated.

## Questions
Open a discussion or issue if anything is unclear.


