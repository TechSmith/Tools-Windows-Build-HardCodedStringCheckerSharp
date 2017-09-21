#Change the directories to the proper locations. These assume repos are checked into C:\Source\

$HCS = "C:\Source\Tools-Windows-Build-HardCodedStringCheckerSharp\HardCodedStringCheckerSharp\bin\Debug\HardCodedStringCheckerSharp.exe"
$repo = "C:\Source\DotNetCommon-Windows-FileSystemUI"

echo "--FailOnHCS"
&$HCS $repo Report --FailOnHCS
if ($LASTEXITCODE -ne 0){
	$FailOnHCS = "fail"
} else {$FailOnHCS = "Pass"}

echo "True"
&$HCS $repo Report True 
if ($LASTEXITCODE -ne 0){
	$Initialcap = "fail"
} else {$Initialcap = "Pass"}

echo "TRUE"
&$HCS $repo Report TRUE
if ($LASTEXITCODE -ne 0){ 
	$allCap = "fail"
} else {$allCap = "Pass"}

echo "true"
&$HCS $repo Report true 
if ($LASTEXITCODE -ne 0){ 
	$lowercase = "fail"
} else {$lowercase = "Pass"}

echo "TrUe"
&$HCS $repo Report TrUe
if ($LASTEXITCODE -ne 0){ 
	$camelCase = "fail"
} else {$camelCase = "Pass"}

echo "false"
&$HCS $repo Report false
if ($LASTEXITCODE -ne 0){ 
	$falseflag = "fail"
} else {$falseflag = "Pass"}

echo "Batman"
&$HCS $repo Report Batman
if ($LASTEXITCODE -ne 0){ 
	$batman = "fail"
} else {$batman = "nana nana nana nana"}
echo ""
echo ""
echo "    TEST RESULTS    "

echo "FailOnHCS = $FailOnHCS. Expected fail."
echo "True = $Initialcap. Expected fail."
echo "TRUE = $allCap. Expected fail."
echo "true = $lowercase. Expected fail."
echo "TrUe = $camelCase. Expected fail."
echo ""
echo "false = $falseflag. Expected pass."
echo "Batman = $batman. Expected nana nana nana nana."
echo ""
echo ""
