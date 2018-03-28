REM msbuild /p:Configuration=Release /p:Platform="Any Cpu" "Fluent.Ribbon\Fluent.Ribbon\Fluent.Ribbon.NET 4.6.2.csproj"
REM msbuild /t:pack /p:Configuration=Release  /p:Platform=Net40 "AvalonEdit\ICSharpCode.AvalonEdit\ICSharpCode.AvalonEdit.csproj"
nuget spec "Fluent.Ribbon\Fluent.Ribbon\Fluent.Ribbon.NET 4.6.2.csproj"
nuget pack "Fluent.Ribbon\Fluent.Ribbon\Fluent.Ribbon.NET 4.6.2.csproj"
nuget spec "AvalonEdit\ICSharpCode.AvalonEdit\ICSharpCode.AvalonEdit.csproj"
nuget pack "AvalonEdit\ICSharpCode.AvalonEdit\ICSharpCode.AvalonEdit.csproj"
pause