<#
.SYNOPSIS
Builds the shadcn registry and publishes it to https://loom.danielmiller.ca/r/.

.DESCRIPTION
Apps install from https://loom.danielmiller.ca/r/{name}.json, which serves the Cloudflare R2
bucket `loom`. This runs `npm run build:registry` to write public/r/, then uploads every file
there to r/<name>.json in the bucket. Uploads overwrite, so running it twice is harmless.

Needs wrangler signed in to the Cloudflare account that owns the bucket: `npx wrangler@4 login`.
#>

$ErrorActionPreference = 'Stop'

$repoRoot = Split-Path -Parent $PSScriptRoot
$outDir   = Join-Path $repoRoot 'public\r'
$bucket   = 'loom'

Push-Location $repoRoot
try {
    npm run build:registry
    if ($LASTEXITCODE -ne 0) { throw 'npm run build:registry failed.' }

    $files = Get-ChildItem -Path $outDir -Filter '*.json' -File | Sort-Object Name
    if (-not $files) { throw "No registry files in $outDir." }

    foreach ($file in $files) {
        # Wrangler 4 writes to its local simulator unless --remote is passed. The five-minute
        # cache keeps a republish visible to apps within minutes.
        $output = npx --yes wrangler@4 r2 object put "$bucket/r/$($file.Name)" `
            --file $file.FullName --remote `
            --content-type application/json --cache-control 'public, max-age=300' 2>&1
        if ($LASTEXITCODE -ne 0) {
            $output | Write-Host
            throw "Upload failed: r/$($file.Name)"
        }
        Write-Host "  r/$($file.Name)"
    }

    Write-Host "Published $($files.Count) files to https://loom.danielmiller.ca/r/"
}
finally {
    Pop-Location
}
