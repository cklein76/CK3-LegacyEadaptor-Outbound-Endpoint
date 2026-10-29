<#
.SYNOPSIS
  Generates the PasswordHash:<UserName> value for Web.config.

.DESCRIPTION
  The service hashes (user name + password) with CargoWise.eHub.Common.SHA512Encryptor.Encrypt,
  which is SHA-512 over Encoding.Default bytes, written as dash-separated hex (A1-B2-...).
  This script calls that same method from CargoWise.eHub.Common.dll, so the result always matches
  what the service computes. Run it in Windows PowerShell 5.1 (powershell.exe), which runs on
  .NET Framework like the service does.

.EXAMPLE
  .\tools\New-PasswordHash.ps1 -UserName BG1LAXTRN
  (prompts for the password, then prints the <add key=.../> line to paste into <appSettings>)
#>
param(
    [Parameter(Mandatory = $true)] [string] $UserName,
    [string] $CommonDll = (Join-Path $PSScriptRoot '..\CargoWise.eHub.Common.dll')
)

$secure = Read-Host -AsSecureString "Password for $UserName"
$bstr = [Runtime.InteropServices.Marshal]::SecureStringToBSTR($secure)
try {
    $password = [Runtime.InteropServices.Marshal]::PtrToStringBSTR($bstr)
} finally {
    [Runtime.InteropServices.Marshal]::ZeroFreeBSTR($bstr)
}

Add-Type -Path (Resolve-Path $CommonDll)
$hash = [CargoWise.eHub.Common.SHA512Encryptor]::Encrypt($UserName + $password)

"<add key=`"PasswordHash:$UserName`" value=`"$hash`"/>"
