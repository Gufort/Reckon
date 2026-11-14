dotnet tool run GPLEX /unicode /out:Task\SimpleLex.cs Task\SimpleLex.lex
dotnet tool run GPPG /no-lines /gplex /out:Task\SimpleYacc.cs Task\SimpleYacc.y