%using SimpleParser;
%using QUT.Gppg;
%using System.Linq;

%namespace SimpleScanner

Alpha  [a-zA-Z_]
Digit   [0-9]
AlphaDigit {Alpha}|{Digit}
INTNUM  {Digit}+
REALNUM {Digit}+\.{Digit}+
IDENT   {Alpha}{AlphaDigit}*

%x COMMENT

%%

"--".*                       { }

"{-"                         { commentDepth = 1; BEGIN(COMMENT); }

<COMMENT>"{-"                { commentDepth++; }
<COMMENT>"-}"                { commentDepth--; if (commentDepth == 0) BEGIN(INITIAL); }
<COMMENT>.                   { }
<COMMENT>\n                  { }
<COMMENT><<EOF>>             { }

\"(\\.|[^"\\])*\"           { yylval.sVal = yytext; return (int)Tokens.STRING; }

#[A-Z0-9/?!]+               { yylval.sVal = yytext; return (int)Tokens.ERROR; }

{INTNUM}                     { yylval.iVal = int.Parse(yytext); return (int)Tokens.NUMBER; }

{REALNUM}                    { yylval.dVal = double.Parse(yytext, CultureInfo.InvariantCulture); return (int)Tokens.REAL; }

\$?[A-Z]+\$?{Digit}+        { yylval.sVal = yytext; return (int)Tokens.CELLREF; }

@[A-Z]+                      { yylval.sVal = yytext; return (int)Tokens.COLREF; }

@[0-9]+                      { yylval.sVal = yytext; return (int)Tokens.ROWREF; }

":="                         { return (int)Tokens.ASSIGN; }

"->"                         { return (int)Tokens.ARROW; }

"<>"                         { return (int)Tokens.NEQ; }
"<="                         { return (int)Tokens.LEQ; }
"<"                          { return (int)Tokens.LT; }
">="                         { return (int)Tokens.GEQ; }
">"                          { return (int)Tokens.GT; }
"="                          { return (int)Tokens.EQ; }

"^"                          { return (int)Tokens.POW; }

"&"                          { return (int)Tokens.CONCAT; }

"+"                          { return (int)Tokens.PLUS; }
"-"                          { return (int)Tokens.MINUS; }
"*"                          { return (int)Tokens.MULT; }
"/"                          { return (int)Tokens.DIV; }

"("                          { return (int)Tokens.LPAREN; }
")"                          { return (int)Tokens.RPAREN; }
","                          { return (int)Tokens.COMMA; }
":"                          { return (int)Tokens.COLON; }
"!"                          { return (int)Tokens.BANG; }

true                         { yylval.bVal = true; return (int)Tokens.BOOL; }
false                        { yylval.bVal = false; return (int)Tokens.BOOL; }
map                          { return (int)Tokens.MAP; }
filter                       { return (int)Tokens.FILTER; }
reduce                       { return (int)Tokens.REDUCE; }
sort                         { return (int)Tokens.SORT; }
unique                       { return (int)Tokens.UNIQUE; }
lambda                       { return (int)Tokens.LAMBDA; }

{IDENT}                      {
                               int res = ScannerHelper.GetIDToken(yytext);
                               if (res == (int)Tokens.NAME || res == (int)Tokens.FUNCNAME)
                                 yylval.sVal = yytext;
                               return res;
                             }

[ \t\r\n]+                   { }

.                            { LexError(); return (int)Tokens.EOF; }

%{
  yylloc = new LexLocation(tokLin, tokCol, tokELin, tokECol);
%}

%%

public int commentDepth;

public override void yyerror(string format, params object[] args)
{
  var ww = args.Skip(1).Cast<string>().ToArray();
  string errorMsg = string.Format("({0},{1}):  {2},   {3}", yyline, yycol, args[0], string.Join("  ", ww));
  throw new SyntaxException(errorMsg);
}

public void LexError()
{
  string errorMsg = string.Format("({0},{1}):   {2}", yyline, yycol, yytext);
  throw new LexException(errorMsg);
}

class ScannerHelper
{
  private static Dictionary<string,int> keywords;

  static ScannerHelper()
  {
    keywords = new Dictionary<string,int>();
    keywords.Add("true",(int)Tokens.BOOL);
    keywords.Add("false",(int)Tokens.BOOL);
    keywords.Add("map",(int)Tokens.MAP);
    keywords.Add("filter",(int)Tokens.FILTER);
    keywords.Add("reduce",(int)Tokens.REDUCE);
    keywords.Add("sort",(int)Tokens.SORT);
    keywords.Add("unique",(int)Tokens.UNIQUE);
    keywords.Add("lambda",(int)Tokens.LAMBDA);
  }
  public static int GetIDToken(string s)
  {
    string lower = s.ToLower();
    if (keywords.ContainsKey(lower))
      return keywords[lower];
    if (s.All(c => char.IsUpper(c) || char.IsDigit(c)))
      return (int)Tokens.FUNCNAME;
    else
      return (int)Tokens.NAME;
  }
}