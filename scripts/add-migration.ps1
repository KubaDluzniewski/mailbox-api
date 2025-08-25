param(
    [Parameter(Mandatory=$true)]
    [string]$MigrationName
)

$infrastructurePath = Join-Path $PSScriptRoot "..\Infrastructure"
$apiPath = Join-Path $PSScriptRoot "..\Api"

dotnet ef migrations add $MigrationName --project $infrastructurePath --startup-project $apiPath --output-dir Migrations
Write-Host "Migracja '$MigrationName' została dodana pomyślnie."
dotnet ef database update --project $infrastructurePath --startup-project $apiPath
Write-Host "Migracja bazy danych została zaktualizowana do najnowszej wersji."
dotnet run --project $apiPath -- seed
Write-Host "Baza danych została pomyślnie zainicjowana danymi."
