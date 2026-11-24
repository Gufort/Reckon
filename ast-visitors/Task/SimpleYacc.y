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
              { root = new ProgramNode($1); $$ = (ProgramNode)root; }
            ;

definitions : definition
              { $$ = new List<CellDefNode> { $1 }; }
            | definitions definition
              { $$ = $1; $1.Add($2); }
            ;

definition  : CELLREF ASSIGN expr
              { $$ = new CellDefNode(new CellRefNode($1), $3); }
            | NAME ASSIGN expr
              { $$ = new CellDefNode(new NameNode($1), $3); }
            | NAME LPAREN params RPAREN ASSIGN expr
              { $$ = new CellDefNode(new NameNode($1), new LambdaNode($3, $6)); }
            ;

expr        : expr EQ additive
              { $$ = new BinOpNode($1, $3, "="); }
            | expr NEQ additive
              { $$ = new BinOpNode($1, $3, "<>"); }
            | expr LT additive
              { $$ = new BinOpNode($1, $3, "<"); }
            | expr GT additive
              { $$ = new BinOpNode($1, $3, ">"); }
            | expr LEQ additive
              { $$ = new BinOpNode($1, $3, "<="); }
            | expr GEQ additive
              { $$ = new BinOpNode($1, $3, ">="); }
            | additive
              { $$ = $1; }
            ;

additive    : additive PLUS multiplicative
              { $$ = new BinOpNode($1, $3, "+"); }
            | additive MINUS multiplicative
              { $$ = new BinOpNode($1, $3, "-"); }
            | additive CONCAT multiplicative
              { $$ = new BinOpNode($1, $3, "&"); }
            | multiplicative
              { $$ = $1; }
            ;

multiplicative : multiplicative MULT power
                { $$ = new BinOpNode($1, $3, "*"); }
               | multiplicative DIV power
                { $$ = new BinOpNode($1, $3, "/"); }
               | power
                { $$ = $1; }
               ;

power       : power POW primary
              { $$ = new BinOpNode($1, $3, "^"); }
            | primary
              { $$ = $1; }
            ;

primary     : MINUS primary %prec UMINUS
              { $$ = new UnaryOpNode($2, "-"); }
            | PLUS primary %prec UPLUS
              { $$ = new UnaryOpNode($2, "+"); }
            | atom
              { $$ = $1; }
            ;

atom        : NUMBER
              { $$ = new NumberNode($1); }
            | REAL
              { $$ = new NumberNode($1); }
            | STRING
              { $$ = new StringNode(ParserHelper.UnescapeString($1)); }
            | BOOL
              { $$ = new BoolNode($1); }
            | ERROR
              { $$ = new ErrorNode($1); }
            | CELLREF
              { $$ = new CellRefNode($1); }
            | COLREF
              { $$ = new ColRefNode($1); }
            | ROWREF
              { $$ = new RowRefNode($1); }
            | NAME
              { $$ = new NameNode($1); }
            | call
              { $$ = $1; }
            | range
              { $$ = $1; }
            | sheetref
              { $$ = $1; }
            | LPAREN expr RPAREN
              { $$ = $2; }
            | lambda
              { $$ = $1; }
            ;

call        : FUNCNAME LPAREN args RPAREN
              { $$ = new CallNode($1, $3); }
            | FUNCNAME LPAREN RPAREN
              { $$ = new CallNode($1, new List<ExprNode>()); }
            | NAME LPAREN args RPAREN
              { $$ = new CallNode($1, $3); }
            | NAME LPAREN RPAREN
              { $$ = new CallNode($1, new List<ExprNode>()); }
            | MAP LPAREN args RPAREN
              { $$ = new CallNode("MAP", $3); }
            | FILTER LPAREN args RPAREN
              { $$ = new CallNode("FILTER", $3); }
            | REDUCE LPAREN args RPAREN
              { $$ = new CallNode("REDUCE", $3); }
            | SORT LPAREN args RPAREN
              { $$ = new CallNode("SORT", $3); }
            | UNIQUE LPAREN args RPAREN
              { $$ = new CallNode("UNIQUE", $3); }
            | LAMBDA LPAREN args RPAREN
              { $$ = new CallNode("LAMBDA", $3); }
            | LAMBDA LPAREN RPAREN
              { $$ = new CallNode("LAMBDA", new List<ExprNode>()); }
            ;

lambda      : NAME ARROW expr
              { $$ = new LambdaNode(new List<string> { $1 }, $3); }
            | LPAREN params RPAREN ARROW expr
              { $$ = new LambdaNode($2, $5); }
            ;

range       : CELLREF COLON CELLREF
              { $$ = new RangeNode($1, $3); }
            ;

sheetref    : NAME BANG CELLREF
              { $$ = new SheetRefNode($1, $3); }
            ;

args        : expr
              { $$ = new List<ExprNode> { $1 }; }
            | args COMMA expr
              { $$ = $1; $1.Add($3); }
            ;

params      : NAME
              { $$ = new List<string> { $1 }; }
            | params COMMA NAME
              { $$ = $1; $1.Add($3); }
            ;

%%