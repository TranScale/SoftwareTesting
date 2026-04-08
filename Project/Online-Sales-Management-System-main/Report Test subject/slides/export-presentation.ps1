param(
    [string]$SourcePath = "E:\Project\Online-Sales-Management-System\Report Test subject\slides\Powered by GPT - Software Quality Verification - Presentation Content.md",
    [string]$OutputPath = "E:\Project\Online-Sales-Management-System\Report Test subject\slides\Powered by GPT - Software Quality Verification - Presentation.pptx"
)

$ErrorActionPreference = "Stop"

$ppt = $null
$presentation = $null

function New-SlideObject {
    return [ordered]@{
        Title = ""
        Bullets = New-Object System.Collections.Generic.List[string]
        Visuals = New-Object System.Collections.Generic.List[string]
    }
}

function Clean-ContentLine {
    param([string]$Text)

    return ($Text -replace '`', '').Trim()
}

try {
    $lines = Get-Content -Path $SourcePath
    $slides = New-Object System.Collections.Generic.List[object]
    $currentSlide = $null
    $mode = ""

    foreach ($line in $lines) {
        if ($line -match '^## Slide \d+\.\s+(.+)$') {
            if ($null -ne $currentSlide) {
                $slides.Add($currentSlide)
            }
            $currentSlide = New-SlideObject
            $currentSlide.Title = $Matches[1]
            $mode = ""
            continue
        }

        if ($null -eq $currentSlide) {
            continue
        }

        if ($line -eq '### Slide content') {
            $mode = "content"
            continue
        }

        if ($line -eq '### Visual to show') {
            $mode = "visual"
            continue
        }

        if ([string]::IsNullOrWhiteSpace($line)) {
            continue
        }

        switch ($mode) {
            "content" {
                if ($line -match '^\*\*(.+)\*\*$') {
                    $clean = Clean-ContentLine $Matches[1]
                    if ($currentSlide.Title -eq "Title" -and $currentSlide.Bullets.Count -eq 0) {
                        $currentSlide.Title = $clean
                    } else {
                        $currentSlide.Bullets.Add($clean)
                    }
                } elseif ($line -match '^\s*-\s+(.+)$') {
                    $currentSlide.Bullets.Add((Clean-ContentLine $Matches[1]))
                } elseif ($line -match '^\d+\.\s+(.+)$') {
                    $currentSlide.Bullets.Add((Clean-ContentLine $line.Trim()))
                } else {
                    $currentSlide.Bullets.Add((Clean-ContentLine $line))
                }
            }
            "visual" {
                $currentSlide.Visuals.Add($line.Trim())
            }
        }
    }

    if ($null -ne $currentSlide) {
        $slides.Add($currentSlide)
    }

    $ppt = New-Object -ComObject PowerPoint.Application
    $ppt.Visible = -1
    $presentation = $ppt.Presentations.Add()

    foreach ($slideData in $slides) {
        $slide = $presentation.Slides.Add($presentation.Slides.Count + 1, 11)
        $slide.Shapes.Title.TextFrame.TextRange.Text = $slideData.Title
        $slide.Shapes.Title.TextFrame.TextRange.Font.Size = 28
        $slide.Shapes.Title.TextFrame.TextRange.Font.Name = "Aptos Display"

        $bodyShape = $slide.Shapes.AddTextbox(1, 55, 125, 520, 430)
        $bodyShape.TextFrame.TextRange.Text = ($slideData.Bullets -join "`r`n")
        $bodyShape.TextFrame.TextRange.Font.Size = 22
        $bodyShape.TextFrame.TextRange.Font.Name = "Aptos"
        $bodyShape.TextFrame.WordWrap = -1

        foreach ($visual in $slideData.Visuals) {
            if ($visual -match '([A-Za-z]:\\[^`]+?\.(png|jpg|jpeg|bmp))') {
                $imagePath = $Matches[1]
            } elseif ($visual -match '`([^`]+?\.(png|jpg|jpeg|bmp))`') {
                $imagePath = Join-Path "E:\Project\Online-Sales-Management-System\Report Test subject" $Matches[1]
            } else {
                continue
            }

            if (Test-Path -LiteralPath $imagePath) {
                $slide.Shapes.AddPicture($imagePath, $false, $true, 620, 120, 620, 350) | Out-Null
                break
            }
        }
    }

    $presentation.SaveAs($OutputPath)
}
finally {
    if ($null -ne $presentation) {
        $presentation.Close()
    }
    if ($null -ne $ppt) {
        $ppt.Quit()
    }

    [System.GC]::Collect()
    [System.GC]::WaitForPendingFinalizers()
    if ($null -ne $presentation) {
        [System.Runtime.InteropServices.Marshal]::ReleaseComObject($presentation) | Out-Null 2>$null
    }
    if ($null -ne $ppt) {
        [System.Runtime.InteropServices.Marshal]::ReleaseComObject($ppt) | Out-Null 2>$null
    }
}
