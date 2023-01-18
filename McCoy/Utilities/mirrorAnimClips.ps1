function read-anim-file ([string]$file, [bool]$doFlip, [string]$suffix)
{ 
  [string[]] $animFileLines = Get-Content -Path $file
  [string[]] $replacementLines = $animFileLines;
  $lineNumber = 0
  $seekingPosition = $false
  $lastValueLines = ""
  foreach($line in $animFileLines)
  {
	  # if we're finding values, hang on to them. might end up these are values for a flipX, and we'll want to go back and flip the values on those lines
	  if($line -match ("value: "))
	  {
		  $lastValueLines = $lastValueLines + "|" + $lineNumber
		  
	  }
	  # any time we reach a new curve, we can reset that lastValueLines string; we must have already evaluated the flipX if that's what we were looking at
	  if($line -match ("- curve:"))
	  {
		  $lastValueLines = ""
	  }
	  # if we do get to a flipX, it's time to process all the value lines we've seen since the last curve started
	  if($line -match ("attribute: m_FlipX"))
	  {
		  if($doFlip)
		  {
			  # grab all our individual lines we've seen
			  $lvlComps = $lastValueLines.Split("|")
			  foreach($lastValueLine in $lvlComps)
			  {
				  # there's probably a leading "" before the first |, so skip that
				  if($lastValueLine.Length -eq 0)
				  {
					  continue;
				  }
				  # grab the line we saw on the number we tracked
				  $flipXValueLine = $replacementLines[$lastValueLine]
				  # split the values on this line, the last one will be either 1 or 0 for the flipX state
				  $flipComponents = $flipXValueLine.Split(" ");
				  # flip it!
				  if($flipComponents[-1] -eq "0")
				  {
					  $flipComponents[-1] = "1"
				  }
				  else
				  {
					  $flipComponents[-1] = "0"
				  }
				  # join the line with spaces again
				  $replacementFlipXLine = $flipComponents -join " "
				  # insert it where the original line was
				  $replacementLines[$lastValueLine] = $replacementFlipXLine
			  }
		  }
	  }
	  # if we're in a chunk that's defining position curves
	if($seekingPosition)
	{
		# here's a regex for a decimal number
		$floatRegex = '(\.*(\d)+\.*(\d)*|-\.*(\d)+\.*(\d)*)'
		$valueRegex = 'value: {x: ' + $floatRegex
		# if we find value: {x: ANUMBER
		if($line -match $valueRegex)
		{
			# split out the x, y, and z components. we only care about X (0-index)
		  $x = $line.Split(",")
		  # remove whitespace
		  $x[0] = $x[0] -replace "\s",""
		  #remove anything that isn't a number or a minus sign
		  $x[0] = $x[0] -replace '\D(?<!-|\.)',""
		  #grab the number value of what's left, which is our X, and negate it
		  [double] $dValue = -1 * $x[0];
		  # negate it and reinsert it into the line
		  $replacementLines[$lineNumber] = $line -replace $x[0], $dValue
		  Write-Host $replacementLines[$lineNumber]
		}
		# if it wasn't our position values, just paste the line in unaltered
		else
		{
			$replacementLines[$lineNumber] = $line
		}
	}
	else
	{
		# we're probably just going to add the line unaltered
		$toAdd = $line
		# if this line is starts the position curves though, flag it so we start looking for X's to flip
	  if($line -match "  m_PositionCurves:")
	  {
		$seekingPosition = $true
	  }
	  # if we find the name of the animation, add "_flip" to it
	  elseif($line -match "m_Name: ")
	  {
		  [string[]] $nameComponents = $line.Split(" ")
		  if($suffix -eq "")
		  {
		  }
		  else
		  {
			  $nameComponents[-1] = $nameComponents[-1]+$suffix
		  }
		  if($doFlip)
		  {
			  $nameComponents[-1] = $nameComponents[-1]+"_flip"
		  }
		  $toAdd = $nameComponents -join " "
	  }
	  $replacementLines[$lineNumber] = $toAdd
	}
	$lineNumber = $lineNumber + 1
  }
  $flipFileName = $file.split("\")[-1].split("\.")
  $flipPath = $flipFileName[0]
  if($suffix -eq "")
  {
  }
  else
  {
	  $flipPath = $flipPath + $suffix
  }
  if($doFlip)
  {
	$flipPath = $flipPath + "_flip"
  }
  $flipPath = $flipPath + "." + $flipFileName[1];
  
  $wholePath = "";
  $components = $file.split("\")
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
  $replacementLines | Out-File $wholePath -Encoding ASCII
}

[string[]] $animFileLines = Get-Content -Path "C:\Users\wespa\Documents\McCoy\McCoy\Utilities\animList.txt"


foreach($line in $animFileLines)
{
  #Write-Host "Getting Content of file: " $line
  read-anim-file $line $true ""
  read-anim-file $line $false "_colossus"
  read-anim-file $line $true "_colossus"
}
<#
Get-Content -path $nextAnimClip
$a -match "^        value: {x:"

$newLine.replace("/^\d*\.?\d*$/, /^\d*\.?\d*$/).replace(/^\d*\.?\d*$/,-/^\d*\.?\d*$/);
#>