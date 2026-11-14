%using ScannerHelper;
%namespace SimpleScanner
%x COMMENT

Alpha   [a-zA-Z_]
Digit   [0-9]
AlphaDigit {Alpha}|{Digit}
INTNUM  {Digit}+
REALNUM {Digit}+"."{Digit}*

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

. { throw new System.NotImplementedException(); }

%%

public void yyerror(string format, params object[] args)
{
  throw new System.NotImplementedException();
}

public void LexError()
{
  throw new System.NotImplementedException();
}

public string TokToString(Tok tok)
{
  throw new System.NotImplementedException();
}