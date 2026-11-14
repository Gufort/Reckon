## Семантические действия при синтаксическом разборе. Построение синтаксического дерева программы

см. [общее описание задания](http://gitlab.mmcs.sfedu.ru/mmcs/compilers-fiit/-/wikis/%D0%A1%D0%B5%D0%BC%D0%B0%D0%BD%D1%82%D0%B8%D1%87%D0%B5%D1%81%D0%BA%D0%B8%D0%B5_%D0%B4%D0%B5%D0%B9%D1%81%D1%82%D0%B2%D0%B8%D1%8F_%D0%BF%D1%80%D0%B8_%D1%81%D0%B8%D0%BD%D1%82%D0%B0%D0%BA%D1%81%D0%B8%D1%87%D0%B5%D1%81%D0%BA%D0%BE%D0%BC_%D1%80%D0%B0%D0%B7%D0%B1%D0%BE%D1%80%D0%B5._%D0%9F%D0%BE%D1%81%D1%82%D1%80%D0%BE%D0%B5%D0%BD%D0%B8%D0%B5_%D1%81%D0%B8%D0%BD%D1%82%D0%B0%D0%BA%D1%81%D0%B8%D1%87%D0%B5%D1%81%D0%BA%D0%BE%D0%B3%D0%BE_%D0%B4%D0%B5%D1%80%D0%B5%D0%B2%D0%B0_%D0%BF%D1%80%D0%BE%D0%B3%D1%80%D0%B0%D0%BC%D0%BC%D1%8B)

### Язык Reckon

Reckon — это декларативный язык вычислений для электронных таблиц. Программа состоит из набора определений ячеек вида `A1 := выражение` или `имя := выражение`.

Поддерживаются:
- Числа, строки, булевы значения, литералы ошибок (`#DIV/0!`, `#N/A`, `#NAME?`, `#REF!`, `#VALUE!`, `#NUM!`, `#NULL!`)
- Ссылки на ячейки: `A1`, `$A$1`, `A$1`, `$A1`
- Ссылки на столбцы: `@A`
- Ссылки на строки: `@1`
- Именованные ячейки: `total := 100`
- Именованные формулы: `tax(rate, amount) := amount * rate`
- Диапазоны: `A1:B10`
- Межлистовые ссылки: `Sheet1!A1`
- Арифметика: `+ - * / ^ &`
- Сравнение: `= <> < > <= >=`
- Унарные операторы: `- +`
- Вызовы функций: `SUM(A1:A10)`, `IF(A1 > 0, "pos", "neg")`
- Функции высшего порядка: `MAP`, `FILTER`, `REDUCE`, `SORT`, `UNIQUE`
- Лямбда-выражения: `x -> x * 2`, `(acc, x) -> acc + x`
- Комментарии: `--` строковые, `{- -}` блочные вложенные

### сборка

Этот проект использует
* пакет GPLex для генерации `Task/SimpleLex.cs`
* пакет GPPG для генерации `Task/SimpleYacc.cs`

Задание основано на предыдущем задании №4 (парсер с помощью GPPG). Добавлены:
- Вычисление значений лексем (yylval) для NUMBER, STRING, BOOL, ERROR
- Семантические действия в грамматике для построения узлов AST
- Классы узлов AST в `Task/ProgramTree.cs` (ProgramNode, CellDefNode, NumberNode, StringNode, BoolNode, ErrorNode, CellRefNode, ColRefNode, RowRefNode, NameNode, RangeNode, SheetRefNode, CallNode, BinOpNode, UnaryOpNode, LambdaNode)

Для установки программ запустите команду
```
dotnet tool install --add-source http://nuget.stud.mmcs.sfedu.ru GPLEXnetcore
dotnet tool install --add-source http://nuget.stud.mmcs.sfedu.ru GPPGnetcore
```

Для запуска этих программ достаточно в корне проекта сделать команды:
```
dotnet tool run GPLEX /unicode /out:Task\SimpleLex.cs Task\SimpleLex.lex
dotnet tool run GPPG /no-lines /gplex /out:Task\SimpleYacc.cs Task\SimpleYacc.y
```

Менять нужно описание лексем в `Task/SimpleLex.lex`, правила грамматики с действиями в `Task/SimpleYacc.y`, классы узлов в `Task/ProgramTree.cs` и каждый раз генерировать лексер и парсер заново.

### задания

В зависимости от варианта задания, поменяйте набор лексем и правил грамматики с действиями для разбора (и тесты)

1. Реализовать построение узла `ProgramNode` для программы как набора определений
2. Реализовать построение узлов `CellDefNode`, `CellRefNode`, `NameNode` для определений ячеек
3. Реализовать построение узлов `BinOpNode`, `UnaryOpNode` для выражений с операторами
4. Реализовать построение узлов `NumberNode`, `StringNode`, `BoolNode`, `ErrorNode` для литералов
5. Реализовать построение узла `CallNode` для вызовов функций
6. Реализовать построение узла `RangeNode` для диапазонов
7. Реализовать построение узла `SheetRefNode` для межлистовых ссылок
8. Реализовать построение узла `LambdaNode` для лямбда-выражений