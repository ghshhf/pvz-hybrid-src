#!/usr/bin/env bash
# 根目录 .cs 整理：按类名前缀分组，纯机械规则，不改文件名不改内容
set -uo pipefail
cd "$(dirname "$0")/.." || exit 1
DRY=${1:-dry}

# 保留在根目录的文件（编译关键 / 特殊命名），不移动
keep() {
  case "$1" in
    --y__InlineArray*.cs) return 0 ;;
    -PrivateImplementationDetails-.cs) return 0 ;;
    __PrivateImplementationDetails.cs) return 0 ;;
    Properties.cs|AssemblyInfo.cs) return 0 ;;
  esac
  return 1
}

# 从文件名取分类键
td_key() {  # TowerDefenseXxx -> Xxx 的首词
  local n=$1
  local rest=${n#TowerDefense}
  if [[ "$rest" =~ ^([A-Z][a-z0-9]*) ]]; then echo "${BASH_REMATCH[1]}"; else echo "_"; fi
}
misc_key() {
  local n=$1
  if [[ "$n" =~ ^I[A-Z][a-z0-9] ]]; then echo "Interfaces"; return; fi
  if [[ "$n" =~ ^([A-Z][a-z0-9]*|[A-Z]+|[a-z0-9_]+) ]]; then echo "${BASH_REMATCH[1]}"; else echo "_"; fi
}

> /tmp/move_plan.txt
declare -A TD_CNT MISC_CNT

# 第一遍：统计
for f in *.cs; do
  [[ -f "$f" ]] || continue
  keep "$f" && continue
  n=${f%.cs}
  if [[ "$n" == TowerDefense* ]]; then
    k=$(td_key "$n"); TD_CNT["$k"]=$(( ${TD_CNT["$k"]:-0} + 1 ))
  else
    k=$(misc_key "$n"); MISC_CNT["$k"]=$(( ${MISC_CNT["$k"]:-0} + 1 ))
  fi
done

# 第二遍：生成计划（TD 键 >=10 成目录；Misc 键 >=5 成目录）
for f in *.cs; do
  [[ -f "$f" ]] || continue
  keep "$f" && continue
  n=${f%.cs}
  if [[ "$n" == TowerDefense* ]]; then
    k=$(td_key "$n")
    if (( ${TD_CNT["$k"]:-0} >= 10 )); then dst="src/TowerDefense/$k"; else dst="src/TowerDefense/Other"; fi
  else
    k=$(misc_key "$n")
    if (( ${MISC_CNT["$k"]:-0} >= 10 )); then dst="src/Misc/$k"; else dst="src/Misc/Other"; fi
  fi
  echo "$f|$dst" >> /tmp/move_plan.txt
done

# 第三遍：Other 组内部按后缀再分一层（Component/Sprite/Definition/Config/State/Manager/Handler）
> /tmp/move_plan2.txt
while IFS='|' read -r f dst; do
  if [[ "$dst" == */Other ]]; then
    n=${f%.cs}
    case "$n" in
      *ComponentDefinition|*Component) sub=Component ;;
      *Sprite) sub=Sprite ;;
      *Definition) sub=Definition ;;
      *Config) sub=Config ;;
      *State) sub=State ;;
      *Manager|*System|*Registry) sub=Manager ;;
      *Handler) sub=Handler ;;
      *) sub=Rest ;;
    esac
    dst="$dst/$sub"
  fi
  echo "$f|$dst" >> /tmp/move_plan2.txt
done < /tmp/move_plan.txt
mv /tmp/move_plan2.txt /tmp/move_plan.txt

echo "=== 计划移动 $(wc -l < /tmp/move_plan.txt) 个文件 ==="
echo "=== 目标目录分布 ==="
cut -d'|' -f2 /tmp/move_plan.txt | sort | uniq -c | sort -rn
echo "=== 保留在根 ==="
for f in *.cs; do [[ -f "$f" ]] && keep "$f" && echo "  $f"; done

if [[ "$DRY" == "apply" ]]; then
  echo "=== 执行移动 ==="
  fail=0
  while IFS='|' read -r f dst; do
    mkdir -p "$dst"
    if [[ -e "$dst/$f" ]]; then echo "冲突: $dst/$f 已存在"; fail=$((fail+1)); continue; fi
    mv -- "./$f" "$dst/$f" || { echo "移动失败: $f"; fail=$((fail+1)); }
  done < /tmp/move_plan.txt
  echo "失败 $fail 个"
  echo "=== 根目录剩余条目 ==="
  ls -1 | head -60
fi
