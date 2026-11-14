## Задание 4. Синтаксический анализатор языка Reckon (GPPG)

**Reckon** — декларативный язык вычислений на основе ячеек (cells) и формул, предназначенный для работы с электронными таблицами. Версия **R0.3** включает определения ячеек, именованные формулы, арифметические и логические выражения, диапазоны, ссылки на листы, вызовы функций и функции высшего порядка (`MAP`, `FILTER`, `REDUCE`, `SORT`, `UNIQUE`, `LAMBDA`).

### Цель

Заменить ручной рекурсивный спуск на парсер, генерируемый GPPG из `.y`-файла. Лексический анализатор генерируется GPLEX.

### Лексемы (токены)

| Токен | Описание |
|---|---|
| `CELLREF` | Ссылка на ячейку (`A1`, `$A$1`, `A$1`, `$A1`) |
| `COLREF` | Ссылка на столбец (`@A`, `@AB`) |
| `ROWREF` | Ссылка на строку (`@1`, `@10`) |
| `NAME` | Имя ячейки или формулы (нижний регистр: `total`) |
| `FUNCNAME` | Имя встроенной функции (верхний регистр: `SUM`, `IF`) |
| `STRING` | Строковый литерал `"..."` |
| `ERROR` | Литерал ошибки (`#DIV/0!`, `#N/A`, `#NAME?`, `#REF!`, `#VALUE!`, `#NUM!`, `#NULL!`) |
| `NUMBER` | Число (целое или десятичное) |
| `BOOL` | Логическое значение (`true`, `false`) |
| `ASSIGN` (`:=`), `EQ` (`=`), `NEQ` (`<>`), `LT` (`<`), `GT` (`>`), `LEQ` (`<=`), `GEQ` (`>=`) | Операторы сравнения и присваивания |
| `PLUS` (`+`), `MINUS` (`-`), `MULT` (`*`), `DIV` (`/`), `POW` (`^`), `CONCAT` (`&`) | Арифметические операторы |
| `LPAREN`, `RPAREN`, `COMMA`, `COLON`, `BANG`, `ARROW` | Пунктуация |
| `MAP`, `FILTER`, `REDUCE`, `SORT`, `UNIQUE`, `LAMBDA` | Функции высшего порядка |

### Комментарии

- `--` до конца строки
- `{- ... -}` — вложенные блочные комментарии

### Грамматика (GPPG-формат, LALR(1))

```
%start program

program     : definitions

definitions : definition
            | definitions definition

definition  : CELLREF ASSIGN expr
            | NAME ASSIGN expr
            | NAME LPAREN params RPAREN ASSIGN expr

expr        : expr EQ additive
            | expr NEQ additive
            | expr LT additive
            | expr GT additive
            | expr LEQ additive
            | expr GEQ additive
            | additive

additive    : additive PLUS multiplicative
            | additive MINUS multiplicative
            | additive CONCAT multiplicative
            | multiplicative

multiplicative : multiplicative MULT power
               | multiplicative DIV power
               | power

power       : power POW primary
            | primary

primary     : MINUS primary
            | PLUS primary
            | atom

atom        : NUMBER | STRING | BOOL | ERROR
            | CELLREF | COLREF | ROWREF | NAME
            | call | range | sheetref
            | LPAREN expr RPAREN

call        : FUNCNAME LPAREN args RPAREN
            | FUNCNAME LPAREN RPAREN
            | NAME LPAREN args RPAREN
            | NAME LPAREN RPAREN

range       : CELLREF COLON CELLREF

sheetref    : NAME BANG CELLREF

args        : expr
            | args COMMA expr

params      : NAME
            | params COMMA NAME
```

### Приоритеты операторов

| Уровень | Ассоциативность | Операторы |
|---|---|---|
| 1 (низший) | left | `=`, `<>`, `<`, `>`, `<=`, `>=` |
| 2 | left | `&` (конкатенация) |
| 3 | left | `+`, `-` |
| 4 | left | `*`, `/` |
| 5 | right | `^` (возведение в степень) |
| 6 (высший) | right | унарные `+`, `-` |

### Структура проекта

- `Task/SimpleLex.lex` — входной файл для GPLEX (с `%using SimpleParser;`)
- `Task/SimpleLex.cs` — сгенерированный сканер (не редактировать вручную)
- `Task/SimpleYacc.y` — входной файл для GPPG (с `%token`, `%left` и правилами)
- `Task/SimpleYacc.cs` — сгенерированный парсер (не редактировать вручную)
- `Task/ScannerHelper.cs` — enum `Tok` для внешнего использования
- `Task/ParserHelper.cs` — классы `LexException`, `SyntaxException`
- `Task/Task.cs` — точка входа `Main()`
- `Tests/Tests.cs` — NUnit-тесты (8 баллов, фикстура `TestParser`)
- `TestRunner/Program.cs` — запускатель тестов (NUnitLite)
- `.config/dotnet-tools.json` — конфигурация GPLEX/GPPG
- `generate_parser.bat` — команды генерации

### Генерация парсера

```bash
dotnet tool run GPLEX /unicode /out:Task\SimpleLex.cs Task\SimpleLex.lex
dotnet tool run GPPG /no-lines /gplex /out:Task\SimpleYacc.cs Task\SimpleYacc.y
```

### Сборка и тестирование

```bash
echo shmester | sudo -S bash templates/tools/build.sh tasks/Reckon/generated-parser
echo shmester | sudo -S docker run --rm \
  -v "$(pwd)/tasks/Reckon/generated-parser:/workspace" \
  -w /workspace mcr.microsoft.com/dotnet/sdk:8.0 \
  bash -c 'dotnet TestRunner/bin/Debug/net8.0/TestRunner.dll 2>&1'
```

### Критерии оценки (8 баллов)

| Фикстура | Баллы | Что проверяет |
|---|---|---|
| `TestParser` | 8 | Разбор определений, выражений и приоритетов, диапазонов, ссылок на листы, вызовов функций, функций высшего порядка, литералов и литералов ошибок |