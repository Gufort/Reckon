%{
    public Parser(AbstractScanner<int, LexLocation> scanner) : base(scanner) { }
%}

%output = SimpleYacc.cs

%namespace SimpleParser

%token CELLREF COLREF ROWREF NAME FUNCNAME STRING ERROR NUMBER BOOL
%token ASSIGN EQ NEQ LT GT LEQ GEQ
%token PLUS MINUS MULT DIV POW CONCAT
%token LPAREN RPAREN COMMA COLON BANG ARROW
%token MAP FILTER REDUCE SORT UNIQUE LAMBDA

%left EQ NEQ LT GT LEQ GEQ
%left CONCAT
%left PLUS MINUS
%left MULT DIV
%right POW
%right UMINUS UPLUS

%start program

%%

program     : definitions
            ;

definitions : definition
            | definitions definition
            ;

definition  : CELLREF ASSIGN expr
            | NAME ASSIGN expr
            | NAME LPAREN params RPAREN ASSIGN expr
            ;

expr        : expr EQ additive
            | expr NEQ additive
            | expr LT additive
            | expr GT additive
            | expr LEQ additive
            | expr GEQ additive
            | additive
            ;

additive    : additive PLUS multiplicative
            | additive MINUS multiplicative
            | additive CONCAT multiplicative
            | multiplicative
            ;

multiplicative : multiplicative MULT power
               | multiplicative DIV power
               | power
               ;

power       : power POW primary
            | primary
            ;

primary     : MINUS primary %prec UMINUS
            | PLUS primary %prec UPLUS
            | atom
            ;

atom        : NUMBER
            | STRING
            | BOOL
            | ERROR
            | CELLREF
            | COLREF
            | ROWREF
            | NAME
            | call
            | range
            | sheetref
            | LPAREN expr RPAREN
            | lambda
            ;

call        : FUNCNAME LPAREN args RPAREN
            | FUNCNAME LPAREN RPAREN
            | NAME LPAREN args RPAREN
            | NAME LPAREN RPAREN
            | MAP LPAREN args RPAREN
            | FILTER LPAREN args RPAREN
            | REDUCE LPAREN args RPAREN
            | SORT LPAREN args RPAREN
            | UNIQUE LPAREN args RPAREN
            | LAMBDA LPAREN args RPAREN
            | LAMBDA LPAREN RPAREN
            ;

lambda      : NAME ARROW expr
            | LPAREN params RPAREN ARROW expr
            ;

range       : CELLREF COLON CELLREF
            ;

sheetref    : NAME BANG CELLREF
            ;

args        : expr
            | args COMMA expr
            ;

params      : NAME
            | params COMMA NAME
            ;

%%