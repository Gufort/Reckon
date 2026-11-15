%using ScannerHelper;
%namespace SimpleScanner
%x COMMENT

Alpha       [a-zA-Z_]
Digit       [0-9]
AlphaDigit  {Alpha}|{Digit}
INTNUM      {Digit}+
REALNUM     {Digit}+"."{Digit}*
ErrorLit    "#DIV/0!"|"#N/A"|"#VALUE!"|"#REF!"|"#NAME?"|"#NUM!"|"#NULL!"

StringChar  [^\"\\\r\n]
Escape      \\.

%{
  public int LexValueInt;
  public double LexValueDouble;
  public int idCount;
  public int funcCount;
  public int kwCount;
  public int numCount;
  public int stringCount;
  public int cellRefCount;
  public int colRefCount;
  public int rowRefCount;
  public int errorCount;
  public int lineCount = 1;
  public int sumInt;
  public bool lexerError;
  public int commentDepth;
%}

%%

\n                                    { lineCount++; }
[ \t\r]+                              { }

"--"[^\n]*                            { }

"{-"                                  { commentDepth = 1; BEGIN(COMMENT); }

<COMMENT>"{-"                         { commentDepth++; }
<COMMENT>"-}"                         { commentDepth--; if (commentDepth == 0) BEGIN(INITIAL); }
<COMMENT>\n                           { lineCount++; }
<COMMENT>.                            { }

\"({StringChar}|{Escape})*\"          { stringCount++; return (int)Tok.STRING; }

{ErrorLit}                            { errorCount++; return (int)Tok.ERROR; }

"TRUE"|"FALSE"                        { kwCount++; return (int)Tok.BOOL; }

\$?[A-Z]+\$?[0-9]+                    { cellRefCount++; return (int)Tok.CELLREF; }
"@"[A-Z]+                             { colRefCount++; return (int)Tok.COLREF; }
"@"[0-9]+                             { rowRefCount++; return (int)Tok.ROWREF; }

[A-Z]+                                { funcCount++; return (int)Tok.FUNCNAME; }
[a-zA-Z_][a-zA-Z0-9_]*                { idCount++; return (int)Tok.NAME; }

{REALNUM}                             { numCount++;
                                        LexValueDouble = double.Parse(yytext,
                                            System.Globalization.CultureInfo.InvariantCulture);
                                        return (int)Tok.NUMBER; }
{INTNUM}                              { numCount++;
                                        LexValueInt = int.Parse(yytext,
                                            System.Globalization.CultureInfo.InvariantCulture);
                                        sumInt += LexValueInt;
                                        return (int)Tok.NUMBER; }

":="                                  { return (int)Tok.ASSIGN; }
"->"                                  { return (int)Tok.ARROW; }
"<="                                  { return (int)Tok.LEQ; }
">="                                  { return (int)Tok.GEQ; }
"<>"                                  { return (int)Tok.NEQ; }
"+"                                   { return (int)Tok.PLUS; }
"-"                                   { return (int)Tok.MINUS; }
"*"                                   { return (int)Tok.MULT; }
"/"                                   { return (int)Tok.DIV; }
"^"                                   { return (int)Tok.POW; }
"&"                                   { return (int)Tok.CONCAT; }
"="                                   { return (int)Tok.EQ; }
"<"                                   { return (int)Tok.LT; }
">"                                   { return (int)Tok.GT; }
"("                                   { return (int)Tok.LPAREN; }
")"                                   { return (int)Tok.RPAREN; }
","                                   { return (int)Tok.COMMA; }
":"                                   { return (int)Tok.COLON; }
"!"                                   { return (int)Tok.BANG; }

.                                     { LexError(); }

%%

public void yyerror(string format, params object[] args)
{
    Console.Error.WriteLine(format, args);
}

public void LexError()
{
    errorCount++;
    lexerError = true;
    Console.Error.WriteLine("Lexical error at line {0}: unexpected character '{1}'",
                            lineCount, yytext);
}

public string TokToString(Tok tok)
{
    if (tok == Tok.EOF)
        return "EOF";
    return string.Format("{0}({1})", tok, yytext);
}