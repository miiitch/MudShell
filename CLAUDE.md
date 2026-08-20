# CLAUDE.md

Project-wide instructions for Claude Code sessions in this repository.

## Branch naming

Every unit of work goes on its own branch off `main`, named by intent:

| Kind of work | Prefix | Example |
| --- | --- | --- |
| New feature / component / enhancement | `features/` | `features/cosmic-night-theme` |
| Bug fix / regression | `fixes/` | `fixes/collapsed-sidebar-scope-attribute` |

Rules:

- Never create branches under `claude/` — that prefix is retired.
- Use the `features/` or `fixes/` prefix even when the work was started from a worktree whose directory name says otherwise; the directory name does not dictate the branch name.
- Keep the suffix short, lowercase, kebab-case, and descriptive of the change rather than of the file touched.
- One branch per PR. Merged branches are deleted locally and on `origin` once the PR lands.
