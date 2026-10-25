param([string]$Tool,[string]$Arguments='{}',[switch]$List)
$start = New-Object System.Diagnostics.ProcessStartInfo
$start.FileName='C:/Users/mandu252/AppData/Local/Unity/bin/unity.exe'
$start.Arguments='mcp --project-path "C:/Unity/MIDTERM EXAM"'
$start.UseShellExecute=$false
$start.RedirectStandardInput=$true
$start.RedirectStandardOutput=$true
$start.RedirectStandardError=$true
$p=[System.Diagnostics.Process]::Start($start)
function ReadResponse([int]$id) {
    while($true){$line=$p.StandardOutput.ReadLine();if($null -eq $line){throw 'MCP server closed'};try{$v=$line|ConvertFrom-Json -Depth 100}catch{continue};if($v.id -eq $id){return $v}}
}
try {
$p.StandardInput.WriteLine('{"jsonrpc":"2.0","id":1,"method":"initialize","params":{"protocolVersion":"2024-11-05","capabilities":{},"clientInfo":{"name":"Codex","version":"1.0"}}}')
$null=ReadResponse 1
$p.StandardInput.WriteLine('{"jsonrpc":"2.0","method":"notifications/initialized"}')
if($List){$request=@{jsonrpc='2.0';id=2;method='tools/list';params=@{}}}else{$request=@{jsonrpc='2.0';id=2;method='tools/call';params=@{name=$Tool;arguments=($Arguments|ConvertFrom-Json -AsHashtable)}}}
$p.StandardInput.WriteLine(($request|ConvertTo-Json -Depth 100 -Compress))
$result=ReadResponse 2
if($List){$result.result.tools | Where-Object { $_.name -match 'game_view|console|play|recompile' } | ConvertTo-Json -Depth 30}else{$result|ConvertTo-Json -Depth 100 -Compress}
}finally{if(!$p.HasExited){$p.Kill()}}

