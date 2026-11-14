%{
    public Node root;
    public Parser(AbstractScanner<ValueType, LexLocation> scanner) : base(scanner) { }
%}

%output = SimpleYacc.cs

%union {
    public int iVal;
    public double dVal;
    public string sVal;
    public bool bVal;
    public Node nVal;
    public ExprNode eVal;
    public CellDefNode cDefVal;
    public ProgramNode pVal;
    public List<CellDefNode> dListVal;
    public List<ExprNode> eListVal;
    public List<string> sListVal;
}

%using ProgramTree;

%namespace SimpleParser

%token <sVal> CELLREF COLREF ROWREF NAME FUNCNAME STRING ERROR
%token <iVal> NUMBER
%token <dVal> REAL
%token <bVal> BOOL
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

%type <pVal> program
%type <dListVal> definitions
%type <cDefVal> definition
%type <eVal> expr additive multiplicative power primary atom call range sheetref lambda
%type <eListVal> args
%type <sListVal> params

%start program

%%

program     : definitions
              { /* stub */ }
            ;

definitions : definition
              { /* stub */ }
            | definitions definition
              { /* stub */ }
            ;

definition  : CELLREF ASSIGN expr
              { /* stub */ }
            | NAME ASSIGN expr
              { /* stub */ }
            | NAME LPAREN params RPAREN ASSIGN expr
              { /* stub */ }
            ;

expr        : expr EQ additive
              { /* stub */ }
            | expr NEQ additive
              { /* stub */ }
            | expr LT additive
              { /* stub */ }
            | expr GT additive
              { /* stub */ }
            | expr LEQ additive
              { /* stub */ }
            | expr GEQ additive
              { /* stub */ }
            | additive
              { $$ = $1; }
            ;

additive    : additive PLUS multiplicative
              { /* stub */ }
            | additive MINUS multiplicative
              { /* stub */ }
            | additive CONCAT multiplicative
              { /* stub */ }
            | multiplicative
              { $$ = $1; }
            ;

multiplicative : multiplicative MULT power
                { /* stub */ }
               | multiplicative DIV power
                { /* stub */ }
               | power
                { $$ = $1; }
               ;

power       : power POW primary
              { /* stub */ }
            | primary
              { $$ = $1; }
            ;

primary     : MINUS primary %prec UMINUS
              { /* stub */ }
            | PLUS primary %prec UPLUS
              { /* stub */ }
            | atom
              { $$ = $1; }
            ;

atom        : NUMBER
              { /* stub */ }
            | REAL
              { /* stub */ }
            | STRING
              { /* stub */ }
            | BOOL
              { /* stub */ }
            | ERROR
              { /* stub */ }
            | CELLREF
              { /* stub */ }
            | COLREF
              { /* stub */ }
            | ROWREF
              { /* stub */ }
            | NAME
              { /* stub */ }
            | call
              { $$ = $1; }
            | range
              { $$ = $1; }
            | sheetref
              { $$ = $1; }
            | LPAREN expr RPAREN
              { /* stub */ }
            | lambda
              { $$ = $1; }
            ;

call        : FUNCNAME LPAREN args RPAREN
              { /* stub */ }
            | FUNCNAME LPAREN RPAREN
              { /* stub */ }
            | NAME LPAREN args RPAREN
              { /* stub */ }
            | NAME LPAREN RPAREN
              { /* stub */ }
            | MAP LPAREN args RPAREN
              { /* stub */ }
            | FILTER LPAREN args RPAREN
              { /* stub */ }
            | REDUCE LPAREN args RPAREN
              { /* stub */ }
            | SORT LPAREN args RPAREN
              { /* stub */ }
            | UNIQUE LPAREN args RPAREN
              { /* stub */ }
            | LAMBDA LPAREN args RPAREN
              { /* stub */ }
            | LAMBDA LPAREN RPAREN
              { /* stub */ }
            ;

lambda      : NAME ARROW expr
              { /* stub */ }
            | LPAREN params RPAREN ARROW expr
              { /* stub */ }
            ;

range       : CELLREF COLON CELLREF
              { /* stub */ }
            ;

sheetref    : NAME BANG CELLREF
              { /* stub */ }
            ;

args        : expr
              { /* stub */ }
            | args COMMA expr
              { /* stub */ }
            ;

params      : NAME
              { /* stub */ }
            | params COMMA NAME
              { /* stub */ }
            ;

%%