#!/bin/bash
# Yorki 프로젝트 자동 커밋 (매일 17:00 cron)
# Usage: auto_commit.sh [--dry-run]
# 변경사항이 있을 때만 커밋. dry-run은 확인만 하고 종료.

export PATH="/opt/homebrew/bin:/usr/local/bin:/usr/bin:/bin"

PROJECT_DIR="/Users/seanka/Project file/Game/Yorki, the portraitist/2D"
cd "$PROJECT_DIR" || { echo "[$(date '+%Y-%m-%d %H:%M:%S')] 프로젝트 폴더 없음"; exit 1; }

# tracked 변경(unstaged + staged) + untracked 모두 확인
if git diff --quiet && git diff --cached --quiet && [ -z "$(git ls-files --others --exclude-standard)" ]; then
    echo "[$(date '+%Y-%m-%d %H:%M:%S')] 변경사항 없음, skip"
    exit 0
fi

if [ "$1" = "--dry-run" ]; then
    echo "[$(date '+%Y-%m-%d %H:%M:%S')] [DRY-RUN] 변경 감지, 실제 커밋은 안 함:"
    git status --short
    exit 0
fi

# 실제 커밋
git add -A
git commit -m "[자동] $(date '+%Y-%m-%d %H:%M') 17시 백업"
echo "[$(date '+%Y-%m-%d %H:%M:%S')] 자동 커밋 완료"
