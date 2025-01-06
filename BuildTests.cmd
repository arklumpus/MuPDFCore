@echo off

echo.
echo [104;97mDeleting previous build...[0m

for /f %%i in ('dir /a:d /b Release\MuPDFCoreTests\*') do rd /s /q Release\MuPDFCoreTests\%%i
del Release\MuPDFCoreTests\* /s /f /q 1>nul

echo.
echo [104;97mCopying common resources...[0m

xcopy Resources\* Release\MuPDFCoreTests\ /s /y /h >nul

echo.
echo Building with target [94mwin-x64[0m

dotnet clean MuPDFCoreTestHost /nowarn:NETSDK1138
dotnet clean Tests /nowarn:NETSDK1138
rmdir /s /q Tests\bin Tests\obj MuPDFCoreTestHost\bin MuPDFCoreTestHost\obj

cd MuPDFCoreTestHost
dotnet publish -c Release /p:PublishProfile=Properties\PublishProfiles\win-x64.pubxml /nowarn:NETSDK1138
cd ..

echo.
echo [104;97mCreating ZIP file...[0m

cd Release\MuPDFCoreTests

move win-x64 MuPDFCoreTests-win-x64
bash -c "zip -r MuPDFCoreTests-win-x64.zip MuPDFCoreTests-win-x64 >/dev/null"

for /f %%i in ('dir /a:d /b "MuPDFCoreTests-win-x64"\*') do rd /s /q "MuPDFCoreTests-win-x64"\%%i
del MuPDFCoreTests-win-x64\* /s /f /q 1>nul
rmdir MuPDFCoreTests-win-x64

cd ..\..

echo.
echo Building with target [94mwin-x86[0m

dotnet clean MuPDFCoreTestHost /nowarn:NETSDK1138
dotnet clean Tests /nowarn:NETSDK1138
rmdir /s /q Tests\bin Tests\obj MuPDFCoreTestHost\bin MuPDFCoreTestHost\obj

cd MuPDFCoreTestHost
dotnet publish -c Release /p:PublishProfile=Properties\PublishProfiles\win-x86.pubxml /p:PlatformTarget=x86 /nowarn:NETSDK1138
cd ..

echo.
echo [104;97mCreating ZIP file...[0m

cd Release\MuPDFCoreTests

move win-x86 MuPDFCoreTests-win-x86
bash -c "zip -r MuPDFCoreTests-win-x86.zip MuPDFCoreTests-win-x86 >/dev/null"

for /f %%i in ('dir /a:d /b "MuPDFCoreTests-win-x86"\*') do rd /s /q "MuPDFCoreTests-win-x86"\%%i
del MuPDFCoreTests-win-x86\* /s /f /q 1>nul
rmdir MuPDFCoreTests-win-x86

cd ..\..

echo.
echo Building with target [94mwin-arm64[0m

dotnet clean MuPDFCoreTestHost /nowarn:NETSDK1138
dotnet clean Tests /nowarn:NETSDK1138
rmdir /s /q Tests\bin Tests\obj MuPDFCoreTestHost\bin MuPDFCoreTestHost\obj

cd MuPDFCoreTestHost
dotnet publish -c Release /p:PublishProfile=Properties\PublishProfiles\win-arm64.pubxml /p:PlatformTarget=arm64 /nowarn:NETSDK1138
cd ..

echo.
echo [104;97mCreating ZIP file...[0m

cd Release\MuPDFCoreTests

move win-arm64 MuPDFCoreTests-win-arm64
bash -c "zip -r MuPDFCoreTests-win-arm64.zip MuPDFCoreTests-win-arm64 >/dev/null"

for /f %%i in ('dir /a:d /b "MuPDFCoreTests-win-arm64"\*') do rd /s /q "MuPDFCoreTests-win-arm64"\%%i
del MuPDFCoreTests-win-arm64\* /s /f /q 1>nul
rmdir MuPDFCoreTests-win-arm64

cd ..\..

echo.
echo Building with target [94mlinux-x64[0m

dotnet clean MuPDFCoreTestHost /nowarn:NETSDK1138
dotnet clean Tests /nowarn:NETSDK1138
rmdir /s /q Tests\bin Tests\obj MuPDFCoreTestHost\bin MuPDFCoreTestHost\obj

cd MuPDFCoreTestHost
dotnet publish -c Release /p:PublishProfile=Properties\PublishProfiles\linux-x64.pubxml /nowarn:NETSDK1138
cd ..

echo.
echo [104;97mCreating tarball...[0m

cd Release\MuPDFCoreTests

move linux-x64 MuPDFCoreTests-linux-x64
bash -c "tar -czf MuPDFCoreTests-linux-x64.tar.gz MuPDFCoreTests-linux-x64"

for /f %%i in ('dir /a:d /b "MuPDFCoreTests-linux-x64"\*') do rd /s /q "MuPDFCoreTests-linux-x64"\%%i
del MuPDFCoreTests-linux-x64\* /s /f /q 1>nul
rmdir MuPDFCoreTests-linux-x64

cd ..\..

echo.
echo Building with target [94mlinux-musl-x64[0m

dotnet clean MuPDFCoreTestHost /nowarn:NETSDK1138
dotnet clean Tests /nowarn:NETSDK1138
rmdir /s /q Tests\bin Tests\obj MuPDFCoreTestHost\bin MuPDFCoreTestHost\obj

cd MuPDFCoreTestHost
dotnet publish -c Release /p:PublishProfile=Properties\PublishProfiles\linux-musl-x64.pubxml /nowarn:NETSDK1138
cd ..

echo.
echo [104;97mCreating tarball...[0m

cd Release\MuPDFCoreTests

move linux-musl-x64 MuPDFCoreTests-linux-musl-x64
bash -c "tar -czf MuPDFCoreTests-linux-musl-x64.tar.gz MuPDFCoreTests-linux-musl-x64"

for /f %%i in ('dir /a:d /b "MuPDFCoreTests-linux-musl-x64"\*') do rd /s /q "MuPDFCoreTests-linux-musl-x64"\%%i
del MuPDFCoreTests-linux-musl-x64\* /s /f /q 1>nul
rmdir MuPDFCoreTests-linux-musl-x64

cd ..\..

echo.
echo Building with target [94mlinux-arm64[0m

dotnet clean MuPDFCoreTestHost /nowarn:NETSDK1138
dotnet clean Tests /nowarn:NETSDK1138
rmdir /s /q Tests\bin Tests\obj MuPDFCoreTestHost\bin MuPDFCoreTestHost\obj

cd MuPDFCoreTestHost
dotnet publish -c Release /p:PublishProfile=Properties\PublishProfiles\linux-arm64.pubxml /p:PlatformTarget=arm64 /nowarn:NETSDK1138
cd ..

echo.
echo [104;97mCreating tarball...[0m

cd Release\MuPDFCoreTests

move linux-arm64 MuPDFCoreTests-linux-arm64
bash -c "tar -czf MuPDFCoreTests-linux-arm64.tar.gz MuPDFCoreTests-linux-arm64"

for /f %%i in ('dir /a:d /b "MuPDFCoreTests-linux-arm64"\*') do rd /s /q "MuPDFCoreTests-linux-arm64"\%%i
del MuPDFCoreTests-linux-arm64\* /s /f /q 1>nul
rmdir MuPDFCoreTests-linux-arm64

cd ..\..

echo.
echo Building with target [94mlinux-musl-arm64[0m

dotnet clean MuPDFCoreTestHost /nowarn:NETSDK1138
dotnet clean Tests /nowarn:NETSDK1138
rmdir /s /q Tests\bin Tests\obj MuPDFCoreTestHost\bin MuPDFCoreTestHost\obj

cd MuPDFCoreTestHost
dotnet publish -c Release /p:PublishProfile=Properties\PublishProfiles\linux-musl-arm64.pubxml /nowarn:NETSDK1138
cd ..

echo.
echo [104;97mCreating tarball...[0m

cd Release\MuPDFCoreTests

move linux-musl-arm64 MuPDFCoreTests-linux-musl-arm64
bash -c "tar -czf MuPDFCoreTests-linux-musl-arm64.tar.gz MuPDFCoreTests-linux-musl-arm64"

for /f %%i in ('dir /a:d /b "MuPDFCoreTests-linux-musl-arm64"\*') do rd /s /q "MuPDFCoreTests-linux-musl-arm64"\%%i
del MuPDFCoreTests-linux-musl-arm64\* /s /f /q 1>nul
rmdir MuPDFCoreTests-linux-musl-arm64

cd ..\..

echo.
echo Building with target [94mmac-x64[0m

dotnet clean MuPDFCoreTestHost /nowarn:NETSDK1138
dotnet clean Tests /nowarn:NETSDK1138
rmdir /s /q Tests\bin Tests\obj MuPDFCoreTestHost\bin MuPDFCoreTestHost\obj

cd MuPDFCoreTestHost
dotnet publish -c Release /p:PublishProfile=Properties\PublishProfiles\mac-x64.pubxml /nowarn:NETSDK1138
cd ..

echo.
echo [104;97mCreating ZIP file...[0m

cd Release\MuPDFCoreTests

move mac-x64 MuPDFCoreTests-mac-x64
bash -c "zip -r MuPDFCoreTests-mac-x64.zip MuPDFCoreTests-mac-x64 >/dev/null"

for /f %%i in ('dir /a:d /b "MuPDFCoreTests-mac-x64"\*') do rd /s /q "MuPDFCoreTests-mac-x64"\%%i
del MuPDFCoreTests-mac-x64\* /s /f /q 1>nul
rmdir MuPDFCoreTests-mac-x64

cd ..\..

echo.
echo Building with target [94mmac-arm64[0m

dotnet clean MuPDFCoreTestHost /nowarn:NETSDK1138
dotnet clean Tests /nowarn:NETSDK1138
rmdir /s /q Tests\bin Tests\obj MuPDFCoreTestHost\bin MuPDFCoreTestHost\obj

cd MuPDFCoreTestHost
dotnet publish -c Release /p:PublishProfile=Properties\PublishProfiles\mac-arm64.pubxml /p:PlatformTarget=arm64 /nowarn:NETSDK1138
cd ..

echo.
echo [104;97mCreating ZIP file...[0m

cd Release\MuPDFCoreTests

move mac-arm64 MuPDFCoreTests-mac-arm64
bash -c "zip -r MuPDFCoreTests-mac-arm64.zip MuPDFCoreTests-mac-arm64 >/dev/null"

for /f %%i in ('dir /a:d /b "MuPDFCoreTests-mac-arm64"\*') do rd /s /q "MuPDFCoreTests-mac-arm64"\%%i
del MuPDFCoreTests-mac-arm64\* /s /f /q 1>nul
rmdir MuPDFCoreTests-mac-arm64

cd ..\..

echo.
echo [94mAll done![0m