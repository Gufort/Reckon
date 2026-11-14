## Задание 8. Инструменты разработчика для языка Reckon

**Reckon** — декларативный язык вычислений на основе формул для электронных таблиц.
Версия **R1.0** поддерживает числа, строки, булевы значения, литералы ошибок,
ссылки на ячейки, именованные ячейки и формулы, диапазоны, листы, функции высшего порядка и лямбда-выражения.

### Цель

Реализовать **детектор циклов** (Cycle Detector CLI) — инструмент командной строки,
который анализирует рабочую книгу Reckon и находит циклические зависимости между ячейками.

### Инструмент: ReckonCycleDetector

Класс `ReckonCycleDetector` обходит AST, запускает `DependencyExtractor` и `CycleDetector`,
и формирует отчёт:

- **Нет циклов:** выводит "No cycles detected"
- **Циклы обнаружены:** для каждого цикла выводит путь, например:
  ```
  Error: cycle detected: A1 -> B1 -> A1
  ```

#### Пример запуска

```bash
reckon-check model.rkn
```

Где `model.rkn`:
```
A1 := B1
B1 := A1
```

Вывод:
```
A1 -> B1
Error: cycle detected: A1 -> B1 -> A1
```

### Структура проекта

- `Task/SimpleLex.lex` — GPLEX-спецификация лексера
- `Task/SimpleYacc.y` — GPPG-спецификация грамматики
- `Task/ScannerHelper.cs` — вспомогательный класс сканера
- `Task/ParserHelper.cs` — исключения и утилиты парсера
- `Task/ProgramTree.cs` — узлы AST
- `Task/Visitors/` — визиторы из задания 6
  - `Visitor.cs` — базовый класс
  - `AutoVisitor.cs` — автоматический обход
  - `PrettyPrintVisitor.cs` — pretty-print
  - `DependencyExtractor.cs` — сбор зависимостей
  - `CycleDetector.cs` — обнаружение циклов
  - `ReferenceChecker.cs` — проверка ссылок
  - `ConstantFolder.cs` — свёртка констант
- `Task/Tool.cs` — `ReckonCycleDetector` и `ReckonTool` (CLI-инструмент)
- `Task/Task.cs` — демонстрационная программа
- `Tests/Tests.cs` — NUnit-тесты
- `TestRunner/Program.cs` — точка входа тестового раннера

### Сборка и тестирование

```bash
echo shmester | sudo -S bash templates/tools/build.sh tasks/Reckon/language-tools
echo shmester | sudo -S docker run --rm \
  -v "$(pwd)/tasks/Reckon/language-tools:/workspace" \
  -w /workspace mcr.microsoft.com/dotnet/sdk:8.0 \
  bash -c 'dotnet TestRunner/bin/Debug/net8.0/TestRunner.dll 2>&1'
```

### Генерация парсера

```bash
dotnet tool run GPLEX /unicode /out:Task\SimpleLex.cs Task\SimpleLex.lex
dotnet tool run GPPG /no-lines /gplex /out:Task\SimpleYacc.cs Task\SimpleYacc.y
```

### Критерии оценки (9 баллов)

| Фикстура | Баллы | Что проверяет |
|---|---|---|
| `TestCycleDetectorTool` | 2 | Работа инструмента: отсутствие циклов, простой цикл, сложный цикл, отсутствие ошибок |
| `TestMultipleDefinitions` | 2 | Именованные ячейки и формулы |
| `TestDependencyExtractor` | 2 | Извлечение зависимостей |
| `TestCycleDetector` | 1.5 | Обнаружение циклов |
| `TestReferenceChecker` | 1.5 | Проверка неопределённых ссылок |