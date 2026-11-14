# Задание 7: Генерация кода (интерпретатор) для языка Reckon

## Цель работы

Разработать интерпретатор для языка Reckon — декларативного языка электронных таблиц. Интерпретатор выполняет вычисления на основе AST, полученного после синтаксического анализа, и возвращает значения ячеек.

## Язык Reckon

Reckon — это язык для описания вычислений в электронных таблицах. Программа представляет собой набор определений ячеек:

```
A1 := 10
A2 := 20
A3 := A1 + A2
```

### Поддерживаемые конструкции

- **Числа**: целые и вещественные (`10`, `3.14`)
- **Строки**: в двойных кавычках (`"hello"`)
- **Булевы значения**: `true`, `false`
- **Ошибки**: `#DIV/0!`, `#VALUE!`, `#NAME?`, `#REF!`, `#NUM!`, `#NULL!`, `#N/A`
- **Ссылки на ячейки**: `A1`, `$A$1`, `A$1`, `$A1`
- **Именованные ячейки**: `total := SUM(A1:A10)`
- **Именованные формулы**: `tax(rate, amount) := amount * rate`
- **Диапазоны**: `A1:B10`
- **Ссылки на листы**: `Sheet1!A1`
- **Ссылки на столбцы**: `@A`
- **Ссылки на строки**: `@1`
- **Арифметические операции**: `+`, `-`, `*`, `/`, `^`
- **Операции сравнения**: `=`, `<>`, `<`, `>`, `<=`, `>=`
- **Конкатенация строк**: `&`
- **Логические операции**: `AND`, `OR`, `NOT`
- **Функции**: `SUM`, `AVG`, `MIN`, `MAX`, `COUNT`, `IF`, `ABS`, `SQRT`
- **Функции высшего порядка**: `MAP`, `FILTER`, `REDUCE`, `SORT`, `UNIQUE`
- **Лямбда-выражения**: `x -> x * 2`, `(acc, x) -> acc + x`
- **Комментарии**: строчные (`--`) и вложенные блочные (`{- -}`)

## Интерпретатор

Разработанный интерпретатор (GenCodeVisitor) выполняет:

- **Ленивое вычисление**: ячейка вычисляется только тогда, когда её значение запрашивается
- **Кеширование**: вычисленные значения сохраняются для повторного использования
- **Обнаружение циклов**: при обнаружении цикла в зависимостях возвращается `#CYCLE!`
- **Распространение ошибок**: ошибочные значения (`#DIV/0!` и др.) распространяются через выражения

## Состав проекта

- `Task/SimpleLex.lex` — GPLEX-спецификация лексического анализатора
- `Task/SimpleYacc.y` — GPPG-спецификация грамматики
- `Task/ProgramTree.cs` — классы узлов AST
- `Task/Visitors/Visitor.cs` — базовый класс Visitor
- `Task/Visitors/AutoVisitor.cs` — автоматический обход
- `Task/Visitors/PrettyPrintVisitor.cs` — pretty-print
- `Task/Visitors/DependencyExtractor.cs` — извлечение зависимостей
- `Task/Visitors/CycleDetector.cs` — обнаружение циклов
- `Task/Visitors/ReferenceChecker.cs` — проверка ссылок
- `Task/Visitors/ConstantFolder.cs` — свёртка констант
- `Task/Visitors/GenCodeVisitors/GenCodeVisitor.cs` — интерпретатор
- `Task/Visitors/GenCodeVisitors/GenCodeCreator.cs` — фабрика для запуска
- `Tests/Tests.cs` — NUnit-тесты

## Запуск

```bash
echo shmester | sudo -S bash templates/tools/build.sh tasks/Reckon/code-generator
echo shmester | sudo -S docker run --rm -v "$(pwd)/tasks/Reckon/code-generator:/workspace" -w /workspace mcr.microsoft.com/dotnet/sdk:8.0 bash -c 'dotnet TestRunner/bin/Debug/net8.0/TestRunner.dll 2>&1'
```

## Пример

Программа:
```
A1 := 10
A2 := 20
A3 := A1 + A2
doubled := MAP(A1:A3, x -> x * 2)
total := REDUCE(A1:A3, 0, (acc, x) -> acc + x)
```

Результат:
```
A1 = 10
A2 = 20
A3 = 30
doubled = [20, 40, 60]
total = 60
```