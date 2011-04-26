@echo off
cd %~dp0

echo "=== Sign EXE ==="
..\..\..\..\solution\tools\simplesigner\simplesigner.exe "coapp.installer.exe"

echo "=== Create Manifest + Catalog ==="
"C:\Program Files (x86)\Microsoft SDKs\Windows\v7.0A\Bin\mt.exe" -manifest "coapp.installer.manifest" -hashupdate -makecdfs
"C:\Program Files (x86)\Microsoft SDKs\Windows\v7.0A\Bin\MakeCat.Exe" "coapp.installer.manifest.cdf" 

echo "=== Create Policy Manifest + Catalog ==="
"C:\Program Files (x86)\Microsoft SDKs\Windows\v7.0A\Bin\mt.exe" -manifest "policy.1.0.coapp.installer.manifest" -hashupdate -makecdfs
"C:\Program Files (x86)\Microsoft SDKs\Windows\v7.0A\Bin\MakeCat.Exe" "policy.1.0.coapp.installer.manifest.cdf" 

echo "=== Sign Catalogs ==="
..\..\..\..\solution\tools\simplesigner\simplesigner.exe --sign-only  "coapp.installer.cat" 
..\..\..\..\solution\tools\simplesigner\simplesigner.exe --sign-only  "policy.1.0.coapp.installer.cat" 

echo "=== Create MSI ==="
"c:\Program Files (x86)\Windows Installer XML v3.5\bin\candle.exe" "coapp.installer.wxs "
"c:\Program Files (x86)\Windows Installer XML v3.5\bin\light.exe" "coapp.installer.wixobj"

echo "=== Sign MSI ==="
..\..\..\..\solution\tools\simplesigner\simplesigner.exe --sign-only "coapp.installer.msi"

if exist ..\..\..\..\release\final copy "coapp.installer.msi" ..\..\..\..\release\final