param(
    [string]$SourcePath = "E:\Project\Online-Sales-Management-System\Report Test subject\report\Powered by GPT - Software Quality Verification - Final Report Content.md",
    [string]$DocxPath = "E:\Project\Online-Sales-Management-System\Report Test subject\report\Powered by GPT - Software Quality Verification - Final Report.docx",
    [string]$PdfPath = "E:\Project\Online-Sales-Management-System\Report Test subject\report\Powered by GPT - Software Quality Verification - Final Report.pdf"
)

$ErrorActionPreference = "Stop"

$word = $null
$document = $null

try {
    $resolvedSource = (Resolve-Path $SourcePath).Path
    $resolvedDocx = [System.IO.Path]::GetFullPath($DocxPath)
    $resolvedPdf = [System.IO.Path]::GetFullPath($PdfPath)

    $word = New-Object -ComObject Word.Application
    $word.Visible = $false
    $word.DisplayAlerts = 0

    $document = $word.Documents.Open($resolvedSource, $false, $true)
    $document.SaveAs([ref]$resolvedDocx, [ref]16)
    $document.ExportAsFixedFormat($resolvedPdf, 17)
}
finally {
    if ($null -ne $document) {
        $document.Close()
        [System.Runtime.InteropServices.Marshal]::ReleaseComObject($document) | Out-Null 2>$null
    }
    if ($null -ne $word) {
        $word.Quit()
        [System.Runtime.InteropServices.Marshal]::ReleaseComObject($word) | Out-Null 2>$null
    }

    [System.GC]::Collect()
    [System.GC]::WaitForPendingFinalizers()
}
