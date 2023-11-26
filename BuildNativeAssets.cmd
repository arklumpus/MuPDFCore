@echo off

cd MuPDFCore.NativeAssets

echo.
echo Building [94mlinux-arm64[0m package...

cd Linux-arm64
dotnet pack -c Release 
cd ..

echo.
echo Building [94mlinux-musl-arm64[0m package...

cd Linux-musl-arm64
dotnet pack -c Release 
cd ..

echo.
echo Building [94mlinux-musl-x64[0m package...

cd Linux-musl-x64
dotnet pack -c Release 
cd ..

echo.
echo Building [94mlinux-x64[0m package...

cd Linux-x64
dotnet pack -c Release 
cd ..

echo.
echo Building [94mmacOS-arm64[0m package...

cd Mac-arm64
dotnet pack -c Release 
cd ..

echo.
echo Building [94mmacOS-x64[0m package...

cd Mac-x64
dotnet pack -c Release 
cd ..

echo.
echo Building [94mwin-arm64[0m package...

cd Win-arm64
dotnet pack -c Release 
cd ..

echo.
echo Building [94mwin-x64[0m package...

cd Win-x64
dotnet pack -c Release 
cd ..

echo.
echo Building [94mwin-x86[0m package...

cd Win-x86
dotnet pack -c Release 
cd ..

cd ..
