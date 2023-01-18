function read-anim-file
{
  param
  (
    $fileName
  )
  [string[]] $animFileLines = Get-Content -Path $fileName
  [string[]] $replacementLines = $animFileLines;
  $lineNumber = 0
  $seekingValues = $false
  foreach($line in $animFileLines)
  {
	if($seekingValues)
	{
		#$firstNum = $line -match '[0-9].[0-9]'
		$floatRegex = '(\.*(\d)+\.*(\d)*|-\.*(\d)+\.*(\d)*)'
		$valueRegex = 'value: {x: ' + $floatRegex
		if($line -match $valueRegex)
		{
		  #Write-Host "First number in line when seeking was " $line
		  #$trimmed = $line -replace '(\D|^-)', ''
		  $x = $line.Split(",")
		  $x[0] = $x[0] -replace "\s",""
		  $x[0] = $x[0] -replace '\D(?<!-|\.)',""
		  #Write-Host $x[0]
		  [double] $dValue = $x[0];
		  $replacementLines[$lineNumber] = $line -replace $x[0], (-1*$dValue)
		  
		}
		else
		{
			$replacementLines[$lineNumber] = $line
		}
	}
	else
	{
	  if($line -match "  m_PositionCurves:")
	  {
		$seekingValues = $true
	  }
	  $replacementLines[$lineNumber] = $line
	}
	Write-Host $replacementLines[$lineNumber]
	$lineNumber = $lineNumber + 1
  }
  $flipFileName = $fileName.split("\")[-1].split("\.")
  $flipPath = $flipFileName[0]+"_flip."+$flipFileName[1];
  $wholePath = "";
  $components = $fileName.split("\")
  foreach($f in $components)
  {
	  if($f -eq $components[-1])
	  {
		  break;
	  }
	  $wholePath = $wholePath+$f+"\"
  }
  $wholePath = $wholePath + $flipPath
  Write-Host $wholePath
  $replacementLines | Out-File -Append $wholePath
}

[string[]] $animFileLines = Get-Content -Path "C:\Users\wespa\Documents\McCoy\McCoy\Utilities\animList.txt"

foreach($line in $animFileLines)
{
  #Write-Host "Getting Content of file: " $line
  read-anim-file($line) 
}
<#
Get-Content -path $nextAnimClip
$a -match "^        value: {x:"

$newLine.replace("/^\d*\.?\d*$/, /^\d*\.?\d*$/).replace(/^\d*\.?\d*$/,-/^\d*\.?\d*$/);
#>