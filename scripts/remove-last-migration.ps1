param(
    [Parameter(Mandatory=$false)]
    [string]$TargetMigration = "0"
)

$infrastructurePath = "..\Infrastructure"
$apiPath = "..\Api"

dotnet ef database update $TargetMigration --project $infrastructurePath --startup-project $apiPath

dotnet ef migrations remove --project $infrastructurePath --startup-project $apiPath
