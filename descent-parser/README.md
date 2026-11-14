## Задание 3. Синтаксический анализатор языка Reckon R0.2 (рекурсивный спуск)

**Reckon** — декларативный язык для описания вычислений в электронных таблицах.
Версия R0.2 добавляет именованные ячейки, именованные формулы и ссылки на листы.

В этом задании необходимо написать вручную синтаксический анализатор методом
рекурсивного спуска. Лексический анализатор берётся из задания 1.

### Грамматика

```
program        : definition*
definition     : CELLREF ASSIGN expr
               | NAME ASSIGN expr
               | NAME LPAREN params RPAREN ASSIGN expr
expr           : additive (comp_op additive)*
additive       : multiplicative (('+' | '-' | '&') multiplicative)*
multiplicative : power (('*' | '/') power)*
power          : primary ('^' primary)*
primary        : ('-' | '+') primary | atom
atom           : NUMBER | STRING | BOOL | ERROR
               | CELLREF | COLREF | ROWREF | NAME
               | call | range | sheetref
               | LPAREN expr RPAREN
call           : (FUNCNAME | NAME) LPAREN args RPAREN
range          : CELLREF COLON CELLREF
sheetref       : NAME BANG CELLREF
args           : expr (COMMA expr)*
params         : NAME (COMMA NAME)*
```

### Лексемы

Числа (целые и десятичные), строки `"..."` (с экранированием), логические
значения (`TRUE`, `FALSE`), литералы ошибок (`#DIV/0!`, `#N/A`, `#VALUE!`,
`#REF!`, `#NAME?`, `#NUM!`, `#NULL!`), ссылки на ячейки (`A1`, `$A$1`, `A$1`,
`$A1`), ссылки на столбцы (`@A`), ссылки на строки (`@1`), имена функций
(`SUM`, `IF`), имена (`total`, `price`), операторы (`+ - * / ^ & := = <> < >
<= >= ->`), разделители (`( ) , : !`), комментарии (`--` до конца строки,
`{- -}` вложенные блочные).

### Структура анализатора

Класс `Parser` содержит по одному методу на каждое правило грамматики:
`Parse`, `ParseDefinition`, `ParseExpr`, `ParseAdditive`,
`ParseMultiplicative`, `ParsePower`, `ParsePrimary`, `ParseAtom`,
а также вспомогательные методы для разбора вызовов, диапазонов,
ссылок на листы, аргументов и параметров.

Публичный интерфейс:

* `Parser(Lexer)` — конструктор, принимающий лексер
* `Parse()` — разбор всей программы, возвращает `List<Definition>`
* `SyntaxError(string)` — сообщение о синтаксической ошибке (бросает `ParserException`)

### Пример

```
price := 100
tax(rate, amount) := amount * rate
total := price + tax(0.2, price)
A1 := Sheet1!B1 + SUM(A1:A10)
```

### Проекты

* `Dependency` — лексический анализатор (`Lexer.cs`)
* `Task` — синтаксический анализатор (`Task.cs`)
* `ParserDemo` — консольное приложение для запуска парсера
* `Tests` — проект с модульными тестами (NUnit)
* `TestRunner` — запускатель тестов (NUnitLite)

### Сборка и запуск

```bash
dotnet build
dotnet TestRunner/bin/Debug/net8.0/TestRunner.dll
```